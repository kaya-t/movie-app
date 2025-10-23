namespace MovieApp.Api.Domain
{
    /// <summary>
    /// Compact movie item for lists. Contains selected external fields + aggregated pricing.
    /// </summary>
    public class MovieItemResponse
    {
        // ----- External API fields (from providers) -----
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Type { get; set; } = "";
        public string Poster { get; set; } = "";

        // ----- Aggregated/computed fields (from our service) -----
        // We treat Title as our stable identifier for this assignment.
        public string Id { get; set; } = "";

        public decimal? CheapestPrice { get; set; }
        public string? CheapestProvider { get; set; }
        public Dictionary<string, decimal> PricesByProvider { get; set; } = new();
        
        // Store provider IDs for detail lookups
        public Dictionary<string, string> ProviderIds { get; set; } = new();
    }

    public class MoviesResponse
    {
        public List<MovieItemResponse> Movies { get; set; } = new();
    }

    /// <summary>
    /// Rich detail model (inherits list fields + adds more metadata when available).
    /// </summary>
    public class MovieDetailResponse : MovieItemResponse
    {
        public string Rated { get; set; } = "";
        public string Released { get; set; } = "";
        public string Runtime { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Director { get; set; } = "";
        public string Actors { get; set; } = "";
        public string Plot { get; set; } = "";
        // Poster inherited from MovieItemResponse
    }
}
