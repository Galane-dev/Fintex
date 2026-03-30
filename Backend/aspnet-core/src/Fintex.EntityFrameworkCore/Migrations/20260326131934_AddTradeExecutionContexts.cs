using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeExecutionContexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppTradeExecutionContexts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    TradeId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalBrokerConnectionId = table.Column<long>(type: "bigint", nullable: false),
                    BrokerProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BrokerPlatform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BrokerEnvironment = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    BrokerSymbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Direction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AssetClass = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MarketDataProvider = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    ReferencePrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    Bid = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Ask = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Spread = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SpreadPercent = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    StopLoss = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    MarketVerdict = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TrendScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    TimeframeAlignmentScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    StructureScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    StructureLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Sma = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Ema = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Rsi = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Macd = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    MacdSignal = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    MacdHistogram = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Momentum = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    RateOfChange = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Atr = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    AtrPercent = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Adx = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    UserRiskTolerance = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    UserBehavioralRiskScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    UserBehavioralSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BrokerOrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerClientOrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerOrderStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BrokerSubmittedQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    BrokerFilledQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    BrokerFilledAveragePrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    BrokerSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BrokerFilledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DecisionSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequestPayloadJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BrokerResponseJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTradeExecutionContexts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppTradeExecutionContexts_TradeId_UserId",
                table: "AppTradeExecutionContexts",
                columns: new[] { "TradeId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppTradeExecutionContexts_UserId_ExternalBrokerConnectionId~",
                table: "AppTradeExecutionContexts",
                columns: new[] { "UserId", "ExternalBrokerConnectionId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppTradeExecutionContexts");
        }
    }
}
