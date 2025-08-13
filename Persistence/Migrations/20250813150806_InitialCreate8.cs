using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    AktifEh = table.Column<string>(type: "text", nullable: false),
                    AnaCustomerKod = table.Column<long>(type: "bigint", nullable: true),
                    Adres = table.Column<string>(type: "text", nullable: true),
                    EPosta = table.Column<string>(type: "text", nullable: true),
                    CustomerTuru = table.Column<string>(type: "text", nullable: false),
                    FaxAlanKod = table.Column<int>(type: "integer", nullable: true),
                    FaxNo = table.Column<string>(type: "text", nullable: true),
                    KimlikNo = table.Column<long>(type: "bigint", nullable: false),
                    KisaAd = table.Column<string>(type: "text", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    Seviye = table.Column<byte>(type: "smallint", nullable: true),
                    TelAlanKod = table.Column<int>(type: "integer", nullable: true),
                    TelNo = table.Column<string>(type: "text", nullable: true),
                    YetkiliAdSoyad = table.Column<string>(type: "text", nullable: true),
                    YetkiliCustomerKod = table.Column<long>(type: "bigint", nullable: true),
                    YetkiliTelNo = table.Column<string>(type: "text", nullable: true),
                    YetkiliUnvani = table.Column<string>(type: "text", nullable: true),
                    AnaCustomerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Customers_AnaCustomerId",
                        column: x => x.AnaCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AnaCustomerId",
                table: "Customers",
                column: "AnaCustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
