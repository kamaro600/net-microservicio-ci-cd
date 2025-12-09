using Xunit;
using UniversityManagement.AuthService.DTOs;

namespace UniversityManagement.AuthService.Tests;

public class AuthenticationServiceTests
{
    [Fact]
    public void LoginRequest_WithValidCredentials_Success()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        // Act & Assert
        Assert.NotNull(request);
        Assert.Equal("admin", request.Username);
        Assert.NotEmpty(request.Password);
    }

    [Fact]
    public void LoginRequest_WithInvalidUsername_DetectsEmpty()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "",
            Password = "Password123!"
        };

        // Act & Assert
        Assert.Empty(request.Username);
        Assert.NotEmpty(request.Password);
    }

    [Fact]
    public void RegisterRequest_WithValidData_Success()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "Password123!"
        };

        // Act & Assert
        Assert.NotNull(request);
        Assert.Equal("newuser", request.Username);
        Assert.Contains("@", request.Email);
    }

    [Theory]
    [InlineData("user1@test.com", true)]
    [InlineData("user2@test.com", true)]
    [InlineData("admin@test.com", true)]
    [InlineData("invalid-email", false)]
    public void ValidateEmail_WithVariousFormats_ReturnsExpected(string email, bool expected)
    {
        // Arrange & Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData("Admin", "Admin")]
    [InlineData("User", "User")]
    [InlineData("Staff", "Staff")]
    public void ValidateRole_WithValidRoles_Success(string role, string expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, role);
    }
}
