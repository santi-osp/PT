-- =============================================================================
-- DATAGO — DOCUMENTACIÓN DE STORED ROUTINE
-- =============================================================================
-- Rutina: fn_residential_customer_rows
-- Tipo: Stored Function interna
-- Propósito: Construye la proyección JSON completa del cliente, dirección, centro y geografía.
-- Usada por: fn_residential_customer_get y fn_residential_customer_search
-- Fuente de verdad runtime:
--   backend/src/DataGo.Infrastructure/Persistence/Migrations/20260907142442_AddResidentialCustomerStoredRoutines.cs
--
-- IMPORTANTE:
--   ESTE ARCHIVO ES SOLO DOCUMENTACIÓN / REFERENCIA PARA LECTURA.
--   NO forma parte del mecanismo de migraciones y NO debe ejecutarse manualmente.
--   Las migraciones de EF Core continúan siendo la fuente de verdad para crear,
--   modificar o eliminar esta rutina en PostgreSQL/Neon.
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_residential_customer_rows()
                RETURNS TABLE (
                    customer_id uuid,
                    code text,
                    document_number text,
                    business_name text,
                    full_name text,
                    extended_legal_name text,
                    is_blocked boolean,
                    created_at timestamp with time zone,
                    result jsonb)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT c."Id", c."Code"::text, c."DocumentNumber"::text, c."BusinessName"::text,
                        c."FullName"::text, c."ExtendedLegalName"::text, c."IsBlocked", c."CreatedAt",
                        jsonb_build_object(
                            'id', c."Id", 'code', c."Code", 'treatment', c."Treatment",
                            'businessName', c."BusinessName", 'extendedLegalName', c."ExtendedLegalName",
                            'fullName', c."FullName", 'firstNames', c."FirstNames", 'lastNames', c."LastNames",
                            'phone', c."Phone", 'phoneExtension', c."PhoneExtension",
                            'mobilePhone', c."MobilePhone", 'email', c."Email",
                            'documentType', c."DocumentType", 'documentNumber', c."DocumentNumber",
                            'verificationDigit', c."VerificationDigit", 'taxClass', c."TaxClass",
                            'paymentCondition', c."PaymentCondition", 'stratum', c."Stratum",
                            'centerId', c."CenterId", 'isBlocked', c."IsBlocked", 'blockedAt', c."BlockedAt",
                            'createdAt', c."CreatedAt", 'updatedAt', c."UpdatedAt",
                            'address', jsonb_build_object(
                                'id', a."Id", 'neighborhoodId', a."NeighborhoodId", 'isRural', a."IsRural",
                                'ruralAddress', a."RuralAddress", 'mainRoadType', a."MainRoadType",
                                'mainRoadNumber', a."MainRoadNumber", 'mainRoadLetter', a."MainRoadLetter",
                                'mainRoadCardinality', a."MainRoadCardinality",
                                'secondaryRoadNumber1', a."SecondaryRoadNumber1",
                                'secondaryRoadLetter', a."SecondaryRoadLetter",
                                'secondaryRoadCardinality1', a."SecondaryRoadCardinality1",
                                'secondaryRoadNumber2', a."SecondaryRoadNumber2",
                                'secondaryRoadCardinality2', a."SecondaryRoadCardinality2",
                                'formattedAddress', a."FormattedAddress"),
                            'center', jsonb_build_object('id', ce."Id", 'code', ce."Code", 'name', ce."Name"),
                            'geography', jsonb_build_object(
                                'neighborhood', n."Name", 'municipality', m."Name", 'department', d."Name",
                                'country', co."Name", 'transportZone', tz."Code"))
                    FROM residential_customers c
                    JOIN customer_addresses a ON a."CustomerId" = c."Id"
                    JOIN centers ce ON ce."Id" = c."CenterId"
                    JOIN neighborhoods n ON n."Id" = a."NeighborhoodId"
                    JOIN municipalities m ON m."Id" = n."MunicipalityId"
                    JOIN departments d ON d."Id" = m."DepartmentId"
                    JOIN countries co ON co."Id" = d."CountryId"
                    JOIN transport_zones tz ON tz."Id" = n."TransportZoneId";
                $$;
