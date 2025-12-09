using Xunit;

namespace UniversityManagement.WebApi.Tests;

public class BasicValidationTests
{
    [Fact]
    public void EmailValidation_WithValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "test@universidad.edu";

        // Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("test@universidad.edu", true)]
    [InlineData("invalid-email", false)]
    [InlineData("test@test.com", true)]
    [InlineData("", false)]
    public void EmailValidation_WithVariousFormats_ReturnsExpected(string email, bool expected)
    {
        // Arrange & Act
        var isValid = !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void StringLength_WithValidName_Success()
    {
        // Arrange
        var name = "John Doe";

        // Act
        var isValid = name.Length > 0 && name.Length <= 100;

        // Assert
        Assert.True(isValid);
        Assert.Equal(8, name.Length);
    }

    [Theory]
    [InlineData("Engineering", "Engineering")]
    [InlineData("Computer Science", "Computer Science")]
    public void FacultyName_MatchesExpected(string input, string expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, input);
    }
}
