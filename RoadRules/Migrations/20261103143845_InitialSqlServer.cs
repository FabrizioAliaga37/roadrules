using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoadRules.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jurisdictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurisdictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AxleLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JurisdictionId = table.Column<int>(type: "int", nullable: false),
                    AxleGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AxleCount = table.Column<int>(type: "int", nullable: false),
                    MaxKg = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AxleLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AxleLimits_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Jurisdictions",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "ON", "Ontario" },
                    { 2, "QC", "Quebec" },
                    { 3, "AB", "Alberta" },
                    { 4, "BC", "British Columbia" },
                    { 5, "MB", "Manitoba" }
                });

            migrationBuilder.InsertData(
                table: "AxleLimits",
                columns: new[] { "Id", "AxleCount", "AxleGroup", "EffectiveFrom", "JurisdictionId", "MaxKg" },
                values: new object[,]
                {
                    { 1, 1, "Single", new DateOnly(2026, 1, 1), 1, 9000 },
                    { 2, 2, "Tandem", new DateOnly(2026, 1, 1), 1, 17000 },
                    { 3, 3, "Tridem", new DateOnly(2026, 1, 1), 1, 24000 },
                    { 4, 1, "Single", new DateOnly(2026, 1, 1), 2, 9000 },
                    { 5, 2, "Tandem", new DateOnly(2026, 1, 1), 2, 18000 },
                    { 6, 3, "Tridem", new DateOnly(2026, 1, 1), 2, 26000 },
                    { 7, 1, "Single", new DateOnly(2026, 1, 1), 3, 9100 },
                    { 8, 2, "Tandem", new DateOnly(2026, 1, 1), 3, 17000 },
                    { 9, 3, "Tridem", new DateOnly(2026, 1, 1), 3, 24000 },
                    { 10, 1, "Single", new DateOnly(2026, 1, 1), 4, 9100 },
                    { 11, 2, "Tandem", new DateOnly(2026, 1, 1), 4, 17000 },
                    { 12, 3, "Tridem", new DateOnly(2026, 1, 1), 4, 24000 },
                    { 13, 1, "Single", new DateOnly(2026, 1, 1), 5, 9100 },
                    { 14, 2, "Tandem", new DateOnly(2026, 1, 1), 5, 17000 },
                    { 15, 3, "Tridem", new DateOnly(2026, 1, 1), 5, 23000 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AxleLimits_JurisdictionId",
                table: "AxleLimits",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Jurisdictions_Code",
                table: "Jurisdictions",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AxleLimits");

            migrationBuilder.DropTable(
                name: "Jurisdictions");
        }
    }
}
