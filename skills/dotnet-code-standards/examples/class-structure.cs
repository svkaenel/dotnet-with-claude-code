using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Evanto.Core.Sample;

///-------------------------------------------------------------------------------------------------
/// <summary>   Sample service demonstrating complete class structure with all patterns. </summary>
///
/// <remarks>   SvK, 2025-01-19. </remarks>
///-------------------------------------------------------------------------------------------------
public partial class EvSampleService(
    IHttpClientFactory httpClientFactory,
    ILogger<EvSampleService> logger) : IEvSampleService, IDisposable
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   HTTP client factory for making requests. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly IHttpClientFactory             mHttpClientFactory = httpClientFactory;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Logger instance for diagnostics. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly ILogger<EvSampleService>       mLogger = logger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Semaphore for thread-safe operations. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly SemaphoreSlim                  mLock = new(1, 1);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Timestamp when service was initialized. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly DateTime                       mStartTime = DateTime.UtcNow;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Concurrent dictionary for caching user data. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly ConcurrentDictionary<String, UserData> mUserCache = new();

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Last time cache was cleaned. </summary>
    ///-------------------------------------------------------------------------------------------------
    private DateTime                                mLastCleanup = DateTime.UtcNow;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Flag indicating if this instance has been disposed. </summary>
    ///-------------------------------------------------------------------------------------------------
    private Boolean                                 mDisposed;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets the number of cached users. </summary>
    ///-------------------------------------------------------------------------------------------------
    public Int32 CachedUserCount => mUserCache.Count;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets the service uptime. </summary>
    ///-------------------------------------------------------------------------------------------------
    public TimeSpan Uptime => DateTime.UtcNow - mStartTime;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Retrieves user data asynchronously with caching. </summary>
    ///
    /// <param name="userID">             The user identifier. </param>
    /// <param name="cancellationToken">  Cancellation token. </param>
    ///
    /// <returns>   The user data if found, null otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<UserData?> GetUserAsync(
        String userID,
        CancellationToken cancellationToken = default)
    {   // check requirements
        ArgumentException.ThrowIfNullOrEmpty(userID);
        ObjectDisposedException.ThrowIf(mDisposed, nameof(EvSampleService));

        try
        {   // check cache first (fast path)
            if (mUserCache.TryGetValue(userID, out var cachedUser))
            {
                LogCacheHit(userID);
                return cachedUser;
            }

            LogCacheMiss(userID);

            // acquire lock for cache update
            await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {   // double-check: another thread might have cached it
                if (mUserCache.TryGetValue(userID, out cachedUser))
                {
                    LogCachedByAnotherThread(userID);
                    return cachedUser;
                }

                // fetch from source
                using var client = mHttpClientFactory.CreateClient();
                var response     = await client.GetAsync($"/api/users/{userID}", cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    LogFetchFailed(userID, response.StatusCode);
                    return null;
                }

                var userData = await response.Content
                    .ReadFromJsonAsync<UserData>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (userData != null)
                {   // cache the result
                    mUserCache[userID] = userData;
                    LogUserCached(userID);
                }

                return userData;
            }

            finally
            {   // always release lock
                mLock.Release();
            }
        }

        catch (Exception ex)
        {
            LogErrorFetchingUser(ex, userID);
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Clears the user cache. </summary>
    ///
    /// <param name="cancellationToken">  Cancellation token. </param>
    ///
    /// <returns>   Number of cache entries cleared. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Int32> ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(mDisposed, nameof(EvSampleService));

        LogClearingCache();

        await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {   // clear all entries
            var count = mUserCache.Count;
            mUserCache.Clear();
            mLastCleanup = DateTime.UtcNow;

            LogCacheCleared(count);

            return count;
        }

        finally
        {
            mLock.Release();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets cache statistics. </summary>
    ///
    /// <returns>   Dictionary containing cache statistics. </returns>
    ///-------------------------------------------------------------------------------------------------
    public IDictionary<String, Object> GetCacheStats()
    {
        ObjectDisposedException.ThrowIf(mDisposed, nameof(EvSampleService));

        var stats = new Dictionary<String, Object>
        {
            ["TotalEntries"] = mUserCache.Count,
            ["LastCleanup"] = mLastCleanup,
            ["Uptime"] = Uptime.ToString(@"hh\:mm\:ss")
        };

        return stats;
    }

    #region IDisposable Implementation

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Releases resources. </summary>
    ///
    /// <param name="disposing">  True to release managed resources. </param>
    ///-------------------------------------------------------------------------------------------------
    protected virtual void Dispose(Boolean disposing)
    {
        if (!mDisposed)
        {
            if (disposing)
            {   // dispose managed resources
                mLock?.Dispose();
            }

            mDisposed = true;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Releases resources. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region LoggerMessage

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "✅ Cache hit for user: {UserID}")]
    private partial void LogCacheHit(String userID);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Cache miss for user: {UserID}")]
    private partial void LogCacheMiss(String userID);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "✅ User {UserID} cached by another thread")]
    private partial void LogCachedByAnotherThread(String userID);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "❌ Failed to fetch user {UserID}: {StatusCode}")]
    private partial void LogFetchFailed(String userID, System.Net.HttpStatusCode statusCode);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "✅ User {UserID} cached successfully")]
    private partial void LogUserCached(String userID);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "❌ Error fetching user {UserID}")]
    private partial void LogErrorFetchingUser(Exception ex, String userID);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Clearing user cache")]
    private partial void LogClearingCache();

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "✅ Cleared {Count} entries from cache")]
    private partial void LogCacheCleared(Int32 count);

    #endregion
}

///-------------------------------------------------------------------------------------------------
/// <summary>   User data record for caching. </summary>
///-------------------------------------------------------------------------------------------------
public record UserData(String ID, String Name, String Email, Boolean IsActive);
