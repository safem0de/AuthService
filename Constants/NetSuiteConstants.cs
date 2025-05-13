namespace AuthService.Constants
{
    public class NetSuiteConstants
    {
        public const string AuthorizationUrlFormat = "https://{0}.app.netsuite.com/app/login/oauth2/authorize";
        public const string TokenUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/auth/oauth2/v1/token";
        public const string BaseApiUrlFormat = "https://{0}.suitetalk.api.netsuite.com/services/rest/record/v1";
    }
}