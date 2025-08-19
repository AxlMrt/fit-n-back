using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Exercises.Migrations
{
    /// <inheritdoc />
    public partial class ExercisesMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exercises");

            migrationBuilder.CreateTable(
                name: "equipment",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exercise_categories",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "muscle_groups",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BodyPart = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscle_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exercises",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    CommonMistakes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DifficultyLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstimatedCaloriesBurn = table.Column<int>(type: "integer", nullable: true),
                    IsBodyweightExercise = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MediaAssetIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercises_exercise_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "exercises",
                        principalTable: "exercise_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "exercise_equipment",
                schema: "exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_equipment", x => new { x.ExerciseId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_exercise_equipment_equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "exercises",
                        principalTable: "equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_equipment_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "exercises",
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_muscle_groups",
                schema: "exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_muscle_groups", x => new { x.ExerciseId, x.MuscleGroupId });
                    table.ForeignKey(
                        name: "FK_exercise_muscle_groups_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "exercises",
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_muscle_groups_muscle_groups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalSchema: "exercises",
                        principalTable: "muscle_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_tags",
                schema: "exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_tags", x => new { x.ExerciseId, x.TagId });
                    table.ForeignKey(
                        name: "FK_exercise_tags_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "exercises",
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_tags_tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "exercises",
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_variations",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ModificationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BaseExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariationExerciseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_variations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercise_variations_exercises_BaseExerciseId",
                        column: x => x.BaseExerciseId,
                        principalSchema: "exercises",
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exercise_variations_exercises_VariationExerciseId",
                        column: x => x.VariationExerciseId,
                        principalSchema: "exercises",
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_Name",
                schema: "exercises",
                table: "equipment",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exercise_categories_Name",
                schema: "exercises",
                table: "exercise_categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exercise_equipment_EquipmentId",
                schema: "exercises",
                table: "exercise_equipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_muscle_groups_MuscleGroupId",
                schema: "exercises",
                table: "exercise_muscle_groups",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_tags_TagId",
                schema: "exercises",
                table: "exercise_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_variations_BaseExerciseId",
                schema: "exercises",
                table: "exercise_variations",
                column: "BaseExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_variations_VariationExerciseId",
                schema: "exercises",
                table: "exercise_variations",
                column: "VariationExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_CategoryId",
                schema: "exercises",
                table: "exercises",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_muscle_groups_Name",
                schema: "exercises",
                table: "muscle_groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_Name",
                schema: "exercises",
                table: "tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_equipment",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "exercise_muscle_groups",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "exercise_tags",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "exercise_variations",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "equipment",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "muscle_groups",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "exercises",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "exercise_categories",
                schema: "exercises");
        }
    }
}
