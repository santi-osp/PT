-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: sp_residential_customer_create
-- Tipo: Stored Procedure
-- Propósito: Crea el Cliente Residencial y su dirección asociada.
-- Usada por: ResidentialCustomerRepository.AddAsync
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE PROCEDURE sp_residential_customer_create(IN p_customer jsonb)
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    INSERT INTO residential_customers (
                        "Id", "Code", "Treatment", "BusinessName", "ExtendedLegalName", "FullName",
                        "FirstNames", "LastNames", "Phone", "PhoneExtension", "MobilePhone", "Email",
                        "DocumentType", "DocumentNumber", "VerificationDigit", "TaxClass", "PaymentCondition",
                        "Stratum", "CenterId", "IsBlocked", "BlockedAt", "CreatedAt", "UpdatedAt")
                    VALUES (
                        (p_customer->>'id')::uuid, p_customer->>'code', (p_customer->>'treatment')::integer,
                        p_customer->>'businessName', p_customer->>'extendedLegalName', p_customer->>'fullName',
                        p_customer->>'firstNames', p_customer->>'lastNames', p_customer->>'phone',
                        p_customer->>'phoneExtension', p_customer->>'mobilePhone', p_customer->>'email',
                        (p_customer->>'documentType')::integer, p_customer->>'documentNumber',
                        p_customer->>'verificationDigit', (p_customer->>'taxClass')::integer,
                        p_customer->>'paymentCondition', (p_customer->>'stratum')::smallint,
                        (p_customer->>'centerId')::uuid, (p_customer->>'isBlocked')::boolean,
                        (p_customer->>'blockedAt')::timestamp with time zone,
                        (p_customer->>'createdAt')::timestamp with time zone,
                        (p_customer->>'updatedAt')::timestamp with time zone);

                    INSERT INTO customer_addresses (
                        "Id", "CustomerId", "NeighborhoodId", "IsRural", "RuralAddress", "MainRoadType",
                        "MainRoadNumber", "MainRoadLetter", "MainRoadCardinality", "SecondaryRoadNumber1",
                        "SecondaryRoadLetter", "SecondaryRoadCardinality1", "SecondaryRoadNumber2",
                        "SecondaryRoadCardinality2", "FormattedAddress")
                    VALUES (
                        (p_customer->'address'->>'id')::uuid, (p_customer->>'id')::uuid,
                        (p_customer->'address'->>'neighborhoodId')::uuid,
                        (p_customer->'address'->>'isRural')::boolean,
                        p_customer->'address'->>'ruralAddress', p_customer->'address'->>'mainRoadType',
                        p_customer->'address'->>'mainRoadNumber', p_customer->'address'->>'mainRoadLetter',
                        p_customer->'address'->>'mainRoadCardinality',
                        p_customer->'address'->>'secondaryRoadNumber1',
                        p_customer->'address'->>'secondaryRoadLetter',
                        p_customer->'address'->>'secondaryRoadCardinality1',
                        p_customer->'address'->>'secondaryRoadNumber2',
                        p_customer->'address'->>'secondaryRoadCardinality2',
                        p_customer->'address'->>'formattedAddress');
                END;
                $$;
