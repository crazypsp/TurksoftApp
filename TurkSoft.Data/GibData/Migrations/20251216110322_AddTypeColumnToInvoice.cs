using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.GibData.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeColumnToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Invoice",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Invoice");
        }
    }
}
