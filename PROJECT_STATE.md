# PROJECT_STATE

- **Backend:** ASP.NET Core 10, Clean Architecture (`Domain <- Application <- Infrastructure/Api`), controllers and reusable application handlers.
- **Frontend:** Not started; intentionally outside Goal 1.
- **Database:** EF Core 10 + Npgsql/PostgreSQL. Real `InitialCreate` migration includes geographic catalogs, 44 Medellín demonstration neighborhoods, 20 fictitious centers, and 5 modern-channel conflicts. Configure `ConnectionStrings__DefaultConnection`; no production secret is committed.
- **Deployment:** Not started; intentionally outside Goal 1.
- **Endpoints:** `POST/GET /api/residential-customers`, `GET /api/residential-customers/{id}`, `GET /api/catalogs/neighborhoods`, `GET /api/catalogs/centers`, `GET /api/health`, `/swagger`.
- **Tests:** 13 passing unit cases covering NIT, documented name splitting, contact/company creation rules, and modern-channel conflict.
- **Known issues:** No accessible local PostgreSQL or Docker daemon was available, so migration application and DB-backed HTTP smoke tests remain for Neon/staging. The migration was generated and successfully rendered to PostgreSQL SQL.
- **Next:** GOAL 2 — modificación + retiro + cierre backend.
