# Rate Limiting

This guide explains the rate limiting implementation for the Solar Projects API.

## Rate Limiting Overview

Rate limiting controls how many API requests a client can make within a specific time period. This helps:

- Prevent accidental or intentional API abuse
- Ensure fair resource allocation
- Maintain system stability and performance
- Protect against brute force attacks

## Current Limits

| Client Type | Requests/Minute | Burst | Daily Limit |
|-------------|-----------------|-------|------------|
| Anonymous | 30 | 5 | 1,000 |
| Authenticated User | 100 | 10 | 5,000 |
| Admin/Manager | 300 | 20 | 15,000 |
| System/Integration | 500 | 30 | 50,000 |

## Rate Limit Response Headers

When making API requests, the following headers are returned:

```
X-RateLimit-Limit: 100          // Your rate limit (requests per minute)
X-RateLimit-Remaining: 80       // Remaining requests in current window
X-RateLimit-Reset: 25           // Seconds until limit resets
X-RateLimit-Type: user          // Rate limit category applied
```

## Rate Limit Exceeded Response

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

## Rate Limit Storage Implementation

The API uses Redis for distributed rate limiting storage, allowing consistent rate limiting across multiple API instances.

```csharp
public class RedisRateLimitStorage : IRateLimitStorage
{
    public async Task<RateLimitResult> CheckRateLimit(string key, int limit, TimeSpan window)
    {
        // Redis script for atomic rate limiting
        // Returns count and retry-after if limit exceeded
    }
}
```

## Client-Side Best Practices

### 1. Respect Retry-After Header

When you receive a 429 response, respect the `Retry-After` header:

```javascript
async function makeApiRequest(endpoint) {
  try {
    return await fetch(endpoint);
  } catch (error) {
    if (error.status === 429) {
      // Wait for the specified retry period
      await new Promise(resolve => setTimeout(resolve, error.retryAfter * 1000));
      // Retry the request
      return await fetch(endpoint);
    }
    throw error;
  }
}
```

### 2. Implement Backoff Strategy

Use exponential backoff for retries when rate limited.

### 3. Request Batching

Batch multiple items into a single API call when possible.

### 4. Cache Responses

Cache API responses to reduce repeated calls.

## Rate Limiting Configuration

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
      }
    ]
  }
}
```

## Monitoring Your Usage

You can monitor your current rate limit usage with the following endpoint:

**GET** `/api/v1/system/rate-limit-status`

**Authentication Required**

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

## Rate Limit Increase Requests

If you need higher rate limits for integration or production usage, contact the API administrators with:

1. Your client ID or account information
2. Reason for increased limit request
3. Expected request volume
4. Project details and use case

---
*Last Updated: July 4, 2025*
