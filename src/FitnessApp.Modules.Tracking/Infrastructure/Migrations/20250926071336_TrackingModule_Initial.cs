using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Tracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrackingModule_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tracking");

            migrationBuilder.CreateTable(
                name: "planned_workouts",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workout_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_date = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    is_from_program = table.Column<bool>(type: "boolean", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: true),
                    workout_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_workouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_metrics",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_sessions",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    planned_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    total_duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    calories_estimated = table.Column<int>(type: "integer", nullable: true),
                    perceived_difficulty = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_from_program = table.Column<bool>(type: "boolean", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_session_exercises",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    workout_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    metric_type = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    performance_score = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_session_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_session_exercises_workout_sessions_workout_session_~",
                        column: x => x.workout_session_id,
                        principalSchema: "tracking",
                        principalTable: "workout_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessionSet",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutSessionExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    Repetitions = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<double>(type: "double precision", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RestTimeSeconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionSet_workout_session_exercises_WorkoutSessionE~",
                        column: x => x.WorkoutSessionExerciseId,
                        principalSchema: "tracking",
                        principalTable: "workout_session_exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_program_id",
                schema: "tracking",
                table: "planned_workouts",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_scheduled_date",
                schema: "tracking",
                table: "planned_workouts",
                column: "scheduled_date");

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_Status",
                schema: "tracking",
                table: "planned_workouts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_user_id",
                schema: "tracking",
                table: "planned_workouts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_user_id_scheduled_date",
                schema: "tracking",
                table: "planned_workouts",
                columns: new[] { "user_id", "scheduled_date" });

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_user_id_Status",
                schema: "tracking",
                table: "planned_workouts",
                columns: new[] { "user_id", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_workout_id",
                schema: "tracking",
                table: "planned_workouts",
                column: "workout_id");

            migrationBuilder.CreateIndex(
                name: "IX_planned_workouts_workout_session_id",
                schema: "tracking",
                table: "planned_workouts",
                column: "workout_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_metrics_metric_type",
                schema: "tracking",
                table: "user_metrics",
                column: "metric_type");

            migrationBuilder.CreateIndex(
                name: "IX_user_metrics_recorded_at",
                schema: "tracking",
                table: "user_metrics",
                column: "recorded_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_metrics_user_id",
                schema: "tracking",
                table: "user_metrics",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_metrics_user_id_metric_type",
                schema: "tracking",
                table: "user_metrics",
                columns: new[] { "user_id", "metric_type" });

            migrationBuilder.CreateIndex(
                name: "IX_user_metrics_user_id_metric_type_recorded_at",
                schema: "tracking",
                table: "user_metrics",
                columns: new[] { "user_id", "metric_type", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_session_exercises_exercise_id",
                schema: "tracking",
                table: "workout_session_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_session_exercises_performed_at",
                schema: "tracking",
                table: "workout_session_exercises",
                column: "performed_at");

            migrationBuilder.CreateIndex(
                name: "IX_workout_session_exercises_workout_session_id",
                schema: "tracking",
                table: "workout_session_exercises",
                column: "workout_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_session_exercises_workout_session_id_Order",
                schema: "tracking",
                table: "workout_session_exercises",
                columns: new[] { "workout_session_id", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_planned_date",
                schema: "tracking",
                table: "workout_sessions",
                column: "planned_date");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_Status",
                schema: "tracking",
                table: "workout_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_UserId",
                schema: "tracking",
                table: "workout_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_UserId_start_time",
                schema: "tracking",
                table: "workout_sessions",
                columns: new[] { "UserId", "start_time" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_UserId_Status",
                schema: "tracking",
                table: "workout_sessions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_WorkoutId",
                schema: "tracking",
                table: "workout_sessions",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionSet_WorkoutSessionExerciseId",
                schema: "tracking",
                table: "WorkoutSessionSet",
                column: "WorkoutSessionExerciseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planned_workouts",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "user_metrics",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "WorkoutSessionSet",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "workout_session_exercises",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "workout_sessions",
                schema: "tracking");
        }
    }
}
