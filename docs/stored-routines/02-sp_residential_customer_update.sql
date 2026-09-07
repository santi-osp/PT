-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: sp_residential_customer_update
-- Tipo: Stored Procedure
-- Propósito: Actualiza los campos editables del cliente y su dirección; protege clientes bloqueados.
-- Usada por: ResidentialCustomerRepository.UpdateAsync
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE PROCEDURE sp_residential_customer_update(IN p_customer jsonb)
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    UPDATE residential_customers
                    SET "BusinessName" = p_customer->>'businessName',
                        "ExtendedLegalName" = p_customer->>'extendedLegalName',
                        "FullName" = p_customer->>'fullName',
                        "FirstNames" = p_customer->>'firstNames',
                        "LastNames" = p_customer->>'lastNames',
                        "Phone" = p_customer->>'phone',
                        "PhoneExtension" = p_customer->>'phoneExtension',
                        "MobilePhone" = p_customer->>'mobilePhone',
                        "Email" = p_customer->>'email',
                        "Stratum" = (p_customer->>'stratum')::smallint,
                        "CenterId" = (p_customer->>'centerId')::uuid,
                        "UpdatedAt" = (p_customer->>'updatedAt')::timestamp with time zone
                    WHERE "Id" = (p_customer->>'id')::uuid AND NOT "IsBlocked";

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'customer_not_updatable' USING ERRCODE = 'P0001';
                    END IF;

                    UPDATE customer_addresses
                    SET "NeighborhoodId" = (p_customer->'address'->>'neighborhoodId')::uuid,
                        "IsRural" = (p_customer->'address'->>'isRural')::boolean,
                        "RuralAddress" = p_customer->'address'->>'ruralAddress',
                        "MainRoadType" = p_customer->'address'->>'mainRoadType',
                        "MainRoadNumber" = p_customer->'address'->>'mainRoadNumber',
                        "MainRoadLetter" = p_customer->'address'->>'mainRoadLetter',
                        "MainRoadCardinality" = p_customer->'address'->>'mainRoadCardinality',
                        "SecondaryRoadNumber1" = p_customer->'address'->>'secondaryRoadNumber1',
                        "SecondaryRoadLetter" = p_customer->'address'->>'secondaryRoadLetter',
                        "SecondaryRoadCardinality1" = p_customer->'address'->>'secondaryRoadCardinality1',
                        "SecondaryRoadNumber2" = p_customer->'address'->>'secondaryRoadNumber2',
                        "SecondaryRoadCardinality2" = p_customer->'address'->>'secondaryRoadCardinality2',
                        "FormattedAddress" = p_customer->'address'->>'formattedAddress'
                    WHERE "CustomerId" = (p_customer->>'id')::uuid;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'customer_address_missing' USING ERRCODE = 'P0002';
                    END IF;
                END;
                $$;
