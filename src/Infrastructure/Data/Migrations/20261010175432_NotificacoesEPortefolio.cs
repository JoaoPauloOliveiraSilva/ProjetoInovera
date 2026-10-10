using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NotificacoesEPortefolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenciaImportacao",
                table: "Iniciativas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelNome",
                table: "Iniciativas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutorNome",
                table: "Ideias",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DestinatarioId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false),
                    EmailEnviado = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Utilizadores_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Iniciativas_ReferenciaImportacao",
                table: "Iniciativas",
                column: "ReferenciaImportacao");

            migrationBuilder.CreateIndex(
                name: "IX_Ideias_IdCaixaInovacao",
                table: "Ideias",
                column: "IdCaixaInovacao");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_DestinatarioId_Lida",
                table: "Notificacoes",
                columns: new[] { "DestinatarioId", "Lida" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_Iniciativas_ReferenciaImportacao",
                table: "Iniciativas");

            migrationBuilder.DropIndex(
                name: "IX_Ideias_IdCaixaInovacao",
                table: "Ideias");

            migrationBuilder.DropColumn(
                name: "ReferenciaImportacao",
                table: "Iniciativas");

            migrationBuilder.DropColumn(
                name: "ResponsavelNome",
                table: "Iniciativas");

            migrationBuilder.DropColumn(
                name: "AutorNome",
                table: "Ideias");
        }
    }
}
