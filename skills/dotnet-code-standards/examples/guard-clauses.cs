using System;

namespace Evanto.Core.Sample.GuardClauses;

///-------------------------------------------------------------------------------------------------
/// <summary>   Demonstrates modern guard clause patterns in C# 11+. </summary>
///-------------------------------------------------------------------------------------------------
public class EvGuardClauseExamples
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ArgumentNullException.ThrowIfNull for null checks. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessUser(EvUser user, ILogger logger)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(logger);

        // proceed with processing
        logger.Log($"Processing user: {user.Name}");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ArgumentException.ThrowIfNullOrEmpty for strings. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void SendEmail(String emailAddress, String subject, String body)
    {   // validate parameters
        ArgumentException.ThrowIfNullOrEmpty(emailAddress);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(body);

        // proceed with email sending
        Console.WriteLine($"Sending email to: {emailAddress}");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ArgumentException.ThrowIfNullOrWhiteSpace for strings. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void CreateUser(String userName, String password)
    {   // validate input
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // proceed with user creation
        Console.WriteLine($"Creating user: {userName}");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ArgumentOutOfRangeException.ThrowIfNegative for numeric checks. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessOrder(Decimal amount, Int32 quantity)
    {   // validate business rules
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        // proceed with order processing
        var total = amount * quantity;
        Console.WriteLine($"Processing order: {quantity} items, total: {total:C}");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ArgumentOutOfRangeException.ThrowIfGreaterThan for range checks. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void SetDiscount(Decimal percentage)
    {   // validate range (0-100)
        ArgumentOutOfRangeException.ThrowIfNegative(percentage);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100m);

        // proceed with discount application
        Console.WriteLine($"Discount set to: {percentage}%");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ArgumentOutOfRangeException.ThrowIfNotEqual for specific value checks. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessProtocolVersion(Int32 version)
    {   // require specific protocol version
        ArgumentOutOfRangeException.ThrowIfNotEqual(version, 2);

        // proceed with protocol v2 processing
        Console.WriteLine("Processing with protocol version 2");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: ObjectDisposedException.ThrowIf for disposed object checks. </summary>
    ///-------------------------------------------------------------------------------------------------
    public class EvDisposableService : IDisposable
    {
        private Boolean mDisposed;

        public void ProcessData(String data)
        {   // check if disposed
            ObjectDisposedException.ThrowIf(mDisposed, nameof(EvDisposableService));
            ArgumentException.ThrowIfNullOrEmpty(data);

            // proceed with data processing
            Console.WriteLine($"Processing: {data}");
        }

        public void Dispose()
        {
            mDisposed = true;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Multiple guard clauses at method start. </summary>
    ///-------------------------------------------------------------------------------------------------
    public Decimal CalculateTotal(
        Decimal unitPrice,
        Int32 quantity,
        Decimal taxRate,
        String customerID)
    {   // validate all parameters
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(taxRate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taxRate, 1.0m);
        ArgumentException.ThrowIfNullOrEmpty(customerID);

        // proceed with calculation
        var subtotal = unitPrice * quantity;
        var tax = subtotal * taxRate;
        var total = subtotal + tax;

        return total;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Custom validation with meaningful messages. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ScheduleAppointment(DateTime appointmentDate, TimeSpan duration)
    {   // validate appointment date
        ArgumentNullException.ThrowIfNull(appointmentDate);

        if (appointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment must be in the future", nameof(appointmentDate));

        // validate duration
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be positive", nameof(duration));

        if (duration > TimeSpan.FromHours(8))
            throw new ArgumentException("Duration cannot exceed 8 hours", nameof(duration));

        // proceed with scheduling
        Console.WriteLine($"Appointment scheduled for: {appointmentDate:yyyy-MM-dd HH:mm}");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Guard clauses for collections. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessUserList(IList<EvUser> users)
    {   // validate collection
        ArgumentNullException.ThrowIfNull(users);

        if (users.Count == 0)
            throw new ArgumentException("User list cannot be empty", nameof(users));

        // proceed with processing
        foreach (var user in users)
        {
            Console.WriteLine($"Processing: {user.Name}");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ❌ WRONG: Old-style guard clauses (avoid this). </summary>
    ///-------------------------------------------------------------------------------------------------
    public void OldStyleGuardClauses(String userName, Int32 age)
    {
        // ❌ DON'T DO THIS - Verbose old-style guard clauses
        if (userName == null)
            throw new ArgumentNullException(nameof(userName));

        if (String.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty", nameof(userName));

        if (age < 0)
            throw new ArgumentOutOfRangeException(nameof(age), "Age cannot be negative");

        // ✅ DO THIS INSTEAD - Modern guard clauses
        // ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        // ArgumentOutOfRangeException.ThrowIfNegative(age);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Combining guard clauses with business logic validation. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void CreateOrder(
        String customerID,
        IList<OrderItem> items,
        String shippingAddress)
    {   // validate parameters (guard clauses)
        ArgumentException.ThrowIfNullOrEmpty(customerID);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrEmpty(shippingAddress);

        if (items.Count == 0)
            throw new ArgumentException("Order must contain at least one item", nameof(items));

        // validate business rules
        var total = items.Sum(i => i.Price * i.Quantity);
        if (total <= 0)
            throw new InvalidOperationException("Order total must be greater than zero");

        if (total > 10000m)
            throw new InvalidOperationException("Order exceeds maximum allowed amount");

        // proceed with order creation
        Console.WriteLine($"Creating order for customer: {customerID}, total: {total:C}");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Guard clauses in async methods. </summary>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvUser> GetUserAsync(
        String userID,
        CancellationToken cancellationToken = default)
    {   // validate parameters before async operations
        ArgumentException.ThrowIfNullOrEmpty(userID);

        // proceed with async operations
        await Task.Delay(100, cancellationToken);

        return new EvUser { Name = "John", Email = "john@example.com" };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Guard clauses in constructors. </summary>
    ///-------------------------------------------------------------------------------------------------
    public class EvUserService
    {
        private readonly IEvUserRepository mRepository;
        private readonly ILogger mLogger;

        public EvUserService(IEvUserRepository repository, ILogger logger)
        {   // validate constructor parameters
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);

            mRepository = repository;
            mLogger = logger;
        }
    }
}

#region Supporting Types

public class EvUser
{
    public String Name { get; set; } = String.Empty;
    public String Email { get; set; } = String.Empty;
}

public class OrderItem
{
    public Decimal Price { get; set; }
    public Int32 Quantity { get; set; }
}

public interface IEvUserRepository { }
public interface ILogger
{
    void Log(String message);
}

#endregion
