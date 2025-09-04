using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initialeate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OlusturanKullaniciId",
                table: "Kullanicilar",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "OlusturanKullaniciId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OlusturanKullaniciId",
                table: "Kullanicilar");
        }
    }
}
