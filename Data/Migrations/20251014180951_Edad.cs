using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class Edad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Edad",
                table: "Pasajero");

            migrationBuilder.DropColumn(
                name: "Edad",
                table: "Coordinador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Edad",
                table: "Pasajero",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Edad",
                table: "Coordinador",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
