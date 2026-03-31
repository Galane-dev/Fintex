using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalBrokerConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppExternalBrokerConnections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AccountLogin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Server = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "text", nullable: false),
                    TerminalPath = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BrokerAccountName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerAccountCurrency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    BrokerCompany = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BrokerLeverage = table.Column<int>(type: "integer", nullable: true),
                    LastKnownBalance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    LastKnownEquity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
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
                    table.PrimaryKey("PK_AppExternalBrokerConnections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalBrokerConnections_TenantId_UserId_IsActive",
                table: "AppExternalBrokerConnections",
                columns: new[] { "TenantId", "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalBrokerConnections_UserId_Provider_AccountLogin_S~",
                table: "AppExternalBrokerConnections",
                columns: new[] { "UserId", "Provider", "AccountLogin", "Server" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExternalBrokerConnections");
        }
    }
}
