using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintex.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademyLessonProgressPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletedIntroLessonKeys",
                table: "AppUserProfiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentIntroLessonKey",
                table: "AppUserProfiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedIntroLessonKeys",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentIntroLessonKey",
                table: "AppUserProfiles");
        }
    }
}
