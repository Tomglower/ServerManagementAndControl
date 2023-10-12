using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlPanel.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Machines",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Machines",
                newName: "Id");
        }
    }
}
