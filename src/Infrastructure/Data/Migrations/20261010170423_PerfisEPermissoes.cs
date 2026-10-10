using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innovera.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PerfisEPermissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Perfil",
                table: "Utilizadores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Perfil",
                table: "Utilizadores",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
