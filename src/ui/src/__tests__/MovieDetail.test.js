import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import MovieDetail from '../components/MovieDetail';
import apiService from '../services/api';

jest.mock('../services/api');
const mockApiService = apiService;

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useParams: () => ({ id: 'test-movie-id' }),
  useNavigate: () => mockNavigate,
}));

const renderWithRouter = (component) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

const mockMovieDetail = {
  id: 'test-movie-id',
  title: 'Star Wars: Episode IV - A New Hope',
  year: '1977',
  poster: 'https://example.com/poster.jpg',
  rated: 'PG',
  released: '25 May 1977',
  runtime: '121 min',
  genre: 'Action, Adventure, Fantasy',
  director: 'George Lucas',
  actors: 'Mark Hamill, Harrison Ford, Carrie Fisher',
  plot: 'Luke Skywalker joins forces with a Jedi Knight...',
  cheapestPrice: 29.5,
  cheapestProvider: 'filmworld',
  pricesByProvider: {
    cinemaworld: 123.5,
    filmworld: 29.5
  }
};

describe('MovieDetail Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders loading state initially', () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    expect(screen.getByText('Loading movie details...')).toBeInTheDocument();
    expect(screen.getByText('← Back to Movies')).toBeInTheDocument();
  });

  test('renders movie details when data is loaded', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
      expect(screen.getByText('1977')).toBeInTheDocument();
      expect(screen.getByText('PG')).toBeInTheDocument();
      expect(screen.getByText('25 May 1977')).toBeInTheDocument();
      expect(screen.getByText('121 min')).toBeInTheDocument();
      expect(screen.getByText('Action, Adventure, Fantasy')).toBeInTheDocument();
      expect(screen.getByText('George Lucas')).toBeInTheDocument();
      expect(screen.getByText('Mark Hamill, Harrison Ford, Carrie Fisher')).toBeInTheDocument();
    });
  });

  test('displays pricing information correctly', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Pricing Information')).toBeInTheDocument();
      expect(screen.getByText('Best Price:')).toBeInTheDocument();
      expect(screen.getByText('$29.50')).toBeInTheDocument();
      expect(screen.getByText('Film World')).toBeInTheDocument();
      expect(screen.getByText('Cinema World')).toBeInTheDocument();
      expect(screen.getByText('$123.50')).toBeInTheDocument();
    });
  });

  test('handles back button navigation', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      const backButton = screen.getByText('← Back to Movies');
      fireEvent.click(backButton);
    });
    
    expect(mockNavigate).toHaveBeenCalledWith('/');
  });

  test('handles error state', async () => {
    mockApiService.fetchMovieDetail.mockRejectedValue(new Error('Movie not found'));
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Error loading movie: Movie not found')).toBeInTheDocument();
      expect(screen.getByText('Retry')).toBeInTheDocument();
    });
  });

  test('retry button works on error', async () => {
    mockApiService.fetchMovieDetail
      .mockRejectedValueOnce(new Error('API Error'))
      .mockResolvedValueOnce(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Error loading movie: API Error')).toBeInTheDocument();
    });
    
    fireEvent.click(screen.getByText('Retry'));
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
    });
  });

  test('displays movie not found when no movie data', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(null);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Movie not found')).toBeInTheDocument();
    });
  });

  test('handles missing optional fields gracefully', async () => {
    const movieWithMinimalData = {
      id: 'test-movie-id',
      title: 'Test Movie',
      year: '2023',
      pricesByProvider: {
        filmworld: 25.0
      }
    };
    
    mockApiService.fetchMovieDetail.mockResolvedValue(movieWithMinimalData);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Test Movie')).toBeInTheDocument();
      expect(screen.getByText('2023')).toBeInTheDocument();
    });
  });

  test('sorts prices correctly', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      const priceElements = screen.getAllByText(/\$\d+\.\d+/);
      expect(priceElements).toHaveLength(3);
      expect(priceElements[0]).toHaveTextContent('$29.50');
      expect(priceElements[1]).toHaveTextContent('$29.50');
      expect(priceElements[2]).toHaveTextContent('$123.50');
    });
  });

  test('handles missing poster gracefully', async () => {
    const movieWithoutPoster = {
      ...mockMovieDetail,
      poster: null
    };
    
    mockApiService.fetchMovieDetail.mockResolvedValue(movieWithoutPoster);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Star Wars: Episode IV - A New Hope')).toBeInTheDocument();
    });
  });

  test('formats prices correctly', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('$29.50')).toBeInTheDocument();
      expect(screen.getByText('$123.50')).toBeInTheDocument();
    });
  });

  test('displays provider names correctly', async () => {
    mockApiService.fetchMovieDetail.mockResolvedValue(mockMovieDetail);
    
    renderWithRouter(<MovieDetail />);
    
    await waitFor(() => {
      expect(screen.getByText('Film World')).toBeInTheDocument();
      expect(screen.getByText('Cinema World')).toBeInTheDocument();
    });
  });
});
