using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StatusChange_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tasks",
                newName: "Status_ChangedAt");

            migrationBuilder.AddColumn<int>(
                name: "Status_Type",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status_Type",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Status_ChangedAt",
                table: "Tasks",
                newName: "Status");
        }
    }
}
