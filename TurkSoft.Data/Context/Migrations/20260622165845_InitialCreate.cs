using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TurkSoft.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DosyaEkleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Etiketler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "KeyAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfilResmiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturanKullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Loglar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Islem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IpAdres = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tarayici = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Luca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UyeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Parola = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Luca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailAyarlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SmtpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false, defaultValue: 587),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SSLKullan = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailAyarlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailGonderimler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Alici = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Konu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasariliMi = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailGonderimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityAsamalari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "SmsAyarlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ApiKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GondericiAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsAyarlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsGonderimler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AliciNumara = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mesaj = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BasariliMi = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsGonderimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunTipleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunTipleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VergiOranlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "WhatsappAyarlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ApiUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Numara = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsappAyarlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsappGonderimler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AliciNumara = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mesaj = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BasariliMi = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsappGonderimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EtiketIliskileri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Bayi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Notlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Paketler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UrunTipiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paketler_UrunTipleri_UrunTipiId",
                        column: x => x.UrunTipiId,
                        principalTable: "UrunTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aktiviteler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "FiyatListeleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Kullanici_Bayi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Kuponlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "MaliMusavirler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VergiNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    TCKN = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaliMusavirler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaliMusavirler_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SanalPos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "WebhookAbonelikleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "BayiKomisyonTarife",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "PaketIskonto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "UrunFiyatlar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PaketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GecerlilikBaslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GecerlilikBitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunFiyatlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunFiyatlar_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AktiviteAtamalari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Firmalar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FirmaAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VergiNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    YetkiliAdSoyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaliMusavirId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BayiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Firmalar_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Firmalar_MaliMusavirler_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "Kullanici_MaliMusavir",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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

            migrationBuilder.CreateTable(
                name: "EntegrasyonHesabi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "IletisimKisileri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Kullanici_Firma",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Satis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "OpportunityAsamaGecisleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Faturalar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "Lisanslar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LisansAnahtari = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YenilendiMi = table.Column<bool>(type: "bit", nullable: false),
                    SatisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lisanslar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lisanslar_Satis_SatisId",
                        column: x => x.SatisId,
                        principalTable: "Satis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Odeme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "TeklifKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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

            migrationBuilder.CreateTable(
                name: "FaturaKalemleri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
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
                name: "LisansAdetler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    LisansId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KuruluCihazSayisi = table.Column<int>(type: "int", nullable: false),
                    Limit = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LisansAdetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LisansAdetler_Lisanslar_LisansId",
                        column: x => x.LisansId,
                        principalTable: "Lisanslar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "AdSoyad", "CreateDate", "DeleteDate", "Eposta", "IsActive", "OlusturanKullaniciId", "ProfilResmiUrl", "Rol", "Sifre", "Telefon", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Sistem Yöneticisi", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@turksoft.local", true, null, null, "Admin", "Admin!12345", "+90 555 000 0000", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Demo Bayi Kullanıcı", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "bayi@turksoft.local", true, null, null, "Bayi", "Bayi!12345", "+90 555 000 0001", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Demo MM Kullanıcı", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "mm@turksoft.local", true, null, null, "MaliMusavir", "Mali!12345", "+90 555 000 0002", null }
                });

            migrationBuilder.InsertData(
                table: "OpportunityAsamalari",
                columns: new[] { "Id", "Ad", "CreateDate", "DeleteDate", "IsActive", "Kod", "OlasilikYuzde", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("bb000001-0000-0000-0000-000000000001"), "Yeni Lead", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "NEW", 10m, null },
                    { new Guid("bb000001-0000-0000-0000-000000000002"), "Nitelendirme", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "QUAL", 25m, null },
                    { new Guid("bb000001-0000-0000-0000-000000000003"), "Teklif Verildi", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PROP", 50m, null },
                    { new Guid("bb000001-0000-0000-0000-000000000004"), "Müzakere", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "NEG", 75m, null },
                    { new Guid("bb000001-0000-0000-0000-000000000005"), "Kazanıldı", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "WON", 100m, null },
                    { new Guid("bb000001-0000-0000-0000-000000000006"), "Kaybedildi", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "LOST", 0m, null }
                });

            migrationBuilder.InsertData(
                table: "UrunTipleri",
                columns: new[] { "Id", "Aciklama", "Ad", "CreateDate", "DeleteDate", "IsActive", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("cc000001-0000-0000-0000-000000000001"), "ERP/Muhasebe yazılım lisansları", "Yazılım Lisansı", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null },
                    { new Guid("cc000001-0000-0000-0000-000000000002"), "Danışmanlık ve uygulama hizmetleri", "Hizmet", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null },
                    { new Guid("cc000001-0000-0000-0000-000000000003"), "Yıllık teknik destek paketleri", "Teknik Destek", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null }
                });

            migrationBuilder.InsertData(
                table: "VergiOranlari",
                columns: new[] { "Id", "CreateDate", "DeleteDate", "IsActive", "Kod", "Oran", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("aa000001-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "KDV0", 0m, null },
                    { new Guid("aa000001-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "KDV1", 1m, null },
                    { new Guid("aa000001-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "KDV10", 10m, null },
                    { new Guid("aa000001-0000-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "KDV20", 20m, null }
                });

            migrationBuilder.InsertData(
                table: "Bayi",
                columns: new[] { "Id", "CreateDate", "DeleteDate", "Eposta", "IsActive", "Kod", "OlusturanKullaniciId", "Telefon", "Unvan", "UpdateDate" },
                values: new object[] { new Guid("ee000001-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "bayi@demo.local", true, "DEMO-BAYI-001", new Guid("11111111-1111-1111-1111-111111111111"), "+90 212 000 0001", "Demo Bayi A.Ş.", null });

            migrationBuilder.InsertData(
                table: "Paketler",
                columns: new[] { "Id", "Aciklama", "Ad", "CreateDate", "DeleteDate", "IsActive", "UpdateDate", "UrunTipiId" },
                values: new object[,]
                {
                    { new Guid("dd000001-0000-0000-0000-000000000001"), "1 kullanıcı, temel muhasebe modülleri", "Starter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, new Guid("cc000001-0000-0000-0000-000000000001") },
                    { new Guid("dd000001-0000-0000-0000-000000000002"), "5 kullanıcı, tüm ERP modülleri", "Professional", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, new Guid("cc000001-0000-0000-0000-000000000001") },
                    { new Guid("dd000001-0000-0000-0000-000000000003"), "Sınırsız kullanıcı, özel entegrasyonlar", "Enterprise", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, new Guid("cc000001-0000-0000-0000-000000000001") },
                    { new Guid("dd000001-0000-0000-0000-000000000004"), "İş günleri 09-18 telefon/e-posta destek", "Destek Temel", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, new Guid("cc000001-0000-0000-0000-000000000003") },
                    { new Guid("dd000001-0000-0000-0000-000000000005"), "7/24 öncelikli destek + yerinde servis", "Destek Premium", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, new Guid("cc000001-0000-0000-0000-000000000003") }
                });

            migrationBuilder.InsertData(
                table: "Kullanici_Bayi",
                columns: new[] { "Id", "AtananRol", "BaslangicTarihi", "BayiId", "BitisTarihi", "CreateDate", "DeleteDate", "IsActive", "IsPrimary", "KullaniciId", "UpdateDate" },
                values: new object[] { new Guid("ff000001-0000-0000-0000-000000000001"), null, null, new Guid("ee000001-0000-0000-0000-000000000001"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("22222222-2222-2222-2222-222222222222"), null });

            migrationBuilder.InsertData(
                table: "MaliMusavirler",
                columns: new[] { "Id", "AdSoyad", "BayiId", "CreateDate", "DeleteDate", "Eposta", "IsActive", "TCKN", "Telefon", "Unvan", "UpdateDate", "VergiNo" },
                values: new object[] { new Guid("a0000007-0000-0000-0000-000000000001"), "Demo Mali Müşavir", new Guid("ee000001-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "mm@demo.local", true, "12345678901", "+90 212 000 0002", "SMMM", null, "1234567890" });

            migrationBuilder.InsertData(
                table: "Firmalar",
                columns: new[] { "Id", "Adres", "BayiId", "CreateDate", "DeleteDate", "Eposta", "FirmaAdi", "IsActive", "MaliMusavirId", "Telefon", "UpdateDate", "VergiNo", "YetkiliAdSoyad" },
                values: new object[] { new Guid("a0000009-0000-0000-0000-000000000001"), "Levent, İstanbul", new Guid("ee000001-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "firma@demo.local", "Demo Şirket Ltd. Şti.", true, new Guid("a0000007-0000-0000-0000-000000000001"), "+90 212 000 0003", null, "9876543210", "Demo Yetkili" });

            migrationBuilder.InsertData(
                table: "Kullanici_MaliMusavir",
                columns: new[] { "Id", "AtananRol", "BaslangicTarihi", "BitisTarihi", "CreateDate", "DeleteDate", "IsActive", "IsPrimary", "KullaniciId", "MaliMusavirId", "UpdateDate" },
                values: new object[] { new Guid("a0000008-0000-0000-0000-000000000001"), null, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, new Guid("33333333-3333-3333-3333-333333333333"), new Guid("a0000007-0000-0000-0000-000000000001"), null });

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
                column: "Kod");

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
                column: "FaturaNo");

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
                name: "IX_Firmalar_MaliMusavirId",
                table: "Firmalar",
                column: "MaliMusavirId");

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
                column: "Kod");

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
                name: "IX_KeyAccount_CreateDate",
                table: "KeyAccount",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_IsActive",
                table: "KeyAccount",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_Kod",
                table: "KeyAccount",
                column: "Kod");

            migrationBuilder.CreateIndex(
                name: "IX_KeyAccount_MaliMusavir_MaliMusavirId",
                table: "KeyAccount_MaliMusavir",
                column: "MaliMusavirId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_CreateDate",
                table: "Kullanicilar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Eposta",
                table: "Kullanicilar",
                column: "Eposta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_IsActive",
                table: "Kullanicilar",
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
                column: "Kod");

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
                column: "LeadNo");

            migrationBuilder.CreateIndex(
                name: "IX_Leadler_SorumluKullaniciId",
                table: "Leadler",
                column: "SorumluKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdetler_CreateDate",
                table: "LisansAdetler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdetler_IsActive",
                table: "LisansAdetler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdetler_LisansId",
                table: "LisansAdetler",
                column: "LisansId");

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
                name: "IX_Lisanslar_SatisId",
                table: "Lisanslar",
                column: "SatisId");

            migrationBuilder.CreateIndex(
                name: "IX_Loglar_CreateDate",
                table: "Loglar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Loglar_IsActive",
                table: "Loglar",
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
                name: "IX_Luca_MaliMusavir_MaliMusavirId",
                table: "Luca_MaliMusavir",
                column: "MaliMusavirId");

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
                name: "IX_MailGonderimler_CreateDate",
                table: "MailGonderimler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MailGonderimler_IsActive",
                table: "MailGonderimler",
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
                column: "FirsatNo");

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
                column: "Kod");

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
                name: "IX_Paketler_CreateDate",
                table: "Paketler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Paketler_IsActive",
                table: "Paketler",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Paketler_UrunTipiId",
                table: "Paketler",
                column: "UrunTipiId");

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
                column: "SatisNo");

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
                name: "IX_SmsAyarlar_CreateDate",
                table: "SmsAyarlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsAyarlar_IsActive",
                table: "SmsAyarlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGonderimler_CreateDate",
                table: "SmsGonderimler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGonderimler_IsActive",
                table: "SmsGonderimler",
                column: "IsActive");

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
                column: "TeklifNo");

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
                name: "IX_UrunTipleri_CreateDate",
                table: "UrunTipleri",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_UrunTipleri_IsActive",
                table: "UrunTipleri",
                column: "IsActive");

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
                column: "Kod");

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

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappAyarlar_CreateDate",
                table: "WhatsappAyarlar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappAyarlar_IsActive",
                table: "WhatsappAyarlar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappGonderimler_CreateDate",
                table: "WhatsappGonderimler",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappGonderimler_IsActive",
                table: "WhatsappGonderimler",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "KeyAccount_MaliMusavir");

            migrationBuilder.DropTable(
                name: "KomisyonOdemePlanlari");

            migrationBuilder.DropTable(
                name: "Kullanici_Bayi");

            migrationBuilder.DropTable(
                name: "Kullanici_Firma");

            migrationBuilder.DropTable(
                name: "Kullanici_MaliMusavir");

            migrationBuilder.DropTable(
                name: "Kuponlar");

            migrationBuilder.DropTable(
                name: "Leadler");

            migrationBuilder.DropTable(
                name: "LisansAdetler");

            migrationBuilder.DropTable(
                name: "Loglar");

            migrationBuilder.DropTable(
                name: "Luca_MaliMusavir");

            migrationBuilder.DropTable(
                name: "MailAyarlar");

            migrationBuilder.DropTable(
                name: "MailGonderimler");

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
                name: "SmsAyarlar");

            migrationBuilder.DropTable(
                name: "SmsGonderimler");

            migrationBuilder.DropTable(
                name: "TeklifKalemleri");

            migrationBuilder.DropTable(
                name: "UrunFiyatlar");

            migrationBuilder.DropTable(
                name: "VergiOranlari");

            migrationBuilder.DropTable(
                name: "WebhookAbonelikleri");

            migrationBuilder.DropTable(
                name: "WhatsappAyarlar");

            migrationBuilder.DropTable(
                name: "WhatsappGonderimler");

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
                name: "KeyAccount");

            migrationBuilder.DropTable(
                name: "Lisanslar");

            migrationBuilder.DropTable(
                name: "Luca");

            migrationBuilder.DropTable(
                name: "SanalPos");

            migrationBuilder.DropTable(
                name: "Teklifler");

            migrationBuilder.DropTable(
                name: "Satis");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "Paketler");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "OpportunityAsamalari");

            migrationBuilder.DropTable(
                name: "UrunTipleri");

            migrationBuilder.DropTable(
                name: "MaliMusavirler");

            migrationBuilder.DropTable(
                name: "Bayi");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
