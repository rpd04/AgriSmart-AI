# AgriSmart AI 

An AI-powered smart agricultural platform: **ASP.NET Core 8 Web API** backend, a **Python FastAPI**
AI microservice (disease detection + yield prediction), and a **React (Vite)** frontend.

This is a real, runnable codebase — not just a scaffold. The AI service ships with demo models
trained on synthetic data so every endpoint works out of the box; swap in real datasets when ready
(see `ai-service/train_models.py`).

##  Team — Group 20
| Roll No. | Name | Role |
|---|---|---|
| IN26013537 | Priyansh Dubey | Team Lead / Backend Architect |
| IN26015080 | Siddhartha Kumar | Backend Developer |
| — | Atharva Bhati | Frontend Developer |
| IN26014186 | Arpit Barnwal | Database Designer / DBA |
| IN26014417 | Bhavyavardhan Singh | AI/ML Engineer |
| IN26014926 | Hemanga Ojah | QA / DevOps Engineer |
| IN26013004 | Ojas Sen | Integration & Documentation Specialist |

##  Features
-  Crop disease detection from a leaf photo
-  Yield prediction from soil, crop and weather data
-  Farmer marketplace with AI-suggested pricing
-  JWT authentication, role-based access (Farmer / Agronomist / Admin)

##  Project Structure
```
src/
  AgriSmart.API/            ASP.NET Core Web API (controllers, DTOs, EF Core, JWT auth)
  AgriSmart.Core/           Domain entities
  AgriSmart.Client/         React (Vite) frontend
ai-service/                 FastAPI + scikit-learn inference service
  train_models.py           Generates synthetic data & trains the demo models
  app.py                    /predict-disease and /predict-yield endpoints
database/schema.sql         Reference SQL Server DDL (SQLite is used for local dev by default)
docs/                       Project report, architecture notes, SRS, individual viva prep guide
```

##  Running it locally (no Docker)

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Python 3.10+](https://www.python.org/downloads/)
- [Node.js 18+](https://nodejs.org/)

### 2. Start the AI service (first — the API calls it)
```bash
cd ai-service
python -m venv venv
venv\Scripts\activate            # Windows
pip install -r requirements.txt
python train_models.py           # trains and saves the demo models (~30s)
uvicorn app:app --reload --port 8000
```
Check it's up: http://localhost:8000/health and http://localhost:8000/docs (interactive Swagger UI).

### 3. Start the API
```bash
cd src/AgriSmart.API
dotnet restore
dotnet run
```
The API creates its SQLite database (`agrismart.db`) automatically on first run **and seeds it with
demo data** (one login, a farm, crop records, disease/yield history, and marketplace listings) — see
`Data/DbSeeder.cs`. Swagger UI opens at http://localhost:5000/swagger.

**Demo login:** `demo@agrismart.ai` / `Demo@1234`

> Before running in anything but local dev, replace `Jwt:Key` in `appsettings.json` with your own
> long random secret.

### 4. Start the frontend
```bash
cd src/AgriSmart.Client
npm install
copy .env.example .env
npm run dev
```
Open http://localhost:5173 — log in with the demo account above (or register a new one), and try
the disease scan and yield predictor against the seeded farm/crop records.

##  Running it with Docker
```bash
docker-compose up --build
```
This builds and starts all three services: API (`:5000`), AI service (`:8000`), client (`:5173`).

##  Trying it out
1. Log in with the seeded demo account (or register a new one).
2. The dashboard already shows a seeded farm and crop record.
3. Go to **Yield Predictor**, use the seeded crop record ID, enter soil values → get a prediction.
4. Go to **Disease Scan**, use the seeded crop record ID, upload any leaf-like photo → get a diagnosis.
5. Go to **Marketplace** to see seeded listings or create a new one.

##  Retraining with real data
`ai-service/train_models.py` currently trains on programmatically generated synthetic data so the
whole pipeline runs without any dataset download. To use real data:
- **Yield model**: replace the synthetic `DataFrame` with your own soil/weather/yield records, keep
  the same feature names, and the rest of the script (train/test split, `RandomForestRegressor`,
  save to `models/yield_model.joblib`) works unchanged.
- **Disease model**: point the training loop at a real labelled leaf-image dataset (e.g.
  [PlantVillage](https://www.kaggle.com/datasets/emmarex/plantdisease)) instead of
  `make_synthetic_leaf`, and consider swapping the RandomForest-on-colour-histograms approach for a
  real CNN (TensorFlow/Keras) once you have enough images.

##  Git workflow
- `main` — always deployable
- `develop` — sprint integration branch
- `feature/<name>` — one branch per feature, merged via PR + review

##  Docs
- `docs/AgriSmart_AI_Project_Report.docx` — full write-up: architecture, database design, team
  roles, and SDLC-phase contributions.
- `docs/Individual_Viva_Prep_Guide.docx` — per-member viva prep: their module, their files, and
  likely questions on C#, EF Core, ASP.NET Core, and their specific area.

##  License
MIT
