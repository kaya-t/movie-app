namespace MovieApp.Api.Configuration
{
    public class ProviderOptions
    {
        public string ApiToken { get; set; } = "";
        public ProviderConfig Cinemaworld { get; set; } = new();
        public ProviderConfig Filmworld { get; set; } = new();
    }

    public class ProviderConfig
    {
        public string BaseUrl { get; set; } = "";
    }
}
