using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Base_ShadowProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Firmalar_MaliMusavirler_MaliMusavirId",
                table: "Firmalar");

            migrationBuilder.DropForeignKey(
                name: "FK_LisansAdetler_MaliMusavirler_MaliMusavirId",
                table: "LisansAdetler");

            migrationBuilder.DropForeignKey(
                name: "FK_Lisanslar_MaliMusavirler_MaliMusavirId",
                table: "Lisanslar");

            migrationBuilder.DropForeignKey(
                name: "FK_Loglar_Kullanicilar_KullaniciId",
                table: "Loglar");

            migrationBuilder.DropForeignKey(
                name: "FK_Loglar_MaliMusavirler_MaliMusavirId",
                table: "Loglar");

            migrationBuilder.DropForeignKey(
                name: "FK_Paketler_UrunTipiler_UrunTipiId",
                table: "Paketler");

            migrationBuilder.DropForeignKey(
                name: "FK_UrunFiyatlar_UrunTipiler_UrunTipiId",
                table: "UrunFiyatlar");

            migrationBuilder.DropIndex(
                name: "IX_UrunFiyatlar_UrunTipiId",
                table: "UrunFiyatlar");

            migrationBuilder.DropIndex(
                name: "IX_Loglar_KullaniciId",
                table: "Loglar");

            migrationBuilder.DropIndex(
                name: "IX_Loglar_MaliMusavirId",
                table: "Loglar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UrunTipiler",
                table: "UrunTipiler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Loglar");

            migrationBuilder.DropColumn(
                name: "MaliMusavirId",
                table: "Loglar");

            migrationBuilder.DropColumn(
                name: "MaxCihazSayisi",
                table: "LisansAdetler");

            migrationBuilder.RenameTable(
                name: "UrunTipiler",
                newName: "UrunTipleri");

            migrationBuilder.RenameColumn(
                name: "UrunTipiId",
                table: "UrunFiyatlar",
                newName: "PaketId");

            migrationBuilder.RenameColumn(
                name: "GecerlilikTarihi",
                table: "UrunFiyatlar",
                newName: "GecerlilikBaslangic");

            migrationBuilder.RenameColumn(
                name: "MaliMusavirId",
                table: "Lisanslar",
                newName: "SatisId");

            migrationBuilder.RenameColumn(
                name: "AktifMi",
                table: "Lisanslar",
                newName: "YenilendiMi");

            migrationBuilder.RenameIndex(
                name: "IX_Lisanslar_MaliMusavirId",
                table: "Lisanslar",
                newName: "IX_Lisanslar_SatisId");

            migrationBuilder.RenameColumn(
                name: "MaliMusavirId",
                table: "LisansAdetler",
                newName: "LisansId");

            migrationBuilder.RenameIndex(
                name: "IX_LisansAdetler_MaliMusavirId",
                table: "LisansAdetler",
                newName: "IX_LisansAdetler_LisansId");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WhatsappGonderimler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WhatsappAyarlar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GecerlilikBitis",
                table: "UrunFiyatlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UrunFiyatlar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SmsGonderimler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SmsAyarlar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Paketler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BayiId",
                table: "MaliMusavirler",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MaliMusavirler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MailGonderimler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "SSLKullan",
                table: "MailAyarlar",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Port",
                table: "MailAyarlar",
                type: "int",
                nullable: false,
                defaultValue: 587,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MailAyarlar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Luca",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Loglar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Lisanslar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Limit",
                table: "LisansAdetler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LisansAdetler",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Kullanicilar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "KeyAccount",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MaliMusavirId",
                table: "Firmalar",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "BayiId",
                table: "Firmalar",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Firmalar",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UrunTipleri",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UrunTipleri",
                table: "UrunTipleri",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Bayi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OlusturanKullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bayi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bayi_Kullanicilar_OlusturanKullaniciId",
                        column: x => x.OlusturanKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DosyaEkleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IlgiliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IlgiliTip = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DosyaAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IcerikTipi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Yol = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Boyut = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosyaEkleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntegrasyonHesabi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SistemTipi = table.Column<int>(type: "int", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VeritabaniAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Parola = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntegrasyonHesabi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntegrasyonHesabi_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntegrasyonHesabi_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Etiketler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etiketler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IletisimKisileri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Adres_Ulke = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres_Sehir = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres_Ilce = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres_PostaKodu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Adres_AcikAdres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Adres_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Adres_IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Adres_CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Adres_UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Adres_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IletisimKisileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IletisimKisileri_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    IlgiliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IlgiliTip = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OlusturanUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notlar_Kullanicilar_OlusturanUserId",
                        column: x => x.OlusturanUserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityAsamalari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OlasilikYuzde = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityAsamalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMesajlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IcerikJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Islenmis = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IslenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMesajlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemBildirimleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kanal = table.Column<int>(type: "int", nullable: false),
                    PlanlananTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    HedefKullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemBildirimleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VergiOranlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Oran = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VergiOranlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Aktiviteler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Konu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tur = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    PlanlananTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GerceklesenTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IlgiliId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IlgiliTip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IlgiliKullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aktiviteler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aktiviteler_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aktiviteler_Kullanicilar_IlgiliKullaniciId",
                        column: x => x.IlgiliKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BayiCariler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayiCariler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiCariler_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BayiFirma",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VergiNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayiFirma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiFirma_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BayiKomisyonTarife",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KomisyonYuzde = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayiKomisyonTarife", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiKomisyonTarife_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BayiKomisyonTarife_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiyatListeleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiyatListeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiyatListeleri_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KomisyonOdemePlanlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonemYil = table.Column<int>(type: "int", nullable: false),
                    DonemAy = table.Column<int>(type: "int", nullable: false),
                    ToplamKomisyon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KomisyonOdemePlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KomisyonOdemePlanlari_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kuponlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IndirimYuzde = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IndirimTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaksKullanim = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Kullanildi = table.Column<int>(type: "int", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kuponlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kuponlar_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leadler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeadNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kaynak = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SorumluKullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Adres_Ulke = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres_Sehir = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres_Ilce = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres_PostaKodu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Adres_AcikAdres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Adres_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Adres_IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Adres_CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Adres_UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Adres_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notlar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leadler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leadler_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leadler_Kullanicilar_SorumluKullaniciId",
                        column: x => x.SorumluKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaketIskonto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IskontoYuzde = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaketIskonto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaketIskonto_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaketIskonto_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanalPos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Saglayici = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BaseApiUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PosAnahtar = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StandartKomisyonYuzde = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanalPos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SanalPos_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Satis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SatisNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KDVOrani = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    KDVTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IskontoTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatisDurumu = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Satis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Satis_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Satis_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Satis_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Satis_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebhookAbonelikleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IletiGizliAnahtar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookAbonelikleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookAbonelikleri_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtiketIliskileri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EtiketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IlgiliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IlgiliTip = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtiketIliskileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtiketIliskileri_Etiketler_EtiketId",
                        column: x => x.EtiketId,
                        principalTable: "Etiketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirsatNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AsamaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TahminiTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_OpportunityAsamalari_AsamaId",
                        column: x => x.AsamaId,
                        principalTable: "OpportunityAsamalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AktiviteAtamalari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AktiviteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AktiviteAtamalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AktiviteAtamalari_Aktiviteler_AktiviteId",
                        column: x => x.AktiviteId,
                        principalTable: "Aktiviteler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AktiviteAtamalari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BayiCariHareketleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BayiCariId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferansId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferansTip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayiCariHareketleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiCariHareketleri_BayiCariler_BayiCariId",
                        column: x => x.BayiCariId,
                        principalTable: "BayiCariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiyatListesiKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiyatListesiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiyatListesiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiyatListesiKalemleri_FiyatListeleri_FiyatListesiId",
                        column: x => x.FiyatListesiId,
                        principalTable: "FiyatListeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FiyatListesiKalemleri_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Faturalar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaturaNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SatisId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kdvoran = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Kdvtutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Toplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Net = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faturalar_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Faturalar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Faturalar_Satis_SatisId",
                        column: x => x.SatisId,
                        principalTable: "Satis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Odeme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SatisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdemeYontemi = table.Column<int>(type: "int", nullable: false),
                    OdemeDurumu = table.Column<int>(type: "int", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KomisyonOrani = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    KomisyonTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SanalPosId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SaglayiciIslemNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Taksit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odeme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Odeme_SanalPos_SanalPosId",
                        column: x => x.SanalPosId,
                        principalTable: "SanalPos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Odeme_Satis_SatisId",
                        column: x => x.SatisId,
                        principalTable: "Satis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisKalem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SatisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Miktar = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisKalem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisKalem_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SatisKalem_Satis_SatisId",
                        column: x => x.SatisId,
                        principalTable: "Satis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityAsamaGecisleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromAsamaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToAsamaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GecisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityAsamaGecisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityAsamaGecisleri_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityAsamaGecisleri_OpportunityAsamalari_FromAsamaId",
                        column: x => x.FromAsamaId,
                        principalTable: "OpportunityAsamalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityAsamaGecisleri_OpportunityAsamalari_ToAsamaId",
                        column: x => x.ToAsamaId,
                        principalTable: "OpportunityAsamalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teklifler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeklifNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kdvoran = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Kdvtutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Toplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Net = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    GecerlilikBitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teklifler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teklifler_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teklifler_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaturaKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Miktar = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturaKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturaKalemleri_Faturalar_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaturaKalemleri_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeklifKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeklifId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Miktar = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifKalemleri_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeklifKalemleri_Teklifler_TeklifId",
                        column: x => x.TeklifId,
                        principalTable: "Teklifler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "AdSoyad", "CreateDate", "DeleteDate", "Eposta", "IsActive", "ProfilResmiUrl", "Rol", "Sifre", "Telefon", "UpdateDate" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Sistem Yöneticisi", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@turksoft.local", true, null, "Admin", "Admin!12345", "+90 555 000 0000", null });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappGonderimler_CreateDate",
                table: "WhatsappGonderimler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappGonderimler_IsActive",
                table: "WhatsappGonderimler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappAyarlar_CreateDate",
                table: "WhatsappAyarlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappAyarlar_IsActive",
                table: "WhatsappAyarlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyatlar_CreateDate",
                table: "UrunFiyatlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyatlar_IsActive",
                table: "UrunFiyatlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyatlar_PaketId_GecerlilikBaslangic_GecerlilikBitis",
                table: "UrunFiyatlar",
                columns: new[] { "PaketId", "GecerlilikBaslangic", "GecerlilikBitis" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsGonderimler_CreateDate",
                table: "SmsGonderimler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGonderimler_IsActive",
                table: "SmsGonderimler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SmsAyarlar_CreateDate",
                table: "SmsAyarlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsAyarlar_IsActive",
                table: "SmsAyarlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Paketler_CreateDate",
                table: "Paketler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Paketler_IsActive",
                table: "Paketler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavirler_BayiId",
                table: "MaliMusavirler",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavirler_CreateDate",
                table: "MaliMusavirler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavirler_IsActive",
                table: "MaliMusavirler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MailGonderimler_CreateDate",
                table: "MailGonderimler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MailGonderimler_IsActive",
                table: "MailGonderimler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MailAyarlar_CreateDate",
                table: "MailAyarlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MailAyarlar_Eposta_SmtpServer",
                table: "MailAyarlar",
                columns: new[] { "Eposta", "SmtpServer" });

            migrationBuilder.CreateIndex(
                name: "IX_MailAyarlar_IsActive",
                table: "MailAyarlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Luca_CreateDate",
                table: "Luca",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Luca_IsActive",
                table: "Luca",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Loglar_CreateDate",
                table: "Loglar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Loglar_IsActive",
                table: "Loglar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Lisanslar_BaslangicTarihi_BitisTarihi",
                table: "Lisanslar",
                columns: new[] { "BaslangicTarihi", "BitisTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Lisanslar_CreateDate",
                table: "Lisanslar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lisanslar_IsActive",
                table: "Lisanslar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdetler_CreateDate",
                table: "LisansAdetler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdetler_IsActive",
                table: "LisansAdetler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_CreateDate",
                table: "Kullanicilar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_IsActive",
                table: "Kullanicilar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_CreateDate",
                table: "KeyAccount",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_IsActive",
                table: "KeyAccount",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_BayiId",
                table: "Firmalar",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_CreateDate",
                table: "Firmalar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_IsActive",
                table: "Firmalar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UrunTipleri_CreateDate",
                table: "UrunTipleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_UrunTipleri_IsActive",
                table: "UrunTipleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtamalari_AktiviteId_KullaniciId",
                table: "AktiviteAtamalari",
                columns: new[] { "AktiviteId", "KullaniciId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtamalari_CreateDate",
                table: "AktiviteAtamalari",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtamalari_IsActive",
                table: "AktiviteAtamalari",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtamalari_KullaniciId",
                table: "AktiviteAtamalari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Aktiviteler_BayiId_Tur_PlanlananTarih",
                table: "Aktiviteler",
                columns: new[] { "BayiId", "Tur", "PlanlananTarih" });

            migrationBuilder.CreateIndex(
                name: "IX_Aktiviteler_CreateDate",
                table: "Aktiviteler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Aktiviteler_IlgiliKullaniciId",
                table: "Aktiviteler",
                column: "IlgiliKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Aktiviteler_IsActive",
                table: "Aktiviteler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Bayi_CreateDate",
                table: "Bayi",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bayi_IsActive",
                table: "Bayi",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Bayi_Kod",
                table: "Bayi",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bayi_OlusturanKullaniciId",
                table: "Bayi",
                column: "OlusturanKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariHareketleri_BayiCariId_IslemTarihi",
                table: "BayiCariHareketleri",
                columns: new[] { "BayiCariId", "IslemTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariHareketleri_CreateDate",
                table: "BayiCariHareketleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariHareketleri_IsActive",
                table: "BayiCariHareketleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariler_BayiId",
                table: "BayiCariler",
                column: "BayiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariler_CreateDate",
                table: "BayiCariler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariler_IsActive",
                table: "BayiCariler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BayiFirma_BayiId",
                table: "BayiFirma",
                column: "BayiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BayiFirma_CreateDate",
                table: "BayiFirma",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_BayiFirma_IsActive",
                table: "BayiFirma",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BayiKomisyonTarife_BayiId_PaketId_Baslangic_Bitis",
                table: "BayiKomisyonTarife",
                columns: new[] { "BayiId", "PaketId", "Baslangic", "Bitis" });

            migrationBuilder.CreateIndex(
                name: "IX_BayiKomisyonTarife_CreateDate",
                table: "BayiKomisyonTarife",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_BayiKomisyonTarife_IsActive",
                table: "BayiKomisyonTarife",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BayiKomisyonTarife_PaketId",
                table: "BayiKomisyonTarife",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEkleri_CreateDate",
                table: "DosyaEkleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEkleri_IlgiliId_IlgiliTip",
                table: "DosyaEkleri",
                columns: new[] { "IlgiliId", "IlgiliTip" });

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEkleri_IsActive",
                table: "DosyaEkleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EntegrasyonHesabi_CreateDate",
                table: "EntegrasyonHesabi",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_EntegrasyonHesabi_FirmaId",
                table: "EntegrasyonHesabi",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntegrasyonHesabi_IsActive",
                table: "EntegrasyonHesabi",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EntegrasyonHesabi_MaliMusavirId_FirmaId_SistemTipi",
                table: "EntegrasyonHesabi",
                columns: new[] { "MaliMusavirId", "FirmaId", "SistemTipi" });

            migrationBuilder.CreateIndex(
                name: "IX_EtiketIliskileri_CreateDate",
                table: "EtiketIliskileri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_EtiketIliskileri_EtiketId_IlgiliId_IlgiliTip",
                table: "EtiketIliskileri",
                columns: new[] { "EtiketId", "IlgiliId", "IlgiliTip" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtiketIliskileri_IsActive",
                table: "EtiketIliskileri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Etiketler_Ad",
                table: "Etiketler",
                column: "Ad");

            migrationBuilder.CreateIndex(
                name: "IX_Etiketler_CreateDate",
                table: "Etiketler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Etiketler_IsActive",
                table: "Etiketler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalemleri_CreateDate",
                table: "FaturaKalemleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalemleri_FaturaId",
                table: "FaturaKalemleri",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalemleri_IsActive",
                table: "FaturaKalemleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalemleri_PaketId",
                table: "FaturaKalemleri",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_BayiId",
                table: "Faturalar",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_CreateDate",
                table: "Faturalar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_FaturaNo",
                table: "Faturalar",
                column: "FaturaNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_FirmaId",
                table: "Faturalar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_IsActive",
                table: "Faturalar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_SatisId",
                table: "Faturalar",
                column: "SatisId");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListeleri_BayiId_Baslangic_Bitis",
                table: "FiyatListeleri",
                columns: new[] { "BayiId", "Baslangic", "Bitis" });

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListeleri_CreateDate",
                table: "FiyatListeleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListeleri_IsActive",
                table: "FiyatListeleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListeleri_Kod",
                table: "FiyatListeleri",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalemleri_CreateDate",
                table: "FiyatListesiKalemleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalemleri_FiyatListesiId_PaketId",
                table: "FiyatListesiKalemleri",
                columns: new[] { "FiyatListesiId", "PaketId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalemleri_IsActive",
                table: "FiyatListesiKalemleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalemleri_PaketId",
                table: "FiyatListesiKalemleri",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_IletisimKisileri_CreateDate",
                table: "IletisimKisileri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_IletisimKisileri_FirmaId_AdSoyad",
                table: "IletisimKisileri",
                columns: new[] { "FirmaId", "AdSoyad" });

            migrationBuilder.CreateIndex(
                name: "IX_IletisimKisileri_IsActive",
                table: "IletisimKisileri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonOdemePlanlari_BayiId_DonemYil_DonemAy",
                table: "KomisyonOdemePlanlari",
                columns: new[] { "BayiId", "DonemYil", "DonemAy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonOdemePlanlari_CreateDate",
                table: "KomisyonOdemePlanlari",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonOdemePlanlari_IsActive",
                table: "KomisyonOdemePlanlari",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kuponlar_BayiId",
                table: "Kuponlar",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Kuponlar_CreateDate",
                table: "Kuponlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kuponlar_IsActive",
                table: "Kuponlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kuponlar_Kod",
                table: "Kuponlar",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leadler_BayiId",
                table: "Leadler",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Leadler_CreateDate",
                table: "Leadler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Leadler_IsActive",
                table: "Leadler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Leadler_LeadNo",
                table: "Leadler",
                column: "LeadNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leadler_SorumluKullaniciId",
                table: "Leadler",
                column: "SorumluKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_CreateDate",
                table: "Notlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_IlgiliId_IlgiliTip",
                table: "Notlar",
                columns: new[] { "IlgiliId", "IlgiliTip" });

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_IsActive",
                table: "Notlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_OlusturanUserId",
                table: "Notlar",
                column: "OlusturanUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Odeme_CreateDate",
                table: "Odeme",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Odeme_IsActive",
                table: "Odeme",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Odeme_SaglayiciIslemNo",
                table: "Odeme",
                column: "SaglayiciIslemNo");

            migrationBuilder.CreateIndex(
                name: "IX_Odeme_SanalPosId",
                table: "Odeme",
                column: "SanalPosId");

            migrationBuilder.CreateIndex(
                name: "IX_Odeme_SatisId_OdemeTarihi_OdemeDurumu",
                table: "Odeme",
                columns: new[] { "SatisId", "OdemeTarihi", "OdemeDurumu" });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AsamaId",
                table: "Opportunities",
                column: "AsamaId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_BayiId_AsamaId_OlusturmaTarihi",
                table: "Opportunities",
                columns: new[] { "BayiId", "AsamaId", "OlusturmaTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CreateDate",
                table: "Opportunities",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_FirmaId",
                table: "Opportunities",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_FirsatNo",
                table: "Opportunities",
                column: "FirsatNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_IsActive",
                table: "Opportunities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_MaliMusavirId",
                table: "Opportunities",
                column: "MaliMusavirId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecisleri_CreateDate",
                table: "OpportunityAsamaGecisleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecisleri_FromAsamaId",
                table: "OpportunityAsamaGecisleri",
                column: "FromAsamaId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecisleri_IsActive",
                table: "OpportunityAsamaGecisleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecisleri_OpportunityId",
                table: "OpportunityAsamaGecisleri",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecisleri_ToAsamaId",
                table: "OpportunityAsamaGecisleri",
                column: "ToAsamaId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamalari_CreateDate",
                table: "OpportunityAsamalari",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamalari_IsActive",
                table: "OpportunityAsamalari",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamalari_Kod",
                table: "OpportunityAsamalari",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMesajlari_CreateDate",
                table: "OutboxMesajlari",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMesajlari_IsActive",
                table: "OutboxMesajlari",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMesajlari_Islenmis_CreateDate",
                table: "OutboxMesajlari",
                columns: new[] { "Islenmis", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PaketIskonto_BayiId",
                table: "PaketIskonto",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketIskonto_CreateDate",
                table: "PaketIskonto",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaketIskonto_IsActive",
                table: "PaketIskonto",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PaketIskonto_PaketId_BayiId_Baslangic_Bitis",
                table: "PaketIskonto",
                columns: new[] { "PaketId", "BayiId", "Baslangic", "Bitis" });

            migrationBuilder.CreateIndex(
                name: "IX_SanalPos_BayiId_Saglayici",
                table: "SanalPos",
                columns: new[] { "BayiId", "Saglayici" });

            migrationBuilder.CreateIndex(
                name: "IX_SanalPos_CreateDate",
                table: "SanalPos",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SanalPos_IsActive",
                table: "SanalPos",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Satis_BayiId_MaliMusavirId_SatisTarihi",
                table: "Satis",
                columns: new[] { "BayiId", "MaliMusavirId", "SatisTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Satis_CreateDate",
                table: "Satis",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Satis_FirmaId",
                table: "Satis",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Satis_IsActive",
                table: "Satis",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Satis_MaliMusavirId",
                table: "Satis",
                column: "MaliMusavirId");

            migrationBuilder.CreateIndex(
                name: "IX_Satis_PaketId",
                table: "Satis",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_Satis_SatisNo",
                table: "Satis",
                column: "SatisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisKalem_CreateDate",
                table: "SatisKalem",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SatisKalem_IsActive",
                table: "SatisKalem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SatisKalem_PaketId",
                table: "SatisKalem",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisKalem_SatisId",
                table: "SatisKalem",
                column: "SatisId");

            migrationBuilder.CreateIndex(
                name: "IX_SistemBildirimleri_CreateDate",
                table: "SistemBildirimleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SistemBildirimleri_IsActive",
                table: "SistemBildirimleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SistemBildirimleri_Kanal_PlanlananTarih_Durum",
                table: "SistemBildirimleri",
                columns: new[] { "Kanal", "PlanlananTarih", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalemleri_CreateDate",
                table: "TeklifKalemleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalemleri_IsActive",
                table: "TeklifKalemleri",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalemleri_PaketId",
                table: "TeklifKalemleri",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalemleri_TeklifId",
                table: "TeklifKalemleri",
                column: "TeklifId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_CreateDate",
                table: "Teklifler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_IsActive",
                table: "Teklifler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_OpportunityId",
                table: "Teklifler",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_PaketId",
                table: "Teklifler",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_TeklifNo",
                table: "Teklifler",
                column: "TeklifNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VergiOranlari_CreateDate",
                table: "VergiOranlari",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_VergiOranlari_IsActive",
                table: "VergiOranlari",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_VergiOranlari_Kod",
                table: "VergiOranlari",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookAbonelikleri_BayiId_Event",
                table: "WebhookAbonelikleri",
                columns: new[] { "BayiId", "Event" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookAbonelikleri_CreateDate",
                table: "WebhookAbonelikleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookAbonelikleri_IsActive",
                table: "WebhookAbonelikleri",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_Firmalar_Bayi_BayiId",
                table: "Firmalar",
                column: "BayiId",
                principalTable: "Bayi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Firmalar_MaliMusavirler_MaliMusavirId",
                table: "Firmalar",
                column: "MaliMusavirId",
                principalTable: "MaliMusavirler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LisansAdetler_Lisanslar_LisansId",
                table: "LisansAdetler",
                column: "LisansId",
                principalTable: "Lisanslar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lisanslar_Satis_SatisId",
                table: "Lisanslar",
                column: "SatisId",
                principalTable: "Satis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaliMusavirler_Bayi_BayiId",
                table: "MaliMusavirler",
                column: "BayiId",
                principalTable: "Bayi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Paketler_UrunTipleri_UrunTipiId",
                table: "Paketler",
                column: "UrunTipiId",
                principalTable: "UrunTipleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UrunFiyatlar_Paketler_PaketId",
                table: "UrunFiyatlar",
                column: "PaketId",
                principalTable: "Paketler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Firmalar_Bayi_BayiId",
                table: "Firmalar");

            migrationBuilder.DropForeignKey(
                name: "FK_Firmalar_MaliMusavirler_MaliMusavirId",
                table: "Firmalar");

            migrationBuilder.DropForeignKey(
                name: "FK_LisansAdetler_Lisanslar_LisansId",
                table: "LisansAdetler");

            migrationBuilder.DropForeignKey(
                name: "FK_Lisanslar_Satis_SatisId",
                table: "Lisanslar");

            migrationBuilder.DropForeignKey(
                name: "FK_MaliMusavirler_Bayi_BayiId",
                table: "MaliMusavirler");

            migrationBuilder.DropForeignKey(
                name: "FK_Paketler_UrunTipleri_UrunTipiId",
                table: "Paketler");

            migrationBuilder.DropForeignKey(
                name: "FK_UrunFiyatlar_Paketler_PaketId",
                table: "UrunFiyatlar");

            migrationBuilder.DropTable(
                name: "AktiviteAtamalari");

            migrationBuilder.DropTable(
                name: "BayiCariHareketleri");

            migrationBuilder.DropTable(
                name: "BayiFirma");

            migrationBuilder.DropTable(
                name: "BayiKomisyonTarife");

            migrationBuilder.DropTable(
                name: "DosyaEkleri");

            migrationBuilder.DropTable(
                name: "EntegrasyonHesabi");

            migrationBuilder.DropTable(
                name: "EtiketIliskileri");

            migrationBuilder.DropTable(
                name: "FaturaKalemleri");

            migrationBuilder.DropTable(
                name: "FiyatListesiKalemleri");

            migrationBuilder.DropTable(
                name: "IletisimKisileri");

            migrationBuilder.DropTable(
                name: "KomisyonOdemePlanlari");

            migrationBuilder.DropTable(
                name: "Kuponlar");

            migrationBuilder.DropTable(
                name: "Leadler");

            migrationBuilder.DropTable(
                name: "Notlar");

            migrationBuilder.DropTable(
                name: "Odeme");

            migrationBuilder.DropTable(
                name: "OpportunityAsamaGecisleri");

            migrationBuilder.DropTable(
                name: "OutboxMesajlari");

            migrationBuilder.DropTable(
                name: "PaketIskonto");

            migrationBuilder.DropTable(
                name: "SatisKalem");

            migrationBuilder.DropTable(
                name: "SistemBildirimleri");

            migrationBuilder.DropTable(
                name: "TeklifKalemleri");

            migrationBuilder.DropTable(
                name: "VergiOranlari");

            migrationBuilder.DropTable(
                name: "WebhookAbonelikleri");

            migrationBuilder.DropTable(
                name: "Aktiviteler");

            migrationBuilder.DropTable(
                name: "BayiCariler");

            migrationBuilder.DropTable(
                name: "Etiketler");

            migrationBuilder.DropTable(
                name: "Faturalar");

            migrationBuilder.DropTable(
                name: "FiyatListeleri");

            migrationBuilder.DropTable(
                name: "SanalPos");

            migrationBuilder.DropTable(
                name: "Teklifler");

            migrationBuilder.DropTable(
                name: "Satis");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "Bayi");

            migrationBuilder.DropTable(
                name: "OpportunityAsamalari");

            migrationBuilder.DropIndex(
                name: "IX_WhatsappGonderimler_CreateDate",
                table: "WhatsappGonderimler");

            migrationBuilder.DropIndex(
                name: "IX_WhatsappGonderimler_IsActive",
                table: "WhatsappGonderimler");

            migrationBuilder.DropIndex(
                name: "IX_WhatsappAyarlar_CreateDate",
                table: "WhatsappAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_WhatsappAyarlar_IsActive",
                table: "WhatsappAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_UrunFiyatlar_CreateDate",
                table: "UrunFiyatlar");

            migrationBuilder.DropIndex(
                name: "IX_UrunFiyatlar_IsActive",
                table: "UrunFiyatlar");

            migrationBuilder.DropIndex(
                name: "IX_UrunFiyatlar_PaketId_GecerlilikBaslangic_GecerlilikBitis",
                table: "UrunFiyatlar");

            migrationBuilder.DropIndex(
                name: "IX_SmsGonderimler_CreateDate",
                table: "SmsGonderimler");

            migrationBuilder.DropIndex(
                name: "IX_SmsGonderimler_IsActive",
                table: "SmsGonderimler");

            migrationBuilder.DropIndex(
                name: "IX_SmsAyarlar_CreateDate",
                table: "SmsAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_SmsAyarlar_IsActive",
                table: "SmsAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_Paketler_CreateDate",
                table: "Paketler");

            migrationBuilder.DropIndex(
                name: "IX_Paketler_IsActive",
                table: "Paketler");

            migrationBuilder.DropIndex(
                name: "IX_MaliMusavirler_BayiId",
                table: "MaliMusavirler");

            migrationBuilder.DropIndex(
                name: "IX_MaliMusavirler_CreateDate",
                table: "MaliMusavirler");

            migrationBuilder.DropIndex(
                name: "IX_MaliMusavirler_IsActive",
                table: "MaliMusavirler");

            migrationBuilder.DropIndex(
                name: "IX_MailGonderimler_CreateDate",
                table: "MailGonderimler");

            migrationBuilder.DropIndex(
                name: "IX_MailGonderimler_IsActive",
                table: "MailGonderimler");

            migrationBuilder.DropIndex(
                name: "IX_MailAyarlar_CreateDate",
                table: "MailAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_MailAyarlar_Eposta_SmtpServer",
                table: "MailAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_MailAyarlar_IsActive",
                table: "MailAyarlar");

            migrationBuilder.DropIndex(
                name: "IX_Luca_CreateDate",
                table: "Luca");

            migrationBuilder.DropIndex(
                name: "IX_Luca_IsActive",
                table: "Luca");

            migrationBuilder.DropIndex(
                name: "IX_Loglar_CreateDate",
                table: "Loglar");

            migrationBuilder.DropIndex(
                name: "IX_Loglar_IsActive",
                table: "Loglar");

            migrationBuilder.DropIndex(
                name: "IX_Lisanslar_BaslangicTarihi_BitisTarihi",
                table: "Lisanslar");

            migrationBuilder.DropIndex(
                name: "IX_Lisanslar_CreateDate",
                table: "Lisanslar");

            migrationBuilder.DropIndex(
                name: "IX_Lisanslar_IsActive",
                table: "Lisanslar");

            migrationBuilder.DropIndex(
                name: "IX_LisansAdetler_CreateDate",
                table: "LisansAdetler");

            migrationBuilder.DropIndex(
                name: "IX_LisansAdetler_IsActive",
                table: "LisansAdetler");

            migrationBuilder.DropIndex(
                name: "IX_Kullanicilar_CreateDate",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_Kullanicilar_IsActive",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_KeyAccount_CreateDate",
                table: "KeyAccount");

            migrationBuilder.DropIndex(
                name: "IX_KeyAccount_IsActive",
                table: "KeyAccount");

            migrationBuilder.DropIndex(
                name: "IX_Firmalar_BayiId",
                table: "Firmalar");

            migrationBuilder.DropIndex(
                name: "IX_Firmalar_CreateDate",
                table: "Firmalar");

            migrationBuilder.DropIndex(
                name: "IX_Firmalar_IsActive",
                table: "Firmalar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UrunTipleri",
                table: "UrunTipleri");

            migrationBuilder.DropIndex(
                name: "IX_UrunTipleri_CreateDate",
                table: "UrunTipleri");

            migrationBuilder.DropIndex(
                name: "IX_UrunTipleri_IsActive",
                table: "UrunTipleri");

            migrationBuilder.DeleteData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WhatsappGonderimler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WhatsappAyarlar");

            migrationBuilder.DropColumn(
                name: "GecerlilikBitis",
                table: "UrunFiyatlar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UrunFiyatlar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SmsGonderimler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SmsAyarlar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Paketler");

            migrationBuilder.DropColumn(
                name: "BayiId",
                table: "MaliMusavirler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MaliMusavirler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MailGonderimler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MailAyarlar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Luca");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Loglar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Lisanslar");

            migrationBuilder.DropColumn(
                name: "Limit",
                table: "LisansAdetler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LisansAdetler");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "KeyAccount");

            migrationBuilder.DropColumn(
                name: "BayiId",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UrunTipleri");

            migrationBuilder.RenameTable(
                name: "UrunTipleri",
                newName: "UrunTipiler");

            migrationBuilder.RenameColumn(
                name: "PaketId",
                table: "UrunFiyatlar",
                newName: "UrunTipiId");

            migrationBuilder.RenameColumn(
                name: "GecerlilikBaslangic",
                table: "UrunFiyatlar",
                newName: "GecerlilikTarihi");

            migrationBuilder.RenameColumn(
                name: "YenilendiMi",
                table: "Lisanslar",
                newName: "AktifMi");

            migrationBuilder.RenameColumn(
                name: "SatisId",
                table: "Lisanslar",
                newName: "MaliMusavirId");

            migrationBuilder.RenameIndex(
                name: "IX_Lisanslar_SatisId",
                table: "Lisanslar",
                newName: "IX_Lisanslar_MaliMusavirId");

            migrationBuilder.RenameColumn(
                name: "LisansId",
                table: "LisansAdetler",
                newName: "MaliMusavirId");

            migrationBuilder.RenameIndex(
                name: "IX_LisansAdetler_LisansId",
                table: "LisansAdetler",
                newName: "IX_LisansAdetler_MaliMusavirId");

            migrationBuilder.AlterColumn<bool>(
                name: "SSLKullan",
                table: "MailAyarlar",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Port",
                table: "MailAyarlar",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 587);

            migrationBuilder.AddColumn<Guid>(
                name: "KullaniciId",
                table: "Loglar",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaliMusavirId",
                table: "Loglar",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCihazSayisi",
                table: "LisansAdetler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "MaliMusavirId",
                table: "Firmalar",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UrunTipiler",
                table: "UrunTipiler",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyatlar_UrunTipiId",
                table: "UrunFiyatlar",
                column: "UrunTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_Loglar_KullaniciId",
                table: "Loglar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Loglar_MaliMusavirId",
                table: "Loglar",
                column: "MaliMusavirId");

            migrationBuilder.AddForeignKey(
                name: "FK_Firmalar_MaliMusavirler_MaliMusavirId",
                table: "Firmalar",
                column: "MaliMusavirId",
                principalTable: "MaliMusavirler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LisansAdetler_MaliMusavirler_MaliMusavirId",
                table: "LisansAdetler",
                column: "MaliMusavirId",
                principalTable: "MaliMusavirler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lisanslar_MaliMusavirler_MaliMusavirId",
                table: "Lisanslar",
                column: "MaliMusavirId",
                principalTable: "MaliMusavirler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Loglar_Kullanicilar_KullaniciId",
                table: "Loglar",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loglar_MaliMusavirler_MaliMusavirId",
                table: "Loglar",
                column: "MaliMusavirId",
                principalTable: "MaliMusavirler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Paketler_UrunTipiler_UrunTipiId",
                table: "Paketler",
                column: "UrunTipiId",
                principalTable: "UrunTipiler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UrunFiyatlar_UrunTipiler_UrunTipiId",
                table: "UrunFiyatlar",
                column: "UrunTipiId",
                principalTable: "UrunTipiler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
