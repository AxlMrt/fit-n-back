using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Content.Migrations
{
    public partial class AddExerciseAssetJoin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercise_media_assets",
                schema: "content",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_media_assets", x => new { x.ExerciseId, x.MediaAssetId });
                    table.ForeignKey(
                        name: "FK_exercise_media_assets_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "exercises",
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_media_assets_media_assets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalSchema: "content",
                        principalTable: "media_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_media_assets_MediaAssetId",
                schema: "content",
                table: "exercise_media_assets",
                column: "MediaAssetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_media_assets",
                schema: "content");
        }
    }
}
