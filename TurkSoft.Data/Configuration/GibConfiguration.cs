// TurkSoft.Data.Configuration/GibConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.Data.Configuration.Gib
{
    // App tarafındaki BaseEntityConfig mimarisiyle aynı desen,
    // ancak GIB modellerine gölge/audit alan eklemiyoruz.
    public abstract class BaseGibEntityConfig<T> : IEntityTypeConfiguration<T> where T : class
    {
        public virtual void Configure(EntityTypeBuilder<T> b)
        {
            // Ortak kurallar gerekiyorsa buraya eklenebilir (şimdilik boş bırakıldı).
        }
    }

    public class AddressConfiguration : BaseGibEntityConfig<Address>
    {
        public override void Configure(EntityTypeBuilder<Address> b)
        {
            base.Configure(b);
            b.ToTable("Address");
            b.HasKey(x => x.Id);
        }
    }

    public class AnnouncementConfiguration : BaseGibEntityConfig<Announcement>
    {
        public override void Configure(EntityTypeBuilder<Announcement> b)
        {
            base.Configure(b);
            b.ToTable("Announcement");
            b.HasKey(x => x.Id);
        }
    }

    public class BankConfiguration : BaseGibEntityConfig<Bank>
    {
        public override void Configure(EntityTypeBuilder<Bank> b)
        {
            base.Configure(b);
            b.ToTable("Bank");
            b.HasKey(x => x.Id);
        }
    }

    public class BrandConfiguration : BaseGibEntityConfig<Brand>
    {
        public override void Configure(EntityTypeBuilder<Brand> b)
        {
            base.Configure(b);
            b.ToTable("Brand");
            b.HasKey(x => x.Id);
        }
    }

    public class CategoryConfiguration : BaseGibEntityConfig<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> b)
        {
            base.Configure(b);
            b.ToTable("Category");
            b.HasKey(x => x.Id);
        }
    }

    public class CityConfiguration : BaseGibEntityConfig<City>
    {
        public override void Configure(EntityTypeBuilder<City> b)
        {
            base.Configure(b);
            b.ToTable("City");
            b.HasKey(x => x.Id);
        }
    }

    public class CommissionsMoveConfiguration : BaseGibEntityConfig<CommissionsMove>
    {
        public override void Configure(EntityTypeBuilder<CommissionsMove> b)
        {
            base.Configure(b);
            b.ToTable("CommissionsMove");
            b.HasKey(x => x.Id);
        }
    }

    public class CountryConfiguration : BaseGibEntityConfig<Country>
    {
        public override void Configure(EntityTypeBuilder<Country> b)
        {
            base.Configure(b);
            b.ToTable("Country");
            b.HasKey(x => x.Id);
        }
    }

    public class CurrencyConfiguration : BaseGibEntityConfig<Currency>
    {
        public override void Configure(EntityTypeBuilder<Currency> b)
        {
            base.Configure(b);
            b.ToTable("Currency");
            b.HasKey(x => x.Id);
        }
    }

    public class CustomerConfiguration : BaseGibEntityConfig<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> b)
        {
            base.Configure(b);
            b.ToTable("Customer");
            b.HasKey(x => x.Id);
        }
    }

    public class CustomersGroupConfiguration : BaseGibEntityConfig<CustomersGroup>
    {
        public override void Configure(EntityTypeBuilder<CustomersGroup> b)
        {
            base.Configure(b);
            b.ToTable("CustomersGroup");
            b.HasKey(x => x.Id);
        }
    }

    public class CustomReportConfiguration : BaseGibEntityConfig<CustomReport>
    {
        public override void Configure(EntityTypeBuilder<CustomReport> b)
        {
            base.Configure(b);
            b.ToTable("CustomReport");
            b.HasKey(x => x.Id);
        }
    }

    public class DealerConfiguration : BaseGibEntityConfig<Dealer>
    {
        public override void Configure(EntityTypeBuilder<Dealer> b)
        {
            base.Configure(b);
            b.ToTable("Dealer");
            b.HasKey(x => x.Id);
        }
    }

    public class DocumentConfiguration : BaseGibEntityConfig<Document>
    {
        public override void Configure(EntityTypeBuilder<Document> b)
        {
            base.Configure(b);
            b.ToTable("Document");
            b.HasKey(x => x.Id);
        }
    }

    public class DocumentTypeConfiguration : BaseGibEntityConfig<DocumentType>
    {
        public override void Configure(EntityTypeBuilder<DocumentType> b)
        {
            base.Configure(b);
            b.ToTable("DocumentType");
            b.HasKey(x => x.Id);
        }
    }

    public class ExchangeRateConfiguration : BaseGibEntityConfig<ExchangeRate>
    {
        public override void Configure(EntityTypeBuilder<ExchangeRate> b)
        {
            base.Configure(b);
            b.ToTable("ExchangeRate");
            b.HasKey(x => x.Id);
        }
    }

    public class ExchangeRatesConfiguration : BaseGibEntityConfig<ExchangeRates>
    {
        public override void Configure(EntityTypeBuilder<ExchangeRates> b)
        {
            base.Configure(b);
            b.ToTable("ExchangeRates");
            b.HasKey(x => x.Id);
        }
    }

    public class GeneralReportConfiguration : BaseGibEntityConfig<GeneralReport>
    {
        public override void Configure(EntityTypeBuilder<GeneralReport> b)
        {
            base.Configure(b);
            b.ToTable("GeneralReport");
            b.HasKey(x => x.Id);
        }
    }

    public class GroupConfiguration : BaseGibEntityConfig<Group>
    {
        public override void Configure(EntityTypeBuilder<Group> b)
        {
            base.Configure(b);
            b.ToTable("Group");
            b.HasKey(x => x.Id);
        }
    }

    public class IdentifiersConfiguration : BaseGibEntityConfig<Identifiers>
    {
        public override void Configure(EntityTypeBuilder<Identifiers> b)
        {
            base.Configure(b);
            b.ToTable("Identifiers");
            b.HasKey(x => x.Id);
        }
    }

    public class InfCodeConfiguration : BaseGibEntityConfig<InfCode>
    {
        public override void Configure(EntityTypeBuilder<InfCode> b)
        {
            base.Configure(b);
            b.ToTable("InfCode");
            b.HasKey(x => x.Id);
        }
    }

    public class InvoiceConfiguration : BaseGibEntityConfig<Invoice>
    {
        public override void Configure(EntityTypeBuilder<Invoice> b)
        {
            base.Configure(b);
            b.ToTable("Invoice");
            b.HasKey(x => x.Id);
        }
    }

    public class InvoicesDiscountConfiguration : BaseGibEntityConfig<InvoicesDiscount>
    {
        public override void Configure(EntityTypeBuilder<InvoicesDiscount> b)
        {
            base.Configure(b);
            b.ToTable("InvoicesDiscount");
            b.HasKey(x => x.Id);
        }
    }

    public class InvoicesItemConfiguration : BaseGibEntityConfig<InvoicesItem>
    {
        public override void Configure(EntityTypeBuilder<InvoicesItem> b)
        {
            base.Configure(b);
            b.ToTable("InvoicesItem");
            b.HasKey(x => x.Id);
        }
    }

    public class InvoicesPaymentConfiguration : BaseGibEntityConfig<InvoicesPayment>
    {
        public override void Configure(EntityTypeBuilder<InvoicesPayment> b)
        {
            base.Configure(b);
            b.ToTable("InvoicesPayment");
            b.HasKey(x => x.Id);
        }
    }

    public class InvoicesTaxConfiguration : BaseGibEntityConfig<InvoicesTax>
    {
        public override void Configure(EntityTypeBuilder<InvoicesTax> b)
        {
            base.Configure(b);
            b.ToTable("InvoicesTax");
            b.HasKey(x => x.Id);
        }
    }

    public class ItemConfiguration : BaseGibEntityConfig<Item>
    {
        public override void Configure(EntityTypeBuilder<Item> b)
        {
            base.Configure(b);
            b.ToTable("Item");
            b.HasKey(x => x.Id);
        }
    }

    public class ItemsCategoryConfiguration : BaseGibEntityConfig<ItemsCategory>
    {
        public override void Configure(EntityTypeBuilder<ItemsCategory> b)
        {
            base.Configure(b);
            b.ToTable("ItemsCategory");
            b.HasKey(x => x.Id);
        }
    }

    public class ItemsDiscountConfiguration : BaseGibEntityConfig<ItemsDiscount>
    {
        public override void Configure(EntityTypeBuilder<ItemsDiscount> b)
        {
            base.Configure(b);
            b.ToTable("ItemsDiscount");
            b.HasKey(x => x.Id);
        }
    }

    public class ItemsExportConfiguration : BaseGibEntityConfig<ItemsExport>
    {
        public override void Configure(EntityTypeBuilder<ItemsExport> b)
        {
            base.Configure(b);
            b.ToTable("ItemsExport");
            b.HasKey(x => x.Id);
        }
    }

    public class LogConfiguration : BaseGibEntityConfig<Log>
    {
        public override void Configure(EntityTypeBuilder<Log> b)
        {
            base.Configure(b);
            b.ToTable("Log");
            b.HasKey(x => x.Id);
        }
    }

    public class PaymentConfiguration : BaseGibEntityConfig<Payment>
    {
        public override void Configure(EntityTypeBuilder<Payment> b)
        {
            base.Configure(b);
            b.ToTable("Payment");
            b.HasKey(x => x.Id);
        }
    }

    public class PaymentAccountConfiguration : BaseGibEntityConfig<PaymentAccount>
    {
        public override void Configure(EntityTypeBuilder<PaymentAccount> b)
        {
            base.Configure(b);
            b.ToTable("PaymentAccount");
            b.HasKey(x => x.Id);
        }
    }

    public class PaymentTypeConfiguration : BaseGibEntityConfig<PaymentType>
    {
        public override void Configure(EntityTypeBuilder<PaymentType> b)
        {
            base.Configure(b);
            b.ToTable("PaymentType");
            b.HasKey(x => x.Id);
        }
    }

    public class PermissionConfiguration : BaseGibEntityConfig<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> b)
        {
            base.Configure(b);
            b.ToTable("Permission");
            b.HasKey(x => x.Id);
        }
    }

    public class ProcesingReportConfiguration : BaseGibEntityConfig<ProcesingReport>
    {
        public override void Configure(EntityTypeBuilder<ProcesingReport> b)
        {
            base.Configure(b);
            b.ToTable("ProcesingReport");
            b.HasKey(x => x.Id);
        }
    }

    public class PurchaseConfiguration : BaseGibEntityConfig<Purchase>
    {
        public override void Configure(EntityTypeBuilder<Purchase> b)
        {
            base.Configure(b);
            b.ToTable("Purchase");
            b.HasKey(x => x.Id);
        }
    }

    public class PurchaseItemConfiguration : BaseGibEntityConfig<PurchaseItem>
    {
        public override void Configure(EntityTypeBuilder<PurchaseItem> b)
        {
            base.Configure(b);
            b.ToTable("PurchaseItem");
            b.HasKey(x => x.Id);
        }
    }

    public class RequestConfiguration : BaseGibEntityConfig<Request>
    {
        public override void Configure(EntityTypeBuilder<Request> b)
        {
            base.Configure(b);
            b.ToTable("Request");
            b.HasKey(x => x.Id);
        }
    }

    public class ReturnsConfiguration : BaseGibEntityConfig<Returns>
    {
        public override void Configure(EntityTypeBuilder<Returns> b)
        {
            base.Configure(b);
            b.ToTable("Returns");
            b.HasKey(x => x.Id);
        }
    }

    public class RoleConfiguration : BaseGibEntityConfig<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> b)
        {
            base.Configure(b);
            b.ToTable("Role");
            b.HasKey(x => x.Id);
        }
    }

    public class RolePermissionConfiguration : BaseGibEntityConfig<RolePermission>
    {
        public override void Configure(EntityTypeBuilder<RolePermission> b)
        {
            base.Configure(b);
            b.ToTable("RolePermission");
            b.HasKey(x => x.Id);
        }
    }

    public class ServicesProviderConfiguration : BaseGibEntityConfig<ServicesProvider>
    {
        public override void Configure(EntityTypeBuilder<ServicesProvider> b)
        {
            base.Configure(b);
            b.ToTable("ServicesProvider");
            b.HasKey(x => x.Id);
        }
    }

    public class SettingConfiguration : BaseGibEntityConfig<Setting>
    {
        public override void Configure(EntityTypeBuilder<Setting> b)
        {
            base.Configure(b);
            b.ToTable("Setting");
            b.HasKey(x => x.Id);
        }
    }

    public class SgkConfiguration : BaseGibEntityConfig<Sgk>
    {
        public override void Configure(EntityTypeBuilder<Sgk> b)
        {
            base.Configure(b);
            b.ToTable("Sgk");
            b.HasKey(x => x.Id);
        }
    }

    public class StockConfiguration : BaseGibEntityConfig<Stock>
    {
        public override void Configure(EntityTypeBuilder<Stock> b)
        {
            base.Configure(b);
            b.ToTable("Stock");
            b.HasKey(x => x.Id);
        }
    }

    public class StockMovementConfiguration : BaseGibEntityConfig<StockMovement>
    {
        public override void Configure(EntityTypeBuilder<StockMovement> b)
        {
            base.Configure(b);
            b.ToTable("StockMovement");
            b.HasKey(x => x.Id);
        }
    }

    public class SupplierConfiguration : BaseGibEntityConfig<Supplier>
    {
        public override void Configure(EntityTypeBuilder<Supplier> b)
        {
            base.Configure(b);
            b.ToTable("Supplier");
            b.HasKey(x => x.Id);
        }
    }

    public class TaxConfiguration : BaseGibEntityConfig<Tax>
    {
        public override void Configure(EntityTypeBuilder<Tax> b)
        {
            base.Configure(b);
            b.ToTable("Tax");
            b.HasKey(x => x.Id);
        }
    }

    public class TouristConfiguration : BaseGibEntityConfig<Tourist>
    {
        public override void Configure(EntityTypeBuilder<Tourist> b)
        {
            base.Configure(b);
            b.ToTable("Tourist");
            b.HasKey(x => x.Id);
        }
    }

    public class UnitConfiguration : BaseGibEntityConfig<Unit>
    {
        public override void Configure(EntityTypeBuilder<Unit> b)
        {
            base.Configure(b);
            b.ToTable("Unit");
            b.HasKey(x => x.Id);
        }
    }

    public class UserConfiguration : BaseGibEntityConfig<User>
    {
        public override void Configure(EntityTypeBuilder<User> b)
        {
            base.Configure(b);
            b.ToTable("User");
            b.HasKey(x => x.Id);
        }
    }

    public class UserAnnouncementReadConfiguration : BaseGibEntityConfig<UserAnnouncementRead>
    {
        public override void Configure(EntityTypeBuilder<UserAnnouncementRead> b)
        {
            base.Configure(b);
            b.ToTable("UserAnnouncementRead");
            b.HasKey(x => x.Id);
        }
    }

    public class UserRoleConfiguration : BaseGibEntityConfig<UserRole>
    {
        public override void Configure(EntityTypeBuilder<UserRole> b)
        {
            base.Configure(b);
            b.ToTable("UserRole");
            b.HasKey(x => x.Id);
        }
    }

    public class UsersConfiguration : BaseGibEntityConfig<Users>
    {
        public override void Configure(EntityTypeBuilder<Users> b)
        {
            base.Configure(b);
            b.ToTable("Users");
            b.HasKey(x => x.Id);
        }
    }

    public class WarehouseConfiguration : BaseGibEntityConfig<Warehouse>
    {
        public override void Configure(EntityTypeBuilder<Warehouse> b)
        {
            base.Configure(b);
            b.ToTable("Warehouse");
            b.HasKey(x => x.Id);
        }
    }
}
