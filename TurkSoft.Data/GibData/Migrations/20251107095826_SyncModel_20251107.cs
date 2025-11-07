using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.GibData.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_20251107 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_Request_UserId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_CustomReport_UserId",
                table: "CustomReport");

            migrationBuilder.DropIndex(
                name: "IX_Announcement_UserId",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Setting",
                newName: "VknTckn");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Setting",
                newName: "Theme");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Setting",
                newName: "Phone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Warehouse",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Warehouse",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Warehouse",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Warehouse",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Warehouse",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Warehouse",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Warehouse",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Warehouse",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Warehouse",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Warehouse",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "UserRole",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "UserRole",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "UserRole",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "UserRole",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "UserRole",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserRole",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserRole",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "UserRole",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "UserAnnouncementRead",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "UserAnnouncementRead",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "UserAnnouncementRead",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "UserAnnouncementRead",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserAnnouncementRead",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserAnnouncementRead",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "UserAnnouncementRead",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "UserAnnouncementRead",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "User",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "User",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "User",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "User",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Unit",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ShortName",
                table: "Unit",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Unit",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Unit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Unit",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Unit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Unit",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Unit",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Unit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Unit",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tourist",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tourist",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Tourist",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Tourist",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Tourist",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Tourist",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tourist",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tourist",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Tourist",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Tourist",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tax",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tax",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Tax",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Tax",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Tax",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Tax",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tax",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tax",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Tax",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Tax",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Supplier",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Supplier",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Supplier",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Supplier",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Supplier",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Supplier",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Supplier",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Supplier",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Supplier",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Supplier",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "StockMovement",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "StockMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "StockMovement",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "StockMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StockMovement",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StockMovement",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "StockMovement",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "StockMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "StockMovement",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Stock",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Stock",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Stock",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Stock",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Stock",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Stock",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Stock",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Stock",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Stock",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Sgk",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Sgk",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Sgk",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Sgk",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Sgk",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Sgk",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sgk",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Sgk",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Sgk",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Sgk",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Setting",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Setting",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Setting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Setting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Setting",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Setting",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Setting",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Setting",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Setting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Setting",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Setting",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Setting",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Setting",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Setting",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ServicesProvider",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ServicesProvider",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ServicesProvider",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ServicesProvider",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ServicesProvider",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServicesProvider",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ServicesProvider",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ServicesProvider",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ServicesProvider",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "RolePermission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "RolePermission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RolePermission",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RolePermission",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "RolePermission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "RolePermission",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Role",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Role",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Role",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Role",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Role",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Role",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Role",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Role",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Role",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Returns",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Returns",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Returns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Returns",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Returns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Returns",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Returns",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Returns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Returns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Request",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Request",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Request",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Request",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Request",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Request",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Request",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Request",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Request",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PurchaseItem",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PurchaseItem",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "PurchaseItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "PurchaseItem",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "PurchaseItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PurchaseItem",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseItem",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "PurchaseItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PurchaseItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Purchase",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Purchase",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Purchase",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Purchase",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Purchase",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Purchase",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Purchase",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Purchase",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Purchase",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ProcesingReport",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ProcesingReport",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ProcesingReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ProcesingReport",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ProcesingReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProcesingReport",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProcesingReport",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ProcesingReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Permission",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Permission",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permission",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Permission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Permission",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Permission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Permission",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Permission",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Permission",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Permission",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PaymentType",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentType",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PaymentType",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "PaymentType",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "PaymentType",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "PaymentType",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PaymentType",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PaymentType",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "PaymentType",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PaymentType",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PaymentAccount",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentAccount",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PaymentAccount",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "PaymentAccount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "PaymentAccount",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "PaymentAccount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PaymentAccount",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PaymentAccount",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "PaymentAccount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PaymentAccount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Payment",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Payment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Payment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Payment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Payment",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Payment",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Payment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Payment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ItemsExport",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ItemsExport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ItemsExport",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ItemsExport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ItemsExport",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ItemsExport",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ItemsExport",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ItemsExport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ItemsExport",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ItemsDiscount",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ItemsDiscount",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ItemsDiscount",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ItemsDiscount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ItemsDiscount",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ItemsDiscount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ItemsDiscount",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ItemsDiscount",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ItemsDiscount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ItemsDiscount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ItemsCategory",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ItemsCategory",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ItemsCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ItemsCategory",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ItemsCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ItemsCategory",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ItemsCategory",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ItemsCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ItemsCategory",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Item",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Item",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Item",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Item",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Item",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Item",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Item",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Item",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "InvoicesTax",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InvoicesTax",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "InvoicesTax",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "InvoicesTax",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "InvoicesTax",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "InvoicesTax",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InvoicesTax",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InvoicesTax",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "InvoicesTax",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "InvoicesTax",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "InvoicesPayment",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "InvoicesPayment",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "InvoicesPayment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "InvoicesPayment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "InvoicesPayment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InvoicesPayment",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InvoicesPayment",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "InvoicesPayment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "InvoicesPayment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "InvoicesItem",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "InvoicesItem",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "InvoicesItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "InvoicesItem",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "InvoicesItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InvoicesItem",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InvoicesItem",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "InvoicesItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "InvoicesItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "InvoicesDiscount",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InvoicesDiscount",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "InvoicesDiscount",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "InvoicesDiscount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "InvoicesDiscount",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "InvoicesDiscount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InvoicesDiscount",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InvoicesDiscount",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "InvoicesDiscount",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "InvoicesDiscount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Invoice",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Invoice",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Invoice",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Invoice",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Invoice",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Invoice",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invoice",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Invoice",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Invoice",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "InfCode",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "InfCode",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "InfCode",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "InfCode",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "InfCode",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InfCode",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InfCode",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "InfCode",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "InfCode",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "InfCode",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Identifiers",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Identifiers",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Identifiers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Identifiers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Identifiers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Identifiers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Identifiers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Identifiers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Identifiers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Group",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Group",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Group",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Group",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Group",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Group",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Group",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Group",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Group",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Group",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "GibLog",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "GibLog",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "GibLog",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "GibLog",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "GibLog",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GibLog",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GibLog",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "GibLog",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "GibLog",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "GeneralReport",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "GeneralReport",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "GeneralReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "GeneralReport",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "GeneralReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GeneralReport",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GeneralReport",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "GeneralReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ExchangeRates",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExchangeRates",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ExchangeRates",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ExchangeRates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ExchangeRates",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ExchangeRates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ExchangeRates",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ExchangeRates",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ExchangeRates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ExchangeRates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ExchangeRate",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ExchangeRate",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ExchangeRate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ExchangeRate",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ExchangeRate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ExchangeRate",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ExchangeRate",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ExchangeRate",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ExchangeRate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "DocumentType",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DocumentType",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DocumentType",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "DocumentType",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "DocumentType",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "DocumentType",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DocumentType",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DocumentType",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "DocumentType",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "DocumentType",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Document",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Document",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Document",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Document",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Document",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Document",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Document",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Document",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Document",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Dealer",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Dealer",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Dealer",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Dealer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Dealer",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Dealer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Dealer",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Dealer",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Dealer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Dealer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CustomReport",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomReport",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CustomReport",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "CustomReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "CustomReport",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "CustomReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomReport",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomReport",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "CustomReport",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CustomersGroup",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CustomersGroup",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "CustomersGroup",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "CustomersGroup",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "CustomersGroup",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomersGroup",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomersGroup",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "CustomersGroup",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "CustomersGroup",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Customer",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "TaxNo",
                table: "Customer",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customer",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Customer",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Customer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Customer",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Customer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Customer",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Customer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Customer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Currency",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Currency",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Currency",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Currency",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Currency",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Currency",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Currency",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Currency",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Currency",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Currency",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Country",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "IsoCode",
                table: "Country",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Country",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Country",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Country",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Country",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Country",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Country",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Country",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Country",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "CommissionsMove",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "CommissionsMove",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "CommissionsMove",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "CommissionsMove",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CommissionsMove",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CommissionsMove",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "CommissionsMove",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "CommissionsMove",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "City",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "City",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "City",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "City",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "City",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "City",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "City",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "City",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "City",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "City",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "City",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Category",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Category",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Category",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Category",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Category",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Category",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Category",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Category",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Category",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Category",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Brand",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brand",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Brand",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Brand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Brand",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Brand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Brand",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Brand",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Brand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Brand",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Bank",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Bank",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Bank",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Bank",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Bank",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Bank",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Bank",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Bank",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Bank",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Bank",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ApiRefreshToken",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "ApiRefreshToken",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "ApiRefreshToken",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "ApiRefreshToken",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ApiRefreshToken",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApiRefreshToken",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ApiRefreshToken",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "ApiRefreshToken",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Announcement",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Announcement",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Announcement",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Announcement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Announcement",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Announcement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Announcement",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Announcement",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Announcement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Address",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Address",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Address",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDate",
                table: "Address",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedByUserId",
                table: "Address",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Address",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Address",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Address",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Address",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId", "UserId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null, 0L });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId", "UserId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null, 0L });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId", "UserId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null, 0L });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId", "UserId" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null, 0L });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "DeleteDate", "DeletedByUserId", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, "PBKDF2$100000$dDSF2N132FQkI11U1m1m5A==$f5V1BBDJOOdE7QjoxPM+b557TmzNGPardO2QnHAho+I=", null, "admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "DeleteDate", "DeletedByUserId", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, "PBKDF2$100000$QTFXdEp+oxfYXdv03gpFzg==$BGgBXx3qgMWCv0nh6/Web5cti+UztJ3EyfH0T12ZBF4=", null, "bayi" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "DeleteDate", "DeletedByUserId", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, "PBKDF2$100000$GQMf5cVH3D+Gk4hYHVeWRQ==$OjQXlOi7CCKny2cdt15McbKWGDuffOv8a8RSqS2CQs4=", null, "malimusavir" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "DeleteDate", "DeletedByUserId", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, true, "PBKDF2$100000$nvzTYnu9jldsQTrX/0spEg==$3tnZ70MM9Fpzx0L8V+QyNLfq97hNSpppA+A7WaJXAMs=", null, "firma" });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "CreatedByUserId", "DeleteDate", "DeletedByUserId", "IsActive", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, true, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouse_UserId_Name",
                table: "Warehouse",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId_Email",
                table: "Users",
                columns: new[] { "UserId", "Email" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId_Username",
                table: "Users",
                columns: new[] { "UserId", "Username" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_UserId_ShortName",
                table: "Unit",
                columns: new[] { "UserId", "ShortName" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tourist_UserId_Name",
                table: "Tourist",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_UserId_Name",
                table: "Tax",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_UserId_Name",
                table: "Supplier",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Sgk_UserId_Code",
                table: "Sgk",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Setting_UserId_Email",
                table: "Setting",
                columns: new[] { "UserId", "Email" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_UserId_RoleId",
                table: "RolePermission",
                columns: new[] { "UserId", "RoleId" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Role_UserId_Name",
                table: "Role",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Request_UserId_Email",
                table: "Request",
                columns: new[] { "UserId", "Email" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_UserId_Code",
                table: "Permission",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentType_UserId_Name",
                table: "PaymentType",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAccount_UserId_Name",
                table: "PaymentAccount",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsDiscount_UserId_Name",
                table: "ItemsDiscount",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UserId_Code",
                table: "Item",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UserId_Name",
                table: "Item",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesTax_UserId_Name",
                table: "InvoicesTax",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesDiscount_UserId_Name",
                table: "InvoicesDiscount",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InfCode_UserId_Code",
                table: "InfCode",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Group_UserId_Name",
                table: "Group",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_UserId_Name",
                table: "ExchangeRates",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentType_UserId_Name",
                table: "DocumentType",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Dealer_UserId_Name",
                table: "Dealer",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomReport_UserId_Name",
                table: "CustomReport",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_UserId_Name",
                table: "Customer",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_UserId_TaxNo",
                table: "Customer",
                columns: new[] { "UserId", "TaxNo" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Currency_UserId_Code",
                table: "Currency",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Country_UserId_IsoCode",
                table: "Country",
                columns: new[] { "UserId", "IsoCode" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_City_UserId_Code",
                table: "City",
                columns: new[] { "UserId", "Code" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_City_UserId_Name",
                table: "City",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Category_UserId_Name",
                table: "Category",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Brand_UserId_Name",
                table: "Brand",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Bank_UserId_Name",
                table: "Bank",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_UserId_Title",
                table: "Announcement",
                columns: new[] { "UserId", "Title" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouse_UserId_Name",
                table: "Warehouse");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserId_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserId_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_Username",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Unit_UserId_ShortName",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_Tourist_UserId_Name",
                table: "Tourist");

            migrationBuilder.DropIndex(
                name: "IX_Tax_UserId_Name",
                table: "Tax");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_UserId_Name",
                table: "Supplier");

            migrationBuilder.DropIndex(
                name: "IX_Sgk_UserId_Code",
                table: "Sgk");

            migrationBuilder.DropIndex(
                name: "IX_Setting_UserId_Email",
                table: "Setting");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_UserId_RoleId",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_Role_Name",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Role_UserId_Name",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Request_UserId_Email",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Permission_UserId_Code",
                table: "Permission");

            migrationBuilder.DropIndex(
                name: "IX_PaymentType_UserId_Name",
                table: "PaymentType");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAccount_UserId_Name",
                table: "PaymentAccount");

            migrationBuilder.DropIndex(
                name: "IX_ItemsDiscount_UserId_Name",
                table: "ItemsDiscount");

            migrationBuilder.DropIndex(
                name: "IX_Item_UserId_Code",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_UserId_Name",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesTax_UserId_Name",
                table: "InvoicesTax");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesDiscount_UserId_Name",
                table: "InvoicesDiscount");

            migrationBuilder.DropIndex(
                name: "IX_InfCode_UserId_Code",
                table: "InfCode");

            migrationBuilder.DropIndex(
                name: "IX_Group_UserId_Name",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_UserId_Name",
                table: "ExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_DocumentType_UserId_Name",
                table: "DocumentType");

            migrationBuilder.DropIndex(
                name: "IX_Dealer_UserId_Name",
                table: "Dealer");

            migrationBuilder.DropIndex(
                name: "IX_CustomReport_UserId_Name",
                table: "CustomReport");

            migrationBuilder.DropIndex(
                name: "IX_Customer_UserId_Name",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_UserId_TaxNo",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Currency_UserId_Code",
                table: "Currency");

            migrationBuilder.DropIndex(
                name: "IX_Country_UserId_IsoCode",
                table: "Country");

            migrationBuilder.DropIndex(
                name: "IX_City_UserId_Code",
                table: "City");

            migrationBuilder.DropIndex(
                name: "IX_City_UserId_Name",
                table: "City");

            migrationBuilder.DropIndex(
                name: "IX_Category_UserId_Name",
                table: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Brand_UserId_Name",
                table: "Brand");

            migrationBuilder.DropIndex(
                name: "IX_Bank_UserId_Name",
                table: "Bank");

            migrationBuilder.DropIndex(
                name: "IX_Announcement_UserId_Title",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Warehouse");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "UserAnnouncementRead");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tourist");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Supplier");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Sgk");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ServicesProvider");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PurchaseItem");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Purchase");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ProcesingReport");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ProcesingReport");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ProcesingReport");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProcesingReport");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProcesingReport");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ProcesingReport");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ItemsExport");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ItemsDiscount");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ItemsCategory");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InvoicesTax");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InvoicesPayment");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InvoicesDiscount");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InfCode");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Identifiers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "GibLog");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "GeneralReport");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "GeneralReport");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "GeneralReport");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GeneralReport");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GeneralReport");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "GeneralReport");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Dealer");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CustomReport");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "CustomReport");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "CustomReport");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomReport");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomReport");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "CustomReport");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CustomersGroup");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Currency");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "CommissionsMove");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "City");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "City");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "City");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "City");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "City");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "City");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "City");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Brand");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ApiRefreshToken");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Address");

            migrationBuilder.RenameColumn(
                name: "VknTckn",
                table: "Setting",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Theme",
                table: "Setting",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Setting",
                newName: "Description");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Warehouse",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Warehouse",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Warehouse",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "User",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Unit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShortName",
                table: "Unit",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Unit",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tourist",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tourist",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tourist",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tax",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tax",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Tax",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Supplier",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Supplier",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Supplier",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StockMovement",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Stock",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Stock",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Sgk",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Sgk",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Sgk",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Setting",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Setting",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ServicesProvider",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ServicesProvider",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "RolePermission",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RolePermission",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Role",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Returns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Returns",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Request",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Request",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Request",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PurchaseItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PurchaseItem",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Purchase",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Purchase",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProcesingReport",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProcesingReport",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Permission",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Permission",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permission",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentType",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentType",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentType",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentAccount",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentAccount",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentAccount",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Payment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Payment",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ItemsDiscount",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ItemsDiscount",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ItemsDiscount",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ItemsCategory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ItemsCategory",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Item",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Item",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Item",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Item",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "InvoicesTax",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InvoicesTax",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "InvoicesTax",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "InvoicesPayment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "InvoicesPayment",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "InvoicesItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "InvoicesItem",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "InvoicesDiscount",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InvoicesDiscount",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "InvoicesDiscount",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invoice",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoice",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "InfCode",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Identifiers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Identifiers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Group",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Group",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Group",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "GibLog",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "GibLog",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "GeneralReport",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "GeneralReport",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExchangeRates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExchangeRates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExchangeRate",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ExchangeRate",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "DocumentType",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DocumentType",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentType",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Document",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Document",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Dealer",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Dealer",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Dealer",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomReport",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomReport",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomReport",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomersGroup",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomersGroup",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Customer",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxNo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Customer",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Currency",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Currency",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Currency",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Country",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IsoCode",
                table: "Country",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Country",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CommissionsMove",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "City",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "City",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "City",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "City",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Category",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Category",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Category",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Brand",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brand",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Brand",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Bank",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Bank",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Announcement",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Announcement",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Announcement",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Address",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Address",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "123456", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "123456", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bayi Kullanıcısı" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "123456", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mali Müşavir" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "123456", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Firma Kullanıcısı" });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_UserId",
                table: "Request",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomReport_UserId",
                table: "CustomReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_UserId",
                table: "Announcement",
                column: "UserId");
        }
    }
}
