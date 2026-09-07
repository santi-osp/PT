-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: fn_centers_get
-- Tipo: Stored Function
-- Propósito: Lista los centros activos.
-- Usada por: CenterRepository
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_centers_get()
                RETURNS TABLE (id uuid, code text, name text)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT "Id", "Code"::text, "Name"::text
                    FROM centers
                    WHERE "IsActive"
                    ORDER BY "Code";
                $$;
