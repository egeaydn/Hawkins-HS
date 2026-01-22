using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hawkins_HS.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoBase64",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhotoBase64",
                table: "Students");
        }
    }
}
