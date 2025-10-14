using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class CambioMoneda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonedaId",
                table: "Viaje",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Moneda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Simbolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoIso = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moneda", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Viaje_MonedaId",
                table: "Viaje",
                column: "MonedaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Viaje_Moneda_MonedaId",
                table: "Viaje",
                column: "MonedaId",
                principalTable: "Moneda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Viaje_Moneda_MonedaId",
                table: "Viaje");

            migrationBuilder.DropTable(
                name: "Moneda");

            migrationBuilder.DropIndex(
                name: "IX_Viaje_MonedaId",
                table: "Viaje");

            migrationBuilder.DropColumn(
                name: "MonedaId",
                table: "Viaje");
        }
    }
}
