using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedRealtimeIndicators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BollingerLower",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BollingerUpper",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceScore",
                table: "AppMarketDataPoints",
                type: "numeric(10,4)",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Macd",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MacdHistogram",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MacdSignal",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Momentum",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RateOfChange",
                table: "AppMarketDataPoints",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TrendScore",
                table: "AppMarketDataPoints",
                type: "numeric(10,4)",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Verdict",
                table: "AppMarketDataPoints",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Hold");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BollingerLower",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "BollingerUpper",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "Macd",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "MacdHistogram",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "MacdSignal",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "Momentum",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "RateOfChange",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "TrendScore",
                table: "AppMarketDataPoints");

            migrationBuilder.DropColumn(
                name: "Verdict",
                table: "AppMarketDataPoints");
        }
    }
}
