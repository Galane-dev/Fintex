using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyValidationRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStrategyValidationRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    StrategyName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Provider = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Timeframe = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    DirectionPreference = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    StrategyText = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    MarketPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    MarketTrendScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    MarketConfidenceScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    MarketVerdict = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    NewsSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ValidationScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StrengthsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    RisksJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ImprovementsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    SuggestedAction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SuggestedEntryPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SuggestedStopLoss = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SuggestedTakeProfit = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    AiProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AiModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStrategyValidationRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStrategyValidationRuns_TenantId_UserId_CreationTime",
                table: "AppStrategyValidationRuns",
                columns: new[] { "TenantId", "UserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStrategyValidationRuns_UserId_Symbol_Provider_Outcome",
                table: "AppStrategyValidationRuns",
                columns: new[] { "UserId", "Symbol", "Provider", "Outcome" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStrategyValidationRuns");
        }
    }
}
