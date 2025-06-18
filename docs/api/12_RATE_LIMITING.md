# ⚡ Rate Limiting

This guide explains the rate limiting implementation for the Solar Projects API.

## 📊 Rate Limiting Overview

Rate limiting controls how many API requests a client can make within a specific time period. This helps:

- Prevent accidental or intentional API abuse
- Ensure fair resource allocation
- Maintain system stability and performance
- Protect against brute force attacks

## 🔢 Current Limits

| Client Type | Requests/Minute | Burst | Daily Limit |
|-------------|-----------------|-------|------------|
| Anonymous | 30 | 5 | 1,000 |
| Authenticated User | 100 | 10 | 5,000 |
| Admin/Manager | 300 | 20 | 15,000 |
| System/Integration | 500 | 30 | 50,000 |

## 🔍 Rate Limit Response Headers

When making API requests, the following headers are returned:

```
X-RateLimit-Limit: 100          // Your rate limit (requests per minute)
X-RateLimit-Remaining: 80       // Remaining requests in current window
X-RateLimit-Reset: 25           // Seconds until limit resets
X-RateLimit-Type: user          // Rate limit category applied
```

## 🚫 Rate Limit Exceeded Response

When you exceed your rate limit, you'll receive a `429 Too Many Requests` response:

```json
{
  "success": false,
  "message": "Rate limit exceeded",
  "data": {
    "retryAfter": 25,           // Seconds until you can retry
    "limit": 100,               // Your rate limit
    "windowSize": 60,           // Window size in seconds
    "type": "user"              // Rate limit category
  },
  "errors": ["Too many requests. Please try again in 25 seconds."]
}
```

## 📦 Rate Limit Storage Implementation

The API uses Redis for distributed rate limiting storage, allowing consistent rate limiting across multiple API instances:

```csharp
// RedisRateLimitStorage.cs implementation
public class RedisRateLimitStorage : IRateLimitStorage
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitStorage> _logger;

    public RedisRateLimitStorage(IConnectionMultiplexer redis, ILogger<RedisRateLimitStorage> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<RateLimitResult> CheckRateLimit(string key, int limit, TimeSpan window)
    {
        try
        {
            var db = _redis.GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var windowSeconds = (int)window.TotalSeconds;
            
            // Execute rate limiting script atomically
            var script = @"
                local key = KEYS[1]
                local now = tonumber(ARGV[1])
                local window = tonumber(ARGV[2])
                local limit = tonumber(ARGV[3])
                
                -- Clean old requests
                redis.call('ZREMRANGEBYSCORE', key, 0, now - window)
                
                -- Count requests in current window
                local count = redis.call('ZCARD', key)
                
                -- Check if under limit
                if count < limit then
                    -- Add new request
                    redis.call('ZADD', key, now, now .. '-' .. math.random())
                    -- Set expiration
                    redis.call('EXPIRE', key, window)
                    return {count + 1, 0}
                else
                    -- Get oldest timestamp to calculate retry-after
                    local oldest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')[2]
                    local retry_after = oldest + window - now
                    return {count, retry_after}
                end
            ";
            
            var result = (RedisValue[])await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[] { now, windowSeconds, limit }
            );
            
            int count = (int)result[0];
            int retryAfter = (int)result[1];
            
            return new RateLimitResult
            {
                IsAllowed = count <= limit,
                Count = count,
                Limit = limit,
                RetryAfter = retryAfter > 0 ? retryAfter : null,
                ResetAt = DateTimeOffset.UtcNow.AddSeconds(windowSeconds)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit: {Message}", ex.Message);
            // Fail open - allow request if Redis is unavailable
            return new RateLimitResult { IsAllowed = true, Limit = limit };
        }
    }
}
```

## 🔧 Client-Side Best Practices

### 1. Respect Retry-After Header

When you receive a 429 response, respect the `Retry-After` header:

```dart
// Flutter example
Future<T> makeApiRequest<T>(String endpoint) async {
  try {
    return await apiClient.get(endpoint);
  } on RateLimitException catch (e) {
    // Wait for the specified retry period
    await Future.delayed(Duration(seconds: e.retryAfter));
    // Retry the request
    return await apiClient.get(endpoint);
  }
}
```

### 2. Implement Backoff Strategy

Use exponential backoff for retries:

```dart
Future<T> requestWithBackoff<T>(Future<T> Function() operation) async {
  int attempt = 0;
  const maxAttempts = 5;
  
  while (true) {
    try {
      return await operation();
    } on RateLimitException catch (e) {
      attempt++;
      if (attempt >= maxAttempts) rethrow;
      
      // Calculate backoff time (respecting retry-after)
      final backoff = max(
        e.retryAfter, 
        pow(2, attempt) * 100 + Random().nextInt(100)
      );
      
      await Future.delayed(Duration(milliseconds: backoff));
    }
  }
}
```

### 3. Request Batching

Batch multiple items into a single API call when possible:

```dart
// Instead of multiple single-item requests
Future<void> updateMultipleItemsInBatch(List<Item> items) async {
  // Single API call with all items
  await apiClient.post('/items/batch-update', {
    'items': items.map((item) => item.toJson()).toList()
  });
  
  // Rather than:
  // for (var item in items) {
  //   await apiClient.put('/items/${item.id}', item.toJson());
  // }
}
```

### 4. Cache Responses

Cache API responses to reduce repeated calls:

```dart
class CachedApiClient {
  final Map<String, CachedResponse> _cache = {};
  final Duration _defaultTtl = Duration(minutes: 5);
  
  Future<T> get<T>(String endpoint, {Duration? cacheTtl}) async {
    final ttl = cacheTtl ?? _defaultTtl;
    
    // Check cache first
    if (_cache.containsKey(endpoint)) {
      final cached = _cache[endpoint]!;
      if (DateTime.now().difference(cached.timestamp) < ttl) {
        return cached.data as T;
      }
    }
    
    // Make API call if not in cache or expired
    final response = await apiClient.get(endpoint);
    
    // Cache the response
    _cache[endpoint] = CachedResponse(
      data: response,
      timestamp: DateTime.now()
    );
    
    return response;
  }
}
```

## 🔢 Rate Limiting Configuration

The API rate limits are configured in `appsettings.json`:

```json
{
  "RateLimiting": {
    "Enabled": true,
    "DefaultLimit": 100,
    "DefaultWindowSeconds": 60,
    "ClientRules": [
      {
        "ClientType": "Anonymous",
        "RequestsPerMinute": 30,
        "BurstLimit": 5,
        "DailyLimit": 1000
      },
      {
        "ClientType": "User",
        "RequestsPerMinute": 100,
        "BurstLimit": 10,
        "DailyLimit": 5000
      },
      {
        "ClientType": "Admin",
        "RequestsPerMinute": 300,
        "BurstLimit": 20,
        "DailyLimit": 15000
      },
      {
        "ClientType": "System",
        "RequestsPerMinute": 500,
        "BurstLimit": 30,
        "DailyLimit": 50000
      }
    ],
    "IpRateLimiting": {
      "Enabled": true,
      "RequestsPerMinute": 120,
      "IpWhitelist": ["127.0.0.1", "::1", "10.0.0.0/8"]
    }
  }
}
```

## 🔍 Monitoring Your Usage

You can monitor your current rate limit usage with the following endpoint:

**GET** `/api/v1/system/rate-limit-status`

**🔒 Authentication Required**

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Rate limit status retrieved",
  "data": {
    "currentMinuteUsage": 42,
    "minuteLimit": 100,
    "remainingMinuteRequests": 58,
    "currentDailyUsage": 1250,
    "dailyLimit": 5000,
    "remainingDailyRequests": 3750,
    "resetAt": "2025-06-15T16:45:00Z",
    "clientType": "User"
  },
  "errors": []
}
```

## 🛡️ Rate Limit Increase Requests

If you need higher rate limits for integration or production usage, contact the API administrators with:

1. Your client ID or account information
2. Reason for increased limit request
3. Expected request volume
4. Project details and use case

---
*Last Updated: June 15, 2025*
