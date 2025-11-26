using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class ahorasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PuntoSubida",
                table: "ViajePasajero");

            migrationBuilder.CreateIndex(
                name: "IX_ViajePasajero_PuntoSubidaId",
                table: "ViajePasajero",
                column: "PuntoSubidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ViajePasajero_puntoSubida_PuntoSubidaId",
                table: "ViajePasajero",
                column: "PuntoSubidaId",
                principalTable: "puntoSubida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ViajePasajero_puntoSubida_PuntoSubidaId",
                table: "ViajePasajero");

            migrationBuilder.DropIndex(
                name: "IX_ViajePasajero_PuntoSubidaId",
                table: "ViajePasajero");

            migrationBuilder.AddColumn<int>(
                name: "PuntoSubida",
                table: "ViajePasajero",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
