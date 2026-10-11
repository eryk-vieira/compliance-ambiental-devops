using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceAmbiental.Api.Migrations;

/// <summary>Migração inicial: cria todo o esquema do domínio de Compliance Ambiental Logtech.</summary>
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TB_TRANSPORTADORA",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RazaoSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                LimiteMensalCo2Kg = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_TRANSPORTADORA", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TB_USUARIO",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Login = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                SenhaHash = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Perfil = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_USUARIO", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TB_LICENCA_AMBIENTAL",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TransportadoraId = table.Column<int>(type: "int", nullable: false),
                NumeroLicenca = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                OrgaoEmissor = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                DataEmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                DataValidade = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_LICENCA_AMBIENTAL", x => x.Id);
                table.ForeignKey(
                    name: "FK_TB_LICENCA_AMBIENTAL_TB_TRANSPORTADORA_TransportadoraId",
                    column: x => x.TransportadoraId,
                    principalTable: "TB_TRANSPORTADORA",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TB_VEICULO",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TransportadoraId = table.Column<int>(type: "int", nullable: false),
                Placa = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                TipoCombustivel = table.Column<int>(type: "int", nullable: false),
                ConsumoMedioKmL = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                Ativo = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_VEICULO", x => x.Id);
                table.ForeignKey(
                    name: "FK_TB_VEICULO_TB_TRANSPORTADORA_TransportadoraId",
                    column: x => x.TransportadoraId,
                    principalTable: "TB_TRANSPORTADORA",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TB_EMISSAO_CARBONO",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TransportadoraId = table.Column<int>(type: "int", nullable: false),
                Ano = table.Column<int>(type: "int", nullable: false),
                Mes = table.Column<int>(type: "int", nullable: false),
                TotalCo2Kg = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: false),
                QtdOperacoes = table.Column<int>(type: "int", nullable: false),
                AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_EMISSAO_CARBONO", x => x.Id);
                table.ForeignKey(
                    name: "FK_TB_EMISSAO_CARBONO_TB_TRANSPORTADORA_TransportadoraId",
                    column: x => x.TransportadoraId,
                    principalTable: "TB_TRANSPORTADORA",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TB_OPERACAO_LOGISTICA",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VeiculoId = table.Column<int>(type: "int", nullable: false),
                DataOperacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                Origem = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Destino = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Modal = table.Column<int>(type: "int", nullable: false),
                DistanciaKm = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                CargaToneladas = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                EmissaoCo2Kg = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TB_OPERACAO_LOGISTICA", x => x.Id);
                table.ForeignKey(
                    name: "FK_TB_OPERACAO_LOGISTICA_TB_VEICULO_VeiculoId",
                    column: x => x.VeiculoId,
                    principalTable: "TB_VEICULO",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TB_TRANSPORTADORA_Cnpj",
            table: "TB_TRANSPORTADORA",
            column: "Cnpj",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TB_USUARIO_Login",
            table: "TB_USUARIO",
            column: "Login",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TB_LICENCA_AMBIENTAL_TransportadoraId_Status",
            table: "TB_LICENCA_AMBIENTAL",
            columns: new[] { "TransportadoraId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_TB_VEICULO_Placa",
            table: "TB_VEICULO",
            column: "Placa");

        migrationBuilder.CreateIndex(
            name: "IX_TB_VEICULO_TransportadoraId",
            table: "TB_VEICULO",
            column: "TransportadoraId");

        migrationBuilder.CreateIndex(
            name: "IX_TB_EMISSAO_CARBONO_TransportadoraId_Ano_Mes",
            table: "TB_EMISSAO_CARBONO",
            columns: new[] { "TransportadoraId", "Ano", "Mes" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TB_OPERACAO_LOGISTICA_DataOperacao",
            table: "TB_OPERACAO_LOGISTICA",
            column: "DataOperacao");

        migrationBuilder.CreateIndex(
            name: "IX_TB_OPERACAO_LOGISTICA_VeiculoId",
            table: "TB_OPERACAO_LOGISTICA",
            column: "VeiculoId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TB_EMISSAO_CARBONO");
        migrationBuilder.DropTable(name: "TB_LICENCA_AMBIENTAL");
        migrationBuilder.DropTable(name: "TB_OPERACAO_LOGISTICA");
        migrationBuilder.DropTable(name: "TB_USUARIO");
        migrationBuilder.DropTable(name: "TB_VEICULO");
        migrationBuilder.DropTable(name: "TB_TRANSPORTADORA");
    }
}
