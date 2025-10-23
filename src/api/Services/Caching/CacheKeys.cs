namespace MovieApp.Api.Services.Caching
{
    public static class CacheConstants
    {
        public const string ListKey = "list";
        public static string DetailKey(string provider, string id) => $"detail:{provider}:{id}";
    }
}
