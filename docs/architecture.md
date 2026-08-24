# Architecture

Client (Blazor/React) → ASP.NET Core Web API → SQL Server (EF Core)
                              ↓
                    Python FastAPI AI service (CNN + ML.NET)
                              ↓
              External APIs: OpenWeatherMap, Maps, SendGrid/Twilio

See the full project report (AgriSmart_AI_Project_Report.docx) for the
detailed component breakdown, database ER description, and SDLC phase notes.
