# Tests

This project contains unit tests for the CarCareTracker application using xUnit testing framework.

## Test Structure

The tests are organized by namespace and functionality:

- **Logic/**: Tests for business logic classes
  - `UserLogicTests.cs` - Tests for user management and access control
  - `VehicleLogicTests.cs` - Tests for vehicle-related business logic
  - `OdometerLogicTests.cs` - Tests for odometer record management
  - `LoginLogicTests.cs` - Tests for authentication and user registration

## Dependencies

- **xUnit** - Testing framework
- **Moq** - Mocking framework for dependencies
- **Microsoft.NET.Test.Sdk** - Test SDK
- **Microsoft.Extensions.Logging.Abstractions** - For logging interfaces
- **Microsoft.Extensions.Caching.Memory** - For memory cache interfaces

## Running Tests

With .NET CLI:
```bash
dotnet test
```

With specific filter:
```bash
dotnet test --filter "FullyQualifiedName~UserLogicTests"
```

With coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

With detailed output:
```bash
dotnet test --logger:"console;verbosity=detailed"
```

## Test Patterns

### Mocking
All tests use Moq to mock dependencies and isolate the units under test:

```csharp
private readonly Mock<IUserAccessDataAccess> _mockUserAccess;
private readonly UserLogic _userLogic;

public UserLogicTests()
{
    _mockUserAccess = new Mock<IUserAccessDataAccess>();
    _userLogic = new UserLogic(_mockUserAccess.Object, ...);
}
```

### Test Structure
Tests follow the Arrange-Act-Assert pattern:

```csharp
[Fact]
public void Method_Scenario_ExpectedResult()
{
    // Arrange
    var input = new TestData();
    _mockDependency.Setup(x => x.Method()).Returns(expectedValue);
    
    // Act
    var result = _unitUnderTest.Method(input);
    
    // Assert
    Assert.Equal(expectedValue, result);
    _mockDependency.Verify(x => x.Method(), Times.Once);
}
```

## Coverage

The test suite covers:
- ✅ **UserLogic** - Complete coverage of user access control methods
- ✅ **VehicleLogic** - Coverage of vehicle record aggregation and calculations
- ✅ **OdometerLogic** - Coverage of odometer record processing
- ✅ **LoginLogic** - Coverage of authentication and user management

## Future Improvements

- Add integration tests for data access layers
- Add controller tests with ASP.NET Core testing utilities
- Add end-to-end tests for critical user workflows
- Add performance tests for heavy calculation methods