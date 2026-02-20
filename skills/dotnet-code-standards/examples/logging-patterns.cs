using System;
using Microsoft.Extensions.Logging;

namespace Evanto.Core.Sample.Logging;

///-------------------------------------------------------------------------------------------------
/// <summary>   Demonstrates correct logging patterns with LoggerMessage delegates. </summary>
///
/// <remarks>   SvK, 2025-01-19. </remarks>
///-------------------------------------------------------------------------------------------------
public partial class EvLoggingService(ILogger<EvLoggingService> logger)
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Processes user login with comprehensive logging. </summary>
    ///
    /// <remarks>   SvK, 2025-01-19. </remarks>
    ///
    /// <param name="userName">  The user name. </param>
    /// <param name="ipAddress"> The IP address. </param>
    ///
    /// <returns>   True if successful, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public Boolean ProcessLogin(String userName, String ipAddress)
    {   // validate input
        ArgumentException.ThrowIfNullOrEmpty(userName);
        ArgumentException.ThrowIfNullOrEmpty(ipAddress);

        LogLoginAttempt(userName, ipAddress);

        try
        {   // validate credentials
            if (!ValidateCredentials(userName))
            {
                LogInvalidCredentials(userName);
                return false;
            }

            // check if account is locked
            if (IsAccountLocked(userName))
            {
                LogAccountLocked(userName);
                return false;
            }

            // successful login
            LogSuccessfulLogin(userName, ipAddress);

            return true;
        }

        catch (Exception ex)
        {   // log error with exception
            LogLoginError(ex, userName);

            throw;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Demonstrates different log levels. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void DemonstrateLogLevels(String userID, Int32 attemptCount)
    {   // trace: very detailed information
        LogTraceDetails(userID, attemptCount);

        // debug: diagnostic information
        LogDebugInfo(userID);

        // information: general informational messages
        LogOperationStarted(userID);

        // warning: potential issues
        if (attemptCount > 3)
            LogHighAttemptCount(userID, attemptCount);

        // error: errors and exceptions (shown in catch blocks)

        // critical: critical failures
        if (attemptCount > 10)
            LogCriticalTooManyAttempts(userID, attemptCount);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Demonstrates structured logging with multiple parameters. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessOrder(String orderID, Decimal amount, String customerEmail)
    {
        LogOrderProcessing(orderID, amount, customerEmail);

        // Process order...

        LogOrderCompleted(orderID, amount);
    }

    #region Helper Methods

    private Boolean ValidateCredentials(String userName) => true;
    private Boolean IsAccountLocked(String userName) => false;

    #endregion

    #region LoggerMessage

    // ✅ INFORMATION LEVEL - General flow
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Login attempt for user: {UserName} from IP: {IpAddress}")]
    private partial void LogLoginAttempt(String userName, String ipAddress);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "✅ Successful login for user: {UserName} from IP: {IpAddress}")]
    private partial void LogSuccessfulLogin(String userName, String ipAddress);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Starting operation for user: {UserID}")]
    private partial void LogOperationStarted(String userID);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "✅ Order {OrderID} completed successfully. Amount: {Amount:C}")]
    private partial void LogOrderCompleted(String orderID, Decimal amount);

    // ✅ WARNING LEVEL - Potential issues
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "⚠️ Invalid credentials for user: {UserName}")]
    private partial void LogInvalidCredentials(String userName);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "⚠️ Account locked for user: {UserName}")]
    private partial void LogAccountLocked(String userName);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Warning,
        Message = "⚠️ High attempt count for user {UserID}: {AttemptCount} attempts")]
    private partial void LogHighAttemptCount(String userID, Int32 attemptCount);

    // ✅ ERROR LEVEL - Errors and exceptions
    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Error,
        Message = "❌ Error during login for user: {UserName}")]
    private partial void LogLoginError(Exception ex, String userName);

    // ✅ CRITICAL LEVEL - Critical failures
    [LoggerMessage(
        EventId = 30,
        Level = LogLevel.Critical,
        Message = "🔥 CRITICAL: Too many login attempts for user {UserID}: {AttemptCount}")]
    private partial void LogCriticalTooManyAttempts(String userID, Int32 attemptCount);

    // ✅ DEBUG LEVEL - Diagnostic information
    [LoggerMessage(
        EventId = 40,
        Level = LogLevel.Debug,
        Message = "Debug: Processing user {UserID}")]
    private partial void LogDebugInfo(String userID);

    // ✅ TRACE LEVEL - Very detailed information
    [LoggerMessage(
        EventId = 50,
        Level = LogLevel.Trace,
        Message = "Trace: User {UserID} details - Attempt: {AttemptCount}")]
    private partial void LogTraceDetails(String userID, Int32 attemptCount);

    // ✅ STRUCTURED LOGGING - Multiple parameters with formatting
    [LoggerMessage(
        EventId = 60,
        Level = LogLevel.Information,
        Message = "Processing order {OrderID} for customer {CustomerEmail}. Amount: {Amount:C}")]
    private partial void LogOrderProcessing(String orderID, Decimal amount, String customerEmail);

    #endregion
}

///-------------------------------------------------------------------------------------------------
/// <summary>   Demonstrates LoggerMessage with custom formatting. </summary>
///-------------------------------------------------------------------------------------------------
public partial class EvFormattingExamples(ILogger<EvFormattingExamples> logger)
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Demonstrates various parameter formatting options. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void DemonstrateFormatting(
        DateTime timestamp,
        TimeSpan duration,
        Decimal amount,
        Double percentage)
    {
        LogFormattedValues(timestamp, duration, amount, percentage);
    }

    #region LoggerMessage

    // Currency formatting: {Amount:C}
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Transaction: {Amount:C} at {Timestamp:yyyy-MM-dd HH:mm:ss}")]
    private partial void LogFormattedValues(
        DateTime timestamp,
        TimeSpan duration,
        Decimal amount,
        Double percentage);

    // Hexadecimal: {Value:X}
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Memory address: 0x{Address:X8}")]
    private partial void LogMemoryAddress(Int32 address);

    // Fixed decimal places: {Value:F2}
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Performance: {ResponseTime:F2}ms, Success rate: {SuccessRate:P2}")]
    private partial void LogPerformanceMetrics(Double responseTime, Double successRate);

    // Custom date format: {Date:yyyy-MM-dd}
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Report generated for {ReportDate:yyyy-MM-dd}")]
    private partial void LogReportGeneration(DateTime reportDate);

    #endregion
}

///-------------------------------------------------------------------------------------------------
/// <summary>   ❌ WRONG: Direct ILogger usage (avoid this). </summary>
///-------------------------------------------------------------------------------------------------
public class BadLoggingExamples
{
    private readonly ILogger<BadLoggingExamples> mLogger;

    public BadLoggingExamples(ILogger<BadLoggingExamples> logger)
    {
        mLogger = logger;
    }

    public void WrongWayToLog(String userName)
    {
        // ❌ DON'T DO THIS - Direct ILogger calls
        mLogger.LogInformation("Processing user: " + userName);
        mLogger.LogError("Error occurred for user: " + userName);

        // ❌ DON'T DO THIS - String interpolation in log calls
        mLogger.LogInformation($"User {userName} logged in");

        // ❌ DON'T DO THIS - Not using structured logging
        mLogger.LogWarning("Warning: " + userName + " has too many attempts");
    }

    // ✅ CORRECT WAY: Use LoggerMessage delegates (shown in examples above)
}
