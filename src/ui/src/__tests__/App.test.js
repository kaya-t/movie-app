import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import App from '../App';

jest.mock('../components/MovieList', () => {
  return function MockMovieList() {
    return <div data-testid="movie-list">Movie List Component</div>;
  };
});

jest.mock('../components/MovieDetail', () => {
  return function MockMovieDetail() {
    return <div data-testid="movie-detail">Movie Detail Component</div>;
  };
});

const renderWithRouter = (component, { route = '/' } = {}) => {
  window.history.pushState({}, 'Test page', route);
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

describe('App Component', () => {
  test('renders without crashing', () => {
    renderWithRouter(<App />);
    expect(screen.getByText('Movie List Component')).toBeInTheDocument();
  });

  test('renders MovieList component on home route', () => {
    renderWithRouter(<App />, { route: '/' });
    expect(screen.getByTestId('movie-list')).toBeInTheDocument();
  });

  test('renders MovieDetail component on movie detail route', () => {
    renderWithRouter(<App />, { route: '/movie/test-id' });
    expect(screen.getByTestId('movie-detail')).toBeInTheDocument();
  });

  test('has container class', () => {
    renderWithRouter(<App />);
    const container = screen.getByTestId('movie-list').closest('.container');
    expect(container).toBeInTheDocument();
  });
});
