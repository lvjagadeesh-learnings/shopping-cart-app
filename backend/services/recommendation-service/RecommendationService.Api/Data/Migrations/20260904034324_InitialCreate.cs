using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecommendationService.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "recommendation");

            migrationBuilder.CreateTable(
                name: "ProductPurchases",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PurchasedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPurchases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductViews",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductViews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPurchases_ProductId",
                schema: "recommendation",
                table: "ProductPurchases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPurchases_UserId",
                schema: "recommendation",
                table: "ProductPurchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductViews_ProductId",
                schema: "recommendation",
                table: "ProductViews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductViews_UserId",
                schema: "recommendation",
                table: "ProductViews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPurchases",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "ProductViews",
                schema: "recommendation");
        }
    }
}
