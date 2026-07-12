using dotnet_rest_api.DTOs;
using dotnet_rest_api.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace UnitTests;

/// <summary>
/// Exercises the FluentValidation validators directly (no DB, no pipeline):
/// password complexity, username charset, status whitelist, coordinate ranges,
/// and the conditional (patch) rules.
/// </summary>
public class ValidatorTests
{
    // ---- RegisterRequestValidator ----

    private static RegisterRequest ValidRegister() => new()
    {
        Username = "valid_user1",
        Email = "user@example.com",
        Password = "Passw0rd!",
        FullName = "Valid User",
        RoleId = 3,
    };

    [Fact]
    public void Register_Valid_Passes()
    {
        var result = new RegisterRequestValidator().TestValidate(ValidRegister());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("nolower1!A", false)]   // has upper/digit/special/lower -> actually valid; kept below
    [InlineData("PASSW0RD!", true)]     // no lowercase
    [InlineData("password!", true)]     // no uppercase, no digit
    [InlineData("Password!", true)]     // no digit
    [InlineData("Password1", true)]     // no special char
    [InlineData("Pw1!", true)]          // too short (< 8)
    public void Register_Password_ComplexityEnforced(string password, bool shouldError)
    {
        var model = ValidRegister();
        model.Password = password;
        var result = new RegisterRequestValidator().TestValidate(model);

        if (shouldError)
            result.ShouldHaveValidationErrorFor(x => x.Password);
        else
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("bad user")]   // space
    [InlineData("bad-user")]   // hyphen
    [InlineData("bad@user")]   // symbol
    public void Register_Username_RejectsNonWordChars(string username)
    {
        var model = ValidRegister();
        model.Username = username;
        new RegisterRequestValidator().TestValidate(model)
            .ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Register_Email_Invalid_Fails()
    {
        var model = ValidRegister();
        model.Email = "not-an-email";
        new RegisterRequestValidator().TestValidate(model)
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Register_RoleId_NonPositive_Fails()
    {
        var model = ValidRegister();
        model.RoleId = 0;
        new RegisterRequestValidator().TestValidate(model)
            .ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    // ---- LoginRequestValidator ----

    [Fact]
    public void Login_Valid_Passes()
    {
        new LoginRequestValidator()
            .TestValidate(new LoginRequest { Username = "alice", Password = "secret6" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("ab", "secret6")]   // username < 3
    [InlineData("alice", "12345")]  // password < 6
    public void Login_TooShort_Fails(string username, string password)
    {
        var result = new LoginRequestValidator()
            .TestValidate(new LoginRequest { Username = username, Password = password });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Login_LongEmailIdentifier_Passes()
    {
        // Regression: the identifier can be an email (login matches username OR email),
        // so a >50-char email must not be rejected by a stale username-length cap.
        var email = new string('a', 60) + "@example.com"; // 72 chars, valid email
        Assert.True(email.Length > 50);

        new LoginRequestValidator()
            .TestValidate(new LoginRequest { Username = email, Password = "secret6" })
            .ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    // ---- CreateProjectRequestValidator ----

    private static CreateProjectRequest ValidProject() => new()
    {
        ProjectName = "Rooftop Solar",
        Address = "123 Sunny Street",
        StartDate = new DateTime(2026, 1, 1),
        EstimatedEndDate = new DateTime(2026, 6, 1),
    };

    [Fact]
    public void CreateProject_Valid_Passes()
    {
        new CreateProjectRequestValidator().TestValidate(ValidProject())
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProject_EstimatedEndBeforeStart_Fails()
    {
        var model = ValidProject();
        model.EstimatedEndDate = model.StartDate.AddDays(-1);
        new CreateProjectRequestValidator().TestValidate(model)
            .ShouldHaveValidationErrorFor(x => x.EstimatedEndDate);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    public void CreateProject_CoordinatesOutOfRange_Fail(decimal lat, decimal lon)
    {
        var model = ValidProject();
        model.LocationCoordinates = new LocationCoordinatesDto { Latitude = lat, Longitude = lon };
        var result = new CreateProjectRequestValidator().TestValidate(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateProject_ShortName_Fails()
    {
        var model = ValidProject();
        model.ProjectName = "ab";
        new CreateProjectRequestValidator().TestValidate(model)
            .ShouldHaveValidationErrorFor(x => x.ProjectName);
    }

    // ---- UpdateProjectRequestValidator ----

    [Theory]
    [InlineData("InProgress", true)]
    [InlineData("Bogus", false)]
    public void UpdateProject_Status_WhitelistEnforced(string status, bool valid)
    {
        var model = new UpdateProjectRequest
        {
            ProjectName = "Rooftop Solar",
            Address = "123 Sunny Street",
            Status = status,
            StartDate = new DateTime(2026, 1, 1),
        };
        var result = new UpdateProjectRequestValidator().TestValidate(model);

        if (valid)
            result.ShouldNotHaveValidationErrorFor(x => x.Status);
        else
            result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    // ---- PatchProjectRequestValidator ----

    [Fact]
    public void PatchProject_AllNull_Passes()
    {
        // Every rule is guarded by a When(non-empty) clause, so an empty patch is valid.
        new PatchProjectRequestValidator().TestValidate(new PatchProjectRequest())
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PatchProject_InvalidStatus_Fails()
    {
        new PatchProjectRequestValidator().TestValidate(new PatchProjectRequest { Status = "Bogus" })
            .ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void PatchProject_ShortName_Fails()
    {
        new PatchProjectRequestValidator().TestValidate(new PatchProjectRequest { ProjectName = "ab" })
            .ShouldHaveValidationErrorFor(x => x.ProjectName);
    }
}
