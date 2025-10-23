import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import apiService from '../services/api';

function MovieDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [movie, setMovie] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchMovieDetail();
  }, [id]);

  const fetchMovieDetail = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiService.fetchMovieDetail(id);
      setMovie(response);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
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
        <button className="back-button" onClick={() => navigate('/')}>
          ← Back to Movies
        </button>
        <div className="loading">Loading movie details...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <button className="back-button" onClick={() => navigate('/')}>
          ← Back to Movies
        </button>
        <div className="error">
          Error loading movie: {error}
          <button onClick={fetchMovieDetail} style={{ marginLeft: '10px', padding: '5px 10px' }}>
            Retry
          </button>
        </div>
      </div>
    );
  }

  if (!movie) {
    return (
      <div>
        <button className="back-button" onClick={() => navigate('/')}>
          ← Back to Movies
        </button>
        <div className="loading">Movie not found</div>
      </div>
    );
  }

  const sortedPrices = movie.pricesByProvider 
    ? Object.entries(movie.pricesByProvider).sort(([,a], [,b]) => a - b)
    : [];

  return (
    <div>
      <button className="back-button" onClick={() => navigate('/')}>
        ← Back to Movies
      </button>

      <div className="movie-detail">
        <div className="movie-detail-header">
          {movie.poster && (
            <img
              src={movie.poster}
              alt={movie.title}
              className="movie-detail-poster"
              onError={(e) => {
                e.target.style.display = 'none';
              }}
            />
          )}
          
          <div className="movie-detail-info">
            <h1>{movie.title}</h1>
            <h2>{movie.year}</h2>
            
            {movie.rated && <p><strong>Rated:</strong> {movie.rated}</p>}
            {movie.released && <p><strong>Released:</strong> {movie.released}</p>}
            {movie.runtime && <p><strong>Runtime:</strong> {movie.runtime}</p>}
            {movie.genre && <p><strong>Genre:</strong> {movie.genre}</p>}
            {movie.director && <p><strong>Director:</strong> {movie.director}</p>}
            {movie.actors && <p><strong>Actors:</strong> {movie.actors}</p>}
            {movie.plot && <p><strong>Plot:</strong> {movie.plot}</p>}
          </div>
        </div>

        {movie.pricesByProvider && Object.keys(movie.pricesByProvider).length > 0 && (
          <div className="detail-price-section">
            <h3>Pricing Information</h3>
            {movie.cheapestPrice && (
              <p>
                <strong>Best Price:</strong> {formatPrice(movie.cheapestPrice)} from {getProviderDisplayName(movie.cheapestProvider)}
              </p>
            )}
            
            <div className="detail-price-grid">
              {sortedPrices.map(([provider, price], index) => (
                <div
                  key={provider}
                  className={`detail-price-item ${index === 0 ? 'cheapest' : ''}`}
                >
                  <div className="provider-name">{getProviderDisplayName(provider)}</div>
                  <div className="price">{formatPrice(price)}</div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default MovieDetail;
