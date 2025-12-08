using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.GibData.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbConnectionSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Server = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Database = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IntegratedSecurity = table.Column<bool>(type: "bit", nullable: false),
                    UseSsl = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbConnectionSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SmtpServer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    EnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FromAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FromDisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UseDefaultCredentials = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDesignTemplate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentTypeId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDesignTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceDesignTemplate_DocumentType_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false),
                    IsUserScoped = table.Column<bool>(type: "bit", nullable: false),
                    ScopedUserId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNumberSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberSetting_DocumentType_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberSetting_User_ScopedUserId",
                        column: x => x.ScopedUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberSetting_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EventKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SendEmail = table.Column<bool>(type: "bit", nullable: false),
                    SendSms = table.Column<bool>(type: "bit", nullable: false),
                    SendPush = table.Column<bool>(type: "bit", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Targets = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParameterSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInvoiceInboxSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InboxAlias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    VknTckn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InboxType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvoiceInboxSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserVerificationDevice",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsTrusted = table.Column<bool>(type: "bit", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerificationDevice", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DbConnectionSetting_UserId_Name",
                table: "DbConnectionSetting",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSetting_UserId_Name",
                table: "EmailSetting",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDesignTemplate_DocumentTypeId",
                table: "InvoiceDesignTemplate",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDesignTemplate_UserId_Name",
                table: "InvoiceDesignTemplate",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberSetting_DocumentTypeId",
                table: "InvoiceNumberSetting",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberSetting_ScopedUserId",
                table: "InvoiceNumberSetting",
                column: "ScopedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberSetting_UserId_WarehouseId_DocumentTypeId_Year_Prefix_ScopedUserId",
                table: "InvoiceNumberSetting",
                columns: new[] { "UserId", "WarehouseId", "DocumentTypeId", "Year", "Prefix", "ScopedUserId" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberSetting_WarehouseId",
                table: "InvoiceNumberSetting",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSetting_UserId_EventKey",
                table: "NotificationSetting",
                columns: new[] { "UserId", "EventKey" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSetting_UserId_Name",
                table: "NotificationSetting",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterSetting_UserId_Code",
                table: "ParameterSetting",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvoiceInboxSetting_UserId_InboxAlias",
                table: "UserInvoiceInboxSetting",
                columns: new[] { "UserId", "InboxAlias" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerificationDevice_UserId_DeviceIdentifier",
                table: "UserVerificationDevice",
                columns: new[] { "UserId", "DeviceIdentifier" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbConnectionSetting");

            migrationBuilder.DropTable(
                name: "EmailSetting");

            migrationBuilder.DropTable(
                name: "InvoiceDesignTemplate");

            migrationBuilder.DropTable(
                name: "InvoiceNumberSetting");

            migrationBuilder.DropTable(
                name: "NotificationSetting");

            migrationBuilder.DropTable(
                name: "ParameterSetting");

            migrationBuilder.DropTable(
                name: "UserInvoiceInboxSetting");

            migrationBuilder.DropTable(
                name: "UserVerificationDevice");
        }
    }
}
