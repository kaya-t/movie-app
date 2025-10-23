# Movie Price Comparison App

A web app that compares movie prices from Cinemaworld and Filmworld providers, showing the cheapest option for each movie.
(url)http://webjetapitest.azurewebsites.net(/url)

## Tech Stack
- **Backend**: .NET 8 Web API, C#
- **Frontend**: React 18, JavaScript
- **Testing**: xUnit, Jest, React Testing Library
- **HTTP**: Polly retry policies, HttpClient
- **Caching**: IMemoryCache

## Project Structure
```
movie-app/
├── src/
│   ├── api/                           # .NET Web API
│   │   ├── api.tests/                 # Backend tests
│   │   │   ├── Integration/
│   │   │   │   └── MoviesControllerIntegrationTests.cs
│   │   │   ├── Unit/
│   │   │   │   ├── Controllers/
│   │   │   │   │   └── MoviesControllerTests.cs
│   │   │   │   ├── Filters/
│   │   │   │   │   └── GlobalExceptionFilterTests.cs
│   │   │   │   └── Services/
│   │   │   │       └── MovieAggregatorServiceTests.cs
│   │   │   ├── Utilities/
│   │   │   │   └── TestDataBuilder.cs
│   │   │   └── MovieApp.Api.Tests.csproj
│   │   ├── Configuration/
│   │   │   ├── AggregatorOptions.cs
│   │   │   └── ProviderOptions.cs
│   │   ├── Controllers/
│   │   │   └── MoviesController.cs
│   │   ├── Domain/
│   │   │   ├── ApiModels.cs
│   │   │   ├── ProviderDtos.cs
│   │   │   └── ValueObjects.cs
│   │   ├── Exceptions/
│   │   │   ├── MovieNotFoundException.cs
│   │   │   └── ProviderException.cs
│   │   ├── Filters/
│   │   │   └── GlobalExceptionFilter.cs
│   │   ├── Providers/
│   │   │   ├── CinemaworldClient.cs
│   │   │   ├── FilmworldClient.cs
│   │   │   └── IMovieProviderClient.cs
│   │   ├── Services/
│   │   │   ├── Caching/
│   │   │   │   └── CacheKeys.cs
│   │   │   ├── IMovieAggregatorService.cs
│   │   │   └── MovieAggregatorService.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── MovieApp.Api.csproj
│   │   └── Program.cs
│   └── ui/                            # React app
│       ├── public/
│       │   └── index.html
│       ├── src/
│       │   ├── __tests__/
│       │   │   ├── App.test.js
│       │   │   ├── MovieDetail.test.js
│       │   │   ├── MovieList.test.js
│       │   │   ├── api.test.js
│       │   │   └── utils.test.js
│       │   ├── components/
│       │   │   ├── MovieDetail.js
│       │   │   └── MovieList.js
│       │   ├── services/
│       │   │   └── api.js
│       │   ├── App.js
│       │   ├── index.css
│       │   ├── index.js
│       │   └── setupTests.js
│       ├── package.json
│       ├── package-lock.json
│       └── README.md
├── CODE.md
└── README.md
```

## Setup

```bash
# 1. Set API token
export WEBJET_API_TOKEN="your_token_here"

# 2. Start backend
cd src/api
dotnet build
dotnet run

# 3. Start frontend (new terminal)
cd src/ui
npm install
npm start
```

## API Endpoints
- **Movies**: `GET /api/movies` - List all movies from a movie provider
- **Movie Detail**: `GET /api/movies/{id}` - Get specific movie details

## Features
- **Fault Tolerance**: Works even if providers are down
- **Price Comparison**: Shows cheapest price across providers
- **Fast Loading**: Caching makes pages load quickly
- **Parallel Processing**: Fetches data from multiple providers at once
- **Memory Caching**: Stores movie lists for 3 minutes, details for 10 minutes
- **Error Recovery**: Automatically retries failed requests
- **Timeout Protection**: Won't hang on slow provider responses
- **Efficient Data**: Only fetches what's needed, when needed

## Running Tests

```bash
# Backend tests
cd src/api/api.tests
dotnet test

# Frontend tests
cd src/ui
npm test
```

## Setup and start application

```bash
# 1. Set API token
export WEBJET_API_TOKEN="your_token_here"

# 2. Start backend (Terminal 1)
cd src/api
dotnet run

# 3. Start frontend (Terminal 2)
cd src/ui
npm install
npm start
```

## Access
- Frontend: http://localhost:3000
- API: http://localhost:5000/api