using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataGo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "centers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "modern_channel_customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modern_channel_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transport_zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "residential_customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Treatment = table.Column<int>(type: "integer", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExtendedLegalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FullName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FirstNames = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LastNames = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    PhoneExtension = table.Column<string>(type: "text", nullable: true),
                    MobilePhone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VerificationDigit = table.Column<string>(type: "text", nullable: true),
                    TaxClass = table.Column<int>(type: "integer", nullable: false),
                    PaymentCondition = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Stratum = table.Column<short>(type: "smallint", nullable: false),
                    CenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residential_customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_residential_customers_centers_CenterId",
                        column: x => x.CenterId,
                        principalTable: "centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_departments_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "municipalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_municipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_municipalities_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "neighborhoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MunicipalityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_neighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_neighborhoods_municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_neighborhoods_transport_zones_TransportZoneId",
                        column: x => x.TransportZoneId,
                        principalTable: "transport_zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    NeighborhoodId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRural = table.Column<bool>(type: "boolean", nullable: false),
                    RuralAddress = table.Column<string>(type: "text", nullable: true),
                    MainRoadType = table.Column<string>(type: "text", nullable: true),
                    MainRoadNumber = table.Column<string>(type: "text", nullable: true),
                    MainRoadLetter = table.Column<string>(type: "text", nullable: true),
                    MainRoadCardinality = table.Column<string>(type: "text", nullable: true),
                    SecondaryRoadNumber1 = table.Column<string>(type: "text", nullable: true),
                    SecondaryRoadLetter = table.Column<string>(type: "text", nullable: true),
                    SecondaryRoadCardinality1 = table.Column<string>(type: "text", nullable: true),
                    SecondaryRoadNumber2 = table.Column<string>(type: "text", nullable: true),
                    SecondaryRoadCardinality2 = table.Column<string>(type: "text", nullable: true),
                    FormattedAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_addresses_neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customer_addresses_residential_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "residential_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "centers",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000200"), "MDE-C001", true, "Centro Medellín Norte" },
                    { new Guid("00000000-0000-0000-0000-000000000201"), "MDE-C002", true, "Centro Medellín Sur" },
                    { new Guid("00000000-0000-0000-0000-000000000202"), "MDE-C003", true, "Centro Medellín Oriente" },
                    { new Guid("00000000-0000-0000-0000-000000000203"), "MDE-C004", true, "Centro Medellín Occidente" },
                    { new Guid("00000000-0000-0000-0000-000000000204"), "MDE-C005", true, "Centro Medellín Centro" },
                    { new Guid("00000000-0000-0000-0000-000000000205"), "MDE-C006", true, "Centro Laureles" },
                    { new Guid("00000000-0000-0000-0000-000000000206"), "MDE-C007", true, "Centro El Poblado" },
                    { new Guid("00000000-0000-0000-0000-000000000207"), "MDE-C008", true, "Centro Belén" },
                    { new Guid("00000000-0000-0000-0000-000000000208"), "MDE-C009", true, "Centro Guayabal" },
                    { new Guid("00000000-0000-0000-0000-000000000209"), "MDE-C010", true, "Centro La América" },
                    { new Guid("00000000-0000-0000-0000-000000000210"), "MDE-C011", true, "Centro Robledo" },
                    { new Guid("00000000-0000-0000-0000-000000000211"), "MDE-C012", true, "Centro Castilla" },
                    { new Guid("00000000-0000-0000-0000-000000000212"), "MDE-C013", true, "Centro Aranjuez" },
                    { new Guid("00000000-0000-0000-0000-000000000213"), "MDE-C014", true, "Centro Buenos Aires" },
                    { new Guid("00000000-0000-0000-0000-000000000214"), "MDE-C015", true, "Centro Manrique" },
                    { new Guid("00000000-0000-0000-0000-000000000215"), "MDE-C016", true, "Centro San Javier" },
                    { new Guid("00000000-0000-0000-0000-000000000216"), "MDE-C017", true, "Centro Doce de Octubre" },
                    { new Guid("00000000-0000-0000-0000-000000000217"), "MDE-C018", true, "Centro Villa Hermosa" },
                    { new Guid("00000000-0000-0000-0000-000000000218"), "MDE-C019", true, "Centro Santa Cruz" },
                    { new Guid("00000000-0000-0000-0000-000000000219"), "MDE-C020", true, "Centro Popular" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Colombia" });

            migrationBuilder.InsertData(
                table: "modern_channel_customers",
                columns: new[] { "Id", "DocumentNumber", "DocumentType" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000301"), "1010000001", 0 },
                    { new Guid("00000000-0000-0000-0000-000000000302"), "1010000002", 0 },
                    { new Guid("00000000-0000-0000-0000-000000000303"), "2010000001", 1 },
                    { new Guid("00000000-0000-0000-0000-000000000304"), "900123456", 2 },
                    { new Guid("00000000-0000-0000-0000-000000000305"), "901234567", 2 }
                });

            migrationBuilder.InsertData(
                table: "transport_zones",
                columns: new[] { "Id", "Code" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), "MDE-NORTE" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "MDE-NORORIENTE" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "MDE-CENTRO" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "MDE-CENTRO-OCCIDENTE" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "MDE-OCCIDENTE" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "MDE-SUROCCIDENTE" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "MDE-SUR" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "MDE-SURORIENTE" }
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), "Antioquia" });

            migrationBuilder.InsertData(
                table: "municipalities",
                columns: new[] { "Id", "DepartmentId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000003"), new Guid("00000000-0000-0000-0000-000000000002"), "Medellín" });

            migrationBuilder.InsertData(
                table: "neighborhoods",
                columns: new[] { "Id", "MunicipalityId", "Name", "TransportZoneId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000100"), new Guid("00000000-0000-0000-0000-000000000003"), "Laureles", new Guid("00000000-0000-0000-0000-000000000010") },
                    { new Guid("00000000-0000-0000-0000-000000000101"), new Guid("00000000-0000-0000-0000-000000000003"), "Conquistadores", new Guid("00000000-0000-0000-0000-000000000011") },
                    { new Guid("00000000-0000-0000-0000-000000000102"), new Guid("00000000-0000-0000-0000-000000000003"), "Florida Nueva", new Guid("00000000-0000-0000-0000-000000000012") },
                    { new Guid("00000000-0000-0000-0000-000000000103"), new Guid("00000000-0000-0000-0000-000000000003"), "Estadio", new Guid("00000000-0000-0000-0000-000000000013") },
                    { new Guid("00000000-0000-0000-0000-000000000104"), new Guid("00000000-0000-0000-0000-000000000003"), "Los Colores", new Guid("00000000-0000-0000-0000-000000000014") },
                    { new Guid("00000000-0000-0000-0000-000000000105"), new Guid("00000000-0000-0000-0000-000000000003"), "Carlos E. Restrepo", new Guid("00000000-0000-0000-0000-000000000015") },
                    { new Guid("00000000-0000-0000-0000-000000000106"), new Guid("00000000-0000-0000-0000-000000000003"), "El Poblado", new Guid("00000000-0000-0000-0000-000000000016") },
                    { new Guid("00000000-0000-0000-0000-000000000107"), new Guid("00000000-0000-0000-0000-000000000003"), "Provenza", new Guid("00000000-0000-0000-0000-000000000017") },
                    { new Guid("00000000-0000-0000-0000-000000000108"), new Guid("00000000-0000-0000-0000-000000000003"), "Manila", new Guid("00000000-0000-0000-0000-000000000010") },
                    { new Guid("00000000-0000-0000-0000-000000000109"), new Guid("00000000-0000-0000-0000-000000000003"), "Astorga", new Guid("00000000-0000-0000-0000-000000000011") },
                    { new Guid("00000000-0000-0000-0000-000000000110"), new Guid("00000000-0000-0000-0000-000000000003"), "Castropol", new Guid("00000000-0000-0000-0000-000000000012") },
                    { new Guid("00000000-0000-0000-0000-000000000111"), new Guid("00000000-0000-0000-0000-000000000003"), "Santa María de los Ángeles", new Guid("00000000-0000-0000-0000-000000000013") },
                    { new Guid("00000000-0000-0000-0000-000000000112"), new Guid("00000000-0000-0000-0000-000000000003"), "Belén", new Guid("00000000-0000-0000-0000-000000000014") },
                    { new Guid("00000000-0000-0000-0000-000000000113"), new Guid("00000000-0000-0000-0000-000000000003"), "Belén Rosales", new Guid("00000000-0000-0000-0000-000000000015") },
                    { new Guid("00000000-0000-0000-0000-000000000114"), new Guid("00000000-0000-0000-0000-000000000003"), "Belén La Palma", new Guid("00000000-0000-0000-0000-000000000016") },
                    { new Guid("00000000-0000-0000-0000-000000000115"), new Guid("00000000-0000-0000-0000-000000000003"), "Belén Fátima", new Guid("00000000-0000-0000-0000-000000000017") },
                    { new Guid("00000000-0000-0000-0000-000000000116"), new Guid("00000000-0000-0000-0000-000000000003"), "Belén Granada", new Guid("00000000-0000-0000-0000-000000000010") },
                    { new Guid("00000000-0000-0000-0000-000000000117"), new Guid("00000000-0000-0000-0000-000000000003"), "Los Alpes", new Guid("00000000-0000-0000-0000-000000000011") },
                    { new Guid("00000000-0000-0000-0000-000000000118"), new Guid("00000000-0000-0000-0000-000000000003"), "La América", new Guid("00000000-0000-0000-0000-000000000012") },
                    { new Guid("00000000-0000-0000-0000-000000000119"), new Guid("00000000-0000-0000-0000-000000000003"), "Santa Lucía", new Guid("00000000-0000-0000-0000-000000000013") },
                    { new Guid("00000000-0000-0000-0000-000000000120"), new Guid("00000000-0000-0000-0000-000000000003"), "Calasanz", new Guid("00000000-0000-0000-0000-000000000014") },
                    { new Guid("00000000-0000-0000-0000-000000000121"), new Guid("00000000-0000-0000-0000-000000000003"), "La Floresta", new Guid("00000000-0000-0000-0000-000000000015") },
                    { new Guid("00000000-0000-0000-0000-000000000122"), new Guid("00000000-0000-0000-0000-000000000003"), "Santa Mónica", new Guid("00000000-0000-0000-0000-000000000016") },
                    { new Guid("00000000-0000-0000-0000-000000000123"), new Guid("00000000-0000-0000-0000-000000000003"), "Robledo", new Guid("00000000-0000-0000-0000-000000000017") },
                    { new Guid("00000000-0000-0000-0000-000000000124"), new Guid("00000000-0000-0000-0000-000000000003"), "Pilarica", new Guid("00000000-0000-0000-0000-000000000010") },
                    { new Guid("00000000-0000-0000-0000-000000000125"), new Guid("00000000-0000-0000-0000-000000000003"), "Córdoba", new Guid("00000000-0000-0000-0000-000000000011") },
                    { new Guid("00000000-0000-0000-0000-000000000126"), new Guid("00000000-0000-0000-0000-000000000003"), "López de Mesa", new Guid("00000000-0000-0000-0000-000000000012") },
                    { new Guid("00000000-0000-0000-0000-000000000127"), new Guid("00000000-0000-0000-0000-000000000003"), "Boston", new Guid("00000000-0000-0000-0000-000000000013") },
                    { new Guid("00000000-0000-0000-0000-000000000128"), new Guid("00000000-0000-0000-0000-000000000003"), "Bomboná", new Guid("00000000-0000-0000-0000-000000000014") },
                    { new Guid("00000000-0000-0000-0000-000000000129"), new Guid("00000000-0000-0000-0000-000000000003"), "Prado", new Guid("00000000-0000-0000-0000-000000000015") },
                    { new Guid("00000000-0000-0000-0000-000000000130"), new Guid("00000000-0000-0000-0000-000000000003"), "Villa Nueva", new Guid("00000000-0000-0000-0000-000000000016") },
                    { new Guid("00000000-0000-0000-0000-000000000131"), new Guid("00000000-0000-0000-0000-000000000003"), "Buenos Aires", new Guid("00000000-0000-0000-0000-000000000017") },
                    { new Guid("00000000-0000-0000-0000-000000000132"), new Guid("00000000-0000-0000-0000-000000000003"), "Miraflores", new Guid("00000000-0000-0000-0000-000000000010") },
                    { new Guid("00000000-0000-0000-0000-000000000133"), new Guid("00000000-0000-0000-0000-000000000003"), "La Milagrosa", new Guid("00000000-0000-0000-0000-000000000011") },
                    { new Guid("00000000-0000-0000-0000-000000000134"), new Guid("00000000-0000-0000-0000-000000000003"), "Aranjuez", new Guid("00000000-0000-0000-0000-000000000012") },
                    { new Guid("00000000-0000-0000-0000-000000000135"), new Guid("00000000-0000-0000-0000-000000000003"), "Manrique Central", new Guid("00000000-0000-0000-0000-000000000013") },
                    { new Guid("00000000-0000-0000-0000-000000000136"), new Guid("00000000-0000-0000-0000-000000000003"), "Campo Valdés", new Guid("00000000-0000-0000-0000-000000000014") },
                    { new Guid("00000000-0000-0000-0000-000000000137"), new Guid("00000000-0000-0000-0000-000000000003"), "Moravia", new Guid("00000000-0000-0000-0000-000000000015") },
                    { new Guid("00000000-0000-0000-0000-000000000138"), new Guid("00000000-0000-0000-0000-000000000003"), "Castilla", new Guid("00000000-0000-0000-0000-000000000016") },
                    { new Guid("00000000-0000-0000-0000-000000000139"), new Guid("00000000-0000-0000-0000-000000000003"), "Pedregal", new Guid("00000000-0000-0000-0000-000000000017") },
                    { new Guid("00000000-0000-0000-0000-000000000140"), new Guid("00000000-0000-0000-0000-000000000003"), "Boyacá Las Brisas", new Guid("00000000-0000-0000-0000-000000000010") },
                    { new Guid("00000000-0000-0000-0000-000000000141"), new Guid("00000000-0000-0000-0000-000000000003"), "Guayabal", new Guid("00000000-0000-0000-0000-000000000011") },
                    { new Guid("00000000-0000-0000-0000-000000000142"), new Guid("00000000-0000-0000-0000-000000000003"), "Cristo Rey", new Guid("00000000-0000-0000-0000-000000000012") },
                    { new Guid("00000000-0000-0000-0000-000000000143"), new Guid("00000000-0000-0000-0000-000000000003"), "Trinidad", new Guid("00000000-0000-0000-0000-000000000013") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_centers_Code",
                table: "centers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_addresses_CustomerId",
                table: "customer_addresses",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_addresses_NeighborhoodId",
                table: "customer_addresses",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_departments_CountryId",
                table: "departments",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_modern_channel_customers_DocumentType_DocumentNumber",
                table: "modern_channel_customers",
                columns: new[] { "DocumentType", "DocumentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_municipalities_DepartmentId",
                table: "municipalities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_neighborhoods_MunicipalityId",
                table: "neighborhoods",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_neighborhoods_Name",
                table: "neighborhoods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_neighborhoods_TransportZoneId",
                table: "neighborhoods",
                column: "TransportZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_residential_customers_CenterId",
                table: "residential_customers",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_residential_customers_Code",
                table: "residential_customers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_residential_customers_DocumentType_DocumentNumber",
                table: "residential_customers",
                columns: new[] { "DocumentType", "DocumentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_zones_Code",
                table: "transport_zones",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_addresses");

            migrationBuilder.DropTable(
                name: "modern_channel_customers");

            migrationBuilder.DropTable(
                name: "neighborhoods");

            migrationBuilder.DropTable(
                name: "residential_customers");

            migrationBuilder.DropTable(
                name: "municipalities");

            migrationBuilder.DropTable(
                name: "transport_zones");

            migrationBuilder.DropTable(
                name: "centers");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
