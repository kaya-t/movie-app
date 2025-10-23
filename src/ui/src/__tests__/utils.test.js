describe('Utility Functions', () => {
  describe('Price Formatting', () => {
    const formatPrice = (price) => `$${price.toFixed(2)}`;

    test('formats whole numbers correctly', () => {
      expect(formatPrice(25)).toBe('$25.00');
      expect(formatPrice(100)).toBe('$100.00');
    });

    test('formats decimal numbers correctly', () => {
      expect(formatPrice(25.5)).toBe('$25.50');
      expect(formatPrice(29.99)).toBe('$29.99');
    });

    test('formats prices with many decimal places', () => {
      expect(formatPrice(25.123456)).toBe('$25.12');
      expect(formatPrice(29.999)).toBe('$30.00');
    });
  });

  describe('Provider Display Names', () => {
    const getProviderDisplayName = (provider) => {
      return provider === 'cinemaworld' ? 'Cinema World' : 'Film World';
    };

    test('converts cinemaworld to Cinema World', () => {
      expect(getProviderDisplayName('cinemaworld')).toBe('Cinema World');
    });

    test('converts filmworld to Film World', () => {
      expect(getProviderDisplayName('filmworld')).toBe('Film World');
    });

    test('handles unknown providers', () => {
      expect(getProviderDisplayName('unknown')).toBe('Film World');
      expect(getProviderDisplayName('')).toBe('Film World');
    });
  });

  describe('URL Encoding', () => {
    test('encodes movie IDs with special characters', () => {
      const movieId = 'movie with spaces & special chars';
      const encoded = encodeURIComponent(movieId);
      expect(encoded).toBe('movie%20with%20spaces%20%26%20special%20chars');
    });

    test('handles simple movie IDs', () => {
      const movieId = 'simple-id';
      const encoded = encodeURIComponent(movieId);
      expect(encoded).toBe('simple-id');
    });
  });

  describe('Array Sorting', () => {
    test('sorts prices in ascending order', () => {
      const prices = [
        { provider: 'filmworld', price: 29.5 },
        { provider: 'cinemaworld', price: 123.5 },
        { provider: 'other', price: 50.0 }
      ];

      const sorted = prices.sort((a, b) => a.price - b.price);
      
      expect(sorted[0].price).toBe(29.5);
      expect(sorted[1].price).toBe(50.0);
      expect(sorted[2].price).toBe(123.5);
    });

    test('sorts movie titles alphabetically', () => {
      const movies = [
        { title: 'Z Movie' },
        { title: 'A Movie' },
        { title: 'M Movie' }
      ];

      const sorted = movies.sort((a, b) => a.title.localeCompare(b.title));
      
      expect(sorted[0].title).toBe('A Movie');
      expect(sorted[1].title).toBe('M Movie');
      expect(sorted[2].title).toBe('Z Movie');
    });
  });

  describe('Object Key Operations', () => {
    test('gets object keys correctly', () => {
      const pricesByProvider = {
        cinemaworld: 123.5,
        filmworld: 29.5
      };

      const keys = Object.keys(pricesByProvider);
      expect(keys).toHaveLength(2);
      expect(keys).toContain('cinemaworld');
      expect(keys).toContain('filmworld');
    });

    test('gets object entries correctly', () => {
      const pricesByProvider = {
        cinemaworld: 123.5,
        filmworld: 29.5
      };

      const entries = Object.entries(pricesByProvider);
      expect(entries).toHaveLength(2);
      expect(entries).toContainEqual(['cinemaworld', 123.5]);
      expect(entries).toContainEqual(['filmworld', 29.5]);
    });
  });

  describe('String Operations', () => {
    test('handles empty strings', () => {
      const emptyString = '';
      expect(emptyString.length).toBe(0);
      expect(emptyString.trim()).toBe('');
    });

    test('trims whitespace', () => {
      const stringWithSpaces = '  test  ';
      expect(stringWithSpaces.trim()).toBe('test');
    });

    test('checks for null/undefined values', () => {
      expect(null == null).toBe(true);
      expect(undefined == null).toBe(true);
      expect('' == null).toBe(false);
      expect(0 == null).toBe(false);
    });
  });
});
