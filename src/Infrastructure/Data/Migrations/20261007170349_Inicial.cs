using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "ideia_numero_seq");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AtividadesPlanoAnual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Cor = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Recorrencia = table.Column<int>(type: "integer", nullable: false),
                    DiasAntecedenciaAviso = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadesPlanoAnual", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AtivosIntangiveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    EntidadeRegisto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Designacao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    NumeroPedido = table.Column<string>(type: "text", nullable: true),
                    Autores = table.Column<string>(type: "text", nullable: true),
                    DataRegisto = table.Column<DateOnly>(type: "date", nullable: true),
                    DataValidade = table.Column<DateOnly>(type: "date", nullable: true),
                    Ambito = table.Column<string>(type: "text", nullable: true),
                    Situacao = table.Column<string>(type: "text", nullable: true),
                    Custo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PerspetivaValor = table.Column<string>(type: "text", nullable: true),
                    NumeroProcesso = table.Column<string>(type: "text", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtivosIntangiveis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConhecimentosTacitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Empresa = table.Column<string>(type: "text", nullable: true),
                    Detentor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Confidencialidade = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConhecimentosTacitos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FerramentasMetodos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SumarioExecutivo = table.Column<string>(type: "text", nullable: true),
                    Potencial = table.Column<string>(type: "text", nullable: true),
                    CustoAquisicao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Divulgacao = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FerramentasMetodos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kpis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Formula = table.Column<string>(type: "text", nullable: true),
                    Fonte = table.Column<int>(type: "integer", nullable: false),
                    ObjetivoAnual = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kpis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parceiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parceiros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistosAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Acao = table.Column<int>(type: "integer", nullable: false),
                    ValoresAntes = table.Column<string>(type: "jsonb", nullable: true),
                    ValoresDepois = table.Column<string>(type: "jsonb", nullable: true),
                    UtilizadorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NumeroColaborador = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Departamento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Empresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Perfil = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValoresExternos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Periodo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValoresExternos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtivoIntangivelAtividades",
                columns: table => new
                {
                    AtivoIntangivelId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Responsavel = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: true),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtivoIntangivelAtividades", x => new { x.AtivoIntangivelId, x.Id });
                    table.ForeignKey(
                        name: "FK_AtivoIntangivelAtividades_AtivosIntangiveis_AtivoIntangivel~",
                        column: x => x.AtivoIntangivelId,
                        principalTable: "AtivosIntangiveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConhecimentoTacitoAtividades",
                columns: table => new
                {
                    ConhecimentoTacitoId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Responsavel = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: true),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConhecimentoTacitoAtividades", x => new { x.ConhecimentoTacitoId, x.Id });
                    table.ForeignKey(
                        name: "FK_ConhecimentoTacitoAtividades_ConhecimentosTacitos_Conhecime~",
                        column: x => x.ConhecimentoTacitoId,
                        principalTable: "ConhecimentosTacitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FerramentaMetodoAnalises",
                columns: table => new
                {
                    FerramentaMetodoId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Analise = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FerramentaMetodoAnalises", x => new { x.FerramentaMetodoId, x.Id });
                    table.ForeignKey(
                        name: "FK_FerramentaMetodoAnalises_FerramentasMetodos_FerramentaMetod~",
                        column: x => x.FerramentaMetodoId,
                        principalTable: "FerramentasMetodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValoresKpi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Periodo = table.Column<int>(type: "integer", nullable: false),
                    ValorAbsoluto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ValorRelativo = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Objetivo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CalculadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValoresKpi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValoresKpi_Kpis_KpiId",
                        column: x => x.KpiId,
                        principalTable: "Kpis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcordosParceria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParceiroId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    ProtocoloEstabelecido = table.Column<bool>(type: "boolean", nullable: false),
                    TipoProtocolo = table.Column<int>(type: "integer", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    IncluiGestaoPropriedadeIntelectual = table.Column<bool>(type: "boolean", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    LinkDocumento = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcordosParceria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcordosParceria_Parceiros_ParceiroId",
                        column: x => x.ParceiroId,
                        principalTable: "Parceiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Alvo = table.Column<int>(type: "integer", nullable: false),
                    AlvoId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarioId = table.Column<int>(type: "integer", nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataPrevista = table.Column<DateOnly>(type: "date", nullable: false),
                    Enviado = table.Column<bool>(type: "boolean", nullable: false),
                    EnviadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Lido = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alertas_Utilizadores_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtividadePlanoAnualResponsaveis",
                columns: table => new
                {
                    AtividadePlanoAnualId = table.Column<int>(type: "integer", nullable: false),
                    ResponsaveisId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadePlanoAnualResponsaveis", x => new { x.AtividadePlanoAnualId, x.ResponsaveisId });
                    table.ForeignKey(
                        name: "FK_AtividadePlanoAnualResponsaveis_AtividadesPlanoAnual_Ativid~",
                        column: x => x.AtividadePlanoAnualId,
                        principalTable: "AtividadesPlanoAnual",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtividadePlanoAnualResponsaveis_Utilizadores_ResponsaveisId",
                        column: x => x.ResponsaveisId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ideias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IdCaixaInovacao = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Privada = table.Column<bool>(type: "boolean", nullable: false),
                    AutorId = table.Column<int>(type: "integer", nullable: true),
                    Anonima = table.Column<bool>(type: "boolean", nullable: false),
                    Vantagens = table.Column<string>(type: "text", nullable: true),
                    Requisitos = table.Column<string>(type: "text", nullable: true),
                    ComoEFeitoAtualmente = table.Column<string>(type: "text", nullable: true),
                    ModeloNegocio = table.Column<string>(type: "text", nullable: true),
                    Competidores = table.Column<string>(type: "text", nullable: true),
                    Custos = table.Column<string>(type: "text", nullable: true),
                    TermosAceitesEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Responsabilidade = table.Column<int>(type: "integer", nullable: false),
                    DataSubmissao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FimDiscussao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Classificacao = table.Column<int>(type: "integer", nullable: true),
                    Classe = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ideias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ideias_Utilizadores_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AcaoKpi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ValorKpiId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateOnly>(type: "date", nullable: false),
                    AnaliseResultados = table.Column<string>(type: "text", nullable: true),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Recursos = table.Column<string>(type: "text", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    Prazo = table.Column<DateOnly>(type: "date", nullable: true),
                    ConcluidaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    Eficaz = table.Column<bool>(type: "boolean", nullable: true),
                    AvaliacaoAnual = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcaoKpi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcaoKpi_Utilizadores_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AcaoKpi_ValoresKpi_ValorKpiId",
                        column: x => x.ValorKpiId,
                        principalTable: "ValoresKpi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnexosIdeia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdeiaId = table.Column<int>(type: "integer", nullable: false),
                    NomeFicheiro = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    TipoConteudo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    ChaveArmazenamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnexosIdeia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnexosIdeia_Ideias_IdeiaId",
                        column: x => x.IdeiaId,
                        principalTable: "Ideias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutorIdeia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdeiaId = table.Column<int>(type: "integer", nullable: false),
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false),
                    Confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutorIdeia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutorIdeia_Ideias_IdeiaId",
                        column: x => x.IdeiaId,
                        principalTable: "Ideias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutorIdeia_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AvaliacaoIdeia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdeiaId = table.Column<int>(type: "integer", nullable: false),
                    Custo = table.Column<int>(type: "integer", nullable: true),
                    Enquadramento = table.Column<int>(type: "integer", nullable: true),
                    Beneficio = table.Column<int>(type: "integer", nullable: true),
                    AdequacaoTecnica = table.Column<int>(type: "integer", nullable: true),
                    Incerteza = table.Column<int>(type: "integer", nullable: true),
                    Nota = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    Aprovada = table.Column<bool>(type: "boolean", nullable: true),
                    DataReuniaoCE = table.Column<DateOnly>(type: "date", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacaoIdeia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacaoIdeia_Ideias_IdeiaId",
                        column: x => x.IdeiaId,
                        principalTable: "Ideias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComentariosIdeia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdeiaId = table.Column<int>(type: "integer", nullable: false),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosIdeia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosIdeia_Ideias_IdeiaId",
                        column: x => x.IdeiaId,
                        principalTable: "Ideias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComentariosIdeia_Utilizadores_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Iniciativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    PontoSituacao = table.Column<string>(type: "text", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Origem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PilarEstrategico = table.Column<int>(type: "integer", nullable: true),
                    Horizonte = table.Column<int>(type: "integer", nullable: true),
                    IdeiaOrigemId = table.Column<int>(type: "integer", nullable: true),
                    Discriminador = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TipoAcao = table.Column<int>(type: "integer", nullable: true),
                    NumeroParticipantes = table.Column<int>(type: "integer", nullable: true),
                    TipoProjeto = table.Column<int>(type: "integer", nullable: true),
                    Ambito = table.Column<int>(type: "integer", nullable: true),
                    Resultado = table.Column<string>(type: "text", nullable: true),
                    NomeAluno = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Curso = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrientadorId = table.Column<int>(type: "integer", nullable: true),
                    Oportunidade_Resultado = table.Column<string>(type: "text", nullable: true),
                    Descritivo = table.Column<string>(type: "text", nullable: true),
                    Periodicidade = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iniciativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Iniciativas_Ideias_IdeiaOrigemId",
                        column: x => x.IdeiaOrigemId,
                        principalTable: "Ideias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Iniciativas_Utilizadores_OrientadorId",
                        column: x => x.OrientadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Iniciativas_Utilizadores_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LikeIdeia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdeiaId = table.Column<int>(type: "integer", nullable: false),
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeIdeia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikeIdeia_Ideias_IdeiaId",
                        column: x => x.IdeiaId,
                        principalTable: "Ideias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikeIdeia_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GostoComentario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComentarioId = table.Column<int>(type: "integer", nullable: false),
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GostoComentario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GostoComentario_ComentariosIdeia_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "ComentariosIdeia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GostoComentario_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExecucaoVigilancia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VigilanciaId = table.Column<int>(type: "integer", nullable: false),
                    DataPrevista = table.Column<DateOnly>(type: "date", nullable: false),
                    DataRealizada = table.Column<DateOnly>(type: "date", nullable: true),
                    Resultado = table.Column<string>(type: "text", nullable: true),
                    LinkResultado = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucaoVigilancia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecucaoVigilancia_Iniciativas_VigilanciaId",
                        column: x => x.VigilanciaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IniciativaParceiros",
                columns: table => new
                {
                    IniciativasId = table.Column<int>(type: "integer", nullable: false),
                    ParceirosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IniciativaParceiros", x => new { x.IniciativasId, x.ParceirosId });
                    table.ForeignKey(
                        name: "FK_IniciativaParceiros_Iniciativas_IniciativasId",
                        column: x => x.IniciativasId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IniciativaParceiros_Parceiros_ParceirosId",
                        column: x => x.ParceirosId,
                        principalTable: "Parceiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LinhasEstrategia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    AreaAtuacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Abordagem = table.Column<string>(type: "text", nullable: true),
                    PlanoAcao = table.Column<string>(type: "text", nullable: true),
                    DuracaoMeses = table.Column<int>(type: "integer", nullable: true),
                    NumeroRecursosHumanos = table.Column<int>(type: "integer", nullable: true),
                    HorasMesPorRecurso = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    RecursosExtra = table.Column<string>(type: "text", nullable: true),
                    Horizonte = table.Column<int>(type: "integer", nullable: true),
                    IniciativaId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhasEstrategia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinhasEstrategia_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LinkDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IniciativaId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkDocumento_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembroEquipa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IniciativaId = table.Column<int>(type: "integer", nullable: false),
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false),
                    Funcao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembroEquipa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembroEquipa_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembroEquipa_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotaPontoSituacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IniciativaId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    AutorId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotaPontoSituacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotaPontoSituacao_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotaPontoSituacao_Utilizadores_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OutputsGerados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Modelo = table.Column<int>(type: "integer", nullable: false),
                    Formato = table.Column<int>(type: "integer", nullable: false),
                    Gatilho = table.Column<int>(type: "integer", nullable: false),
                    IniciativaId = table.Column<int>(type: "integer", nullable: true),
                    Periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GeradoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeradoPorId = table.Column<int>(type: "integer", nullable: true),
                    NomeFicheiro = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputsGerados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutputsGerados_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OutputsGerados_Utilizadores_GeradoPorId",
                        column: x => x.GeradoPorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProjectCharters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IniciativaId = table.Column<int>(type: "integer", nullable: false),
                    SupervisorId = table.Column<int>(type: "integer", nullable: true),
                    Fase = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Classe = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContextoJustificacao = table.Column<string>(type: "text", nullable: true),
                    BusinessCase = table.Column<string>(type: "text", nullable: true),
                    DecisaoAprovacao = table.Column<string>(type: "text", nullable: true),
                    Objetivos = table.Column<string>(type: "text", nullable: true),
                    ResultadosEsperados = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCharters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectCharters_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectCharters_Utilizadores_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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

            migrationBuilder.CreateTable(
                name: "AlocacaoMensal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembroEquipaId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Percentagem = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlocacaoMensal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlocacaoMensal_MembroEquipa_MembroEquipaId",
                        column: x => x.MembroEquipaId,
                        principalTable: "MembroEquipa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharterId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Prazo = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MeioVerificacao = table.Column<string>(type: "text", nullable: true),
                    Controlo = table.Column<string>(type: "text", nullable: true),
                    DataConclusao = table.Column<DateOnly>(type: "date", nullable: true),
                    LinkRelatorio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entrega_ProjectCharters_CharterId",
                        column: x => x.CharterId,
                        principalTable: "ProjectCharters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicaoAprendida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharterId = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Licao = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Impacto = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicaoAprendida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicaoAprendida_ProjectCharters_CharterId",
                        column: x => x.CharterId,
                        principalTable: "ProjectCharters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LinhaOrcamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharterId = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    ValorPrevisto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorReal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhaOrcamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinhaOrcamento_ProjectCharters_CharterId",
                        column: x => x.CharterId,
                        principalTable: "ProjectCharters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequisitoConformidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharterId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Requisito = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Entidade = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Resposta = table.Column<string>(type: "text", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    DataPrevista = table.Column<DateOnly>(type: "date", nullable: true),
                    DataConfirmacao = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisitoConformidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisitoConformidade_ProjectCharters_CharterId",
                        column: x => x.CharterId,
                        principalTable: "ProjectCharters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequisitoConformidade_Utilizadores_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Risco",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharterId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Impacto = table.Column<int>(type: "integer", nullable: false),
                    Probabilidade = table.Column<int>(type: "integer", nullable: false),
                    Resposta = table.Column<int>(type: "integer", nullable: true),
                    PlanoAcao = table.Column<string>(type: "text", nullable: true),
                    Recursos = table.Column<string>(type: "text", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    Prazo = table.Column<DateOnly>(type: "date", nullable: true),
                    DataConclusao = table.Column<DateOnly>(type: "date", nullable: true),
                    AvaliacaoEficacia = table.Column<string>(type: "text", nullable: true),
                    ImpactoReavaliado = table.Column<int>(type: "integer", nullable: true),
                    ProbabilidadeReavaliada = table.Column<int>(type: "integer", nullable: true),
                    RespostaReavaliada = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Risco_ProjectCharters_CharterId",
                        column: x => x.CharterId,
                        principalTable: "ProjectCharters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Risco_Utilizadores_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConhecimentosCodificados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Documento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ArquivadoEm = table.Column<DateOnly>(type: "date", nullable: true),
                    ModoConsulta = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Resultado = table.Column<string>(type: "text", nullable: true),
                    Origem = table.Column<int>(type: "integer", nullable: false),
                    IniciativaId = table.Column<int>(type: "integer", nullable: true),
                    EntregaId = table.Column<int>(type: "integer", nullable: true),
                    ExecucaoVigilanciaId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConhecimentosCodificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConhecimentosCodificados_Entrega_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ConhecimentosCodificados_ExecucaoVigilancia_ExecucaoVigilan~",
                        column: x => x.ExecucaoVigilanciaId,
                        principalTable: "ExecucaoVigilancia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ConhecimentosCodificados_Iniciativas_IniciativaId",
                        column: x => x.IniciativaId,
                        principalTable: "Iniciativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TarefaWbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharterId = table.Column<int>(type: "integer", nullable: false),
                    CodigoWbs = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    DependeDeId = table.Column<int>(type: "integer", nullable: true),
                    MilestoneId = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    InicioPlaneado = table.Column<DateOnly>(type: "date", nullable: true),
                    FimPlaneado = table.Column<DateOnly>(type: "date", nullable: true),
                    InicioReal = table.Column<DateOnly>(type: "date", nullable: true),
                    FimReal = table.Column<DateOnly>(type: "date", nullable: true),
                    ResponsavelId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarefaWbs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TarefaWbs_Entrega_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Entrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TarefaWbs_ProjectCharters_CharterId",
                        column: x => x.CharterId,
                        principalTable: "ProjectCharters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TarefaWbs_TarefaWbs_DependeDeId",
                        column: x => x.DependeDeId,
                        principalTable: "TarefaWbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TarefaWbs_Utilizadores_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConhecimentoCodificadoAtividades",
                columns: table => new
                {
                    ConhecimentoCodificadoId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Responsavel = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: true),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConhecimentoCodificadoAtividades", x => new { x.ConhecimentoCodificadoId, x.Id });
                    table.ForeignKey(
                        name: "FK_ConhecimentoCodificadoAtividades_ConhecimentosCodificados_C~",
                        column: x => x.ConhecimentoCodificadoId,
                        principalTable: "ConhecimentosCodificados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcaoKpi_ResponsavelId",
                table: "AcaoKpi",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_AcaoKpi_ValorKpiId",
                table: "AcaoKpi",
                column: "ValorKpiId");

            migrationBuilder.CreateIndex(
                name: "IX_AcordosParceria_ParceiroId",
                table: "AcordosParceria",
                column: "ParceiroId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_Alvo_AlvoId_Tipo_DataPrevista",
                table: "Alertas",
                columns: new[] { "Alvo", "AlvoId", "Tipo", "DataPrevista" });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_DestinatarioId",
                table: "Alertas",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AlocacaoMensal_MembroEquipaId_Ano_Mes",
                table: "AlocacaoMensal",
                columns: new[] { "MembroEquipaId", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnexosIdeia_IdeiaId",
                table: "AnexosIdeia",
                column: "IdeiaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtividadePlanoAnualResponsaveis_ResponsaveisId",
                table: "AtividadePlanoAnualResponsaveis",
                column: "ResponsaveisId");

            migrationBuilder.CreateIndex(
                name: "IX_AutorIdeia_IdeiaId_UtilizadorId",
                table: "AutorIdeia",
                columns: new[] { "IdeiaId", "UtilizadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutorIdeia_UtilizadorId",
                table: "AutorIdeia",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacaoIdeia_IdeiaId",
                table: "AvaliacaoIdeia",
                column: "IdeiaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosIdeia_AutorId",
                table: "ComentariosIdeia",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosIdeia_IdeiaId",
                table: "ComentariosIdeia",
                column: "IdeiaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConhecimentosCodificados_EntregaId",
                table: "ConhecimentosCodificados",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConhecimentosCodificados_ExecucaoVigilanciaId",
                table: "ConhecimentosCodificados",
                column: "ExecucaoVigilanciaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConhecimentosCodificados_IniciativaId",
                table: "ConhecimentosCodificados",
                column: "IniciativaId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrega_CharterId",
                table: "Entrega",
                column: "CharterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucaoVigilancia_VigilanciaId",
                table: "ExecucaoVigilancia",
                column: "VigilanciaId");

            migrationBuilder.CreateIndex(
                name: "IX_GostoComentario_ComentarioId_UtilizadorId",
                table: "GostoComentario",
                columns: new[] { "ComentarioId", "UtilizadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GostoComentario_UtilizadorId",
                table: "GostoComentario",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideias_AutorId",
                table: "Ideias",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideias_Codigo",
                table: "Ideias",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_Ideias_Estado",
                table: "Ideias",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Ideias_Numero",
                table: "Ideias",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IniciativaParceiros_ParceirosId",
                table: "IniciativaParceiros",
                column: "ParceirosId");

            migrationBuilder.CreateIndex(
                name: "IX_Iniciativas_Codigo",
                table: "Iniciativas",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_Iniciativas_IdeiaOrigemId",
                table: "Iniciativas",
                column: "IdeiaOrigemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Iniciativas_OrientadorId",
                table: "Iniciativas",
                column: "OrientadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Iniciativas_ResponsavelId",
                table: "Iniciativas",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Kpis_Numero",
                table: "Kpis",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicaoAprendida_CharterId",
                table: "LicaoAprendida",
                column: "CharterId");

            migrationBuilder.CreateIndex(
                name: "IX_LikeIdeia_IdeiaId_UtilizadorId",
                table: "LikeIdeia",
                columns: new[] { "IdeiaId", "UtilizadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LikeIdeia_UtilizadorId",
                table: "LikeIdeia",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_LinhaEstrategiaKpis_LinhaEstrategiaId",
                table: "LinhaEstrategiaKpis",
                column: "LinhaEstrategiaId");

            migrationBuilder.CreateIndex(
                name: "IX_LinhaOrcamento_CharterId",
                table: "LinhaOrcamento",
                column: "CharterId");

            migrationBuilder.CreateIndex(
                name: "IX_LinhasEstrategia_IniciativaId",
                table: "LinhasEstrategia",
                column: "IniciativaId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkDocumento_IniciativaId",
                table: "LinkDocumento",
                column: "IniciativaId");

            migrationBuilder.CreateIndex(
                name: "IX_MembroEquipa_IniciativaId_UtilizadorId",
                table: "MembroEquipa",
                columns: new[] { "IniciativaId", "UtilizadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembroEquipa_UtilizadorId",
                table: "MembroEquipa",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaPontoSituacao_AutorId",
                table: "NotaPontoSituacao",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaPontoSituacao_IniciativaId",
                table: "NotaPontoSituacao",
                column: "IniciativaId");

            migrationBuilder.CreateIndex(
                name: "IX_OutputsGerados_GeradoPorId",
                table: "OutputsGerados",
                column: "GeradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_OutputsGerados_IniciativaId",
                table: "OutputsGerados",
                column: "IniciativaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCharters_IniciativaId",
                table: "ProjectCharters",
                column: "IniciativaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCharters_SupervisorId",
                table: "ProjectCharters",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistosAuditoria_Entidade_EntidadeId",
                table: "RegistosAuditoria",
                columns: new[] { "Entidade", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_RequisitoConformidade_CharterId",
                table: "RequisitoConformidade",
                column: "CharterId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisitoConformidade_ResponsavelId",
                table: "RequisitoConformidade",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Risco_CharterId",
                table: "Risco",
                column: "CharterId");

            migrationBuilder.CreateIndex(
                name: "IX_Risco_ResponsavelId",
                table: "Risco",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_TarefaWbs_CharterId",
                table: "TarefaWbs",
                column: "CharterId");

            migrationBuilder.CreateIndex(
                name: "IX_TarefaWbs_DependeDeId",
                table: "TarefaWbs",
                column: "DependeDeId");

            migrationBuilder.CreateIndex(
                name: "IX_TarefaWbs_MilestoneId",
                table: "TarefaWbs",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_TarefaWbs_ResponsavelId",
                table: "TarefaWbs",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_Email",
                table: "Utilizadores",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_IdentityId",
                table: "Utilizadores",
                column: "IdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValoresExternos_Tipo_Ano_Periodo",
                table: "ValoresExternos",
                columns: new[] { "Tipo", "Ano", "Periodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValoresKpi_KpiId_Ano_Periodo",
                table: "ValoresKpi",
                columns: new[] { "KpiId", "Ano", "Periodo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcaoKpi");

            migrationBuilder.DropTable(
                name: "AcordosParceria");

            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "AlocacaoMensal");

            migrationBuilder.DropTable(
                name: "AnexosIdeia");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AtividadePlanoAnualResponsaveis");

            migrationBuilder.DropTable(
                name: "AtivoIntangivelAtividades");

            migrationBuilder.DropTable(
                name: "AutorIdeia");

            migrationBuilder.DropTable(
                name: "AvaliacaoIdeia");

            migrationBuilder.DropTable(
                name: "ConhecimentoCodificadoAtividades");

            migrationBuilder.DropTable(
                name: "ConhecimentoTacitoAtividades");

            migrationBuilder.DropTable(
                name: "FerramentaMetodoAnalises");

            migrationBuilder.DropTable(
                name: "GostoComentario");

            migrationBuilder.DropTable(
                name: "IniciativaParceiros");

            migrationBuilder.DropTable(
                name: "LicaoAprendida");

            migrationBuilder.DropTable(
                name: "LikeIdeia");

            migrationBuilder.DropTable(
                name: "LinhaEstrategiaKpis");

            migrationBuilder.DropTable(
                name: "LinhaOrcamento");

            migrationBuilder.DropTable(
                name: "LinkDocumento");

            migrationBuilder.DropTable(
                name: "NotaPontoSituacao");

            migrationBuilder.DropTable(
                name: "OutputsGerados");

            migrationBuilder.DropTable(
                name: "RegistosAuditoria");

            migrationBuilder.DropTable(
                name: "RequisitoConformidade");

            migrationBuilder.DropTable(
                name: "Risco");

            migrationBuilder.DropTable(
                name: "TarefaWbs");

            migrationBuilder.DropTable(
                name: "ValoresExternos");

            migrationBuilder.DropTable(
                name: "ValoresKpi");

            migrationBuilder.DropTable(
                name: "MembroEquipa");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AtividadesPlanoAnual");

            migrationBuilder.DropTable(
                name: "AtivosIntangiveis");

            migrationBuilder.DropTable(
                name: "ConhecimentosCodificados");

            migrationBuilder.DropTable(
                name: "ConhecimentosTacitos");

            migrationBuilder.DropTable(
                name: "FerramentasMetodos");

            migrationBuilder.DropTable(
                name: "ComentariosIdeia");

            migrationBuilder.DropTable(
                name: "Parceiros");

            migrationBuilder.DropTable(
                name: "LinhasEstrategia");

            migrationBuilder.DropTable(
                name: "Kpis");

            migrationBuilder.DropTable(
                name: "Entrega");

            migrationBuilder.DropTable(
                name: "ExecucaoVigilancia");

            migrationBuilder.DropTable(
                name: "ProjectCharters");

            migrationBuilder.DropTable(
                name: "Iniciativas");

            migrationBuilder.DropTable(
                name: "Ideias");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropSequence(
                name: "ideia_numero_seq");
        }
    }
}
