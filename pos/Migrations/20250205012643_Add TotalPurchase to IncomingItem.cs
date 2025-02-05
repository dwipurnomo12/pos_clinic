using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalPurchasetoIncomingItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "TransactionDetails",
                newName: "ItemName");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPurchase",
                table: "IncomingItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPurchase",
                table: "IncomingItems");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "TransactionDetails",
                newName: "ProductName");
        }
    }
}
