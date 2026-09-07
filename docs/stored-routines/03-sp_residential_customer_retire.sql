-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: sp_residential_customer_retire
-- Tipo: Stored Procedure
-- Propósito: Realiza la baja lógica del cliente marcándolo como bloqueado.
-- Usada por: ResidentialCustomerRepository.RetireAsync
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE PROCEDURE sp_residential_customer_retire(
                    IN p_id uuid,
                    IN p_blocked_at timestamp with time zone,
                    IN p_updated_at timestamp with time zone)
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    UPDATE residential_customers
                    SET "IsBlocked" = TRUE,
                        "BlockedAt" = p_blocked_at,
                        "UpdatedAt" = p_updated_at
                    WHERE "Id" = p_id AND NOT "IsBlocked";

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'customer_already_blocked' USING ERRCODE = 'P0001';
                    END IF;
                END;
                $$;
