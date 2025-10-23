import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiService from '../services/api';

function MovieList() {
  const [movies, setMovies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    fetchMovies();
  }, []);

  const fetchMovies = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiService.fetchMovies();
      setMovies(response.movies || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleMovieClick = (movieId) => {
    navigate(`/movie/${encodeURIComponent(movieId)}`);
  };

  const formatPrice = (price) => {
    return `$${price.toFixed(2)}`;
  };

  const getProviderDisplayName = (provider) => {
    return provider === 'cinemaworld' ? 'Cinema World' : 'Film World';
  };

  if (loading) {
    return (
      <div>
        <div className="header">
          <h1>Movie App</h1>
          <p>Find the best movie prices across providers</p>
        </div>
        <div className="loading">Loading movies...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <div className="header">
          <h1>Movie App</h1>
          <p>Find the best movie prices across providers</p>
        </div>
        <div className="error">
          Error loading movies: {error}
          <button onClick={fetchMovies} style={{ marginLeft: '10px', padding: '5px 10px' }}>
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="header">
        <h1>Movie App</h1>
        <p>Find the best movie prices across providers</p>
      </div>

      {movies.length === 0 ? (
        <div className="loading">No movies available</div>
      ) : (
        <div className="movie-grid">
          {movies.map((movie) => (
            <div
              key={movie.id}
              className="movie-card"
              onClick={() => handleMovieClick(movie.id)}
            >
              {movie.poster && (
                <img
                  src={movie.poster}
                  alt={movie.title}
                  className="movie-poster"
                  onError={(e) => {
                    e.target.style.display = 'none';
                  }}
                />
              )}
              
              <div className="movie-title">{movie.title}</div>
              <div className="movie-year">{movie.year}</div>
              
              <div className="price-section">
                {movie.cheapestPrice && (
                  <>
                    <div className="cheapest-price">
                      {formatPrice(movie.cheapestPrice)}
                    </div>
                    <div className="cheapest-provider">
                      Best price from {getProviderDisplayName(movie.cheapestProvider)}
                    </div>
                  </>
                )}
                
                {movie.pricesByProvider && Object.keys(movie.pricesByProvider).length > 1 && (
                  <div className="price-comparison">
                    <div style={{ marginBottom: '5px', fontWeight: '600' }}>All prices:</div>
                    {Object.entries(movie.pricesByProvider)
                      .sort(([,a], [,b]) => a - b)
                      .map(([provider, price]) => (
                        <div key={provider} className="price-item">
                          <span>{getProviderDisplayName(provider)}</span>
                          <span>{formatPrice(price)}</span>
                        </div>
                      ))}
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default MovieList;
