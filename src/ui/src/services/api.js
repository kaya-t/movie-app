const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

class ApiService {
  async fetchMovies() {
    try {
      const response = await fetch(`${API_BASE_URL}/movies`);
      if (!response.ok) {
        throw new Error(`HTTP error: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Error fetching movies:', error);
      throw error;
    }
  }

  async fetchMovieDetail(id) {
    try {
      const response = await fetch(`${API_BASE_URL}/movies/${id}`);
      if (!response.ok) {
        if (response.status === 404) {
          throw new Error('Movie not found');
        }
        throw new Error(`HTTP error: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Error fetching movie detail:', error);
      throw error;
    }
  }
}

export default new ApiService();
