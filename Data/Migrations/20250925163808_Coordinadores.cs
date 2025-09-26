using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyetoSetilPF.Data.Migrations
{
    /// <inheritdoc />
    public partial class Coordinadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Coordinador",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinador_UserId",
                table: "Coordinador",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coordinador_AspNetUsers_UserId",
                table: "Coordinador",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coordinador_AspNetUsers_UserId",
                table: "Coordinador");

            migrationBuilder.DropIndex(
                name: "IX_Coordinador_UserId",
                table: "Coordinador");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Coordinador");
        }
    }
}
