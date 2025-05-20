namespace AuthService.Constants
{
    public class NetSuiteConstants
    {
        public const string AuthorizationUrlFormat = "https://{0}.app.netsuite.com/app/login/oauth2/authorize.nl";
        public const string TokenUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/auth/oauth2/v1/token";
        public const string BaseRestApiUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/record/v1";
        public const string BaseSuiteQLApiUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/query/v1/suiteql";

        /* OAuth 2.0 */
        public static string GetAuthorizationUrl(string account) => string.Format(AuthorizationUrlFormat, account);
        public static string GetTokenUrl(string account) => string.Format(TokenUrlFormat, account);
        public static string GetRestApiBaseUrl(string account) => string.Format(BaseRestApiUrlFormat, account);
        public static string GetCustomersUrl(string account) => $"{GetRestApiBaseUrl(account)}/customer";
        public static string GetEmployeesUrl(string account) => $"{GetRestApiBaseUrl(account)}/employee";
        /* OAuth 1.0 */
        public static string GetSuiteQLApiBaseUrl(string account) => string.Format(BaseSuiteQLApiUrlFormat, account);

        /* LDAP */
        public static readonly List<string> excludeWords = new List<string> {
            "3MGSR",
            "BPC",
            "BSCSurat",
            "btlfl",
            "CSS Temp",
            "Driver",
            "Fax",
            "FTL",
            "Guest",
            "HRM",
            "Helpdesk",
            "Import",
            "Mail",
            "Meeting Zoom Room",
            "noreply",
            "noreply2",
            "Operator",
            "Payroll",
            "salecen",
            "Test",
            "WH-",
            "Zoom"
        };

    }
}