# PROJECT_STATE

- **Backend:** COMPLETE. ASP.NET Core 10 Clean Architecture implementation and real persistence smoke are validated.
- **Frontend:** Not started; intentionally outside Goal 2.
- **Database:** NEON CONNECTED. EF Core 10 + Npgsql 10.0.3; `InitialCreate` and `AddResidentialCustomerStoredRoutines` are applied. Three command procedures and six functions are verified in `pg_proc`.
- **CRUD:** CREATE, READ, SEARCH, UPDATE, and RETIRE implemented. Blocked customers remain visible and may be filtered with `status`.
- **Endpoints:** POST/GET/PUT residential customers, PATCH retire, catalogs, health, and Swagger.
- **Tests:** 18 focused cases passing. Post-migration Neon smoke passed stored-routine create/read/search/update/retire, blocked read, blocked PUT 409, and repeated retire 409.
- **Known issues:** None known for the Goal 2 backend scope.
- **Deployment:** Not started; configuration is environment-driven and ready for a later Render goal.
- **Next:** GOAL 3 — Angular UI + complete API integration.
