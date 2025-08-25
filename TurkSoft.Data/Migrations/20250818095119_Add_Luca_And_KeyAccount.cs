using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Luca_And_KeyAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaliMusavirler_KeyAccount_KeyAccountId",
                table: "MaliMusavirler");

            migrationBuilder.DropForeignKey(
                name: "FK_MaliMusavirler_Luca_LucaId",
                table: "MaliMusavirler");

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

            migrationBuilder.AlterColumn<string>(
                name: "UyeNo",
                table: "Luca",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Parola",
                table: "Luca",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciAdi",
                table: "Luca",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Luca",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Kod",
                table: "KeyAccount",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "KeyAccount",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "KeyAccount",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "KeyAccount_MaliMusavir",
                columns: table => new
                {
                    KeyAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyAccount_MaliMusavir", x => new { x.KeyAccountId, x.MaliMusavirId });
                    table.ForeignKey(
                        name: "FK_KeyAccount_MaliMusavir_KeyAccount_KeyAccountId",
                        column: x => x.KeyAccountId,
                        principalTable: "KeyAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KeyAccount_MaliMusavir_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Luca_MaliMusavir",
                columns: table => new
                {
                    LucaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Luca_MaliMusavir", x => new { x.LucaId, x.MaliMusavirId });
                    table.ForeignKey(
                        name: "FK_Luca_MaliMusavir_Luca_LucaId",
                        column: x => x.LucaId,
                        principalTable: "Luca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Luca_MaliMusavir_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_Kod",
                table: "KeyAccount",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_MaliMusavir_MaliMusavirId",
                table: "KeyAccount_MaliMusavir",
                column: "MaliMusavirId");

            migrationBuilder.CreateIndex(
                name: "IX_Luca_MaliMusavir_MaliMusavirId",
                table: "Luca_MaliMusavir",
                column: "MaliMusavirId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyAccount_MaliMusavir");

            migrationBuilder.DropTable(
                name: "Luca_MaliMusavir");

            migrationBuilder.DropIndex(
                name: "IX_KeyAccount_Kod",
                table: "KeyAccount");

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

            migrationBuilder.AlterColumn<string>(
                name: "UyeNo",
                table: "Luca",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Parola",
                table: "Luca",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciAdi",
                table: "Luca",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Luca",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Kod",
                table: "KeyAccount",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "KeyAccount",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "KeyAccount",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

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
    }
}
