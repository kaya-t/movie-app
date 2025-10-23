using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieApp.Api.Domain
{
    /// <summary>
    /// Provider list response DTO (raw external shape).
    /// </summary>
    public class MovieListResponse
    {
        public List<MovieSummary> Movies { get; set; } = new();
    }

    /// <summary>
    /// Minimal provider list item (Cinemaworld/Filmworld).
    /// </summary>
    public class MovieSummary
    {
        public string ID { get; set; } = "";
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Type { get; set; } = "";   // often "movie"
        public string Poster { get; set; } = ""; // url
    }

    /// <summary>
    /// Provider detail DTO (includes price).
    /// </summary>
    public class MovieDetail
    {
        public string ID { get; set; } = "";
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Rated { get; set; } = "";
        public string Released { get; set; } = "";
        public string Runtime { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Director { get; set; } = "";
        public string Writer { get; set; } = "";
        public string Actors { get; set; } = "";
        public string Plot { get; set; } = "";
        public string Language { get; set; } = "";
        public string Country { get; set; } = "";
        public string Awards { get; set; } = "";
        public string Poster { get; set; } = "";
        public string Metascore { get; set; } = "";
        public string Rating { get; set; } = "";
        public string Votes { get; set; } = "";
        public string Type { get; set; } = "";
        
        [JsonConverter(typeof(DecimalStringConverter))]
        public decimal Price { get; set; }
    }

    public class DecimalStringConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (decimal.TryParse(reader.GetString(), out decimal result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            
            throw new JsonException($"Unable to convert to decimal. TokenType: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
