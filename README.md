# What is Redis Cache?
Redis is an open-source in-memory data store, which is often used as a distributed database cache. It is written in ANSI C language and provides data structures such as strings, hashes, lists, sets, sorted sets with range queries, bitmaps, hyperloglogs, geospatial indexes, and streams. It is a blazing fast key-value based database and that’s why it is used by some of the biggest apps such as Twitter, Github, Pinterest, Snapchat, Flickr, Digg, Stackoverflow, etc. You can use Redis from most programming languages. It is such a popular and widely used cache that Microsoft Azure also provides its cloud-based version with the name Azure Cache for Redis.

# Setting Up Redis Server On Windows
Download & Extract the zip file from below link and simply run the redis-server.exe file to start the Redis server.

https://github.com/microsoftarchive/redis/releases/tag/win-3.0.504

# Integrating Redis Cache in ASP.NET Core

First we need to install the following package from NuGet.

Microsoft.Extensions.Caching.StackExchangeRedis

Next, we need to configure our application to support Redis cache so we need to call the AddStackExchangeRedisCache method in the ConfigureServices method of the Startup.cs file. 

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = Configuration["RedisCacheServerUrl"];
    });

Then, we should add the RedisCacheServerUrl in the appsettings.json file and the value of this setting will specify the port on which Redis Server is available to listen to client requests.

    {
      "RedisCacheServerUrl": "127.0.0.1:6379",
      "ConnectionStrings": {...},
      ...
    }
    
After that we need to inject IDistributedCache interface in our controller (In Constructor).

      private readonly ApplicationDbContext _context;
      private readonly IDistributedCache _cache;
      public ProductController(ApplicationDbContext context, IDistributedCache cache)
      {
          _context = context;
          _cache = cache;
      }
      
Now We can use GetAsync and SetAsync methods to get and set the products list in the cache in our api methods: 

      [HttpGet]
      public async Task<IActionResult> GetAll()
      {
          var cacheKey = "GET_ALL_PRODUCTS";
          List<Product> products = new List<Product>();

          // Get data from cache
          var cachedData = await _cache.GetAsync(cacheKey);
          if (cachedData != null)
          {
              // If data found in cache, encode and deserialize cached data
              var cachedDataString = Encoding.UTF8.GetString(cachedData);
              products = JsonConvert.DeserializeObject<List<Product>>(cachedDataString);
          }
          else
          {
              // If not found, then fetch data from database
              products = await _context.Products.ToListAsync();

             // serialize data
              var cachedDataString = JsonConvert.SerializeObject(products);
              var newDataToCache = Encoding.UTF8.GetBytes(cachedDataString);

              // set cache options 
              var options = new DistributedCacheEntryOptions()
                  .SetAbsoluteExpiration(DateTime.Now.AddMinutes(2))
                  .SetSlidingExpiration(TimeSpan.FromMinutes(1));

              // Add data in cache
              await _cache.SetAsync(cacheKey, newDataToCache, options);
          }

          return Ok(products);
      }
  
