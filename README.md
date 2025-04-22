# How to use Redis caching in ASP .NET?
This project shows how to build an ASP.NET Core Web API using **CQRS architecture** with **Redis for caching** and **PostgreSQL** as the main database.

## 🧠 Overview
The main goal of this project is to optimize data fetching performance by implementing Redis as a caching layer. When a product is requested:

1. The app first checks Redis.
2. If the product is cached, it returns immediately.
3. If not found, the app queries PostgreSQL, returns the data, and caches it in Redis for future use.

---
## 📦 Technologies Used

- ASP .NET Core & Entity Framework Core
- Redis
- PostgreSQL
- Docker & Docker Compose
- MediatR for CQRS
- IDistributedCache
---

## 🧰 Redis Caching Implementation

Caching logic is cleanly separated implementation in the `Infrastructure/Caching` folder.

### `IRedisCacheService`

This is the implementation for cache **operations**:

```csharp
namespace Infrastructure.Caching
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    }
}
```


### `RedisCacheService`

This class handles actual Redis interactions using `IDistributedCache`.

```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Infrastructure.Caching
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            };

            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
        }
    }
}
```

## 🧩 Usage in Repositories

In the `ProductRepository`, caching is used like this:


```csharp
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

    // Fetch from PostgreSQL
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
```

## Testing Redis

### 🔌 Accessing Redis CLI

To manually inspect or debug Redis entries, you can use the Redis CLI inside the Docker container.

```
docker exec -it redis redis-cli
```
### 🛠 Redis Commands
Now we can have access through our cached data with this commands:

| Command               | Description                                                 |
|-----------------------|-------------------------------------------------------------|
| `KEYS *`              | Lists all keys in the current Redis database                |
| `HGETALL <key>`       | Retrieves all fields and values of a hash stored at `<key>` |


