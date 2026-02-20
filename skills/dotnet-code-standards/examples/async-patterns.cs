using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Evanto.Core.Sample.Async;

///-------------------------------------------------------------------------------------------------
/// <summary>   Demonstrates correct async/await patterns. </summary>
///-------------------------------------------------------------------------------------------------
public class EvAsyncPatterns
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Async method with proper suffix and CancellationToken. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async Task<String> FetchDataAsync(String url, CancellationToken ct = default)
    {
        using var client = new HttpClient();

        // ✅ Use ConfigureAwait(false) in library code
        var response = await client
            .GetStringAsync(url, ct)
            .ConfigureAwait(false);

        return response;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Multiple awaits with ConfigureAwait. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async Task<UserData> GetUserWithDetailsAsync(
        String userID,
        CancellationToken ct = default)
    {   // fetch user profile
        var profile = await FetchUserProfileAsync(userID, ct)
            .ConfigureAwait(false);

        // fetch user settings
        var settings = await FetchUserSettingsAsync(userID, ct)
            .ConfigureAwait(false);

        // combine results
        return new UserData(
            ID: userID,
            Name: profile.Name,
            Email: profile.Email,
            Settings: settings);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Return task directly when no await needed. </summary>
    ///-------------------------------------------------------------------------------------------------
    public Task<String> GetCachedValueAsync(String key)
    {
        // No async/await needed - return task directly
        return Task.FromResult(mCache.TryGetValue(key, out var value) ? value : String.Empty);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Parallel async operations with WhenAll. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<UserData>> GetMultipleUsersAsync(
        IEnumerable<String> userIDs,
        CancellationToken ct = default)
    {   // start all operations in parallel
        var tasks = userIDs.Select(id => GetUserWithDetailsAsync(id, ct));

        // wait for all to complete
        var results = await Task
            .WhenAll(tasks)
            .ConfigureAwait(false);

        return results;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Sequential operations with proper error handling. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> ProcessUserAsync(
        String userID,
        CancellationToken ct = default)
    {
        try
        {   // step 1: validate user
            var isValid = await ValidateUserAsync(userID, ct).ConfigureAwait(false);
            if (!isValid)
                return false;

            // step 2: process data
            await ProcessDataAsync(userID, ct).ConfigureAwait(false);

            // step 3: send notification
            await SendNotificationAsync(userID, ct).ConfigureAwait(false);

            return true;
        }

        catch (OperationCanceledException)
        {   // operation was cancelled
            throw;
        }

        catch (Exception ex)
        {   // log and rethrow
            Console.WriteLine($"Error processing user {userID}: {ex.Message}");
            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ❌ WRONG: async void (only allowed for event handlers). </summary>
    ///-------------------------------------------------------------------------------------------------
    // DON'T DO THIS:
    // public async void ProcessUser(String userID) { }
    //
    // DO THIS INSTEAD:
    public async Task ProcessUserSafeAsync(String userID)
    {
        await ProcessDataAsync(userID, CancellationToken.None).ConfigureAwait(false);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Event handler can be async void. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async void OnButtonClick(Object sender, EventArgs e)
    {
        try
        {   // event handlers are exception boundary
            await ProcessUserSafeAsync("user123").ConfigureAwait(false);
        }

        catch (Exception ex)
        {   // always catch exceptions in async void
            Console.WriteLine($"Error in event handler: {ex.Message}");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ❌ WRONG: Blocking on async code. </summary>
    ///-------------------------------------------------------------------------------------------------
    // DON'T DO THIS:
    // public String FetchData(String url)
    // {
    //     return FetchDataAsync(url).Result;  // ❌ Can cause deadlock!
    // }
    //
    // DON'T DO THIS:
    // public String FetchData(String url)
    // {
    //     FetchDataAsync(url).Wait();  // ❌ Can cause deadlock!
    // }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ValueTask for hot path optimization. </summary>
    ///-------------------------------------------------------------------------------------------------
    public ValueTask<String> GetValueAsync(String key, CancellationToken ct = default)
    {   // if value is cached, return synchronously (no allocation)
        if (mCache.TryGetValue(key, out var value))
            return new ValueTask<String>(value);

        // otherwise, fetch asynchronously
        return new ValueTask<String>(FetchFromSourceAsync(key, ct));
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Timeout pattern with CancellationTokenSource. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async Task<String> FetchWithTimeoutAsync(
        String url,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            return await FetchDataAsync(url, cts.Token).ConfigureAwait(false);
        }

        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {   // timeout occurred
            throw new TimeoutException($"Request timed out after {timeout}");
        }
    }

    #region Helper Methods (implementation not shown)

    private readonly Dictionary<String, String> mCache = new();

    private Task<UserProfile> FetchUserProfileAsync(String userID, CancellationToken ct) =>
        Task.FromResult(new UserProfile("John", "john@example.com"));

    private Task<UserSettings> FetchUserSettingsAsync(String userID, CancellationToken ct) =>
        Task.FromResult(new UserSettings(true, "en"));

    private Task<Boolean> ValidateUserAsync(String userID, CancellationToken ct) =>
        Task.FromResult(true);

    private Task ProcessDataAsync(String userID, CancellationToken ct) =>
        Task.CompletedTask;

    private Task SendNotificationAsync(String userID, CancellationToken ct) =>
        Task.CompletedTask;

    private Task<String> FetchFromSourceAsync(String key, CancellationToken ct) =>
        Task.FromResult($"value-{key}");

    #endregion
}

///-------------------------------------------------------------------------------------------------
/// <summary>   User profile data. </summary>
///-------------------------------------------------------------------------------------------------
public record UserProfile(String Name, String Email);

///-------------------------------------------------------------------------------------------------
/// <summary>   User settings data. </summary>
///-------------------------------------------------------------------------------------------------
public record UserSettings(Boolean NotificationsEnabled, String Language);

///-------------------------------------------------------------------------------------------------
/// <summary>   Complete user data. </summary>
///-------------------------------------------------------------------------------------------------
public record UserData(String ID, String Name, String Email, UserSettings Settings);
