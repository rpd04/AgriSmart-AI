from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from PIL import Image
import numpy as np
import joblib
import io
import os

MODELS_DIR = os.path.join(os.path.dirname(__file__), "models")

app = FastAPI(
    title="AgriSmart AI Service",
    description="Disease detection and yield prediction inference API for AgriSmart AI.",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # tighten this to your API/frontend origin in production
    allow_methods=["*"],
    allow_headers=["*"],
)

# ---- load models at startup -------------------------------------------------
yield_bundle = None
disease_bundle = None


@app.on_event("startup")
def load_models():
    global yield_bundle, disease_bundle
    yield_path = os.path.join(MODELS_DIR, "yield_model.joblib")
    disease_path = os.path.join(MODELS_DIR, "disease_model.joblib")
    if not os.path.exists(yield_path) or not os.path.exists(disease_path):
        raise RuntimeError(
            "Model files not found. Run `python train_models.py` before starting the service."
        )
    yield_bundle = joblib.load(yield_path)
    disease_bundle = joblib.load(disease_path)


# ---- schemas ------------------------------------------------------------
class YieldRequest(BaseModel):
    nitrogen: float = Field(..., ge=0, le=300, description="Soil nitrogen, ppm")
    phosphorus: float = Field(..., ge=0, le=300, description="Soil phosphorus, ppm")
    potassium: float = Field(..., ge=0, le=400, description="Soil potassium, ppm")
    ph: float = Field(..., ge=0, le=14, description="Soil pH")
    moisture: float = Field(..., ge=0, le=100, description="Soil moisture, %")
    rainfall: float = Field(..., ge=0, le=1000, description="Recent rainfall, mm")
    temperature: float = Field(..., ge=-10, le=55, description="Average temperature, C")


class YieldResponse(BaseModel):
    predicted_yield_kg_per_acre: float
    limiting_factor: str
    model_version: str = "yield-rf-v1"


class DiseaseResponse(BaseModel):
    predicted_disease: str
    confidence: float
    treatment_advice: str
    model_version: str = "disease-rf-v1"


# ---- helpers ------------------------------------------------------------
OPTIMAL = {
    "nitrogen": (60, 100), "phosphorus": (30, 60), "potassium": (80, 140),
    "ph": (6.0, 7.0), "moisture": (25, 40), "rainfall": (80, 180),
}


def find_limiting_factor(payload: YieldRequest) -> str:
    worst_field, worst_score = None, 1.0
    for field, (lo, hi) in OPTIMAL.items():
        val = getattr(payload, field)
        mid, span = (lo + hi) / 2, (hi - lo) / 2
        score = max(0.0, 1 - abs(val - mid) / (span * 1.5))
        if score < worst_score:
            worst_score, worst_field = score, field
    if worst_field is None or worst_score > 0.6:
        return "None — inputs are within a healthy range"
    return f"{worst_field} (outside optimal range {OPTIMAL[worst_field]})"


def image_features(img: Image.Image, img_size: int):
    arr = np.array(img.resize((img_size, img_size)).convert("RGB")) / 255.0
    means = arr.mean(axis=(0, 1))
    stds = arr.std(axis=(0, 1))
    hist_r = np.histogram(arr[:, :, 0], bins=8, range=(0, 1))[0] / arr[:, :, 0].size
    hist_g = np.histogram(arr[:, :, 1], bins=8, range=(0, 1))[0] / arr[:, :, 1].size
    hist_b = np.histogram(arr[:, :, 2], bins=8, range=(0, 1))[0] / arr[:, :, 2].size
    return np.concatenate([means, stds, hist_r, hist_g, hist_b]).reshape(1, -1)


# ---- endpoints ------------------------------------------------------------
@app.get("/health")
def health():
    return {"status": "ok", "models_loaded": yield_bundle is not None and disease_bundle is not None}


@app.post("/predict-yield", response_model=YieldResponse)
def predict_yield(payload: YieldRequest):
    model = yield_bundle["model"]
    features = yield_bundle["features"]
    row = [[getattr(payload, f) for f in features]]
    prediction = float(model.predict(row)[0])
    return YieldResponse(
        predicted_yield_kg_per_acre=round(prediction, 1),
        limiting_factor=find_limiting_factor(payload),
    )


@app.post("/predict-disease", response_model=DiseaseResponse)
async def predict_disease(file: UploadFile = File(...)):
    if not file.content_type or not file.content_type.startswith("image/"):
        raise HTTPException(status_code=400, detail="Please upload an image file.")
    contents = await file.read()
    try:
        img = Image.open(io.BytesIO(contents))
    except Exception:
        raise HTTPException(status_code=400, detail="Could not read the uploaded image.")

    model = disease_bundle["model"]
    labels = disease_bundle["labels"]
    treatments = disease_bundle["treatments"]
    img_size = disease_bundle["img_size"]

    features = image_features(img, img_size)
    probs = model.predict_proba(features)[0]
    best_idx = int(np.argmax(probs))
    predicted_label = model.classes_[best_idx]

    return DiseaseResponse(
        predicted_disease=predicted_label,
        confidence=round(float(probs[best_idx]), 4),
        treatment_advice=treatments.get(predicted_label, "Consult a local agronomist."),
    )
