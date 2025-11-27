// ApiSettings.cs
// =============================================================
// appsettings.json'daki "Api": { "BaseUrl": "..." } bölümünü
// Options pattern ile C# sınıfına bağlıyoruz.
// =============================================================


namespace TurkSoft.GIBWebUI.AppSettings
{
    /// <summary>ERP API kök adresi (örn: https://gibapi.noxmusavir.com/api/v1)</summary>
    public sealed class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty; //Boş gelirse build-time’da fark edelim
        public string GibPortalApiBaseUrl { get; set; }  // yeni
    }
}
