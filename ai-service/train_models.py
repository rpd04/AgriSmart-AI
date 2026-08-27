"""
Trains the two demo models used by the AI service:

1. Yield-prediction model (RandomForestRegressor) — trained on synthetic
   but agronomically-plausible data: yield goes up as N/P/K and pH move
   toward known optimal ranges for the crop, and as rainfall/moisture
   are adequate but not excessive.

2. Disease-detection model (RandomForestClassifier on colour-histogram
   features) — trained on synthetic leaf-coloured images: healthy leaves
   are mostly green, "blight"/"rust"/"mildew" leaves have programmatically
   added brown/yellow/white patches.

IMPORTANT: these are demo models trained on synthetic data so the whole
pipeline (upload -> inference -> stored result) is real and runnable
end-to-end. For production, retrain on real agronomic records and a real
labelled leaf-image dataset (e.g. PlantVillage) using the same interface
(`models/yield_model.joblib`, `models/disease_model.joblib`).
"""
import numpy as np
import pandas as pd
from PIL import Image, ImageDraw
from sklearn.ensemble import RandomForestRegressor, RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_absolute_error, accuracy_score
import joblib
import os
import random

os.makedirs("models", exist_ok=True)
rng = np.random.default_rng(42)

# ---------------------------------------------------------------------
# 1. Yield prediction model
# ---------------------------------------------------------------------
N = 4000
nitrogen = rng.uniform(0, 140, N)
phosphorus = rng.uniform(0, 100, N)
potassium = rng.uniform(0, 200, N)
ph = rng.uniform(4.5, 8.5, N)
moisture = rng.uniform(5, 60, N)
rainfall = rng.uniform(0, 300, N)
temperature = rng.uniform(10, 42, N)

# Optimal ranges (rough real-world agronomy heuristics)
def closeness(x, lo, hi):
    mid = (lo + hi) / 2
    span = (hi - lo) / 2
    return np.clip(1 - np.abs(x - mid) / (span * 1.5), 0, 1)

score = (
    0.25 * closeness(nitrogen, 60, 100)
    + 0.15 * closeness(phosphorus, 30, 60)
    + 0.15 * closeness(potassium, 80, 140)
    + 0.15 * closeness(ph, 6.0, 7.0)
    + 0.15 * closeness(moisture, 25, 40)
    + 0.15 * closeness(rainfall, 80, 180)
)
base_yield_kg_per_acre = 2500  # a reasonable baseline for many staple crops
yield_kg = base_yield_kg_per_acre * (0.4 + 1.1 * score) + rng.normal(0, 80, N)
yield_kg = np.clip(yield_kg, 200, None)

df = pd.DataFrame({
    "nitrogen": nitrogen, "phosphorus": phosphorus, "potassium": potassium,
    "ph": ph, "moisture": moisture, "rainfall": rainfall, "temperature": temperature,
    "yield_kg_per_acre": yield_kg,
})

X = df.drop(columns=["yield_kg_per_acre"])
y = df["yield_kg_per_acre"]
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

yield_model = RandomForestRegressor(n_estimators=200, max_depth=10, random_state=42)
yield_model.fit(X_train, y_train)
mae = mean_absolute_error(y_test, yield_model.predict(X_test))
print(f"[yield model] test MAE: {mae:.1f} kg/acre (baseline ~{base_yield_kg_per_acre} kg/acre)")

joblib.dump({"model": yield_model, "features": list(X.columns)}, "models/yield_model.joblib")

# ---------------------------------------------------------------------
# 2. Disease detection model (synthetic leaf images -> colour features)
# ---------------------------------------------------------------------
IMG_SIZE = 64
LABELS = ["Healthy", "Leaf Blight", "Powdery Mildew", "Rust"]
TREATMENTS = {
    "Healthy": "No treatment needed. Continue regular monitoring and balanced fertilization.",
    "Leaf Blight": "Remove and destroy infected leaves. Apply a copper-based fungicide and avoid overhead irrigation.",
    "Powdery Mildew": "Improve air circulation, avoid excess nitrogen, and apply a sulfur-based or neem-oil fungicide.",
    "Rust": "Apply a triazole fungicide, remove volunteer plants nearby, and rotate crops next season.",
}

def make_synthetic_leaf(label, seed):
    rnd = random.Random(seed)
    img = Image.new("RGB", (IMG_SIZE, IMG_SIZE), (34, 120, 34))
    draw = ImageDraw.Draw(img)
    # base green leaf shading noise
    for _ in range(30):
        x, y = rnd.randint(0, IMG_SIZE), rnd.randint(0, IMG_SIZE)
        g = rnd.randint(90, 150)
        draw.ellipse([x, y, x + 4, y + 4], fill=(20, g, 20))
    if label == "Leaf Blight":
        for _ in range(rnd.randint(6, 12)):
            x, y = rnd.randint(0, IMG_SIZE), rnd.randint(0, IMG_SIZE)
            r = rnd.randint(3, 7)
            draw.ellipse([x - r, y - r, x + r, y + r], fill=(101, 67, 33))
    elif label == "Powdery Mildew":
        for _ in range(rnd.randint(15, 30)):
            x, y = rnd.randint(0, IMG_SIZE), rnd.randint(0, IMG_SIZE)
            r = rnd.randint(2, 4)
            draw.ellipse([x - r, y - r, x + r, y + r], fill=(230, 230, 230))
    elif label == "Rust":
        for _ in range(rnd.randint(10, 20)):
            x, y = rnd.randint(0, IMG_SIZE), rnd.randint(0, IMG_SIZE)
            r = rnd.randint(2, 5)
            draw.ellipse([x - r, y - r, x + r, y + r], fill=(180, 90, 20))
    return img

def image_features(img: Image.Image):
    arr = np.array(img.resize((IMG_SIZE, IMG_SIZE)).convert("RGB")) / 255.0
    means = arr.mean(axis=(0, 1))
    stds = arr.std(axis=(0, 1))
    hist_r = np.histogram(arr[:, :, 0], bins=8, range=(0, 1))[0] / arr[:, :, 0].size
    hist_g = np.histogram(arr[:, :, 1], bins=8, range=(0, 1))[0] / arr[:, :, 1].size
    hist_b = np.histogram(arr[:, :, 2], bins=8, range=(0, 1))[0] / arr[:, :, 2].size
    return np.concatenate([means, stds, hist_r, hist_g, hist_b])

X_img, y_img = [], []
for i in range(1500):
    label = LABELS[i % len(LABELS)]
    img = make_synthetic_leaf(label, seed=i)
    X_img.append(image_features(img))
    y_img.append(label)

X_img = np.array(X_img)
Xi_train, Xi_test, yi_train, yi_test = train_test_split(X_img, y_img, test_size=0.2, random_state=42, stratify=y_img)

disease_model = RandomForestClassifier(n_estimators=200, max_depth=12, random_state=42)
disease_model.fit(Xi_train, yi_train)
acc = accuracy_score(yi_test, disease_model.predict(Xi_test))
print(f"[disease model] test accuracy: {acc:.3f}")

joblib.dump(
    {"model": disease_model, "labels": LABELS, "treatments": TREATMENTS, "img_size": IMG_SIZE},
    "models/disease_model.joblib",
)

print("Saved models/yield_model.joblib and models/disease_model.joblib")
