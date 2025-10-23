import apiService from '../services/api';

global.fetch = jest.fn();

describe('API Service', () => {
  beforeEach(() => {
    fetch.mockClear();
  });

  describe('fetchMovies', () => {
    test('fetches movies successfully', async () => {
      const mockMovies = {
        movies: [
          { id: '1', title: 'Movie 1' },
          { id: '2', title: 'Movie 2' }
        ]
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockMovies
      });

      const result = await apiService.fetchMovies();

      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/movies');
      expect(result).toEqual(mockMovies);
    });

    test('handles HTTP error responses', async () => {
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 500
      });

      await expect(apiService.fetchMovies()).rejects.toThrow('HTTP error! status: 500');
    });

    test('handles network errors', async () => {
      fetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(apiService.fetchMovies()).rejects.toThrow('Network error');
    });

    test('uses correct API endpoint', async () => {
      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ movies: [] })
      });

      await apiService.fetchMovies();

      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/movies');
    });
  });

  describe('fetchMovieDetail', () => {
    test('fetches movie detail successfully', async () => {
      const mockMovie = {
        id: 'test-id',
        title: 'Test Movie',
        year: '2023'
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockMovie
      });

      const result = await apiService.fetchMovieDetail('test-id');

      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/movies/test-id');
      expect(result).toEqual(mockMovie);
    });

    test('handles 404 error specifically', async () => {
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 404
      });

      await expect(apiService.fetchMovieDetail('nonexistent')).rejects.toThrow('Movie not found');
    });

    test('handles other HTTP error responses', async () => {
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 500
      });

      await expect(apiService.fetchMovieDetail('test-id')).rejects.toThrow('HTTP error! status: 500');
    });

    test('handles network errors', async () => {
      fetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(apiService.fetchMovieDetail('test-id')).rejects.toThrow('Network error');
    });

    test('uses correct API endpoint with movie ID', async () => {
      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: 'test-id' })
      });

      await apiService.fetchMovieDetail('test-id');

      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/movies/test-id');
    });
  });

  describe('API configuration', () => {
    test('uses default API base URL', () => {
      const originalEnv = process.env.REACT_APP_API_URL;
      delete process.env.REACT_APP_API_URL;

      jest.resetModules();
      const freshApiService = require('../services/api').default;

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ movies: [] })
      });

      freshApiService.fetchMovies();

      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/movies');

      if (originalEnv) {
        process.env.REACT_APP_API_URL = originalEnv;
      }
    });

    test('uses custom API base URL from environment', () => {
      const originalEnv = process.env.REACT_APP_API_URL;
      process.env.REACT_APP_API_URL = 'https://custom-api.com/api';

      jest.resetModules();
      const freshApiService = require('../services/api').default;

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ movies: [] })
      });

      freshApiService.fetchMovies();

      expect(fetch).toHaveBeenCalledWith('https://custom-api.com/api/movies');

      if (originalEnv) {
        process.env.REACT_APP_API_URL = originalEnv;
      } else {
        delete process.env.REACT_APP_API_URL;
      }
    });
  });
});
