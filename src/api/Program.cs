using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using MovieApp.Api.Domain;
using MovieApp.Api.Configuration;
using MovieApp.Api.Providers;
using MovieApp.Api.Services;
using MovieApp.Api.Filters;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod())
);

builder.Services.Configure<ProviderOptions>(builder.Configuration.GetSection("Providers"));
builder.Services.Configure<AggregatorOptions>(builder.Configuration.GetSection("Webjet"));

var jitterDelays = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(150), retryCount: 2);
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(jitterDelays);
var perTryTimeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3));
var policyWrap = Policy.WrapAsync(retryPolicy, perTryTimeout);

Action<IServiceProvider, HttpClient> configureHttp = (sp, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(5);
    http.DefaultRequestHeaders.UserAgent.ParseAdd("MovieApp.Api/1.0");
    http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    http.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
};

Func<HttpMessageHandler> handlerFactory = () => new SocketsHttpHandler
{
    AutomaticDecompression = DecompressionMethods.GZip 
                           | DecompressionMethods.Deflate 
                           | DecompressionMethods.Brotli,
    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
};

builder.Services
  .AddHttpClient<CinemaworldClient>(configureHttp)
  .ConfigurePrimaryHttpMessageHandler(handlerFactory)
  .AddPolicyHandler(policyWrap);

builder.Services
  .AddHttpClient<FilmworldClient>(configureHttp)
  .ConfigurePrimaryHttpMessageHandler(handlerFactory)
  .AddPolicyHandler(policyWrap);

builder.Services.AddScoped<IMovieProviderClient, CinemaworldClient>();
builder.Services.AddScoped<IMovieProviderClient, FilmworldClient>();
builder.Services.AddScoped<IMovieAggregatorService, MovieAggregatorService>();

var app = builder.Build();

app.UseCors();


app.MapControllers();
app.Run();

public partial class Program { }
