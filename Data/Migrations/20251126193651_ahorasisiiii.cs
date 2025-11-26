using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class ahorasisiiii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ViajePasajero_puntoSubida_PuntoSubidaId",
                table: "ViajePasajero");

            migrationBuilder.AlterColumn<int>(
                name: "PuntoSubidaId",
                table: "ViajePasajero",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "PuntoSubidaId",
                table: "ViajePasajero",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ViajePasajero_puntoSubida_PuntoSubidaId",
                table: "ViajePasajero",
                column: "PuntoSubidaId",
                principalTable: "puntoSubida",
                principalColumn: "Id");
        }
    }
}
