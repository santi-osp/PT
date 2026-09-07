-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: fn_residential_customer_search
-- Tipo: Stored Function
-- Propósito: Busca clientes por texto y estado (all/active/blocked).
-- Usada por: ResidentialCustomerRepository.SearchAsync
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_residential_customer_search(
                    p_search text DEFAULT NULL,
                    p_status text DEFAULT 'all')
                RETURNS TABLE (result jsonb)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT rows.result
                    FROM fn_residential_customer_rows() rows
                    WHERE (p_status = 'all'
                            OR (p_status = 'active' AND NOT rows.is_blocked)
                            OR (p_status = 'blocked' AND rows.is_blocked))
                      AND (p_search IS NULL OR btrim(p_search) = ''
                            OR rows.code ILIKE '%' || btrim(p_search) || '%'
                            OR rows.document_number ILIKE '%' || btrim(p_search) || '%'
                            OR rows.business_name ILIKE '%' || btrim(p_search) || '%'
                            OR rows.full_name ILIKE '%' || btrim(p_search) || '%'
                            OR rows.extended_legal_name ILIKE '%' || btrim(p_search) || '%')
                    ORDER BY rows.created_at DESC
                    LIMIT 100;
                $$;
