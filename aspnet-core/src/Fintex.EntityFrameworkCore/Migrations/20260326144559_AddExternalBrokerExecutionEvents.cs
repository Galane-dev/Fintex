using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalBrokerExecutionEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppExternalBrokerExecutionEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalBrokerConnectionId = table.Column<long>(type: "bigint", nullable: false),
                    BrokerProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BrokerPlatform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BrokerEnvironment = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExecutionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerOrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerClientOrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerSymbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    NormalizedSymbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BrokerOrderStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Direction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    AssetClass = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    FilledQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    EventQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    FilledAveragePrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    PositionQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExternalBrokerExecutionEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalBrokerExecutionEvents_ExternalBrokerConnectionI~1",
                table: "AppExternalBrokerExecutionEvents",
                columns: new[] { "ExternalBrokerConnectionId", "BrokerOrderId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalBrokerExecutionEvents_ExternalBrokerConnectionId~",
                table: "AppExternalBrokerExecutionEvents",
                columns: new[] { "ExternalBrokerConnectionId", "ExecutionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalBrokerExecutionEvents_UserId_ExternalBrokerConne~",
                table: "AppExternalBrokerExecutionEvents",
                columns: new[] { "UserId", "ExternalBrokerConnectionId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExternalBrokerExecutionEvents");
        }
    }
}
