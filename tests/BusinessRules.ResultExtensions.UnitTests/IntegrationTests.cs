using HelperMonads;

namespace BusinessRules.ResultExtensions.UnitTests;

/// <summary>
///     Integration tests demonstrating real-world usage scenarios
/// </summary>
[TestFixture]
public class IntegrationTests
{
    private class User
    {
        public string Username { get; init; } = string.Empty;
        public int Age { get; init; }
        public string Password { get; init; } = string.Empty;
    }

    [Test]
    public void UserCreation_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var username = "john_doe";
        var age = 25;
        var password = "SecurePass123";

        // Act
        var result = CreateUser(username, age, password);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Username, Is.EqualTo(username));
            Assert.That(result.Data.Age, Is.EqualTo(age));
            Assert.That(result.Data.Password, Is.EqualTo(password));
        });
    }

    [Test]
    public void UserCreation_WithInvalidAge_ReturnsFailureResult()
    {
        // Arrange
        var username = "jane_doe";
        var age = 16;
        var password = "SecurePass123";

        // Act
        var result = CreateUser(username, age, password);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Key, Is.EqualTo("TEST_USER_AGE_MIN"));
    }

    [Test]
    public void UserCreation_WithShortPassword_ReturnsFailureResult()
    {
        // Arrange
        var username = "bob";
        var age = 30;
        var password = "short";

        // Act
        var result = CreateUser(username, age, password);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Key, Is.EqualTo("TEST_PWD_MIN_LENGTH"));
    }

    [Test]
    public void UserCreation_WithPasswordMissingUppercase_ReturnsFailureResult()
    {
        // Arrange
        var username = "alice";
        var age = 28;
        var password = "password123";

        // Act
        var result = CreateUser(username, age, password);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Key, Is.EqualTo("TEST_PWD_UPPERCASE"));
    }

    [Test]
    public void UserCreation_WithPasswordMissingNumber_ReturnsFailureResult()
    {
        // Arrange
        var username = "charlie";
        var age = 22;
        var password = "SecurePassword";

        // Act
        var result = CreateUser(username, age, password);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Key, Is.EqualTo("TEST_PWD_NUMBER"));
    }

    [Test]
    public async Task AsyncUserCreation_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var username = "async_user";
        var age = 30;
        var password = "SecurePass123";

        // Act
        var result = await CreateUserAsync(username, age, password);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Username, Is.EqualTo(username));
        });
    }

    [Test]
    public async Task AsyncUserCreation_WithInvalidAge_ReturnsFailureResult()
    {
        // Arrange
        var username = "young_user";
        var age = 15;
        var password = "SecurePass123";

        // Act
        var result = await CreateUserAsync(username, age, password);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception!.Key, Is.EqualTo("TEST_USER_AGE_MIN"));
    }

    [Test]
    public void ChainedValidation_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var age = 25;
        var password = "SecurePass123";

        // Act
        var result = ValidateAge(age)
            .BindAndTransform(_ => ValidatePassword(password))
            .Map(validPassword => new User
            {
                Age = age,
                Password = validPassword
            });
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Age, Is.EqualTo(age));
        });
    }

    [Test]
    public void ChainedValidation_WithInvalidAge_ShortCircuitsAtFirstFailure()
    {
        // Arrange
        var age = 16;
        var password = "SecurePass123";

        // Act
        var result = ValidateAge(age)
            .BindAndTransform(_ => ValidatePassword(password))
            .Map(validPassword => new User
            {
                Age = age,
                Password = validPassword
            });

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception!.Key, Is.EqualTo("TEST_USER_AGE_MIN"));
    }

    [Test]
    public void ChainedValidation_WithValidAgeButInvalidPassword_FailsAtPasswordValidation()
    {
        // Arrange
        var age = 25;
        var password = "short";

        // Act
        var result = ValidateAge(age)
            .BindAndTransform(_ => ValidatePassword(password))
            .Map(validPassword => new User
            {
                Age = age,
                Password = validPassword
            });

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var exception = result.Error?.Exception as BusinessRuleViolationException;
        Assert.That(exception!.Key, Is.EqualTo("TEST_PWD_MIN_LENGTH"));
    }

    [Test]
    public void OnSuccess_WithSuccessfulResult_ExecutesAction()
    {
        // Arrange
        var actionExecuted = false;
        var age = 25;

        // Act
        var result = ValidateAge(age)
            .OnSuccess(_ => actionExecuted = true);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(actionExecuted, Is.True);
    }

    [Test]
    public void OnFailure_WithFailedResult_ExecutesAction()
    {
        // Arrange
        Error? capturedError = null;
        var age = 16;

        // Act
        var result = ValidateAge(age)
            .OnFailure(error => capturedError = error);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(capturedError, Is.Not.Null);
            Assert.That(capturedError!.Exception, Is.InstanceOf<BusinessRuleViolationException>());
        });
    }

    // Helper methods simulating real-world service methods

    private Result<User> CreateUser(string username, int age, string password)
    {
        return ValidateAge(age)
            .BindAndTransform(_ => ValidatePassword(password))
            .Map(validPassword => new User
            {
                Username = username,
                Age = age,
                Password = validPassword
            });
    }

    private async Task<Result<User>> CreateUserAsync(string username, int age, string password)
    {
        var ageResult = await ValidateAgeAsync(age);
        var passwordResult =
            await ageResult.BindAndTransformAsync(async _ => await ValidatePasswordAsync(password));

        return await passwordResult.MapAsync(async validPassword =>
        {
            await Task.Delay(1); // Simulate async operation
            return new User
            {
                Username = username,
                Age = age,
                Password = validPassword
            };
        });
    }

    private Result<int> ValidateAge(int age)
    {
        return age.EnsureBusinessRule(
            a => a >= 18,
            new TestUserMustBeAdult(),
            $"Age {age} is below minimum requirement of 18");
    }

    private async Task<Result<int>> ValidateAgeAsync(int age)
    {
        await Task.Delay(1); // Simulate async operation
        return age.EnsureBusinessRule(
            a => a >= 18,
            new TestUserMustBeAdult(),
            $"Age {age} is below minimum requirement of 18");
    }

    private Result<string> ValidatePassword(string password)
    {
        return password.ValidateAll(
            (p => p.Length >= 8, new TestPasswordMinLength(), "Password is too short"),
            (p => p.Any(char.IsUpper), new TestPasswordMustContainUppercase(), "Password must have uppercase letters"),
            (p => p.Any(char.IsDigit), new TestPasswordMustContainNumber(), "Password must contain numbers")
        );
    }

    private async Task<Result<string>> ValidatePasswordAsync(string password)
    {
        await Task.Delay(1); // Simulate async operation
        return password.ValidateAll(
            (p => p.Length >= 8, new TestPasswordMinLength(), "Password is too short"),
            (p => p.Any(char.IsUpper), new TestPasswordMustContainUppercase(), "Password must have uppercase letters"),
            (p => p.Any(char.IsDigit), new TestPasswordMustContainNumber(), "Password must contain numbers")
        );
    }
}