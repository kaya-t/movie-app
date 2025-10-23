namespace MovieApp.Api.Domain
{
    public record MoviePrice(string Provider, decimal Price);

    public record CheapestResult(string? Provider, decimal? Price);
}
