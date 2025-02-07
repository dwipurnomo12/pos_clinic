using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos.Migrations
{
    /// <inheritdoc />
    public partial class addSeedFinanceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Finances",
                columns: new[] { "Id", "Nominal" },
                values: new object[] { 1, 0m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Finances",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
