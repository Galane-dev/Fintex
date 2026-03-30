using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationAlertCreatedPriceCrossing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppNotificationAlertRules_UserId_Symbol_Provider_Direction_~",
                table: "AppNotificationAlertRules");

            migrationBuilder.AddColumn<decimal>(
                name: "CreatedPrice",
                table: "AppNotificationAlertRules",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppNotificationAlertRules_UserId_Symbol_Provider_TargetPric~",
                table: "AppNotificationAlertRules",
                columns: new[] { "UserId", "Symbol", "Provider", "TargetPrice", "CreatedPrice" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppNotificationAlertRules_UserId_Symbol_Provider_TargetPric~",
                table: "AppNotificationAlertRules");

            migrationBuilder.DropColumn(
                name: "CreatedPrice",
                table: "AppNotificationAlertRules");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotificationAlertRules_UserId_Symbol_Provider_Direction_~",
                table: "AppNotificationAlertRules",
                columns: new[] { "UserId", "Symbol", "Provider", "Direction", "TargetPrice" });
        }
    }
}
