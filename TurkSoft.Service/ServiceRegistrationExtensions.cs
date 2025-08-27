using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Manager;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

namespace TurkSoft.Service
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddEntityServices(this IServiceCollection services)
        {
            // Generic base
            services.AddScoped(typeof(IBaseService<>), typeof(BaseManager<>));

            // Buraya tüm I{Entity}Service → {Entity}Manager eşleşmelerini ekliyoruz:
            services.AddScoped<IKullaniciService, KullaniciManager>();
            services.AddScoped<ILogService, LogManager>();
            services.AddScoped<IMailAyarService, MailAyarManager>();
            services.AddScoped<IMailGonderimService, MailGonderimManager>();
            services.AddScoped<ISmsAyarService, SmsAyarManager>();
            services.AddScoped<ISmsGonderimService, SmsGonderimManager>();
            services.AddScoped<IWhatsappAyarService, WhatsappAyarManager>();
            services.AddScoped<IWhatsappGonderimService, WhatsappGonderimManager>();
            services.AddScoped<ILucaService, LucaManager>();
            services.AddScoped<IKeyAccountService, KeyAccountManager>();
            services.AddScoped<IUrunTipiService, UrunTipiManager>();
            services.AddScoped<IPaketService, PaketManager>();
            services.AddScoped<IUrunFiyatService, UrunFiyatManager>();
            services.AddScoped<IPaketIskontoService, PaketIskontoManager>();
            services.AddScoped<IBayiService, BayiManager>();
            services.AddScoped<IBayiFirmaService, BayiFirmaManager>();
            services.AddScoped<IMaliMusavirService, MaliMusavirManager>();
            services.AddScoped<IFirmaService, FirmaManager>();
            services.AddScoped<ISatisService, SatisManager>();
            services.AddScoped<ISatisKalemService, SatisKalemManager>();
            services.AddScoped<ILisansService, LisansManager>();
            services.AddScoped<ILisansAdetService, LisansAdetManager>();
            services.AddScoped<IOdemeService, OdemeManager>();
            services.AddScoped<ISanalPosService, SanalPosManager>();
            services.AddScoped<IEntegrasyonHesabiService, EntegrasyonHesabiManager>();
            services.AddScoped<ILeadService, LeadManager>();
            services.AddScoped<IOpportunityAsamaService, OpportunityAsamaManager>();
            services.AddScoped<IOpportunityService, OpportunityManager>();
            services.AddScoped<IOpportunityAsamaGecisService, OpportunityAsamaGecisManager>();
            services.AddScoped<ITeklifService, TeklifManager>();
            services.AddScoped<ITeklifKalemService, TeklifKalemManager>();
            services.AddScoped<IAktiviteService, AktiviteManager>();
            services.AddScoped<IAktiviteAtamaService, AktiviteAtamaManager>();
            services.AddScoped<IIletisimKisiService, IletisimKisiManager>();
            services.AddScoped<IFaturaService, FaturaManager>();
            services.AddScoped<IFaturaKalemService, FaturaKalemManager>();
            services.AddScoped<IBayiCariService, BayiCariManager>();
            services.AddScoped<IBayiCariHareketService, BayiCariHareketManager>();
            services.AddScoped<IKomisyonOdemePlaniService, KomisyonOdemePlaniManager>();
            services.AddScoped<IBayiKomisyonTarifeService, BayiKomisyonTarifeManager>();
            services.AddScoped<IFiyatListesiService, FiyatListesiManager>();
            services.AddScoped<IFiyatListesiKalemService, FiyatListesiKalemManager>();
            services.AddScoped<IVergiOraniService, VergiOraniManager>();
            services.AddScoped<IKuponService, KuponManager>();
            services.AddScoped<INotService, NotManager>();
            services.AddScoped<IDosyaEkService, DosyaEkManager>();
            services.AddScoped<IEtiketService, EtiketManager>();
            services.AddScoped<IEtiketIliskiService, EtiketIliskiManager>();
            services.AddScoped<ISistemBildirimService, SistemBildirimManager>();
            services.AddScoped<IOutboxMesajService, OutboxMesajManager>();
            services.AddScoped<IWebhookAbonelikService, WebhookAbonelikManager>();

            return services;
        }
    }
}
