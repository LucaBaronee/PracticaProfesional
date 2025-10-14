using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class Agencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Agencia",
                table: "Pasajero");

            migrationBuilder.AddColumn<int>(
                name: "AgenciaId",
                table: "ViajePasajero",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Agencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencia", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViajePasajero_AgenciaId",
                table: "ViajePasajero",
                column: "AgenciaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ViajePasajero_Agencia_AgenciaId",
                table: "ViajePasajero",
                column: "AgenciaId",
                principalTable: "Agencia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ViajePasajero_Agencia_AgenciaId",
                table: "ViajePasajero");

            migrationBuilder.DropTable(
                name: "Agencia");

            migrationBuilder.DropIndex(
                name: "IX_ViajePasajero_AgenciaId",
                table: "ViajePasajero");

            migrationBuilder.DropColumn(
                name: "AgenciaId",
                table: "ViajePasajero");

            migrationBuilder.AddColumn<string>(
                name: "Agencia",
                table: "Pasajero",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
