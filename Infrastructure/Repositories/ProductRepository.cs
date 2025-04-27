using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;


        public ProductRepository(IDistributedCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            string cacheKey = "product:all";

            // Try to get products list from Redis
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var productsFromCache = JsonSerializer.Deserialize<List<Product>>(cachedData);
                if (productsFromCache != null)
                    return productsFromCache;
            }

            // Simulate delay ONLY when fetching from database
            await Task.Delay(2000);

            // Fetch from PostgreSQL (your real database now)
            var productsFromDb = await _context.Products.ToListAsync();

            // Store in Redis
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3) // Cache for 3 minutes
            };

            var serializedProducts = JsonSerializer.Serialize(productsFromDb);
            await _cache.SetStringAsync(cacheKey, serializedProducts, cacheOptions);

            return productsFromDb;
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

            // Simulate a delay to show benefit of caching (only happens when not cached)
            await Task.Delay(2000);

            // Fetch from PostgreSQL using EF Core
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            // Store the product in Redis with expiration
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            };

            var serializedProduct = JsonSerializer.Serialize(product);
            await _cache.SetStringAsync(cacheKey, serializedProduct, cacheOptions);

            return product;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Update the cached product list if it exists
            var cachedData = await _cache.GetStringAsync("product:all");
            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedList = JsonSerializer.Deserialize<List<Product>>(cachedData);
                if (cachedList != null)
                {
                    cachedList.Add(product);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                    };

                    var updatedSerializedList = JsonSerializer.Serialize(cachedList);
                    await _cache.SetStringAsync("product:all", updatedSerializedList, cacheOptions);
                }
            }

            return product;
        }
    }
}
