using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataGo.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DataGoDbContext))]
[Migration("20260907160000_ExpandNeighborhoodCatalogRoutine")]
public sealed class ExpandNeighborhoodCatalogRoutine : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP FUNCTION IF EXISTS fn_neighborhoods_search(text);

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
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP FUNCTION IF EXISTS fn_neighborhoods_search(text);

            CREATE FUNCTION fn_neighborhoods_search(p_query text DEFAULT NULL)
            RETURNS TABLE (id uuid, name text)
            LANGUAGE sql
            STABLE
            AS $$
                SELECT "Id", "Name"::text
                FROM neighborhoods
                WHERE p_query IS NULL OR btrim(p_query) = ''
                    OR "Name" ILIKE '%' || btrim(p_query) || '%'
                ORDER BY "Name"
                LIMIT 100;
            $$;
            """);
    }
}
