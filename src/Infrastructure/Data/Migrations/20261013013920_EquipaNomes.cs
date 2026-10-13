using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EquipaNomes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlocacaoMensal_MembroEquipa_MembroEquipaId",
                table: "AlocacaoMensal");

            migrationBuilder.DropForeignKey(
                name: "FK_MembroEquipa_Iniciativas_IniciativaId",
                table: "MembroEquipa");

            migrationBuilder.DropForeignKey(
                name: "FK_MembroEquipa_Utilizadores_UtilizadorId",
                table: "MembroEquipa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembroEquipa",
                table: "MembroEquipa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlocacaoMensal",
                table: "AlocacaoMensal");

            migrationBuilder.RenameTable(
                name: "MembroEquipa",
                newName: "MembrosEquipa");

            migrationBuilder.RenameTable(
                name: "AlocacaoMensal",
                newName: "AlocacoesMensais");

            migrationBuilder.RenameIndex(
                name: "IX_MembroEquipa_UtilizadorId",
                table: "MembrosEquipa",
                newName: "IX_MembrosEquipa_UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_MembroEquipa_IniciativaId_UtilizadorId",
                table: "MembrosEquipa",
                newName: "IX_MembrosEquipa_IniciativaId_UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_AlocacaoMensal_MembroEquipaId_Ano_Mes",
                table: "AlocacoesMensais",
                newName: "IX_AlocacoesMensais_MembroEquipaId_Ano_Mes");

            migrationBuilder.AlterColumn<int>(
                name: "UtilizadorId",
                table: "MembrosEquipa",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "MembrosEquipa",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembrosEquipa",
                table: "MembrosEquipa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlocacoesMensais",
                table: "AlocacoesMensais",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlocacoesMensais_MembrosEquipa_MembroEquipaId",
                table: "AlocacoesMensais",
                column: "MembroEquipaId",
                principalTable: "MembrosEquipa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembrosEquipa_Iniciativas_IniciativaId",
                table: "MembrosEquipa",
                column: "IniciativaId",
                principalTable: "Iniciativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembrosEquipa_Utilizadores_UtilizadorId",
                table: "MembrosEquipa",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlocacoesMensais_MembrosEquipa_MembroEquipaId",
                table: "AlocacoesMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_MembrosEquipa_Iniciativas_IniciativaId",
                table: "MembrosEquipa");

            migrationBuilder.DropForeignKey(
                name: "FK_MembrosEquipa_Utilizadores_UtilizadorId",
                table: "MembrosEquipa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembrosEquipa",
                table: "MembrosEquipa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlocacoesMensais",
                table: "AlocacoesMensais");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "MembrosEquipa");

            migrationBuilder.RenameTable(
                name: "MembrosEquipa",
                newName: "MembroEquipa");

            migrationBuilder.RenameTable(
                name: "AlocacoesMensais",
                newName: "AlocacaoMensal");

            migrationBuilder.RenameIndex(
                name: "IX_MembrosEquipa_UtilizadorId",
                table: "MembroEquipa",
                newName: "IX_MembroEquipa_UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_MembrosEquipa_IniciativaId_UtilizadorId",
                table: "MembroEquipa",
                newName: "IX_MembroEquipa_IniciativaId_UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_AlocacoesMensais_MembroEquipaId_Ano_Mes",
                table: "AlocacaoMensal",
                newName: "IX_AlocacaoMensal_MembroEquipaId_Ano_Mes");

            migrationBuilder.AlterColumn<int>(
                name: "UtilizadorId",
                table: "MembroEquipa",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembroEquipa",
                table: "MembroEquipa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlocacaoMensal",
                table: "AlocacaoMensal",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlocacaoMensal_MembroEquipa_MembroEquipaId",
                table: "AlocacaoMensal",
                column: "MembroEquipaId",
                principalTable: "MembroEquipa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembroEquipa_Iniciativas_IniciativaId",
                table: "MembroEquipa",
                column: "IniciativaId",
                principalTable: "Iniciativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembroEquipa_Utilizadores_UtilizadorId",
                table: "MembroEquipa",
                column: "UtilizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
