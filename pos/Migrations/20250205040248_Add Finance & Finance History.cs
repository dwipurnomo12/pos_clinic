using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceFinanceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPurchase",
                table: "IncomingItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Finances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nominal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Finances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinanceStatus = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinanceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialHistories_Finances_FinanceId",
                        column: x => x.FinanceId,
                        principalTable: "Finances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialHistories_FinanceId",
                table: "FinancialHistories",
                column: "FinanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialHistories");

            migrationBuilder.DropTable(
                name: "Finances");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPurchase",
                table: "IncomingItems",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
