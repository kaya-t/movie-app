import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import MovieList from '../components/MovieList';
import apiService from '../services/api';

jest.mock('../services/api');
const mockApiService = apiService;

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

const renderWithRouter = (component) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

const mockMovies = [
  {
    id: 'movie-1',
    title: 'Star Wars: Episode IV - A New Hope',
    year: '1977',
    poster: 'https://example.com/poster1.jpg',
    cheapestPrice: 29.5,
    cheapestProvider: 'filmworld',
    pricesByProvider: {
      cinemaworld: 123.5,
      filmworld: 29.5
    }
  },
  {
    id: 'movie-2',
    title: 'The Empire Strikes Back',
    year: '1980',
    poster: 'https://example.com/poster2.jpg',
    cheapestPrice: 31.0,
    cheapestProvider: 'filmworld',
    pricesByProvider: {
      cinemaworld: 125.0,
      filmworld: 31.0
    }
  }
];

describe('MovieList Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders loading state initially', () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: [] });
    
    renderWithRouter(<MovieList />);
    
    expect(screen.getByText('Loading movies...')).toBeInTheDocument();
    expect(screen.getByText('Movie App')).toBeInTheDocument();
  });

  test('renders movies when data is loaded', async () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: mockMovies });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
      expect(screen.getByText('The Empire Strikes Back')).toBeInTheDocument();
    });
  });

  test('displays movie information correctly', async () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: [mockMovies[0]] });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
      expect(screen.getByText('1977')).toBeInTheDocument();
      expect(screen.getAllByText('$29.50')).toHaveLength(2);
      expect(screen.getByText('Best price from Film World')).toBeInTheDocument();
    });
  });

  test('handles movie click navigation', async () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: [mockMovies[0]] });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      const movieCard = screen.getByText('Star Wars: Episode IV - A New Hope').closest('.movie-card');
      fireEvent.click(movieCard);
    });
    
    expect(mockNavigate).toHaveBeenCalledWith('/movie/movie-1');
  });

  test('displays price comparison when multiple providers', async () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: [mockMovies[0]] });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('All prices:')).toBeInTheDocument();
      expect(screen.getByText('Film World')).toBeInTheDocument();
      expect(screen.getByText('Cinema World')).toBeInTheDocument();
    });
  });

  test('handles error state', async () => {
    mockApiService.fetchMovies.mockRejectedValue(new Error('API Error'));
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('Error loading movies: API Error')).toBeInTheDocument();
      expect(screen.getByText('Retry')).toBeInTheDocument();
    });
  });

  test('retry button works on error', async () => {
    mockApiService.fetchMovies
      .mockRejectedValueOnce(new Error('API Error'))
      .mockResolvedValueOnce({ movies: mockMovies });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('Error loading movies: API Error')).toBeInTheDocument();
    });
    
    fireEvent.click(screen.getByText('Retry'));
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
    });
  });

  test('displays no movies message when empty', async () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: [] });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('No movies available')).toBeInTheDocument();
    });
  });

  test('formats prices correctly', async () => {
    mockApiService.fetchMovies.mockResolvedValue({ movies: [mockMovies[0]] });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getAllByText('$29.50')).toHaveLength(2);
      expect(screen.getByText('$123.50')).toBeInTheDocument();
    });
  });

  test('handles missing poster gracefully', async () => {
    const movieWithoutPoster = {
      ...mockMovies[0],
      poster: null
    };
    mockApiService.fetchMovies.mockResolvedValue({ movies: [movieWithoutPoster] });
    
    renderWithRouter(<MovieList />);
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
    });
  });
});
