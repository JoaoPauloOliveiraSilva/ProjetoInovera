using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectCharterNomes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConhecimentosCodificados_Entrega_EntregaId",
                table: "ConhecimentosCodificados");

            migrationBuilder.DropForeignKey(
                name: "FK_Entrega_ProjectCharters_CharterId",
                table: "Entrega");

            migrationBuilder.DropForeignKey(
                name: "FK_LicaoAprendida_ProjectCharters_CharterId",
                table: "LicaoAprendida");

            migrationBuilder.DropForeignKey(
                name: "FK_LinhaOrcamento_ProjectCharters_CharterId",
                table: "LinhaOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkDocumento_Iniciativas_IniciativaId",
                table: "LinkDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisitoConformidade_ProjectCharters_CharterId",
                table: "RequisitoConformidade");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisitoConformidade_Utilizadores_ResponsavelId",
                table: "RequisitoConformidade");

            migrationBuilder.DropForeignKey(
                name: "FK_Risco_ProjectCharters_CharterId",
                table: "Risco");

            migrationBuilder.DropForeignKey(
                name: "FK_Risco_Utilizadores_ResponsavelId",
                table: "Risco");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefaWbs_Entrega_MilestoneId",
                table: "TarefaWbs");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefaWbs_ProjectCharters_CharterId",
                table: "TarefaWbs");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefaWbs_TarefaWbs_DependeDeId",
                table: "TarefaWbs");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefaWbs_Utilizadores_ResponsavelId",
                table: "TarefaWbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TarefaWbs",
                table: "TarefaWbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Risco",
                table: "Risco");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequisitoConformidade",
                table: "RequisitoConformidade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LinkDocumento",
                table: "LinkDocumento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LinhaOrcamento",
                table: "LinhaOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LicaoAprendida",
                table: "LicaoAprendida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entrega",
                table: "Entrega");

            migrationBuilder.RenameTable(
                name: "TarefaWbs",
                newName: "TarefasWbs");

            migrationBuilder.RenameTable(
                name: "Risco",
                newName: "Riscos");

            migrationBuilder.RenameTable(
                name: "RequisitoConformidade",
                newName: "RequisitosConformidade");

            migrationBuilder.RenameTable(
                name: "LinkDocumento",
                newName: "LinksDocumento");

            migrationBuilder.RenameTable(
                name: "LinhaOrcamento",
                newName: "LinhasOrcamento");

            migrationBuilder.RenameTable(
                name: "LicaoAprendida",
                newName: "LicoesAprendidas");

            migrationBuilder.RenameTable(
                name: "Entrega",
                newName: "Entregas");

            migrationBuilder.RenameIndex(
                name: "IX_TarefaWbs_ResponsavelId",
                table: "TarefasWbs",
                newName: "IX_TarefasWbs_ResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_TarefaWbs_MilestoneId",
                table: "TarefasWbs",
                newName: "IX_TarefasWbs_MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_TarefaWbs_DependeDeId",
                table: "TarefasWbs",
                newName: "IX_TarefasWbs_DependeDeId");

            migrationBuilder.RenameIndex(
                name: "IX_TarefaWbs_CharterId",
                table: "TarefasWbs",
                newName: "IX_TarefasWbs_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_Risco_ResponsavelId",
                table: "Riscos",
                newName: "IX_Riscos_ResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_Risco_CharterId",
                table: "Riscos",
                newName: "IX_Riscos_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_RequisitoConformidade_ResponsavelId",
                table: "RequisitosConformidade",
                newName: "IX_RequisitosConformidade_ResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_RequisitoConformidade_CharterId",
                table: "RequisitosConformidade",
                newName: "IX_RequisitosConformidade_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkDocumento_IniciativaId",
                table: "LinksDocumento",
                newName: "IX_LinksDocumento_IniciativaId");

            migrationBuilder.RenameIndex(
                name: "IX_LinhaOrcamento_CharterId",
                table: "LinhasOrcamento",
                newName: "IX_LinhasOrcamento_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_LicaoAprendida_CharterId",
                table: "LicoesAprendidas",
                newName: "IX_LicoesAprendidas_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_Entrega_CharterId",
                table: "Entregas",
                newName: "IX_Entregas_CharterId");

            migrationBuilder.AddColumn<string>(
                name: "SupervisorNome",
                table: "ProjectCharters",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelNome",
                table: "TarefasWbs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelNome",
                table: "Riscos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelNome",
                table: "RequisitosConformidade",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TarefasWbs",
                table: "TarefasWbs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Riscos",
                table: "Riscos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequisitosConformidade",
                table: "RequisitosConformidade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinksDocumento",
                table: "LinksDocumento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinhasOrcamento",
                table: "LinhasOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicoesAprendidas",
                table: "LicoesAprendidas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entregas",
                table: "Entregas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConhecimentosCodificados_Entregas_EntregaId",
                table: "ConhecimentosCodificados",
                column: "EntregaId",
                principalTable: "Entregas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Entregas_ProjectCharters_CharterId",
                table: "Entregas",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LicoesAprendidas_ProjectCharters_CharterId",
                table: "LicoesAprendidas",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LinhasOrcamento_ProjectCharters_CharterId",
                table: "LinhasOrcamento",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LinksDocumento_Iniciativas_IniciativaId",
                table: "LinksDocumento",
                column: "IniciativaId",
                principalTable: "Iniciativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitosConformidade_ProjectCharters_CharterId",
                table: "RequisitosConformidade",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitosConformidade_Utilizadores_ResponsavelId",
                table: "RequisitosConformidade",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Riscos_ProjectCharters_CharterId",
                table: "Riscos",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Riscos_Utilizadores_ResponsavelId",
                table: "Riscos",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefasWbs_Entregas_MilestoneId",
                table: "TarefasWbs",
                column: "MilestoneId",
                principalTable: "Entregas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefasWbs_ProjectCharters_CharterId",
                table: "TarefasWbs",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefasWbs_TarefasWbs_DependeDeId",
                table: "TarefasWbs",
                column: "DependeDeId",
                principalTable: "TarefasWbs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefasWbs_Utilizadores_ResponsavelId",
                table: "TarefasWbs",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConhecimentosCodificados_Entregas_EntregaId",
                table: "ConhecimentosCodificados");

            migrationBuilder.DropForeignKey(
                name: "FK_Entregas_ProjectCharters_CharterId",
                table: "Entregas");

            migrationBuilder.DropForeignKey(
                name: "FK_LicoesAprendidas_ProjectCharters_CharterId",
                table: "LicoesAprendidas");

            migrationBuilder.DropForeignKey(
                name: "FK_LinhasOrcamento_ProjectCharters_CharterId",
                table: "LinhasOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_LinksDocumento_Iniciativas_IniciativaId",
                table: "LinksDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisitosConformidade_ProjectCharters_CharterId",
                table: "RequisitosConformidade");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisitosConformidade_Utilizadores_ResponsavelId",
                table: "RequisitosConformidade");

            migrationBuilder.DropForeignKey(
                name: "FK_Riscos_ProjectCharters_CharterId",
                table: "Riscos");

            migrationBuilder.DropForeignKey(
                name: "FK_Riscos_Utilizadores_ResponsavelId",
                table: "Riscos");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefasWbs_Entregas_MilestoneId",
                table: "TarefasWbs");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefasWbs_ProjectCharters_CharterId",
                table: "TarefasWbs");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefasWbs_TarefasWbs_DependeDeId",
                table: "TarefasWbs");

            migrationBuilder.DropForeignKey(
                name: "FK_TarefasWbs_Utilizadores_ResponsavelId",
                table: "TarefasWbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TarefasWbs",
                table: "TarefasWbs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Riscos",
                table: "Riscos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequisitosConformidade",
                table: "RequisitosConformidade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LinksDocumento",
                table: "LinksDocumento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LinhasOrcamento",
                table: "LinhasOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LicoesAprendidas",
                table: "LicoesAprendidas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entregas",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "SupervisorNome",
                table: "ProjectCharters");

            migrationBuilder.DropColumn(
                name: "ResponsavelNome",
                table: "TarefasWbs");

            migrationBuilder.DropColumn(
                name: "ResponsavelNome",
                table: "Riscos");

            migrationBuilder.DropColumn(
                name: "ResponsavelNome",
                table: "RequisitosConformidade");

            migrationBuilder.RenameTable(
                name: "TarefasWbs",
                newName: "TarefaWbs");

            migrationBuilder.RenameTable(
                name: "Riscos",
                newName: "Risco");

            migrationBuilder.RenameTable(
                name: "RequisitosConformidade",
                newName: "RequisitoConformidade");

            migrationBuilder.RenameTable(
                name: "LinksDocumento",
                newName: "LinkDocumento");

            migrationBuilder.RenameTable(
                name: "LinhasOrcamento",
                newName: "LinhaOrcamento");

            migrationBuilder.RenameTable(
                name: "LicoesAprendidas",
                newName: "LicaoAprendida");

            migrationBuilder.RenameTable(
                name: "Entregas",
                newName: "Entrega");

            migrationBuilder.RenameIndex(
                name: "IX_TarefasWbs_ResponsavelId",
                table: "TarefaWbs",
                newName: "IX_TarefaWbs_ResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_TarefasWbs_MilestoneId",
                table: "TarefaWbs",
                newName: "IX_TarefaWbs_MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_TarefasWbs_DependeDeId",
                table: "TarefaWbs",
                newName: "IX_TarefaWbs_DependeDeId");

            migrationBuilder.RenameIndex(
                name: "IX_TarefasWbs_CharterId",
                table: "TarefaWbs",
                newName: "IX_TarefaWbs_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_Riscos_ResponsavelId",
                table: "Risco",
                newName: "IX_Risco_ResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_Riscos_CharterId",
                table: "Risco",
                newName: "IX_Risco_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_RequisitosConformidade_ResponsavelId",
                table: "RequisitoConformidade",
                newName: "IX_RequisitoConformidade_ResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_RequisitosConformidade_CharterId",
                table: "RequisitoConformidade",
                newName: "IX_RequisitoConformidade_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_LinksDocumento_IniciativaId",
                table: "LinkDocumento",
                newName: "IX_LinkDocumento_IniciativaId");

            migrationBuilder.RenameIndex(
                name: "IX_LinhasOrcamento_CharterId",
                table: "LinhaOrcamento",
                newName: "IX_LinhaOrcamento_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_LicoesAprendidas_CharterId",
                table: "LicaoAprendida",
                newName: "IX_LicaoAprendida_CharterId");

            migrationBuilder.RenameIndex(
                name: "IX_Entregas_CharterId",
                table: "Entrega",
                newName: "IX_Entrega_CharterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TarefaWbs",
                table: "TarefaWbs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Risco",
                table: "Risco",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequisitoConformidade",
                table: "RequisitoConformidade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinkDocumento",
                table: "LinkDocumento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinhaOrcamento",
                table: "LinhaOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicaoAprendida",
                table: "LicaoAprendida",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entrega",
                table: "Entrega",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConhecimentosCodificados_Entrega_EntregaId",
                table: "ConhecimentosCodificados",
                column: "EntregaId",
                principalTable: "Entrega",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Entrega_ProjectCharters_CharterId",
                table: "Entrega",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LicaoAprendida_ProjectCharters_CharterId",
                table: "LicaoAprendida",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LinhaOrcamento_ProjectCharters_CharterId",
                table: "LinhaOrcamento",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LinkDocumento_Iniciativas_IniciativaId",
                table: "LinkDocumento",
                column: "IniciativaId",
                principalTable: "Iniciativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitoConformidade_ProjectCharters_CharterId",
                table: "RequisitoConformidade",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitoConformidade_Utilizadores_ResponsavelId",
                table: "RequisitoConformidade",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Risco_ProjectCharters_CharterId",
                table: "Risco",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Risco_Utilizadores_ResponsavelId",
                table: "Risco",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefaWbs_Entrega_MilestoneId",
                table: "TarefaWbs",
                column: "MilestoneId",
                principalTable: "Entrega",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefaWbs_ProjectCharters_CharterId",
                table: "TarefaWbs",
                column: "CharterId",
                principalTable: "ProjectCharters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefaWbs_TarefaWbs_DependeDeId",
                table: "TarefaWbs",
                column: "DependeDeId",
                principalTable: "TarefaWbs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TarefaWbs_Utilizadores_ResponsavelId",
                table: "TarefaWbs",
                column: "ResponsavelId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
