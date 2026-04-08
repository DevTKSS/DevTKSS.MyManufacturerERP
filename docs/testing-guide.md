# Testing Guide

This guide explains how to write and run tests for DevTKSS.MyManufacturerERP.

## Test Projects

The solution contains the following test projects:

- **DevTKSS.MyManufacturerERP.Tests** - Runtime unit tests using MSTest
- **DevTKSS.MyManufacturerERP.xUnitTests** - Runtime unit tests using xUnit
- **DevTKSS.MyManufacturerERP.UITests** - UI tests for validating the user interface

## Running Tests

### Using Visual Studio

1. Open the solution in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Open Test Explorer (Test > Test Explorer)
4. Click "Run All" to execute all tests
5. Or right-click specific tests/projects to run them individually

### Using Command Line

```bash
# Run all tests
dotnet test

# Run tests in a specific project
dotnet test src/Tests/DevTKSS.MyManufacturerERP.Tests
dotnet test src/Tests/DevTKSS.MyManufacturerERP.xUnitTests
dotnet test src/Tests/DevTKSS.MyManufacturerERP.UITests

# Run with verbose output
dotnet test --verbosity detailed
```

## Writing Tests

### Runtime Tests (MSTest)

Create test classes in `DevTKSS.MyManufacturerERP.Tests`:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevTKSS.MyManufacturerERP.Tests;

[TestClass]
public class YourFeatureTests
{
    [TestMethod]
    public void YourTest_WhenCondition_ShouldExpectedBehavior()
    {
        // Arrange
        var sut = new YourClass();
        
        // Act
        var result = sut.YourMethod();
        
        // Assert
        Assert.AreEqual(expectedValue, result);
    }
}
```

### Runtime Tests (xUnit)

Create test classes in `DevTKSS.MyManufacturerERP.xUnitTests`:

```csharp
using Xunit;

namespace DevTKSS.MyManufacturerERP.xUnitTests;

public class YourFeatureTests
{
    [Fact]
    public void YourTest_WhenCondition_ShouldExpectedBehavior()
    {
        // Arrange
        var sut = new YourClass();
        
        // Act
        var result = sut.YourMethod();
        
        // Assert
        Assert.Equal(expectedValue, result);
    }
}
```

### UI Tests

Create UI test classes in `DevTKSS.MyManufacturerERP.UITests`:

```csharp
namespace DevTKSS.MyManufacturerERP.UITests;

public class Given_YourPage : TestBase
{
    [Test]
    public async Task When_UserAction_Then_ExpectedUIBehavior()
    {
        // Arrange
        var page = App.Marked("YourPageElement");
        
        // Act
        page.Tap();
        await Task.Delay(500); // Wait for UI update
        
        // Assert
        App.WaitForElement("ExpectedElement");
    }
}
```

## Test Naming Convention

Use the following naming pattern for test methods:

```
MethodName_WhenCondition_ShouldExpectedBehavior
```

Examples:
- `Calculate_WhenValidInput_ShouldReturnCorrectSum`
- `SaveData_WhenDatabaseUnavailable_ShouldThrowException`
- `LoginButton_WhenClicked_ShouldNavigateToMainPage`

## When to Add Tests

### Bug Fixes
- Add a test that reproduces the bug
- Verify the test fails before the fix
- Verify the test passes after the fix

### New Features
- Add tests for all new public APIs
- Add tests for edge cases and error conditions
- Add UI tests if the feature includes user interface changes

### Refactoring
- Ensure all existing tests still pass
- Add tests for any new code paths introduced

## Manual Testing

If automated tests are not feasible, provide manual test steps in your PR:

1. Describe the setup required
2. List step-by-step actions to perform
3. Describe the expected results
4. Include screenshots or screen recordings if helpful

## Test Coverage

Aim for:
- **Business Logic**: 80%+ code coverage
- **UI Components**: Test critical user flows
- **Edge Cases**: Handle nulls, empty collections, invalid inputs

## Continuous Integration

Tests are automatically run on:
- Every push to the repository
- Every pull request
- Before merging to main/master

Ensure all tests pass before requesting a PR review.
