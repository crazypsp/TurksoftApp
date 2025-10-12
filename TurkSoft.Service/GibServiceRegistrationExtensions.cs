using Microsoft.Extensions.DependencyInjection;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Manager;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Interface.Gib;
using TurkSoft.Service.Manager;
using TurkSoft.Service.Manager.Gib;

namespace TurkSoft.Service
{
    /// <summary>
    /// GIB Entity servis–manager dependency injection kayıtlarını yapar.
    /// Bu sınıf, Generated.Gib.cs ve GibAppDbContext.cs içeriğine birebir uyumludur.
    /// </summary>
    public static class GibServiceRegistrationExtensions
    {
        public static IServiceCollection AddGibEntityServices(this IServiceCollection services)
        {
            // === Generic base kayıtları ===
            // BaseGibManager artık iki generic parametre alıyor (TEntity, TKey)
            services.AddScoped(typeof(IBaseGibService<,>), typeof(BaseGibManager<,>));
            services.AddScoped(typeof(IEntityGibService<,>), typeof(EntityGibManager<,>));

            // === GIB Entity Servis–Manager Eşleşmeleri ===
            services.AddScoped<IGibAddressService, GibAddressManager>();
            services.AddScoped<IGibAnnouncementService, GibAnnouncementManager>();
            services.AddScoped<IGibBankService, GibBankManager>();
            services.AddScoped<IGibBrandService, GibBrandManager>();
            services.AddScoped<IGibCategoryService, GibCategoryManager>();
            services.AddScoped<IGibCityService, GibCityManager>();
            services.AddScoped<IGibCommissionsMoveService, GibCommissionsMoveManager>();
            services.AddScoped<IGibCountryService, GibCountryManager>();
            services.AddScoped<IGibCurrencyService, GibCurrencyManager>();
            services.AddScoped<IGibCustomerService, GibCustomerManager>();
            services.AddScoped<IGibCustomersGroupService, GibCustomersGroupManager>();
            services.AddScoped<IGibCustomReportService, GibCustomReportManager>();
            services.AddScoped<IGibDealerService, GibDealerManager>();
            services.AddScoped<IGibDocumentService, GibDocumentManager>();
            services.AddScoped<IGibDocumentTypeService, GibDocumentTypeManager>();
            services.AddScoped<IGibExchangeRateService, GibExchangeRateManager>();
            services.AddScoped<IGibExchangeRatesService, GibExchangeRatesManager>();
            services.AddScoped<IGibGeneralReportService, GibGeneralReportManager>();
            services.AddScoped<IGibGroupService, GibGroupManager>();
            services.AddScoped<IGibIdentifiersService, GibIdentifiersManager>();
            services.AddScoped<IGibInfCodeService, GibInfCodeManager>();
            services.AddScoped<IGibInvoiceService, GibInvoiceManager>();
            services.AddScoped<IGibInvoicesDiscountService, GibInvoicesDiscountManager>();
            services.AddScoped<IGibInvoicesItemService, GibInvoicesItemManager>();
            services.AddScoped<IGibInvoicesPaymentService, GibInvoicesPaymentManager>();
            services.AddScoped<IGibInvoicesTaxService, GibInvoicesTaxManager>();
            services.AddScoped<IGibItemService, GibItemManager>();
            services.AddScoped<IGibItemsCategoryService, GibItemsCategoryManager>();
            services.AddScoped<IGibItemsDiscountService, GibItemsDiscountManager>();
            services.AddScoped<IGibItemsExportService, GibItemsExportManager>();
            services.AddScoped<IGibLogService, GibLogManager>();
            services.AddScoped<IGibPaymentService, GibPaymentManager>();
            services.AddScoped<IGibPaymentAccountService, GibPaymentAccountManager>();
            services.AddScoped<IGibPaymentTypeService, GibPaymentTypeManager>();
            services.AddScoped<IGibPermissionService, GibPermissionManager>();
            services.AddScoped<IGibProcesingReportService, GibProcesingReportManager>();
            services.AddScoped<IGibPurchaseService, GibPurchaseManager>();
            services.AddScoped<IGibPurchaseItemService, GibPurchaseItemManager>();
            services.AddScoped<IGibRequestService, GibRequestManager>();
            services.AddScoped<IGibReturnsService, GibReturnsManager>();
            services.AddScoped<IGibRoleService, GibRoleManager>();
            services.AddScoped<IGibRolePermissionService, GibRolePermissionManager>();
            services.AddScoped<IGibServicesProviderService, GibServicesProviderManager>();
            services.AddScoped<IGibSettingService, GibSettingManager>();
            services.AddScoped<IGibSgkService, GibSgkManager>();
            services.AddScoped<IGibStockService, GibStockManager>();
            services.AddScoped<IGibStockMovementService, GibStockMovementManager>();
            services.AddScoped<IGibSupplierService, GibSupplierManager>();
            services.AddScoped<IGibTaxService, GibTaxManager>();
            services.AddScoped<IGibTouristService, GibTouristManager>();
            services.AddScoped<IGibUnitService, GibUnitManager>();
            services.AddScoped<IGibUserService, GibUserManager>();
            services.AddScoped<IGibUserAnnouncementReadService, GibUserAnnouncementReadManager>();
            services.AddScoped<IGibUserRoleService, GibUserRoleManager>();
            services.AddScoped<IGibUsersService, GibUsersManager>();
            services.AddScoped<IGibWarehouseService, GibWarehouseManager>();

            return services;
        }
    }
}
