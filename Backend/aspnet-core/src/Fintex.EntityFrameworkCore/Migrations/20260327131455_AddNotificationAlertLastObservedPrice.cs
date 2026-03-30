using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationAlertLastObservedPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LastObservedPrice",
                table: "AppNotificationAlertRules",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastObservedPrice",
                table: "AppNotificationAlertRules");
        }
    }
}
