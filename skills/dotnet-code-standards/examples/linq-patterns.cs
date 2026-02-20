using System;
using System.Collections.Generic;
using System.Linq;

namespace Evanto.Core.Sample.Linq;

///-------------------------------------------------------------------------------------------------
/// <summary>   Demonstrates efficient LINQ patterns and best practices. </summary>
///-------------------------------------------------------------------------------------------------
public class EvLinqPatterns
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Method syntax (preferred over query syntax). </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<UserViewModel> GetActiveUsers(IEnumerable<User> users)
    {   // use method syntax for clarity and consistency
        return users
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserViewModel(u.Id, u.FullName, u.Email))
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Materialize once to avoid multiple enumeration. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessUsers(IEnumerable<User> users)
    {   // materialize the query once
        var activeUsers = users
            .Where(u => u.IsActive)
            .ToList();  // ✅ Materialize here

        // now we can enumerate multiple times without performance penalty
        var count = activeUsers.Count;
        var first = activeUsers.FirstOrDefault();
        var last = activeUsers.LastOrDefault();

        Console.WriteLine($"Found {count} active users");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ❌ WRONG: Multiple enumeration (inefficient). </summary>
    ///-------------------------------------------------------------------------------------------------
    public void ProcessUsersWrong(IEnumerable<User> users)
    {   // DON'T DO THIS - query executed multiple times
        var activeUsers = users.Where(u => u.IsActive);  // ❌ Not materialized

        var count = activeUsers.Count();        // Enumeration 1
        var first = activeUsers.FirstOrDefault(); // Enumeration 2
        var last = activeUsers.LastOrDefault();  // Enumeration 3

        // Each call above re-executes the Where clause!
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Efficient filtering and projection. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<OrderSummary> GetOrderSummaries(IEnumerable<Order> orders)
    {   // filter first, then project
        return orders
            .Where(o => o.Status == OrderStatus.Completed)
            .Where(o => o.TotalAmount > 100m)
            .Select(o => new OrderSummary
            {
                OrderID      = o.Id,
                CustomerName = o.Customer.Name,
                Total        = o.TotalAmount,
                Date         = o.CompletedDate
            })
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Use Any() instead of Count() > 0. </summary>
    ///-------------------------------------------------------------------------------------------------
    public Boolean HasActiveUsers(IEnumerable<User> users)
    {   // ✅ Efficient - stops at first match
        return users.Any(u => u.IsActive);

        // ❌ DON'T DO THIS - counts all items
        // return users.Count(u => u.IsActive) > 0;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Use FirstOrDefault() instead of Where().First(). </summary>
    ///-------------------------------------------------------------------------------------------------
    public User? FindUserByEmail(IEnumerable<User> users, String email)
    {   // ✅ Efficient - single operation
        return users.FirstOrDefault(u => u.Email == email);

        // ❌ DON'T DO THIS - two operations
        // return users.Where(u => u.Email == email).FirstOrDefault();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: GroupBy with aggregation. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<CategoryStatistics> GetCategoryStatistics(IEnumerable<Product> products)
    {   // group and aggregate in single query
        return products
            .GroupBy(p => p.Category)
            .Select(g => new CategoryStatistics
            {
                Category     = g.Key,
                ProductCount = g.Count(),
                AveragePrice = g.Average(p => p.Price),
                TotalValue   = g.Sum(p => p.Price * p.Stock)
            })
            .OrderByDescending(c => c.TotalValue)
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Joining collections efficiently. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<OrderWithCustomer> GetOrdersWithCustomers(
        IEnumerable<Order> orders,
        IEnumerable<Customer> customers)
    {   // efficient join operation
        return orders
            .Join(
                customers,
                order => order.CustomerID,
                customer => customer.Id,
                (order, customer) => new OrderWithCustomer
                {
                    OrderID      = order.Id,
                    OrderDate    = order.Date,
                    CustomerName = customer.Name,
                    TotalAmount  = order.TotalAmount
                })
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Distinct values efficiently. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<String> GetUniqueCategories(IEnumerable<Product> products)
    {   // get distinct values
        return products
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Paging with Skip and Take. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<Product> GetProductsPage(
        IEnumerable<Product> products,
        Int32 pageNumber,
        Int32 pageSize)
    {   // validate parameters
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        // efficient paging
        return products
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Complex filtering with multiple conditions. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<Product> SearchProducts(
        IEnumerable<Product> products,
        String? searchTerm,
        Decimal? minPrice,
        Decimal? maxPrice,
        String? category)
    {   // build query with conditional filters
        var query = products.AsQueryable();

        if (!String.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (!String.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        return query
            .OrderBy(p => p.Price)
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Aggregation operations. </summary>
    ///-------------------------------------------------------------------------------------------------
    public OrderStatistics GetOrderStatistics(IEnumerable<Order> orders)
    {   // materialize once for multiple aggregations
        var completedOrders = orders
            .Where(o => o.Status == OrderStatus.Completed)
            .ToList();

        return new OrderStatistics
        {
            TotalOrders  = completedOrders.Count,
            TotalRevenue = completedOrders.Sum(o => o.TotalAmount),
            AverageOrder = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0m,
            MinOrder     = completedOrders.Any() ? completedOrders.Min(o => o.TotalAmount) : 0m,
            MaxOrder     = completedOrders.Any() ? completedOrders.Max(o => o.TotalAmount) : 0m
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Using ToDictionary for fast lookups. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IDictionary<Int32, User> CreateUserLookup(IEnumerable<User> users)
    {   // create dictionary for O(1) lookups
        return users
            .Where(u => u.IsActive)
            .ToDictionary(u => u.Id, u => u);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: SelectMany for flattening nested collections. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<OrderItem> GetAllOrderItems(IEnumerable<Order> orders)
    {   // flatten nested collections
        return orders
            .Where(o => o.Status == OrderStatus.Completed)
            .SelectMany(o => o.Items)
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Zip for combining two sequences. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<ProductWithCategory> CombineProductsWithCategories(
        IEnumerable<Product> products,
        IEnumerable<Category> categories)
    {   // combine two sequences element-by-element
        return products
            .Zip(categories, (product, category) => new ProductWithCategory
            {
                ProductName  = product.Name,
                CategoryName = category.Name
            })
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Partition with TakeWhile and SkipWhile. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<Product> GetProductsUntilOutOfStock(IEnumerable<Product> products)
    {   // take items while condition is true
        return products
            .OrderBy(p => p.Stock)
            .TakeWhile(p => p.Stock > 0)
            .ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   ✅ CORRECT: Deferred execution awareness. </summary>
    ///-------------------------------------------------------------------------------------------------
    public void DemonstrateDeferred Execution()
    {   // query is not executed until enumeration
        var users = new List<User>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", IsActive = true },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", IsActive = false }
        };

        var query = users.Where(u => u.IsActive);  // ✅ Not executed yet (deferred)

        users.Add(new User { Id = 3, FirstName = "Bob", LastName = "Wilson", IsActive = true });

        var result = query.ToList();  // ✅ Executed now - includes Bob!

        Console.WriteLine($"Found {result.Count} active users");  // Output: 2
    }
}

#region Supporting Types

public class User
{
    public Int32 Id { get; set; }
    public String FirstName { get; set; } = String.Empty;
    public String LastName { get; set; } = String.Empty;
    public String Email { get; set; } = String.Empty;
    public Boolean IsActive { get; set; }
    public String FullName => $"{FirstName} {LastName}";
}

public record UserViewModel(Int32 Id, String Name, String Email);

public class Order
{
    public Int32 Id { get; set; }
    public Int32 CustomerID { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime Date { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public Int32 ProductID { get; set; }
    public String ProductName { get; set; } = String.Empty;
    public Decimal Price { get; set; }
    public Int32 Quantity { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

public class Customer
{
    public Int32 Id { get; set; }
    public String Name { get; set; } = String.Empty;
}

public class Product
{
    public Int32 Id { get; set; }
    public String Name { get; set; } = String.Empty;
    public String Category { get; set; } = String.Empty;
    public Decimal Price { get; set; }
    public Int32 Stock { get; set; }
}

public class Category
{
    public String Name { get; set; } = String.Empty;
}

public class OrderSummary
{
    public Int32 OrderID { get; set; }
    public String CustomerName { get; set; } = String.Empty;
    public Decimal Total { get; set; }
    public DateTime? Date { get; set; }
}

public class CategoryStatistics
{
    public String Category { get; set; } = String.Empty;
    public Int32 ProductCount { get; set; }
    public Decimal AveragePrice { get; set; }
    public Decimal TotalValue { get; set; }
}

public class OrderWithCustomer
{
    public Int32 OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public String CustomerName { get; set; } = String.Empty;
    public Decimal TotalAmount { get; set; }
}

public class ProductWithCategory
{
    public String ProductName { get; set; } = String.Empty;
    public String CategoryName { get; set; } = String.Empty;
}

public class OrderStatistics
{
    public Int32 TotalOrders { get; set; }
    public Decimal TotalRevenue { get; set; }
    public Decimal AverageOrder { get; set; }
    public Decimal MinOrder { get; set; }
    public Decimal MaxOrder { get; set; }
}

#endregion
