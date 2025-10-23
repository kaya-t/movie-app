using MovieApp.Api.Domain;
using MovieApp.Api.Configuration;

namespace MovieApp.Api.Tests.Utilities
{
    public static class TestDataBuilder
    {
        public static List<MovieSummary> GetCinemaworldMovies()
        {
            return new List<MovieSummary>
            {
                new MovieSummary
                {
                    ID = "cw0076759",
                    Title = "Star Wars: Episode IV - A New Hope",
                    Year = "1977",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BOTIyMDY2NGQtOGJjNi00OTk4LWFhMDgtYmE3M2NiYzM0YTVmXkEyXkFqcGdeQXVyNTU1NTcwOTk@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "cw0080684",
                    Title = "Star Wars: Episode V - The Empire Strikes Back",
                    Year = "1980",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMjE2MzQwMTgxN15BMl5BanBnXkFtZTcwMDQzNjk2OQ@@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "cw0086190",
                    Title = "Star Wars: Episode VI - Return of the Jedi",
                    Year = "1983",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMTQ0MzI1NjYwOF5BMl5BanBnXkFtZTgwODU3NDU2MTE@._V1._CR93,97,1209,1861_SX89_AL_.jpg_V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "cw2488496",
                    Title = "Star Wars: The Force Awakens",
                    Year = "2015",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BOTAzODEzNDAzMl5BMl5BanBnXkFtZTgwMDU1MTgzNzE@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "cw0120915",
                    Title = "Star Wars: Episode I - The Phantom Menace",
                    Year = "1999",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMTQ4NjEwNDA2Nl5BMl5BanBnXkFtZTcwNDUyNDQzNw@@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "cw0121766",
                    Title = "Star Wars: Episode III - Revenge of the Sith",
                    Year = "2005",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BNTc4MTc3NTQ5OF5BMl5BanBnXkFtZTcwOTg0NjI4NA@@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "cw0121765",
                    Title = "Star Wars: Episode II - Attack of the Clones",
                    Year = "2002",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMTY5MjI5NTIwNl5BMl5BanBnXkFtZTYwMTM1Njg2._V1_SX300.jpg"
                }
            };
        }

        public static List<MovieSummary> GetFilmworldMovies()
        {
            return new List<MovieSummary>
            {
                new MovieSummary
                {
                    ID = "fw0076759",
                    Title = "Star Wars: Episode IV - A New Hope",
                    Year = "1977",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BOTIyMDY2NGQtOGJjNi00OTk4LWFhMDgtYmE3M2NiYzM0YTVmXkEyXkFqcGdeQXVyNTU1NTfwOTk@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "fw0080684",
                    Title = "Star Wars: Episode V - The Empire Strikes Back",
                    Year = "1980",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMjE2MzQwMTgxN15BMl5BanBnXkFtZTfwMDQzNjk2OQ@@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "fw0086190",
                    Title = "Star Wars: Episode VI - Return of the Jedi",
                    Year = "1983",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMTQ0MzI1NjYwOF5BMl5BanBnXkFtZTgwODU3NDU2MTE@._V1._CR93,97,1209,1861_SX89_AL_.jpg_V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "fw0120915",
                    Title = "Star Wars: Episode I - The Phantom Menace",
                    Year = "1999",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMTQ4NjEwNDA2Nl5BMl5BanBnXkFtZTfwNDUyNDQzNw@@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "fw0121766",
                    Title = "Star Wars: Episode III - Revenge of the Sith",
                    Year = "2005",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BNTc4MTc3NTQ5OF5BMl5BanBnXkFtZTfwOTg0NjI4NA@@._V1_SX300.jpg"
                },
                new MovieSummary
                {
                    ID = "fw0121765",
                    Title = "Star Wars: Episode II - Attack of the Clones",
                    Year = "2002",
                    Type = "movie",
                    Poster = "https://m.media-amazon.com/images/M/MV5BMTY5MjI5NTIwNl5BMl5BanBnXkFtZTYwMTM1Njg2._V1_SX300.jpg"
                }
            };
        }

        public static MovieSummary CreateMovieSummary(string id = "cw123", string title = "Test Movie", string year = "2023")
        {
            return new MovieSummary
            {
                ID = id,
                Title = title,
                Year = year,
                Type = "movie",
                Poster = "https://example.com/poster.jpg"
            };
        }

        public static MovieDetail CreateCinemaworldStarWarsDetail(string id, decimal price)
        {
            return new MovieDetail
            {
                ID = id,
                Title = "Star Wars: Episode IV - A New Hope",
                Year = "1977",
                Rated = "PG",
                Released = "25 May 1977",
                Runtime = "121 min",
                Genre = "Action, Adventure, Fantasy",
                Director = "George Lucas",
                Writer = "George Lucas",
                Actors = "Mark Hamill, Harrison Ford, Carrie Fisher, Peter Cushing",
                Plot = "Luke Skywalker joins forces with a Jedi Knight, a cocky pilot, a wookiee and two droids to save the galaxy from the Empire's world-destroying battle-station, while also attempting to rescue Princess Leia from the evil Darth Vader.",
                Language = "English",
                Country = "USA",
                Awards = "Won 6 Oscars. Another 48 wins & 28 nominations.",
                Poster = "https://m.media-amazon.com/images/M/MV5BOTIyMDY2NGQtOGJjNi00OTk4LWFhMDgtYmE3M2NiYzM0YTVmXkEyXkFqcGdeQXVyNTU1NTcwOTk@._V1_SX300.jpg",
                Metascore = "92",
                Rating = "8.7",
                Votes = "915,459",
                Type = "movie",
                Price = price
            };
        }

        public static MovieDetail CreateFilmworldStarWarsDetail(string id, decimal price)
        {
            return new MovieDetail
            {
                ID = id,
                Title = "Star Wars: Episode IV - A New Hope",
                Year = "1977",
                Rated = "PG",
                Released = "25 May 1977",
                Runtime = "121 min",
                Genre = "Action, Adventure, Fantasy",
                Director = "George Lucas",
                Writer = "George Lucas",
                Actors = "Mark Hamill, Harrison Ford, Carrie Fisher, Peter Cushing",
                Plot = "Luke Skywalker joins forces with a Jedi Knight, a cocky pilot, a wookiee and two droids to save the galaxy from the Empire's world-destroying battle-station, while also attempting to rescue Princess Leia from the evil Darth Vader.",
                Language = "English",
                Country = "USA",
                Poster = "https://m.media-amazon.com/images/M/MV5BOTIyMDY2NGQtOGJjNi00OTk4LWFhMDgtYmE3M2NiYzM0YTVmXkEyXkFqcGdeQXVyNTU1NTfwOTk@._V1_SX300.jpg",
                Metascore = "92",
                Rating = "8.7",
                Votes = "915,459",
                Type = "movie",
                Price = price
            };
        }


        public static MovieItemResponse CreateMovieItemResponse(string title = "Test Movie", decimal? cheapestPrice = 10.99m)
        {
            return new MovieItemResponse
            {
                Id = title,
                Title = title,
                Year = "2023",
                Type = "movie",
                Poster = "https://example.com/poster.jpg",
                CheapestPrice = cheapestPrice,
                CheapestProvider = cheapestPrice.HasValue ? "cinemaworld" : null,
                PricesByProvider = cheapestPrice.HasValue 
                    ? new Dictionary<string, decimal> { ["cinemaworld"] = cheapestPrice.Value }
                    : new Dictionary<string, decimal>()
            };
        }

        public static ProviderOptions CreateProviderOptions(string apiToken = "test-token")
        {
            return new ProviderOptions
            {
                ApiToken = apiToken,
                Cinemaworld = new ProviderConfig
                {
                    BaseUrl = "https://test-cinemaworld.com/api"
                },
                Filmworld = new ProviderConfig
                {
                    BaseUrl = "https://test-filmworld.com/api"
                }
            };
        }

        public static AggregatorOptions CreateAggregatorOptions()
        {
            return new AggregatorOptions
            {
                ListTtlMinutes = 5,
                DetailTtlMinutes = 10,
                MaxDetailConcurrency = 3
            };
        }
    }
}
