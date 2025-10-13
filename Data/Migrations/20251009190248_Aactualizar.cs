using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class Aactualizar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PuntoSubida",
                table: "Pasajero");

            migrationBuilder.DropColumn(
                name: "RegOpc",
                table: "Pasajero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PuntoSubida",
                table: "Pasajero",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegOpc",
                table: "Pasajero",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
