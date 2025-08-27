using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserRelationsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanici_Bayi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    AtananRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanici_Bayi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanici_Bayi_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kullanici_Bayi_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kullanici_Firma",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    AtananRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanici_Firma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanici_Firma_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kullanici_Firma_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kullanici_MaliMusavir",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    AtananRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanici_MaliMusavir", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanici_MaliMusavir_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kullanici_MaliMusavir_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Bayi_BayiId",
                table: "Kullanici_Bayi",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Bayi_CreateDate",
                table: "Kullanici_Bayi",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Bayi_IsActive",
                table: "Kullanici_Bayi",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Bayi_KullaniciId",
                table: "Kullanici_Bayi",
                column: "KullaniciId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Bayi_KullaniciId_BayiId",
                table: "Kullanici_Bayi",
                columns: new[] { "KullaniciId", "BayiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Firma_CreateDate",
                table: "Kullanici_Firma",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Firma_FirmaId",
                table: "Kullanici_Firma",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Firma_IsActive",
                table: "Kullanici_Firma",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Firma_KullaniciId",
                table: "Kullanici_Firma",
                column: "KullaniciId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Firma_KullaniciId_FirmaId",
                table: "Kullanici_Firma",
                columns: new[] { "KullaniciId", "FirmaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_MaliMusavir_CreateDate",
                table: "Kullanici_MaliMusavir",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_MaliMusavir_IsActive",
                table: "Kullanici_MaliMusavir",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_MaliMusavir_KullaniciId",
                table: "Kullanici_MaliMusavir",
                column: "KullaniciId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_MaliMusavir_KullaniciId_MaliMusavirId",
                table: "Kullanici_MaliMusavir",
                columns: new[] { "KullaniciId", "MaliMusavirId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_MaliMusavir_MaliMusavirId",
                table: "Kullanici_MaliMusavir",
                column: "MaliMusavirId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kullanici_Bayi");

            migrationBuilder.DropTable(
                name: "Kullanici_Firma");

            migrationBuilder.DropTable(
                name: "Kullanici_MaliMusavir");
        }
    }
}
