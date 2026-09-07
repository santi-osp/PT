-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: fn_residential_customer_get
-- Tipo: Stored Function
-- Propósito: Obtiene un Cliente Residencial por su UUID.
-- Usada por: ResidentialCustomerRepository.ReadOneAsync
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_residential_customer_get(p_id uuid)
                RETURNS TABLE (result jsonb)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT rows.result
                    FROM fn_residential_customer_rows() rows
                    WHERE rows.customer_id = p_id;
                $$;
