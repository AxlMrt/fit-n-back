using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeightUnit",
                schema: "users",
                table: "UserProfiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeightUnit",
                schema: "users",
                table: "UserProfiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightUnit",
                schema: "users",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                schema: "users",
                table: "UserProfiles");
        }
    }
}
