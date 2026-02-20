using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Evanto.Core.Sample.Tests;

///-------------------------------------------------------------------------------------------------
/// <summary>   Unit tests for EvUserService demonstrating xUnit testing patterns. </summary>
///
/// <remarks>   SvK, 2025-01-19. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvUserServiceTests
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Mock repository for user data access. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly Mock<IEvUserRepository> mMockRepo;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Mock logger for testing logging calls. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly Mock<ILogger<EvUserService>> mMockLogger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   System under test. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly EvUserService mSut;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initializes test fixtures and dependencies. </summary>
    ///
    /// <remarks>   Constructor runs before each test method. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public EvUserServiceTests()
    {   // arrange shared test dependencies
        mMockRepo = new Mock<IEvUserRepository>();
        mMockLogger = new Mock<ILogger<EvUserService>>();
        mSut = new EvUserService(mMockRepo.Object, mMockLogger.Object);
    }

    #region GetByIdAsync Tests

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that GetByIdAsync returns user when ID is valid. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {   // arrange
        var expectedUser = new EvUser { Id = 1, Name = "John", Email = "john@example.com" };
        mMockRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // act
        var result = await mSut.GetByIdAsync(1);

        // assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that GetByIdAsync returns null when user not found. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ReturnsNull()
    {   // arrange
        mMockRepo
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EvUser?)null);

        // act
        var result = await mSut.GetByIdAsync(999);

        // assert
        Assert.Null(result);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that GetByIdAsync throws when ID is invalid. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
    {   // arrange
        var invalidId = -1;

        // act & assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await mSut.GetByIdAsync(invalidId));
    }

    #endregion

    #region CreateAsync Tests

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that CreateAsync successfully creates user with valid data. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesUser()
    {   // arrange
        var newUser = new EvUser { Name = "Jane", Email = "jane@example.com" };
        var createdUser = new EvUser { Id = 2, Name = "Jane", Email = "jane@example.com" };

        mMockRepo
            .Setup(r => r.CreateAsync(It.IsAny<EvUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // act
        var result = await mSut.CreateAsync(newUser);

        // assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Jane", result.Name);
        mMockRepo.Verify(r => r.CreateAsync(It.IsAny<EvUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that CreateAsync throws when data is invalid. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Theory]
    [InlineData(null, "test@example.com")]
    [InlineData("", "test@example.com")]
    [InlineData("John", null)]
    [InlineData("John", "")]
    [InlineData("John", "invalid-email")]
    public async Task CreateAsync_WithInvalidData_ThrowsValidationException(String name, String email)
    {   // arrange
        var invalidUser = new EvUser { Name = name, Email = email };

        // act & assert
        await Assert.ThrowsAsync<ValidationException>(async () =>
            await mSut.CreateAsync(invalidUser));
    }

    #endregion

    #region UpdateAsync Tests

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that UpdateAsync successfully updates existing user. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_WithExistingUser_UpdatesUser()
    {   // arrange
        var existingUser = new EvUser { Id = 1, Name = "John", Email = "john@example.com" };
        var updatedUser = new EvUser { Id = 1, Name = "John Updated", Email = "john.updated@example.com" };

        mMockRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        mMockRepo
            .Setup(r => r.UpdateAsync(It.IsAny<EvUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // act
        var result = await mSut.UpdateAsync(updatedUser);

        // assert
        Assert.NotNull(result);
        Assert.Equal("John Updated", result.Name);
        Assert.Equal("john.updated@example.com", result.Email);
        mMockRepo.Verify(r => r.UpdateAsync(It.IsAny<EvUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that UpdateAsync throws when user not found. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ThrowsNotFoundException()
    {   // arrange
        var nonExistentUser = new EvUser { Id = 999, Name = "Ghost", Email = "ghost@example.com" };

        mMockRepo
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EvUser?)null);

        // act & assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await mSut.UpdateAsync(nonExistentUser));
    }

    #endregion

    #region DeleteAsync Tests

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that DeleteAsync successfully deletes existing user. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task DeleteAsync_WithExistingUser_DeletesUser()
    {   // arrange
        var existingUser = new EvUser { Id = 1, Name = "John", Email = "john@example.com" };

        mMockRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        mMockRepo
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // act
        var result = await mSut.DeleteAsync(1);

        // assert
        Assert.True(result);
        mMockRepo.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that DeleteAsync returns false when user not found. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_ReturnsFalse()
    {   // arrange
        mMockRepo
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EvUser?)null);

        // act
        var result = await mSut.DeleteAsync(999);

        // assert
        Assert.False(result);
        mMockRepo.Verify(r => r.DeleteAsync(It.IsAny<Int32>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetAllAsync Tests

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that GetAllAsync returns all users. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {   // arrange
        var users = new List<EvUser>
        {
            new() { Id = 1, Name = "John", Email = "john@example.com" },
            new() { Id = 2, Name = "Jane", Email = "jane@example.com" },
            new() { Id = 3, Name = "Bob", Email = "bob@example.com" }
        };

        mMockRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // act
        var result = await mSut.GetAllAsync();

        // assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Contains(result, u => u.Name == "John");
        Assert.Contains(result, u => u.Name == "Jane");
        Assert.Contains(result, u => u.Name == "Bob");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests that GetAllAsync returns empty list when no users exist. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task GetAllAsync_WhenNoUsers_ReturnsEmptyList()
    {   // arrange
        mMockRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EvUser>());

        // act
        var result = await mSut.GetAllAsync();

        // assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Integration Test Example

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Example integration test using real dependencies. </summary>
    ///
    /// <remarks>   This would typically be in a separate integration test project. </remarks>
    ///-------------------------------------------------------------------------------------------------
    [Fact(Skip = "Integration test - requires database")]
    public async Task IntegrationTest_CreateAndRetrieveUser()
    {   // arrange
        using var context = new TestDbContext();
        var repo = new EvUserRepository(context);
        var logger = new Mock<ILogger<EvUserService>>().Object;
        var service = new EvUserService(repo, logger);

        var newUser = new EvUser { Name = "Integration Test", Email = "test@integration.com" };

        // act
        var created = await service.CreateAsync(newUser);
        var retrieved = await service.GetByIdAsync(created.Id);

        // assert
        Assert.NotNull(retrieved);
        Assert.Equal("Integration Test", retrieved.Name);
        Assert.Equal("test@integration.com", retrieved.Email);
    }

    #endregion
}

#region Test Models and Interfaces

///-------------------------------------------------------------------------------------------------
/// <summary>   User entity for testing. </summary>
///-------------------------------------------------------------------------------------------------
public class EvUser
{
    public Int32 Id { get; set; }
    public String Name { get; set; } = String.Empty;
    public String Email { get; set; } = String.Empty;
}

///-------------------------------------------------------------------------------------------------
/// <summary>   User repository interface. </summary>
///-------------------------------------------------------------------------------------------------
public interface IEvUserRepository
{
    Task<EvUser?> GetByIdAsync(Int32 id, CancellationToken ct = default);
    Task<IEnumerable<EvUser>> GetAllAsync(CancellationToken ct = default);
    Task<EvUser> CreateAsync(EvUser user, CancellationToken ct = default);
    Task<EvUser> UpdateAsync(EvUser user, CancellationToken ct = default);
    Task<Boolean> DeleteAsync(Int32 id, CancellationToken ct = default);
}

///-------------------------------------------------------------------------------------------------
/// <summary>   User service for business logic. </summary>
///-------------------------------------------------------------------------------------------------
public class EvUserService
{
    private readonly IEvUserRepository mRepo;
    private readonly ILogger<EvUserService> mLogger;

    public EvUserService(IEvUserRepository repo, ILogger<EvUserService> logger)
    {
        mRepo = repo;
        mLogger = logger;
    }

    public Task<EvUser?> GetByIdAsync(Int32 id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));

        return mRepo.GetByIdAsync(id, ct);
    }

    public Task<IEnumerable<EvUser>> GetAllAsync(CancellationToken ct = default) =>
        mRepo.GetAllAsync(ct);

    public Task<EvUser> CreateAsync(EvUser user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (String.IsNullOrWhiteSpace(user.Name))
            throw new ValidationException("Name is required");

        if (String.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains('@'))
            throw new ValidationException("Valid email is required");

        return mRepo.CreateAsync(user, ct);
    }

    public async Task<EvUser> UpdateAsync(EvUser user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var existing = await mRepo.GetByIdAsync(user.Id, ct);
        if (existing == null)
            throw new NotFoundException($"User {user.Id} not found");

        return await mRepo.UpdateAsync(user, ct);
    }

    public async Task<Boolean> DeleteAsync(Int32 id, CancellationToken ct = default)
    {
        var existing = await mRepo.GetByIdAsync(id, ct);
        if (existing == null)
            return false;

        return await mRepo.DeleteAsync(id, ct);
    }
}

///-------------------------------------------------------------------------------------------------
/// <summary>   Custom exception types. </summary>
///-------------------------------------------------------------------------------------------------
public class ValidationException : Exception
{
    public ValidationException(String message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(String message) : base(message) { }
}

///-------------------------------------------------------------------------------------------------
/// <summary>   Mock logger interface. </summary>
///-------------------------------------------------------------------------------------------------
public interface ILogger<T> { }

///-------------------------------------------------------------------------------------------------
/// <summary>   Test database context placeholder. </summary>
///-------------------------------------------------------------------------------------------------
public class TestDbContext : IDisposable
{
    public void Dispose() { }
}

///-------------------------------------------------------------------------------------------------
/// <summary>   User repository implementation placeholder. </summary>
///-------------------------------------------------------------------------------------------------
public class EvUserRepository : IEvUserRepository
{
    public EvUserRepository(TestDbContext context) { }

    public Task<EvUser?> GetByIdAsync(Int32 id, CancellationToken ct = default) =>
        Task.FromResult<EvUser?>(null);

    public Task<IEnumerable<EvUser>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult(Enumerable.Empty<EvUser>());

    public Task<EvUser> CreateAsync(EvUser user, CancellationToken ct = default) =>
        Task.FromResult(user);

    public Task<EvUser> UpdateAsync(EvUser user, CancellationToken ct = default) =>
        Task.FromResult(user);

    public Task<Boolean> DeleteAsync(Int32 id, CancellationToken ct = default) =>
        Task.FromResult(true);
}

#endregion
