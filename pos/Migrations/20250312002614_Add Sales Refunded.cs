using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesRefunded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialHistories_Finances_FinanceId",
                table: "FinancialHistories");

            migrationBuilder.AlterColumn<int>(
                name: "FinanceId",
                table: "FinancialHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SalesRefundeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    StockReturned = table.Column<int>(type: "int", nullable: false),
                    Information = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRefundeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesRefundeds_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesRefundeds_TransactionId",
                table: "SalesRefundeds",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialHistories_Finances_FinanceId",
                table: "FinancialHistories",
                column: "FinanceId",
                principalTable: "Finances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialHistories_Finances_FinanceId",
                table: "FinancialHistories");

            migrationBuilder.DropTable(
                name: "SalesRefundeds");

            migrationBuilder.AlterColumn<int>(
                name: "FinanceId",
                table: "FinancialHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialHistories_Finances_FinanceId",
                table: "FinancialHistories",
                column: "FinanceId",
                principalTable: "Finances",
                principalColumn: "Id");
        }
    }
}
