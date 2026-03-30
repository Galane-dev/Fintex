using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademyOnboardingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcademyGraduatedAt",
                table: "AppUserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcademyStage",
                table: "AppUserProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BestIntroQuizScore",
                table: "AppUserProfiles",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "IntroQuizAttemptsCount",
                table: "AppUserProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "IntroQuizPassedAt",
                table: "AppUserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppAcademyQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CourseKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    ScorePercent = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    RequiredScorePercent = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    AnswersJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_AppAcademyQuizAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAcademyQuizAttempts_TenantId_UserId_CreationTime",
                table: "AppAcademyQuizAttempts",
                columns: new[] { "TenantId", "UserId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAcademyQuizAttempts");

            migrationBuilder.DropColumn(
                name: "AcademyGraduatedAt",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "AcademyStage",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "BestIntroQuizScore",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "IntroQuizAttemptsCount",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "IntroQuizPassedAt",
                table: "AppUserProfiles");
        }
    }
}
