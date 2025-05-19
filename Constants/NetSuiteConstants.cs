namespace AuthService.Constants
{
    public class NetSuiteConstants
    {
        public const string AuthorizationUrlFormat = "https://{0}.app.netsuite.com/app/login/oauth2/authorize.nl";
        public const string TokenUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/auth/oauth2/v1/token";
        public const string BaseApiUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/record/v1";

        public static string GetAuthorizationUrl(string account) => string.Format(AuthorizationUrlFormat, account);
        public static string GetTokenUrl(string account) => string.Format(TokenUrlFormat, account);
        public static string GetApiBaseUrl(string account) => string.Format(BaseApiUrlFormat, account);
        public static string GetCustomersUrl(string account) => $"{GetApiBaseUrl(account)}/customer";
        public static string GetEmployeesUrl(string account) => $"{GetApiBaseUrl(account)}/employee";

        public static readonly List<string> excludeWords = new List<string> {
            "BPC",
            "CSS Temp",
            "Driver",
            "Guest",
            "Mail",
            "Meeting Zoom Room",
            "noreply",
            "noreply2",
            "Test",
            "Zoom"
        };
    }
}