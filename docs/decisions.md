# Decision log

1. **Monorepo:** backend, future frontend, and documentation share one delivery unit.
2. **Clean Architecture:** Domain and Application remain independent from HTTP and persistence; Infrastructure implements inward repository ports.
3. **PostgreSQL/Neon:** production-shaped persistence uses Npgsql and real EF migrations; no SQLite substitute.
4. **Blocked visibility:** logically retired customers remain in detail, search, and the default `all` list for traceability. Optional `active`/`blocked` filters are supported.
5. **Logical retirement:** retirement only sets `IsBlocked`, `BlockedAt`, and `UpdatedAt`; repeated retirement is a conflict.
6. **Independent business name:** editing `BusinessName` never changes `ExtendedLegalName`.
7. **Company update:** one legal company name is retained. If both `FirstNames` and `LastNames` are supplied they must match; either single value becomes the legal name, and all four name fields are synchronized.
8. **Future assistant:** AI reuses Application use cases after draft, validation, and user confirmation; it never accesses EF/database directly.
9. **External secrets:** local credentials live in ASP.NET user-secrets and deployment credentials will use `ConnectionStrings__DefaultConnection`; real credentials are never tracked.
