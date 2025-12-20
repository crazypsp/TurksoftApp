namespace TurkSoft.BankWebUI.AppSettings
{
    public sealed class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKeyHeaderName { get; set; } = "X-API-KEY";
        public string ApiKey { get; set; } = string.Empty;
    }
}
