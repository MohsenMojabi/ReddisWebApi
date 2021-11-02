# What is Redis Cache?
Redis is an open-source in-memory data store, which is often used as a distributed database cache. It is written in ANSI C language and provides data structures such as strings, hashes, lists, sets, sorted sets with range queries, bitmaps, hyperloglogs, geospatial indexes, and streams. It is a blazing fast key-value based database and that’s why it is used by some of the biggest apps such as Twitter, Github, Pinterest, Snapchat, Flickr, Digg, Stackoverflow, etc. You can use Redis from most programming languages. It is such a popular and widely used cache that Microsoft Azure also provides its cloud-based version with the name Azure Cache for Redis.

# Setting Up Redis Server On Windows
Redis has been developed and tested mostly in BSD, Linus, and OSX operating systems and unfortunately, there is no official support for the Windows based operating systems but there are some ports available that can be used to install and use Redis on Windows. Typically, a separate machine is used by developers to serve as cache memory for multiple applications. Visit the following URL to download the Redis Cache for Windows supported by Microsoft.

https://github.com/microsoftarchive/redis/releases/tag/win-3.0.504

Download & Extract the zip file and simply run the redis-server.exe file to start the Redis server.

# Integrating Redis Cache in ASP.NET Core

To connect and start caching data from .NET Core applications, we need to install the following package from NuGet.

Microsoft.Extensions.Caching.StackExchangeRedis

Next, we need to configure our application to support Redis cache and for this purpose, we need to call the AddStackExchangeRedisCache method in the ConfigureServices method of the Startup.cs file. 

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = Configuration["RedisCacheServerUrl"];
    });

We can add the RedisCacheServerUrl setting in the appsettings.json file and the value of this setting will specify the port on which Redis Server is available to listen to client requests.

    {
      "RedisCacheServerUrl": "127.0.0.1:6379",
      "ConnectionStrings": {...},
      ...
    }
    
Once the Redis server settings are configured, we are allowed to inject the IDistributedCache interface in our services and controllers.  The following is the updated code of our ProductController in which I first injected the IDistributedCache in the constructor and then used GetAsync and SetAsync methods to get and set the products list in the cache.

      public ProductController(AdventureWorksDbContext context, IDistributedCache cache)
      {
          _context = context;
          _cache = cache;
      }
      
  And the api method: 

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
  
