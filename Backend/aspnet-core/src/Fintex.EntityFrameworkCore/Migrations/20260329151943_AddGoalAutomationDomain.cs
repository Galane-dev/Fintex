using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalAutomationDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppGoalEvaluationRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GoalTargetId = table.Column<long>(type: "bigint", nullable: false),
                    GoalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentEquity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    RequiredGrowthPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    RequiredDailyGrowthPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    FeasibilityScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CounterProposalTargetEquity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    CounterProposalTargetPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AppGoalEvaluationRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppGoalExecutionEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GoalTargetId = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    TradeId = table.Column<long>(type: "bigint", nullable: true),
                    EquityAfterExecution = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AppGoalExecutionEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppGoalExecutionPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GoalTargetId = table.Column<long>(type: "bigint", nullable: false),
                    ExecutionSymbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SuggestedDirection = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SuggestedQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SuggestedStopLoss = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SuggestedTakeProfit = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    RiskScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    NextAction = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AppGoalExecutionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppGoalTargets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AccountType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExternalConnectionId = table.Column<long>(type: "bigint", nullable: true),
                    MarketSymbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowedSymbols = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartEquity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrentEquity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    TargetEquity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    TargetPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    DeadlineUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxAcceptableRisk = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    MaxDrawdownPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    MaxPositionSizePercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TradingSession = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowOvernightPositions = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LatestPlanSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LatestNextAction = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProgressPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    RequiredDailyGrowthPercent = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    LastEvaluatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastExecutedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastExecutionAttemptAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedTradesCount = table.Column<int>(type: "integer", nullable: false),
                    LastTradeId = table.Column<long>(type: "bigint", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_AppGoalTargets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalEvaluationRuns_GoalTargetId_OccurredAtUtc",
                table: "AppGoalEvaluationRuns",
                columns: new[] { "GoalTargetId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalEvaluationRuns_UserId_GoalStatus_OccurredAtUtc",
                table: "AppGoalEvaluationRuns",
                columns: new[] { "UserId", "GoalStatus", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalExecutionEvents_GoalTargetId_OccurredAtUtc",
                table: "AppGoalExecutionEvents",
                columns: new[] { "GoalTargetId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalExecutionEvents_UserId_EventType_OccurredAtUtc",
                table: "AppGoalExecutionEvents",
                columns: new[] { "UserId", "EventType", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalExecutionPlans_GoalTargetId_GeneratedAtUtc",
                table: "AppGoalExecutionPlans",
                columns: new[] { "GoalTargetId", "GeneratedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalExecutionPlans_UserId_ExecutionSymbol_GeneratedAtUtc",
                table: "AppGoalExecutionPlans",
                columns: new[] { "UserId", "ExecutionSymbol", "GeneratedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalTargets_TenantId_UserId_Status",
                table: "AppGoalTargets",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGoalTargets_UserId_MarketSymbol_AccountType_DeadlineUtc",
                table: "AppGoalTargets",
                columns: new[] { "UserId", "MarketSymbol", "AccountType", "DeadlineUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppGoalEvaluationRuns");

            migrationBuilder.DropTable(
                name: "AppGoalExecutionEvents");

            migrationBuilder.DropTable(
                name: "AppGoalExecutionPlans");

            migrationBuilder.DropTable(
                name: "AppGoalTargets");
        }
    }
}
