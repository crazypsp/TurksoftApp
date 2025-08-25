using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyAccountAndLuca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KeyAccountId",
                table: "MaliMusavirler",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LucaId",
                table: "MaliMusavirler",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KeyAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Luca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UyeNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parola = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Luca", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavirler_KeyAccountId",
                table: "MaliMusavirler",
                column: "KeyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavirler_LucaId",
                table: "MaliMusavirler",
                column: "LucaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaliMusavirler_KeyAccount_KeyAccountId",
                table: "MaliMusavirler",
                column: "KeyAccountId",
                principalTable: "KeyAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaliMusavirler_Luca_LucaId",
                table: "MaliMusavirler",
                column: "LucaId",
                principalTable: "Luca",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaliMusavirler_KeyAccount_KeyAccountId",
                table: "MaliMusavirler");

            migrationBuilder.DropForeignKey(
                name: "FK_MaliMusavirler_Luca_LucaId",
                table: "MaliMusavirler");

            migrationBuilder.DropTable(
                name: "KeyAccount");

            migrationBuilder.DropTable(
                name: "Luca");

            migrationBuilder.DropIndex(
                name: "IX_MaliMusavirler_KeyAccountId",
                table: "MaliMusavirler");

            migrationBuilder.DropIndex(
                name: "IX_MaliMusavirler_LucaId",
                table: "MaliMusavirler");

            migrationBuilder.DropColumn(
                name: "KeyAccountId",
                table: "MaliMusavirler");

            migrationBuilder.DropColumn(
                name: "LucaId",
                table: "MaliMusavirler");
        }
    }
}
