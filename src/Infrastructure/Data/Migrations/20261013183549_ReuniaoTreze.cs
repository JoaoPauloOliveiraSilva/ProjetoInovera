using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReuniaoTreze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChaveArmazenamento",
                table: "ExecucaoVigilancia",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeFicheiro",
                table: "ExecucaoVigilancia",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TamanhoBytes",
                table: "ExecucaoVigilancia",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoConteudo",
                table: "ExecucaoVigilancia",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChaveArmazenamento",
                table: "ExecucaoVigilancia");

            migrationBuilder.DropColumn(
                name: "NomeFicheiro",
                table: "ExecucaoVigilancia");

            migrationBuilder.DropColumn(
                name: "TamanhoBytes",
                table: "ExecucaoVigilancia");

            migrationBuilder.DropColumn(
                name: "TipoConteudo",
                table: "ExecucaoVigilancia");
        }
    }
}
