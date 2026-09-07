-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: fn_modern_channel_customer_exists
-- Tipo: Stored Function
-- Propósito: Comprueba si un documento ya existe en Clientes Canal Moderno.
-- Usada por: ModernChannelCustomerRepository
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_modern_channel_customer_exists(p_type integer, p_number text)
                RETURNS boolean
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT EXISTS (
                        SELECT 1 FROM modern_channel_customers
                        WHERE "DocumentType" = p_type AND "DocumentNumber" = p_number);
                $$;
