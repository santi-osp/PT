using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataGo.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DataGoDbContext))]
[Migration("20260907170000_AddAdvancedCustomerSearch")]
public sealed class AddAdvancedCustomerSearch : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE FUNCTION fn_residential_customer_search_advanced(
            p_search text DEFAULT NULL, p_status text DEFAULT 'all',
            p_neighborhood uuid DEFAULT NULL, p_center uuid DEFAULT NULL,
            p_stratum smallint DEFAULT NULL, p_treatment integer DEFAULT NULL,
            p_document_type integer DEFAULT NULL)
        RETURNS TABLE (result jsonb)
        LANGUAGE sql STABLE AS $$
            SELECT r.result
            FROM fn_residential_customer_rows() r
            JOIN residential_customers c ON c."Id" = r.customer_id
            JOIN customer_addresses a ON a."CustomerId" = c."Id"
            WHERE (p_status = 'all' OR (p_status = 'active' AND NOT c."IsBlocked")
                OR (p_status = 'blocked' AND c."IsBlocked"))
              AND (p_search IS NULL OR btrim(p_search) = ''
                OR r.code ILIKE '%' || btrim(p_search) || '%'
                OR r.document_number ILIKE '%' || btrim(p_search) || '%'
                OR r.business_name ILIKE '%' || btrim(p_search) || '%'
                OR r.full_name ILIKE '%' || btrim(p_search) || '%'
                OR r.extended_legal_name ILIKE '%' || btrim(p_search) || '%')
              AND (p_neighborhood IS NULL OR a."NeighborhoodId" = p_neighborhood)
              AND (p_center IS NULL OR c."CenterId" = p_center)
              AND (p_stratum IS NULL OR c."Stratum" = p_stratum)
              AND (p_treatment IS NULL OR c."Treatment" = p_treatment)
              AND (p_document_type IS NULL OR c."DocumentType" = p_document_type)
            ORDER BY c."CreatedAt" DESC, c."Id"
            LIMIT 101;
        $$;

        CREATE FUNCTION fn_residential_customer_count_advanced(
            p_search text DEFAULT NULL, p_status text DEFAULT 'all',
            p_neighborhood uuid DEFAULT NULL, p_center uuid DEFAULT NULL,
            p_stratum smallint DEFAULT NULL, p_treatment integer DEFAULT NULL,
            p_document_type integer DEFAULT NULL)
        RETURNS integer LANGUAGE sql STABLE AS $$
            SELECT count(*)::integer
            FROM residential_customers c
            JOIN customer_addresses a ON a."CustomerId" = c."Id"
            WHERE (p_status = 'all' OR (p_status = 'active' AND NOT c."IsBlocked")
                OR (p_status = 'blocked' AND c."IsBlocked"))
              AND (p_search IS NULL OR btrim(p_search) = ''
                OR c."Code" ILIKE '%' || btrim(p_search) || '%'
                OR c."DocumentNumber" ILIKE '%' || btrim(p_search) || '%'
                OR c."BusinessName" ILIKE '%' || btrim(p_search) || '%'
                OR c."FullName" ILIKE '%' || btrim(p_search) || '%'
                OR c."ExtendedLegalName" ILIKE '%' || btrim(p_search) || '%')
              AND (p_neighborhood IS NULL OR a."NeighborhoodId" = p_neighborhood)
              AND (p_center IS NULL OR c."CenterId" = p_center)
              AND (p_stratum IS NULL OR c."Stratum" = p_stratum)
              AND (p_treatment IS NULL OR c."Treatment" = p_treatment)
              AND (p_document_type IS NULL OR c."DocumentType" = p_document_type);
        $$;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP FUNCTION IF EXISTS fn_residential_customer_count_advanced(text,text,uuid,uuid,smallint,integer,integer);
        DROP FUNCTION IF EXISTS fn_residential_customer_search_advanced(text,text,uuid,uuid,smallint,integer,integer);
        """);
}
