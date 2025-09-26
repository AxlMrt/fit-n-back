using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Workouts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutsModule_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workouts");

            migrationBuilder.CreateTable(
                name: "workouts",
                schema: "workouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    estimated_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ImageContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByCoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_phases",
                schema: "workouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estimated_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_phases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_phases_workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalSchema: "workouts",
                        principalTable: "workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_exercises",
                schema: "workouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: true),
                    Reps = table.Column<int>(type: "integer", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    distance_meters = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    weight_kg = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    rest_seconds = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkoutPhaseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_exercises_workout_phases_WorkoutPhaseId",
                        column: x => x.WorkoutPhaseId,
                        principalSchema: "workouts",
                        principalTable: "workout_phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workout_exercises_exercise_id",
                schema: "workouts",
                table: "workout_exercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "ix_workout_exercises_order",
                schema: "workouts",
                table: "workout_exercises",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "ix_workout_exercises_phase_exercise",
                schema: "workouts",
                table: "workout_exercises",
                columns: new[] { "WorkoutPhaseId", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workout_exercises_phase_id",
                schema: "workouts",
                table: "workout_exercises",
                column: "WorkoutPhaseId");

            migrationBuilder.CreateIndex(
                name: "ix_workout_exercises_phase_order",
                schema: "workouts",
                table: "workout_exercises",
                columns: new[] { "WorkoutPhaseId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workout_phases_order",
                schema: "workouts",
                table: "workout_phases",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "ix_workout_phases_type",
                schema: "workouts",
                table: "workout_phases",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "ix_workout_phases_workout_id",
                schema: "workouts",
                table: "workout_phases",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "ix_workout_phases_workout_order",
                schema: "workouts",
                table: "workout_phases",
                columns: new[] { "WorkoutId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workouts_created_at",
                schema: "workouts",
                table: "workouts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_created_by_coach",
                schema: "workouts",
                table: "workouts",
                column: "CreatedByCoachId");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_created_by_user",
                schema: "workouts",
                table: "workouts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_difficulty",
                schema: "workouts",
                table: "workouts",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_is_active",
                schema: "workouts",
                table: "workouts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_name",
                schema: "workouts",
                table: "workouts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "ix_workouts_type",
                schema: "workouts",
                table: "workouts",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workout_exercises",
                schema: "workouts");

            migrationBuilder.DropTable(
                name: "workout_phases",
                schema: "workouts");

            migrationBuilder.DropTable(
                name: "workouts",
                schema: "workouts");
        }
    }
}
