namespace AuthService.Utils
{
    public static class Helpers
    {
        public static string GetSafe(this Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var val) ? val?.ToString() ?? "" : "";
        }
    }
}