using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConhecimentoEstrategia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinhaEstrategiaKpis");

            migrationBuilder.AlterColumn<string>(
                name: "RecursosExtra",
                table: "LinhasEstrategia",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Abordagem",
                table: "LinhasEstrategia",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "Kpis",
                table: "LinhasEstrategia",
                type: "integer[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kpis",
                table: "LinhasEstrategia");

            migrationBuilder.AlterColumn<string>(
                name: "RecursosExtra",
                table: "LinhasEstrategia",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Abordagem",
                table: "LinhasEstrategia",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LinhaEstrategiaKpis",
                columns: table => new
                {
                    KpisId = table.Column<int>(type: "integer", nullable: false),
                    LinhaEstrategiaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhaEstrategiaKpis", x => new { x.KpisId, x.LinhaEstrategiaId });
                    table.ForeignKey(
                        name: "FK_LinhaEstrategiaKpis_Kpis_KpisId",
                        column: x => x.KpisId,
                        principalTable: "Kpis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LinhaEstrategiaKpis_LinhasEstrategia_LinhaEstrategiaId",
                        column: x => x.LinhaEstrategiaId,
                        principalTable: "LinhasEstrategia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinhaEstrategiaKpis_LinhaEstrategiaId",
                table: "LinhaEstrategiaKpis",
                column: "LinhaEstrategiaId");
        }
    }
}
