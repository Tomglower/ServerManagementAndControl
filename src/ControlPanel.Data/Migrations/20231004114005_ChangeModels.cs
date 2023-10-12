using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlPanel.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "link",
                table: "Machines",
                newName: "Link");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Machines",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "Machines",
                newName: "link");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Machines",
                newName: "id");
        }
    }
}
