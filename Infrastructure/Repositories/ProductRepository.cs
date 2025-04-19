using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        // Simulated data source
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Keyboard", Price = 30 },
            new Product { Id = 2, Name = "Mouse", Price = 20 },
            new Product { Id = 3, Name = "Monitor", Price = 200 }
        };

        private readonly IDistributedCache _cache;

        public ProductRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task<List<Product>> GetAllAsync()
        {
            // Directly return the list as it's static (no need for caching)
            return Task.FromResult(_products);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            string cacheKey = $"product:{id}";

            // Try to get product from Redis cache
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var productFromCache = JsonSerializer.Deserialize<Product>(cachedData);
                if (productFromCache != null)
                    return productFromCache;
            }

            // Simulate a delay to show benefit of caching
            await Task.Delay(2000);

            // Fetch from "data source"
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return null;

            // Store the product in Redis with expiration
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3) // Cache for 3 minutes
            };

            var serializedProduct = JsonSerializer.Serialize(product);

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), cacheOptions);

            return product;
        }
    }
}
