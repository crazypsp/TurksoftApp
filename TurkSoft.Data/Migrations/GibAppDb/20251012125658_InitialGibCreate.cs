using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations.GibAppDb
{
    /// <inheritdoc />
    public partial class InitialGibCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiRefreshToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRefreshToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsoCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dealer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VknTckn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxOffice = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Commission = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DosyaEk",
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
                    table.PrimaryKey("PK_DosyaEk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Etiket",
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
                    table.PrimaryKey("PK_Etiket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Buying = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Selling = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InfCode",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Solution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Solution2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsExport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GtipNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowCodeNo = table.Column<short>(type: "smallint", nullable: false),
                    KapNo = table.Column<int>(type: "int", nullable: false),
                    KapPiece = table.Column<short>(type: "smallint", nullable: false),
                    KapType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShipMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShipDetail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrossKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Insurance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsExport", x => x.Id);
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
                name: "Kullanici",
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
                    table.PrimaryKey("PK_Kullanici", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
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
                    table.PrimaryKey("PK_Log", x => x.Id);
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
                name: "MailAyar",
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
                    table.PrimaryKey("PK_MailAyar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailGonderim",
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
                    table.PrimaryKey("PK_MailGonderim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityAsama",
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
                    table.PrimaryKey("PK_OpportunityAsama", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMesaj",
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
                    table.PrimaryKey("PK_OutboxMesaj", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemBildirim",
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
                    table.PrimaryKey("PK_SistemBildirim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsAyar",
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
                    table.PrimaryKey("PK_SmsAyar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsGonderim",
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
                    table.PrimaryKey("PK_SmsGonderim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tax",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunTipi",
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
                    table.PrimaryKey("PK_UrunTipi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeId = table.Column<short>(type: "smallint", nullable: false),
                    IsSuperUser = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SecureToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OtpCode = table.Column<int>(type: "int", nullable: false),
                    OtpSendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OtpVerify = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DealerId = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MailVerify = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VergiOrani",
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
                    table.PrimaryKey("PK_VergiOrani", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsappAyar",
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
                    table.PrimaryKey("PK_WhatsappAyar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsappGonderim",
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
                    table.PrimaryKey("PK_WhatsappGonderim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAccount",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAccount_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRate_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentTypeId = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_DocumentType_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EtiketIliski",
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
                    table.PrimaryKey("PK_EtiketIliski", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtiketIliski_Etiket_EtiketId",
                        column: x => x.EtiketId,
                        principalTable: "Etiket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomersGroup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomersGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomersGroup_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomersGroup_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_Bayi_Kullanici_OlusturanKullaniciId",
                        column: x => x.OlusturanKullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Not",
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
                    table.PrimaryKey("PK_Not", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Not_Kullanici_OlusturanUserId",
                        column: x => x.OlusturanUserId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchase_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Unit_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Paket",
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
                    table.PrimaryKey("PK_Paket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paket_UrunTipi_UrunTipiId",
                        column: x => x.UrunTipiId,
                        principalTable: "UrunTipi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcement_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionsMove",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealerId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    OrderIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionsMove", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionsMove_Dealer_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionsMove_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    SelectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomReport_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneralReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DesignId = table.Column<int>(type: "int", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralReport_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GibLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GibLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GibLog_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcesingReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesingReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesingReport_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Request",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Request_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Request_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTypeId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_PaymentAccount_PaymentAccountId",
                        column: x => x.PaymentAccountId,
                        principalTable: "PaymentAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payment_PaymentType_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoicesTax",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaxId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesTax", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicesTax_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicesTax_Tax_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Returns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Returns_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicesProvider",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    No = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesProvider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesProvider_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sgk",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    No = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sgk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sgk_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tourist",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tourist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tourist_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aktivite",
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
                    table.PrimaryKey("PK_Aktivite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aktivite_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aktivite_Kullanici_IlgiliKullaniciId",
                        column: x => x.IlgiliKullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BayiCari",
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
                    table.PrimaryKey("PK_BayiCari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiCari_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FiyatListesi",
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
                    table.PrimaryKey("PK_FiyatListesi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiyatListesi_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KomisyonOdemePlani",
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
                    table.PrimaryKey("PK_KomisyonOdemePlani", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KomisyonOdemePlani_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_Kullanici_Bayi_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kupon",
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
                    table.PrimaryKey("PK_Kupon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kupon_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lead",
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
                    table.PrimaryKey("PK_Lead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lead_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Kullanici_SorumluKullaniciId",
                        column: x => x.SorumluKullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaliMusavir",
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
                    table.PrimaryKey("PK_MaliMusavir", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaliMusavir_Bayi_BayiId",
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
                name: "WebhookAbonelik",
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
                    table.PrimaryKey("PK_WebhookAbonelik", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookAbonelik_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Identifiers",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Identifiers_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoicesDiscount",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Base = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicesDiscount_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicesDiscount_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoicesItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicesItem_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicesItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsCategory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemsCategory_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemsDiscount",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsDiscount_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemsDiscount_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Purchase_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stock_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stock_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BayiKomisyonTarife_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_PaketIskonto_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UrunFiyat",
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
                    table.PrimaryKey("PK_UrunFiyat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunFiyat_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAnnouncementRead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnnouncementRead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnnouncementRead_Announcement_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAnnouncementRead_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserAnnouncementRead_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoicesPayment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicesPayment_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicesPayment_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AktiviteAtama",
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
                    table.PrimaryKey("PK_AktiviteAtama", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AktiviteAtama_Aktivite_AktiviteId",
                        column: x => x.AktiviteId,
                        principalTable: "Aktivite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AktiviteAtama_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BayiCariHareket",
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
                    table.PrimaryKey("PK_BayiCariHareket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiCariHareket_BayiCari_BayiCariId",
                        column: x => x.BayiCariId,
                        principalTable: "BayiCari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FiyatListesiKalem",
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
                    table.PrimaryKey("PK_FiyatListesiKalem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiyatListesiKalem_FiyatListesi_FiyatListesiId",
                        column: x => x.FiyatListesiId,
                        principalTable: "FiyatListesi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FiyatListesiKalem_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Firma",
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
                    table.PrimaryKey("PK_Firma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Firma_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Firma_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeyAccount_MaliMusavir_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_Kullanici_MaliMusavir_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kullanici_MaliMusavir_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Luca_MaliMusavir_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<long>(type: "bigint", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovement_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_EntegrasyonHesabi_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntegrasyonHesabi_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IletisimKisi",
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
                    table.PrimaryKey("PK_IletisimKisi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IletisimKisi_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_Kullanici_Firma_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kullanici_Firma_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Opportunity",
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
                    table.PrimaryKey("PK_Opportunity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunity_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunity_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunity_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunity_OpportunityAsama_AsamaId",
                        column: x => x.AsamaId,
                        principalTable: "OpportunityAsama",
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
                        name: "FK_Satis_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Satis_MaliMusavir_MaliMusavirId",
                        column: x => x.MaliMusavirId,
                        principalTable: "MaliMusavir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Satis_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityAsamaGecis",
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
                    table.PrimaryKey("PK_OpportunityAsamaGecis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityAsamaGecis_OpportunityAsama_FromAsamaId",
                        column: x => x.FromAsamaId,
                        principalTable: "OpportunityAsama",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityAsamaGecis_OpportunityAsama_ToAsamaId",
                        column: x => x.ToAsamaId,
                        principalTable: "OpportunityAsama",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityAsamaGecis_Opportunity_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teklif",
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
                    table.PrimaryKey("PK_Teklif", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teklif_Opportunity_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teklif_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fatura",
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
                    table.PrimaryKey("PK_Fatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fatura_Bayi_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fatura_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fatura_Satis_SatisId",
                        column: x => x.SatisId,
                        principalTable: "Satis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lisans",
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
                    table.PrimaryKey("PK_Lisans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lisans_Satis_SatisId",
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
                        onDelete: ReferentialAction.Restrict);
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
                        name: "FK_SatisKalem_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SatisKalem_Satis_SatisId",
                        column: x => x.SatisId,
                        principalTable: "Satis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeklifKalem",
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
                    table.PrimaryKey("PK_TeklifKalem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifKalem_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeklifKalem_Teklif_TeklifId",
                        column: x => x.TeklifId,
                        principalTable: "Teklif",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaturaKalem",
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
                    table.PrimaryKey("PK_FaturaKalem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturaKalem_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturaKalem_Paket_PaketId",
                        column: x => x.PaketId,
                        principalTable: "Paket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LisansAdet",
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
                    table.PrimaryKey("PK_LisansAdet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LisansAdet_Lisans_LisansId",
                        column: x => x.LisansId,
                        principalTable: "Lisans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CustomerId",
                table: "Address",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Aktivite_BayiId_Tur_PlanlananTarih",
                table: "Aktivite",
                columns: new[] { "BayiId", "Tur", "PlanlananTarih" });

            migrationBuilder.CreateIndex(
                name: "IX_Aktivite_CreateDate",
                table: "Aktivite",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Aktivite_IlgiliKullaniciId",
                table: "Aktivite",
                column: "IlgiliKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Aktivite_IsActive",
                table: "Aktivite",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtama_AktiviteId_KullaniciId",
                table: "AktiviteAtama",
                columns: new[] { "AktiviteId", "KullaniciId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtama_CreateDate",
                table: "AktiviteAtama",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtama_IsActive",
                table: "AktiviteAtama",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteAtama_KullaniciId",
                table: "AktiviteAtama",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_UserId",
                table: "Announcement",
                column: "UserId");

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
                name: "IX_BayiCari_BayiId",
                table: "BayiCari",
                column: "BayiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BayiCari_CreateDate",
                table: "BayiCari",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCari_IsActive",
                table: "BayiCari",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariHareket_BayiCariId_IslemTarihi",
                table: "BayiCariHareket",
                columns: new[] { "BayiCariId", "IslemTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariHareket_CreateDate",
                table: "BayiCariHareket",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_BayiCariHareket_IsActive",
                table: "BayiCariHareket",
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
                name: "IX_City_CountryId",
                table: "City",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionsMove_DealerId",
                table: "CommissionsMove",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionsMove_UserId",
                table: "CommissionsMove",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomersGroup_CustomerId",
                table: "CustomersGroup",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomersGroup_GroupId",
                table: "CustomersGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomReport_UserId",
                table: "CustomReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentTypeId",
                table: "Document",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEk_CreateDate",
                table: "DosyaEk",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEk_IlgiliId_IlgiliTip",
                table: "DosyaEk",
                columns: new[] { "IlgiliId", "IlgiliTip" });

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEk_IsActive",
                table: "DosyaEk",
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
                name: "IX_Etiket_Ad",
                table: "Etiket",
                column: "Ad");

            migrationBuilder.CreateIndex(
                name: "IX_Etiket_CreateDate",
                table: "Etiket",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Etiket_IsActive",
                table: "Etiket",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EtiketIliski_CreateDate",
                table: "EtiketIliski",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_EtiketIliski_EtiketId_IlgiliId_IlgiliTip",
                table: "EtiketIliski",
                columns: new[] { "EtiketId", "IlgiliId", "IlgiliTip" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtiketIliski_IsActive",
                table: "EtiketIliski",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRate_CurrencyId",
                table: "ExchangeRate",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_BayiId",
                table: "Fatura",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_CreateDate",
                table: "Fatura",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_FaturaNo",
                table: "Fatura",
                column: "FaturaNo");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_FirmaId",
                table: "Fatura",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_IsActive",
                table: "Fatura",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_SatisId",
                table: "Fatura",
                column: "SatisId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalem_CreateDate",
                table: "FaturaKalem",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalem_FaturaId",
                table: "FaturaKalem",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalem_IsActive",
                table: "FaturaKalem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalem_PaketId",
                table: "FaturaKalem",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_Firma_BayiId",
                table: "Firma",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Firma_CreateDate",
                table: "Firma",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Firma_IsActive",
                table: "Firma",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Firma_MaliMusavirId",
                table: "Firma",
                column: "MaliMusavirId");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesi_BayiId_Baslangic_Bitis",
                table: "FiyatListesi",
                columns: new[] { "BayiId", "Baslangic", "Bitis" });

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesi_CreateDate",
                table: "FiyatListesi",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesi_IsActive",
                table: "FiyatListesi",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesi_Kod",
                table: "FiyatListesi",
                column: "Kod");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalem_CreateDate",
                table: "FiyatListesiKalem",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalem_FiyatListesiId_PaketId",
                table: "FiyatListesiKalem",
                columns: new[] { "FiyatListesiId", "PaketId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalem_IsActive",
                table: "FiyatListesiKalem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatListesiKalem_PaketId",
                table: "FiyatListesiKalem",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralReport_UserId",
                table: "GeneralReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GibLog_UserId",
                table: "GibLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Identifiers_ItemId",
                table: "Identifiers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IletisimKisi_CreateDate",
                table: "IletisimKisi",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_IletisimKisi_FirmaId_AdSoyad",
                table: "IletisimKisi",
                columns: new[] { "FirmaId", "AdSoyad" });

            migrationBuilder.CreateIndex(
                name: "IX_IletisimKisi_IsActive",
                table: "IletisimKisi",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CustomerId",
                table: "Invoice",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesDiscount_InvoiceId",
                table: "InvoicesDiscount",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesDiscount_ItemId",
                table: "InvoicesDiscount",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItem_InvoiceId",
                table: "InvoicesItem",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItem_ItemId",
                table: "InvoicesItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPayment_InvoiceId",
                table: "InvoicesPayment",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPayment_PaymentId",
                table: "InvoicesPayment",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesTax_InvoiceId",
                table: "InvoicesTax",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesTax_TaxId",
                table: "InvoicesTax",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_BrandId",
                table: "Item",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UnitId",
                table: "Item",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsCategory_CategoryId",
                table: "ItemsCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsCategory_ItemId",
                table: "ItemsCategory",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsDiscount_InvoiceId",
                table: "ItemsDiscount",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsDiscount_ItemId",
                table: "ItemsDiscount",
                column: "ItemId");

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
                name: "IX_KomisyonOdemePlani_BayiId_DonemYil_DonemAy",
                table: "KomisyonOdemePlani",
                columns: new[] { "BayiId", "DonemYil", "DonemAy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonOdemePlani_CreateDate",
                table: "KomisyonOdemePlani",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonOdemePlani_IsActive",
                table: "KomisyonOdemePlani",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_CreateDate",
                table: "Kullanici",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_Eposta",
                table: "Kullanici",
                column: "Eposta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_IsActive",
                table: "Kullanici",
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
                name: "IX_Kupon_BayiId",
                table: "Kupon",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Kupon_CreateDate",
                table: "Kupon",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Kupon_IsActive",
                table: "Kupon",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Kupon_Kod",
                table: "Kupon",
                column: "Kod");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_BayiId",
                table: "Lead",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_CreateDate",
                table: "Lead",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_IsActive",
                table: "Lead",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_LeadNo",
                table: "Lead",
                column: "LeadNo");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_SorumluKullaniciId",
                table: "Lead",
                column: "SorumluKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Lisans_BaslangicTarihi_BitisTarihi",
                table: "Lisans",
                columns: new[] { "BaslangicTarihi", "BitisTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Lisans_CreateDate",
                table: "Lisans",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lisans_IsActive",
                table: "Lisans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Lisans_SatisId",
                table: "Lisans",
                column: "SatisId");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdet_CreateDate",
                table: "LisansAdet",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdet_IsActive",
                table: "LisansAdet",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LisansAdet_LisansId",
                table: "LisansAdet",
                column: "LisansId");

            migrationBuilder.CreateIndex(
                name: "IX_Log_CreateDate",
                table: "Log",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Log_IsActive",
                table: "Log",
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
                name: "IX_MailAyar_CreateDate",
                table: "MailAyar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MailAyar_Eposta_SmtpServer",
                table: "MailAyar",
                columns: new[] { "Eposta", "SmtpServer" });

            migrationBuilder.CreateIndex(
                name: "IX_MailAyar_IsActive",
                table: "MailAyar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MailGonderim_CreateDate",
                table: "MailGonderim",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MailGonderim_IsActive",
                table: "MailGonderim",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavir_BayiId",
                table: "MaliMusavir",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavir_CreateDate",
                table: "MaliMusavir",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaliMusavir_IsActive",
                table: "MaliMusavir",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Not_CreateDate",
                table: "Not",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Not_IlgiliId_IlgiliTip",
                table: "Not",
                columns: new[] { "IlgiliId", "IlgiliTip" });

            migrationBuilder.CreateIndex(
                name: "IX_Not_IsActive",
                table: "Not",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Not_OlusturanUserId",
                table: "Not",
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
                name: "IX_Opportunity_AsamaId",
                table: "Opportunity",
                column: "AsamaId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_BayiId_AsamaId_OlusturmaTarihi",
                table: "Opportunity",
                columns: new[] { "BayiId", "AsamaId", "OlusturmaTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_CreateDate",
                table: "Opportunity",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_FirmaId",
                table: "Opportunity",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_FirsatNo",
                table: "Opportunity",
                column: "FirsatNo");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_IsActive",
                table: "Opportunity",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunity_MaliMusavirId",
                table: "Opportunity",
                column: "MaliMusavirId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsama_CreateDate",
                table: "OpportunityAsama",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsama_IsActive",
                table: "OpportunityAsama",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsama_Kod",
                table: "OpportunityAsama",
                column: "Kod");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecis_CreateDate",
                table: "OpportunityAsamaGecis",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecis_FromAsamaId",
                table: "OpportunityAsamaGecis",
                column: "FromAsamaId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecis_IsActive",
                table: "OpportunityAsamaGecis",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecis_OpportunityId",
                table: "OpportunityAsamaGecis",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityAsamaGecis_ToAsamaId",
                table: "OpportunityAsamaGecis",
                column: "ToAsamaId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMesaj_CreateDate",
                table: "OutboxMesaj",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMesaj_IsActive",
                table: "OutboxMesaj",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMesaj_Islenmis_CreateDate",
                table: "OutboxMesaj",
                columns: new[] { "Islenmis", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Paket_CreateDate",
                table: "Paket",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Paket_IsActive",
                table: "Paket",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Paket_UrunTipiId",
                table: "Paket",
                column: "UrunTipiId");

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
                name: "IX_Payment_PaymentAccountId",
                table: "Payment",
                column: "PaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentTypeId",
                table: "Payment",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAccount_BankId",
                table: "PaymentAccount",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesingReport_UserId",
                table: "ProcesingReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_SupplierId",
                table: "Purchase",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_ItemId",
                table: "PurchaseItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId",
                table: "PurchaseItem",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_UserId",
                table: "Request",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_UsersId",
                table: "Request",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_InvoiceId",
                table: "Returns",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId",
                table: "RolePermission",
                column: "RoleId");

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
                name: "IX_ServicesProvider_InvoiceId",
                table: "ServicesProvider",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sgk_InvoiceId",
                table: "Sgk",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SistemBildirim_CreateDate",
                table: "SistemBildirim",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SistemBildirim_IsActive",
                table: "SistemBildirim",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SistemBildirim_Kanal_PlanlananTarih_Durum",
                table: "SistemBildirim",
                columns: new[] { "Kanal", "PlanlananTarih", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsAyar_CreateDate",
                table: "SmsAyar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsAyar_IsActive",
                table: "SmsAyar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGonderim_CreateDate",
                table: "SmsGonderim",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGonderim_IsActive",
                table: "SmsGonderim",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ItemId",
                table: "Stock",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_WarehouseId",
                table: "Stock",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_StockId",
                table: "StockMovement",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklif_CreateDate",
                table: "Teklif",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Teklif_IsActive",
                table: "Teklif",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Teklif_OpportunityId",
                table: "Teklif",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklif_PaketId",
                table: "Teklif",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklif_TeklifNo",
                table: "Teklif",
                column: "TeklifNo");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalem_CreateDate",
                table: "TeklifKalem",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalem_IsActive",
                table: "TeklifKalem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalem_PaketId",
                table: "TeklifKalem",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifKalem_TeklifId",
                table: "TeklifKalem",
                column: "TeklifId");

            migrationBuilder.CreateIndex(
                name: "IX_Tourist_InvoiceId",
                table: "Tourist",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyat_CreateDate",
                table: "UrunFiyat",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyat_IsActive",
                table: "UrunFiyat",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyat_PaketId_GecerlilikBaslangic_GecerlilikBitis",
                table: "UrunFiyat",
                columns: new[] { "PaketId", "GecerlilikBaslangic", "GecerlilikBitis" });

            migrationBuilder.CreateIndex(
                name: "IX_UrunTipi_CreateDate",
                table: "UrunTipi",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_UrunTipi_IsActive",
                table: "UrunTipi",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnnouncementRead_AnnouncementId",
                table: "UserAnnouncementRead",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnnouncementRead_UserId",
                table: "UserAnnouncementRead",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnnouncementRead_UsersId",
                table: "UserAnnouncementRead",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VergiOrani_CreateDate",
                table: "VergiOrani",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_VergiOrani_IsActive",
                table: "VergiOrani",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_VergiOrani_Kod",
                table: "VergiOrani",
                column: "Kod");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookAbonelik_BayiId_Event",
                table: "WebhookAbonelik",
                columns: new[] { "BayiId", "Event" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookAbonelik_CreateDate",
                table: "WebhookAbonelik",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookAbonelik_IsActive",
                table: "WebhookAbonelik",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappAyar_CreateDate",
                table: "WhatsappAyar",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappAyar_IsActive",
                table: "WhatsappAyar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappGonderim_CreateDate",
                table: "WhatsappGonderim",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsappGonderim_IsActive",
                table: "WhatsappGonderim",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "AktiviteAtama");

            migrationBuilder.DropTable(
                name: "ApiRefreshToken");

            migrationBuilder.DropTable(
                name: "BayiCariHareket");

            migrationBuilder.DropTable(
                name: "BayiFirma");

            migrationBuilder.DropTable(
                name: "BayiKomisyonTarife");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "CommissionsMove");

            migrationBuilder.DropTable(
                name: "CustomersGroup");

            migrationBuilder.DropTable(
                name: "CustomReport");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "DosyaEk");

            migrationBuilder.DropTable(
                name: "EntegrasyonHesabi");

            migrationBuilder.DropTable(
                name: "EtiketIliski");

            migrationBuilder.DropTable(
                name: "ExchangeRate");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "FaturaKalem");

            migrationBuilder.DropTable(
                name: "FiyatListesiKalem");

            migrationBuilder.DropTable(
                name: "GeneralReport");

            migrationBuilder.DropTable(
                name: "GibLog");

            migrationBuilder.DropTable(
                name: "Identifiers");

            migrationBuilder.DropTable(
                name: "IletisimKisi");

            migrationBuilder.DropTable(
                name: "InfCode");

            migrationBuilder.DropTable(
                name: "InvoicesDiscount");

            migrationBuilder.DropTable(
                name: "InvoicesItem");

            migrationBuilder.DropTable(
                name: "InvoicesPayment");

            migrationBuilder.DropTable(
                name: "InvoicesTax");

            migrationBuilder.DropTable(
                name: "ItemsCategory");

            migrationBuilder.DropTable(
                name: "ItemsDiscount");

            migrationBuilder.DropTable(
                name: "ItemsExport");

            migrationBuilder.DropTable(
                name: "KeyAccount_MaliMusavir");

            migrationBuilder.DropTable(
                name: "KomisyonOdemePlani");

            migrationBuilder.DropTable(
                name: "Kullanici_Bayi");

            migrationBuilder.DropTable(
                name: "Kullanici_Firma");

            migrationBuilder.DropTable(
                name: "Kullanici_MaliMusavir");

            migrationBuilder.DropTable(
                name: "Kupon");

            migrationBuilder.DropTable(
                name: "Lead");

            migrationBuilder.DropTable(
                name: "LisansAdet");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Luca_MaliMusavir");

            migrationBuilder.DropTable(
                name: "MailAyar");

            migrationBuilder.DropTable(
                name: "MailGonderim");

            migrationBuilder.DropTable(
                name: "Not");

            migrationBuilder.DropTable(
                name: "Odeme");

            migrationBuilder.DropTable(
                name: "OpportunityAsamaGecis");

            migrationBuilder.DropTable(
                name: "OutboxMesaj");

            migrationBuilder.DropTable(
                name: "PaketIskonto");

            migrationBuilder.DropTable(
                name: "ProcesingReport");

            migrationBuilder.DropTable(
                name: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "Request");

            migrationBuilder.DropTable(
                name: "Returns");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "SatisKalem");

            migrationBuilder.DropTable(
                name: "ServicesProvider");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "Sgk");

            migrationBuilder.DropTable(
                name: "SistemBildirim");

            migrationBuilder.DropTable(
                name: "SmsAyar");

            migrationBuilder.DropTable(
                name: "SmsGonderim");

            migrationBuilder.DropTable(
                name: "StockMovement");

            migrationBuilder.DropTable(
                name: "TeklifKalem");

            migrationBuilder.DropTable(
                name: "Tourist");

            migrationBuilder.DropTable(
                name: "UrunFiyat");

            migrationBuilder.DropTable(
                name: "UserAnnouncementRead");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "VergiOrani");

            migrationBuilder.DropTable(
                name: "WebhookAbonelik");

            migrationBuilder.DropTable(
                name: "WhatsappAyar");

            migrationBuilder.DropTable(
                name: "WhatsappGonderim");

            migrationBuilder.DropTable(
                name: "Aktivite");

            migrationBuilder.DropTable(
                name: "BayiCari");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Dealer");

            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropTable(
                name: "DocumentType");

            migrationBuilder.DropTable(
                name: "Etiket");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "Fatura");

            migrationBuilder.DropTable(
                name: "FiyatListesi");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Tax");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "KeyAccount");

            migrationBuilder.DropTable(
                name: "Lisans");

            migrationBuilder.DropTable(
                name: "Luca");

            migrationBuilder.DropTable(
                name: "SanalPos");

            migrationBuilder.DropTable(
                name: "Purchase");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "Teklif");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "Announcement");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "PaymentAccount");

            migrationBuilder.DropTable(
                name: "PaymentType");

            migrationBuilder.DropTable(
                name: "Satis");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Warehouse");

            migrationBuilder.DropTable(
                name: "Opportunity");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "Paket");

            migrationBuilder.DropTable(
                name: "Brand");

            migrationBuilder.DropTable(
                name: "Unit");

            migrationBuilder.DropTable(
                name: "Firma");

            migrationBuilder.DropTable(
                name: "OpportunityAsama");

            migrationBuilder.DropTable(
                name: "UrunTipi");

            migrationBuilder.DropTable(
                name: "MaliMusavir");

            migrationBuilder.DropTable(
                name: "Bayi");

            migrationBuilder.DropTable(
                name: "Kullanici");
        }
    }
}
