using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Exercises.Migrations
{
    /// <inheritdoc />
    public partial class InitialExercisesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exercises");

            migrationBuilder.CreateTable(
                name: "Exercises",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    MuscleGroups = table.Column<int>(type: "integer", nullable: false),
                    ImageContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    VideoContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Equipment = table.Column<int>(type: "integer", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedAt",
                schema: "exercises",
                table: "Exercises",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Difficulty",
                schema: "exercises",
                table: "Exercises",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_IsActive",
                schema: "exercises",
                table: "Exercises",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                schema: "exercises",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Type",
                schema: "exercises",
                table: "Exercises",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises",
                schema: "exercises");
        }
    }
}
