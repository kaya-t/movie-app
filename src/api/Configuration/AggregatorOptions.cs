namespace MovieApp.Api.Configuration
{
    // Bound to the "Webjet" section in appsettings.json
    public class AggregatorOptions
    {
        // minutes
        public int ListTtlMinutes { get; set; } = 3;
        public int DetailTtlMinutes { get; set; } = 10;

        // max parallel detail calls per provider
        public int MaxDetailConcurrency { get; set; } = 6;
    }
}
