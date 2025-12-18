//namespace Turksoft.BankServiceAPI.Security
//{
//    public sealed class ApiKeyOptions
//    {
//        public string HeaderName { get; set; } = "X-API-KEY";
//        public string Key { get; set; } = "TS_DEFAULT_KEY_CHANGE_ME_2025";
//    }
//}


namespace Turksoft.BankServiceAPI.Security
{
    public sealed class ApiKeyOptions
    {
        public string HeaderName { get; set; } = "X-API-KEY";
        public string Key { get; set; } = string.Empty;
    }
}