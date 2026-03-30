using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsIngestionDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNewsAnalysisSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    FocusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArticleCount = table.Column<int>(type: "integer", nullable: false),
                    LatestArticlePublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sentiment = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ImpactScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    KeyHeadlines = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RawPayloadJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNewsAnalysisSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNewsArticles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    SourceId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Summary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Author = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Tags = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsBitcoinRelevant = table.Column<bool>(type: "boolean", nullable: false),
                    IsUsdRelevant = table.Column<bool>(type: "boolean", nullable: false),
                    RelevanceScore = table.Column<int>(type: "integer", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
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
                    table.PrimaryKey("PK_AppNewsArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNewsRefreshRuns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    FocusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Trigger = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SourceCount = table.Column<int>(type: "integer", nullable: false),
                    ArticlesFetched = table.Column<int>(type: "integer", nullable: false),
                    NewArticles = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNewsRefreshRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNewsSources",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SiteUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FeedUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FocusTags = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastAttemptedRefreshTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulRefreshTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
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
                    table.PrimaryKey("PK_AppNewsSources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsAnalysisSnapshots_FocusKey_GeneratedAt",
                table: "AppNewsAnalysisSnapshots",
                columns: new[] { "FocusKey", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsAnalysisSnapshots_FocusKey_LatestArticlePublishedAt",
                table: "AppNewsAnalysisSnapshots",
                columns: new[] { "FocusKey", "LatestArticlePublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsArticles_IsBitcoinRelevant_IsUsdRelevant_PublishedAt",
                table: "AppNewsArticles",
                columns: new[] { "IsBitcoinRelevant", "IsUsdRelevant", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsArticles_RelevanceScore_PublishedAt",
                table: "AppNewsArticles",
                columns: new[] { "RelevanceScore", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsArticles_SourceId_Url",
                table: "AppNewsArticles",
                columns: new[] { "SourceId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsRefreshRuns_FocusKey_Status_CreationTime",
                table: "AppNewsRefreshRuns",
                columns: new[] { "FocusKey", "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsSources_IsActive_Category",
                table: "AppNewsSources",
                columns: new[] { "IsActive", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNewsSources_TenantId_Name",
                table: "AppNewsSources",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNewsAnalysisSnapshots");

            migrationBuilder.DropTable(
                name: "AppNewsArticles");

            migrationBuilder.DropTable(
                name: "AppNewsRefreshRuns");

            migrationBuilder.DropTable(
                name: "AppNewsSources");
        }
    }
}
