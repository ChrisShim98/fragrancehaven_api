using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdminTransactionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Transactions");
        }
    }
}
