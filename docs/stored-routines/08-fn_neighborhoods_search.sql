-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: fn_neighborhoods_search
-- Tipo: Stored Function
-- Propósito: Busca barrios y devuelve municipio, departamento, país y zona de transporte.
-- Usada por: NeighborhoodRepository
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907160000_ExpandNeighborhoodCatalogRoutine.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE FUNCTION fn_neighborhoods_search(p_query text DEFAULT NULL)
            RETURNS TABLE (
                id uuid,
                name text,
                municipality text,
                department text,
                country text,
                transport_zone text)
            LANGUAGE sql
            STABLE
            AS $$
                SELECT n."Id", n."Name"::text, m."Name"::text, d."Name"::text,
                    c."Name"::text, tz."Code"::text
                FROM neighborhoods n
                JOIN municipalities m ON m."Id" = n."MunicipalityId"
                JOIN departments d ON d."Id" = m."DepartmentId"
                JOIN countries c ON c."Id" = d."CountryId"
                JOIN transport_zones tz ON tz."Id" = n."TransportZoneId"
                WHERE p_query IS NULL OR btrim(p_query) = ''
                    OR n."Name" ILIKE '%' || btrim(p_query) || '%'
                ORDER BY n."Name"
                LIMIT 100;
            $$;
