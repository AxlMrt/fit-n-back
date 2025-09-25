using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessApp.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserProfileForPhysicalMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Preferences_UserProfiles_UserId",
                schema: "users",
                table: "Preferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Preferences_UserProfiles_UserId",
                schema: "users",
                table: "Preferences",
                column: "UserId",
                principalSchema: "users",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
