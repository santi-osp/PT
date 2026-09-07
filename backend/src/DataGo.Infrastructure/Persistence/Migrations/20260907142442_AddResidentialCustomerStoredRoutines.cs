using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataGo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentialCustomerStoredRoutines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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

                CREATE OR REPLACE FUNCTION fn_residential_customer_get(p_id uuid)
                RETURNS TABLE (result jsonb)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT rows.result
                    FROM fn_residential_customer_rows() rows
                    WHERE rows.customer_id = p_id;
                $$;

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

                CREATE OR REPLACE FUNCTION fn_neighborhoods_search(p_query text DEFAULT NULL)
                RETURNS TABLE (id uuid, name text)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT "Id", "Name"::text
                    FROM neighborhoods
                    WHERE p_query IS NULL OR btrim(p_query) = '' OR "Name" ILIKE '%' || btrim(p_query) || '%'
                    ORDER BY "Name"
                    LIMIT 100;
                $$;

                CREATE OR REPLACE FUNCTION fn_modern_channel_customer_exists(p_type integer, p_number text)
                RETURNS boolean
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT EXISTS (
                        SELECT 1 FROM modern_channel_customers
                        WHERE "DocumentType" = p_type AND "DocumentNumber" = p_number);
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS fn_modern_channel_customer_exists(integer, text);
                DROP FUNCTION IF EXISTS fn_neighborhoods_search(text);
                DROP FUNCTION IF EXISTS fn_centers_get();
                DROP FUNCTION IF EXISTS fn_residential_customer_search(text, text);
                DROP FUNCTION IF EXISTS fn_residential_customer_get(uuid);
                DROP FUNCTION IF EXISTS fn_residential_customer_rows();
                DROP PROCEDURE IF EXISTS sp_residential_customer_retire(uuid, timestamp with time zone, timestamp with time zone);
                DROP PROCEDURE IF EXISTS sp_residential_customer_update(jsonb);
                DROP PROCEDURE IF EXISTS sp_residential_customer_create(jsonb);
                """);
        }
    }
}
