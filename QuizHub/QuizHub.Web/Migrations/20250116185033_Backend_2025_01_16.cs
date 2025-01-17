using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace QuizHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class Backend_2025_01_16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "IdUsuario",
                table: "puntuacion",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "puntuacion",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");


        }
    }
}
