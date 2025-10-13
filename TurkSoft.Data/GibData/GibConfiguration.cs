﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.Data.GibData
{
    /// <summary>
    /// Tüm GIB Entity configuration’ları için temel sınıf.
    /// </summary>
    public abstract class BaseGibEntityConfig<T> : IEntityTypeConfiguration<T> where T : class
    {
        public virtual void Configure(EntityTypeBuilder<T> b)
        {
            // Ortak kurallar (gerekirse burada ekle)
        }
    }

    // --- Aşağıda her tablo için config sınıfları ---

    public class AddressConfiguration : BaseGibEntityConfig<Address> { public override void Configure(EntityTypeBuilder<Address> b) { b.ToTable("Address"); b.HasKey(x => x.Id); } }
    public class AnnouncementConfiguration : BaseGibEntityConfig<Announcement> { public override void Configure(EntityTypeBuilder<Announcement> b) { b.ToTable("Announcement"); b.HasKey(x => x.Id); } }
    public class BankConfiguration : BaseGibEntityConfig<Bank> { public override void Configure(EntityTypeBuilder<Bank> b) { b.ToTable("Bank"); b.HasKey(x => x.Id); } }
    public class BrandConfiguration : BaseGibEntityConfig<Brand> { public override void Configure(EntityTypeBuilder<Brand> b) { b.ToTable("Brand"); b.HasKey(x => x.Id); } }
    public class CategoryConfiguration : BaseGibEntityConfig<Category> { public override void Configure(EntityTypeBuilder<Category> b) { b.ToTable("Category"); b.HasKey(x => x.Id); } }
    public class CityConfiguration : BaseGibEntityConfig<City> { public override void Configure(EntityTypeBuilder<City> b) { b.ToTable("City"); b.HasKey(x => x.Id); } }
    public class CommissionsMoveConfiguration : BaseGibEntityConfig<CommissionsMove> { public override void Configure(EntityTypeBuilder<CommissionsMove> b) { b.ToTable("CommissionsMove"); b.HasKey(x => x.Id); } }
    public class CountryConfiguration : BaseGibEntityConfig<Country> { public override void Configure(EntityTypeBuilder<Country> b) { b.ToTable("Country"); b.HasKey(x => x.Id); } }
    public class CurrencyConfiguration : BaseGibEntityConfig<Currency> { public override void Configure(EntityTypeBuilder<Currency> b) { b.ToTable("Currency"); b.HasKey(x => x.Id); } }
    public class CustomerConfiguration : BaseGibEntityConfig<Customer> { public override void Configure(EntityTypeBuilder<Customer> b) { b.ToTable("Customer"); b.HasKey(x => x.Id); } }
    public class CustomersGroupConfiguration : BaseGibEntityConfig<CustomersGroup> { public override void Configure(EntityTypeBuilder<CustomersGroup> b) { b.ToTable("CustomersGroup"); b.HasKey(x => x.Id); } }
    public class CustomReportConfiguration : BaseGibEntityConfig<CustomReport> { public override void Configure(EntityTypeBuilder<CustomReport> b) { b.ToTable("CustomReport"); b.HasKey(x => x.Id); } }
    public class DealerConfiguration : BaseGibEntityConfig<Dealer> { public override void Configure(EntityTypeBuilder<Dealer> b) { b.ToTable("Dealer"); b.HasKey(x => x.Id); } }
    public class DocumentConfiguration : BaseGibEntityConfig<Document> { public override void Configure(EntityTypeBuilder<Document> b) { b.ToTable("Document"); b.HasKey(x => x.Id); } }
    public class DocumentTypeConfiguration : BaseGibEntityConfig<DocumentType> { public override void Configure(EntityTypeBuilder<DocumentType> b) { b.ToTable("DocumentType"); b.HasKey(x => x.Id); } }
    public class ExchangeRateConfiguration : BaseGibEntityConfig<ExchangeRate> { public override void Configure(EntityTypeBuilder<ExchangeRate> b) { b.ToTable("ExchangeRate"); b.HasKey(x => x.Id); } }
    public class ExchangeRatesConfiguration : BaseGibEntityConfig<ExchangeRates> { public override void Configure(EntityTypeBuilder<ExchangeRates> b) { b.ToTable("ExchangeRates"); b.HasKey(x => x.Id); } }
    public class GeneralReportConfiguration : BaseGibEntityConfig<GeneralReport> { public override void Configure(EntityTypeBuilder<GeneralReport> b) { b.ToTable("GeneralReport"); b.HasKey(x => x.Id); } }
    public class GroupConfiguration : BaseGibEntityConfig<Group> { public override void Configure(EntityTypeBuilder<Group> b) { b.ToTable("Group"); b.HasKey(x => x.Id); } }
    public class IdentifiersConfiguration : BaseGibEntityConfig<Identifiers> { public override void Configure(EntityTypeBuilder<Identifiers> b) { b.ToTable("Identifiers"); b.HasKey(x => x.Id); } }
    public class InfCodeConfiguration : BaseGibEntityConfig<InfCode> { public override void Configure(EntityTypeBuilder<InfCode> b) { b.ToTable("InfCode"); b.HasKey(x => x.Id); } }
    public class InvoiceConfiguration : BaseGibEntityConfig<Invoice> { public override void Configure(EntityTypeBuilder<Invoice> b) { b.ToTable("Invoice"); b.HasKey(x => x.Id); } }
    public class InvoicesDiscountConfiguration : BaseGibEntityConfig<InvoicesDiscount> { public override void Configure(EntityTypeBuilder<InvoicesDiscount> b) { b.ToTable("InvoicesDiscount"); b.HasKey(x => x.Id); } }
    public class InvoicesItemConfiguration : BaseGibEntityConfig<InvoicesItem> { public override void Configure(EntityTypeBuilder<InvoicesItem> b) { b.ToTable("InvoicesItem"); b.HasKey(x => x.Id); } }
    public class InvoicesPaymentConfiguration : BaseGibEntityConfig<InvoicesPayment> { public override void Configure(EntityTypeBuilder<InvoicesPayment> b) { b.ToTable("InvoicesPayment"); b.HasKey(x => x.Id); } }
    public class InvoicesTaxConfiguration : BaseGibEntityConfig<InvoicesTax> { public override void Configure(EntityTypeBuilder<InvoicesTax> b) { b.ToTable("InvoicesTax"); b.HasKey(x => x.Id); } }
    public class ItemConfiguration : BaseGibEntityConfig<Item> { public override void Configure(EntityTypeBuilder<Item> b) { b.ToTable("Item"); b.HasKey(x => x.Id); } }
    public class ItemsCategoryConfiguration : BaseGibEntityConfig<ItemsCategory> { public override void Configure(EntityTypeBuilder<ItemsCategory> b) { b.ToTable("ItemsCategory"); b.HasKey(x => x.Id); } }
    public class ItemsDiscountConfiguration : BaseGibEntityConfig<ItemsDiscount> { public override void Configure(EntityTypeBuilder<ItemsDiscount> b) { b.ToTable("ItemsDiscount"); b.HasKey(x => x.Id); } }
    public class ItemsExportConfiguration : BaseGibEntityConfig<ItemsExport> { public override void Configure(EntityTypeBuilder<ItemsExport> b) { b.ToTable("ItemsExport"); b.HasKey(x => x.Id); } }

    // 🔹 Log tablosu özel
    public class LogConfiguration : BaseGibEntityConfig<Log>
    {
        public override void Configure(EntityTypeBuilder<Log> b)
        {
            b.ToTable("GibLog"); // çakışma engellendi
            b.HasKey(x => x.Id);
        }
    }

    public class PaymentConfiguration : BaseGibEntityConfig<Payment> { public override void Configure(EntityTypeBuilder<Payment> b) { b.ToTable("Payment"); b.HasKey(x => x.Id); } }
    public class PaymentAccountConfiguration : BaseGibEntityConfig<PaymentAccount> { public override void Configure(EntityTypeBuilder<PaymentAccount> b) { b.ToTable("PaymentAccount"); b.HasKey(x => x.Id); } }
    public class PaymentTypeConfiguration : BaseGibEntityConfig<PaymentType> { public override void Configure(EntityTypeBuilder<PaymentType> b) { b.ToTable("PaymentType"); b.HasKey(x => x.Id); } }
    public class PermissionConfiguration : BaseGibEntityConfig<Permission> { public override void Configure(EntityTypeBuilder<Permission> b) { b.ToTable("Permission"); b.HasKey(x => x.Id); } }
    public class ProcesingReportConfiguration : BaseGibEntityConfig<ProcesingReport> { public override void Configure(EntityTypeBuilder<ProcesingReport> b) { b.ToTable("ProcesingReport"); b.HasKey(x => x.Id); } }
    public class PurchaseConfiguration : BaseGibEntityConfig<Purchase> { public override void Configure(EntityTypeBuilder<Purchase> b) { b.ToTable("Purchase"); b.HasKey(x => x.Id); } }
    public class PurchaseItemConfiguration : BaseGibEntityConfig<PurchaseItem> { public override void Configure(EntityTypeBuilder<PurchaseItem> b) { b.ToTable("PurchaseItem"); b.HasKey(x => x.Id); } }
    public class RequestConfiguration : BaseGibEntityConfig<Request> { public override void Configure(EntityTypeBuilder<Request> b) { b.ToTable("Request"); b.HasKey(x => x.Id); } }
    public class ReturnsConfiguration : BaseGibEntityConfig<Returns> { public override void Configure(EntityTypeBuilder<Returns> b) { b.ToTable("Returns"); b.HasKey(x => x.Id); } }
    public class RoleConfiguration : BaseGibEntityConfig<Role> { public override void Configure(EntityTypeBuilder<Role> b) { b.ToTable("Role"); b.HasKey(x => x.Id); } }
    public class RolePermissionConfiguration : BaseGibEntityConfig<RolePermission> { public override void Configure(EntityTypeBuilder<RolePermission> b) { b.ToTable("RolePermission"); b.HasKey(x => x.Id); } }
    public class ServicesProviderConfiguration : BaseGibEntityConfig<ServicesProvider> { public override void Configure(EntityTypeBuilder<ServicesProvider> b) { b.ToTable("ServicesProvider"); b.HasKey(x => x.Id); } }
    public class SettingConfiguration : BaseGibEntityConfig<Setting> { public override void Configure(EntityTypeBuilder<Setting> b) { b.ToTable("Setting"); b.HasKey(x => x.Id); } }
    public class SgkConfiguration : BaseGibEntityConfig<Sgk> { public override void Configure(EntityTypeBuilder<Sgk> b) { b.ToTable("Sgk"); b.HasKey(x => x.Id); } }
    public class StockConfiguration : BaseGibEntityConfig<Stock> { public override void Configure(EntityTypeBuilder<Stock> b) { b.ToTable("Stock"); b.HasKey(x => x.Id); } }
    public class StockMovementConfiguration : BaseGibEntityConfig<StockMovement> { public override void Configure(EntityTypeBuilder<StockMovement> b) { b.ToTable("StockMovement"); b.HasKey(x => x.Id); } }
    public class SupplierConfiguration : BaseGibEntityConfig<Supplier> { public override void Configure(EntityTypeBuilder<Supplier> b) { b.ToTable("Supplier"); b.HasKey(x => x.Id); } }
    public class TaxConfiguration : BaseGibEntityConfig<Tax> { public override void Configure(EntityTypeBuilder<Tax> b) { b.ToTable("Tax"); b.HasKey(x => x.Id); } }
    public class TouristConfiguration : BaseGibEntityConfig<Tourist> { public override void Configure(EntityTypeBuilder<Tourist> b) { b.ToTable("Tourist"); b.HasKey(x => x.Id); } }
    public class UnitConfiguration : BaseGibEntityConfig<Unit> { public override void Configure(EntityTypeBuilder<Unit> b) { b.ToTable("Unit"); b.HasKey(x => x.Id); } }
    public class UserConfiguration : BaseGibEntityConfig<User> { public override void Configure(EntityTypeBuilder<User> b) { b.ToTable("User"); b.HasKey(x => x.Id); } }

    // 🔹 UserAnnouncementRead — Cascade Delete düzeltildi
    public class UserAnnouncementReadConfiguration : BaseGibEntityConfig<UserAnnouncementRead>
    {
        public override void Configure(EntityTypeBuilder<UserAnnouncementRead> b)
        {
            b.ToTable("UserAnnouncementRead");
            b.HasKey(x => x.Id);

            b.HasOne(x => x.User)
             .WithMany(x => x.UserAnnouncementReads)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class UserRoleConfiguration : BaseGibEntityConfig<UserRole> { public override void Configure(EntityTypeBuilder<UserRole> b) { b.ToTable("UserRole"); b.HasKey(x => x.Id); } }
    public class UsersConfiguration : BaseGibEntityConfig<Users> { public override void Configure(EntityTypeBuilder<Users> b) { b.ToTable("Users"); b.HasKey(x => x.Id); } }
    public class WarehouseConfiguration : BaseGibEntityConfig<Warehouse> { public override void Configure(EntityTypeBuilder<Warehouse> b) { b.ToTable("Warehouse"); b.HasKey(x => x.Id); } }
}
