using CarCareTracker.Controllers;
using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CarCareTracker.Tests.Controllers
{
    public class APIControllerTests
    {
        private readonly Mock<IVehicleDataAccess> _mockVehicleDataAccess;
        private readonly Mock<INoteDataAccess> _mockNoteDataAccess;
        private readonly Mock<IServiceRecordDataAccess> _mockServiceRecordDataAccess;
        private readonly Mock<IGasRecordDataAccess> _mockGasRecordDataAccess;
        private readonly Mock<ICollisionRecordDataAccess> _mockCollisionRecordDataAccess;
        private readonly Mock<ITaxRecordDataAccess> _mockTaxRecordDataAccess;
        private readonly Mock<IReminderRecordDataAccess> _mockReminderRecordDataAccess;
        private readonly Mock<IUpgradeRecordDataAccess> _mockUpgradeRecordDataAccess;
        private readonly Mock<IOdometerRecordDataAccess> _mockOdometerRecordDataAccess;
        private readonly Mock<ISupplyRecordDataAccess> _mockSupplyRecordDataAccess;
        private readonly Mock<IPlanRecordDataAccess> _mockPlanRecordDataAccess;
        private readonly Mock<IPlanRecordTemplateDataAccess> _mockPlanRecordTemplateDataAccess;
        private readonly Mock<IUserAccessDataAccess> _mockUserAccessDataAccess;
        private readonly Mock<IUserRecordDataAccess> _mockUserRecordDataAccess;
        private readonly Mock<IReminderHelper> _mockReminderHelper;
        private readonly Mock<IGasHelper> _mockGasHelper;
        private readonly Mock<IUserLogic> _mockUserLogic;
        private readonly Mock<IVehicleLogic> _mockVehicleLogic;
        private readonly Mock<IOdometerLogic> _mockOdometerLogic;
        private readonly Mock<IFileHelper> _mockFileHelper;
        private readonly Mock<IMailHelper> _mockMailHelper;
        private readonly Mock<IConfigHelper> _mockConfigHelper;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly APIController _apiController;

        public APIControllerTests()
        {
            _mockVehicleDataAccess = new Mock<IVehicleDataAccess>();
            _mockNoteDataAccess = new Mock<INoteDataAccess>();
            _mockServiceRecordDataAccess = new Mock<IServiceRecordDataAccess>();
            _mockGasRecordDataAccess = new Mock<IGasRecordDataAccess>();
            _mockCollisionRecordDataAccess = new Mock<ICollisionRecordDataAccess>();
            _mockTaxRecordDataAccess = new Mock<ITaxRecordDataAccess>();
            _mockReminderRecordDataAccess = new Mock<IReminderRecordDataAccess>();
            _mockUpgradeRecordDataAccess = new Mock<IUpgradeRecordDataAccess>();
            _mockOdometerRecordDataAccess = new Mock<IOdometerRecordDataAccess>();
            _mockSupplyRecordDataAccess = new Mock<ISupplyRecordDataAccess>();
            _mockPlanRecordDataAccess = new Mock<IPlanRecordDataAccess>();
            _mockPlanRecordTemplateDataAccess = new Mock<IPlanRecordTemplateDataAccess>();
            _mockUserAccessDataAccess = new Mock<IUserAccessDataAccess>();
            _mockUserRecordDataAccess = new Mock<IUserRecordDataAccess>();
            _mockReminderHelper = new Mock<IReminderHelper>();
            _mockGasHelper = new Mock<IGasHelper>();
            _mockUserLogic = new Mock<IUserLogic>();
            _mockVehicleLogic = new Mock<IVehicleLogic>();
            _mockOdometerLogic = new Mock<IOdometerLogic>();
            _mockFileHelper = new Mock<IFileHelper>();
            _mockMailHelper = new Mock<IMailHelper>();
            _mockConfigHelper = new Mock<IConfigHelper>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            _apiController = new APIController(
                _mockVehicleDataAccess.Object,
                _mockGasHelper.Object,
                _mockReminderHelper.Object,
                _mockNoteDataAccess.Object,
                _mockServiceRecordDataAccess.Object,
                _mockGasRecordDataAccess.Object,
                _mockCollisionRecordDataAccess.Object,
                _mockTaxRecordDataAccess.Object,
                _mockReminderRecordDataAccess.Object,
                _mockUpgradeRecordDataAccess.Object,
                _mockOdometerRecordDataAccess.Object,
                _mockSupplyRecordDataAccess.Object,
                _mockPlanRecordDataAccess.Object,
                _mockPlanRecordTemplateDataAccess.Object,
                _mockUserAccessDataAccess.Object,
                _mockUserRecordDataAccess.Object,
                _mockMailHelper.Object,
                _mockFileHelper.Object,
                _mockConfigHelper.Object,
                _mockUserLogic.Object,
                _mockVehicleLogic.Object,
                _mockOdometerLogic.Object,
                _mockWebHostEnvironment.Object
            );

            // Setup controller context with authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser")
            }, "test"));

            _apiController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        [Fact]
        public void AddVehicle_ValidInput_ReturnsSuccessResponse()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Year = "2023",
                Make = "Toyota",
                Model = "Camry",
                LicensePlate = "ABC123",
                PurchaseDate = "2023-01-01",
                PurchasePrice = "25000"
            };

            var expectedVehicle = new Vehicle
            {
                Id = 1,
                Year = 2023,
                Make = "Toyota",
                Model = "Camry",
                LicensePlate = "ABC123"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>()))
                .Returns(true)
                .Callback<Vehicle>(v => v.Id = 1);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, 1)).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);
            Assert.Equal("Vehicle created successfully", operationResponse.Message);

            // Verify the vehicle was saved with correct properties
            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Year == 2023 &&
                v.Make == "Toyota" &&
                v.Model == "Camry" &&
                v.LicensePlate == "ABC123" &&
                v.ImageLocation == "/defaults/noimage.png" &&
                v.MapLocation == ""
            )), Times.Once);

            _mockUserLogic.Verify(x => x.AddUserAccessToVehicle(1, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void AddVehicle_MissingMake_ReturnsBadRequest()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Year = "2023",
                Model = "Camry" // Make is missing
            };

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Equal("Make and Model are required", operationResponse.Message);
            Assert.Equal(400, _apiController.Response.StatusCode);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void AddVehicle_MissingModel_ReturnsBadRequest()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Year = "2023",
                Make = "Toyota" // Model is missing
            };

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Equal("Make and Model are required", operationResponse.Message);
            Assert.Equal(400, _apiController.Response.StatusCode);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void AddVehicle_SaveVehicleFails_ReturnsFailureResponse()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Year = "2023",
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(false);

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Equal("Failed to create vehicle", operationResponse.Message);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.IsAny<Vehicle>()), Times.Once);
            _mockUserLogic.Verify(x => x.AddUserAccessToVehicle(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void AddVehicle_OptionalFieldsHandledCorrectly()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Honda",
                Model = "Civic",
                IsElectric = "true",
                IsDiesel = "false",
                UseHours = "true",
                OdometerOptional = "true",
                HasOdometerAdjustment = "true",
                OdometerMultiplier = "1.5",
                OdometerDifference = "100",
                VehicleIdentifier = "CustomField",
                Tags = "tag1 tag2 tag3",
                ExtraFields = new List<ExtraField> { new ExtraField { Name = "Color", Value = "Red" } }
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            // Verify optional fields are handled correctly
            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.IsElectric == true &&
                v.IsDiesel == false &&
                v.UseHours == true &&
                v.OdometerOptional == true &&
                v.HasOdometerAdjustment == true &&
                v.OdometerMultiplier == "1.5" &&
                v.OdometerDifference == "100" &&
                v.VehicleIdentifier == "CustomField" &&
                v.Tags.Count == 3 &&
                v.Tags.Contains("tag1") &&
                v.ExtraFields.Count == 1 &&
                v.ExtraFields.First().Name == "Color"
            )), Times.Once);
        }

        [Fact]
        public void AddVehicle_DefaultsHandledCorrectly()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Ford",
                Model = "F150"
                // All optional fields are null/empty
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            // Verify defaults are applied correctly
            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Year == DateTime.Now.Year && // Default to current year
                v.LicensePlate == "" &&
                v.PurchasePrice == 0 &&
                v.SoldPrice == 0 &&
                v.IsElectric == false &&
                v.IsDiesel == false &&
                v.UseHours == false &&
                v.OdometerOptional == false &&
                v.HasOdometerAdjustment == false &&
                v.OdometerMultiplier == "1" &&
                v.OdometerDifference == "0" &&
                v.VehicleIdentifier == "LicensePlate" &&
                v.Tags.Count == 0 &&
                v.ExtraFields.Count == 0
            )), Times.Once);
        }

        [Fact]
        public void AddVehicle_ExceptionThrown_ReturnsServerError()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>()))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Contains("Error creating vehicle", operationResponse.Message);
            Assert.Contains("Database error", operationResponse.Message);
            Assert.Equal(500, _apiController.Response.StatusCode);
        }

        [Fact]
        public void AddVehicleJson_CallsAddVehicle()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicleJson(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.IsAny<Vehicle>()), Times.Once);
        }

        [Theory]
        [InlineData("", "Model")]
        [InlineData(null, "Model")]
        [InlineData("Make", "")]
        [InlineData("Make", null)]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void AddVehicle_InvalidMakeOrModel_ReturnsBadRequest(string make, string model)
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = make,
                Model = model
            };

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Equal("Make and Model are required", operationResponse.Message);
            Assert.Equal(400, _apiController.Response.StatusCode);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.IsAny<Vehicle>()), Times.Never);
        }

        [Fact]
        public void AddVehicle_WhitespaceOnlyMakeModel_ReturnsBadRequest()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "   ",
                Model = "   "
            };

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Equal("Make and Model are required", operationResponse.Message);
            Assert.Equal(400, _apiController.Response.StatusCode);
        }

        [Theory]
        [InlineData("invalid-year")]
        [InlineData("abc")]
        [InlineData("20xx")]
        public void AddVehicle_InvalidYear_UsesCurrentYear(string invalidYear)
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Year = invalidYear,
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Year == DateTime.Now.Year
            )), Times.Once);
        }

        [Theory]
        [InlineData("invalid-date")]
        [InlineData("2023-13-01")]
        [InlineData("not-a-date")]
        public void AddVehicle_InvalidDates_IgnoresInvalidDates(string invalidDate)
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry",
                PurchaseDate = invalidDate,
                SoldDate = invalidDate
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                string.IsNullOrEmpty(v.PurchaseDate) && string.IsNullOrEmpty(v.SoldDate)
            )), Times.Once);
        }

        [Theory]
        [InlineData("invalid-price")]
        [InlineData("abc")]
        [InlineData("$1000")]
        public void AddVehicle_InvalidPrices_UsesZero(string invalidPrice)
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry",
                PurchasePrice = invalidPrice,
                SoldPrice = invalidPrice
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.PurchasePrice == 0 && v.SoldPrice == 0
            )), Times.Once);
        }

        [Theory]
        [InlineData("invalid-bool")]
        [InlineData("yes")]
        [InlineData("1")]
        [InlineData("")]
        public void AddVehicle_InvalidBooleans_UsesFalse(string invalidBool)
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry",
                IsElectric = invalidBool,
                IsDiesel = invalidBool,
                UseHours = invalidBool,
                OdometerOptional = invalidBool,
                HasOdometerAdjustment = invalidBool
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.IsElectric == false &&
                v.IsDiesel == false &&
                v.UseHours == false &&
                v.OdometerOptional == false &&
                v.HasOdometerAdjustment == false
            )), Times.Once);
        }

        [Fact]
        public void AddVehicle_NullExtraFields_CreatesEmptyList()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry",
                ExtraFields = null
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.ExtraFields != null && v.ExtraFields.Count == 0
            )), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void AddVehicle_EmptyTags_CreatesEmptyTagsList(string emptyTags)
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry",
                Tags = emptyTags
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Tags != null && v.Tags.Count == 0
            )), Times.Once);
        }

        [Fact]
        public void AddVehicle_TagsWithExtraSpaces_HandlesCorrectly()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry",
                Tags = "  tag1   tag2    tag3  "
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Tags.Count == 3 && // filters out empty strings from extra spaces
                v.Tags.Contains("tag1") &&
                v.Tags.Contains("tag2") &&
                v.Tags.Contains("tag3")
            )), Times.Once);
        }

        [Fact]
        public void AddVehicle_UserAccessFails_StillReturnsSuccess()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(false);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);
            Assert.Equal("Vehicle created successfully", operationResponse.Message);

            _mockUserLogic.Verify(x => x.AddUserAccessToVehicle(1, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void AddVehicle_CompleteVehicleData_AllFieldsSet()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Year = "2020",
                Make = "Tesla",
                Model = "Model 3",
                LicensePlate = "TESLA123",
                PurchaseDate = "2020-06-15",
                SoldDate = "2023-08-20",
                PurchasePrice = "45000.50",
                SoldPrice = "35000.75",
                IsElectric = "true",
                IsDiesel = "false",
                UseHours = "false",
                OdometerOptional = "true",
                HasOdometerAdjustment = "true",
                OdometerMultiplier = "0.8",
                OdometerDifference = "500",
                VehicleIdentifier = "VIN",
                Tags = "electric luxury sedan",
                ExtraFields = new List<ExtraField>
                {
                    new ExtraField { Name = "Color", Value = "White" },
                    new ExtraField { Name = "Autopilot", Value = "Yes" }
                }
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Year == 2020 &&
                v.Make == "Tesla" &&
                v.Model == "Model 3" &&
                v.LicensePlate == "TESLA123" &&
                v.PurchasePrice == 45000.50m &&
                v.SoldPrice == 35000.75m &&
                v.IsElectric == true &&
                v.IsDiesel == false &&
                v.UseHours == false &&
                v.OdometerOptional == true &&
                v.HasOdometerAdjustment == true &&
                v.OdometerMultiplier == "0.8" &&
                v.OdometerDifference == "500" &&
                v.VehicleIdentifier == "VIN" &&
                v.Tags.Count == 3 &&
                v.Tags.Contains("electric") &&
                v.Tags.Contains("luxury") &&
                v.Tags.Contains("sedan") &&
                v.ExtraFields.Count == 2 &&
                v.ExtraFields.Any(ef => ef.Name == "Color" && ef.Value == "White") &&
                v.ExtraFields.Any(ef => ef.Name == "Autopilot" && ef.Value == "Yes") &&
                v.ImageLocation == "/defaults/noimage.png" &&
                v.MapLocation == ""
            )), Times.Once);
        }

        [Fact] 
        public void AddVehicle_DatabaseError_ReturnsServerError()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>()))
                .Throws(new System.Data.DataException("Connection timeout"));

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Contains("Error creating vehicle", operationResponse.Message);
            Assert.Contains("Connection timeout", operationResponse.Message);
            Assert.Equal(500, _apiController.Response.StatusCode);
        }

        [Fact]
        public void AddVehicle_ArgumentNullException_ReturnsServerError()
        {
            // Arrange
            var vehicleInput = new VehicleExportModel
            {
                Make = "Toyota",
                Model = "Camry"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>()))
                .Throws(new ArgumentNullException("vehicle", "Vehicle cannot be null"));

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.False(operationResponse.Success);
            Assert.Contains("Error creating vehicle", operationResponse.Message);
            Assert.Contains("Vehicle cannot be null", operationResponse.Message);
            Assert.Equal(500, _apiController.Response.StatusCode);
        }

        [Fact]
        public void AddVehicle_MinimalValidData_CreatesVehicleWithDefaults()
        {
            // Arrange - Only required fields
            var vehicleInput = new VehicleExportModel
            {
                Make = "Hyundai",
                Model = "Elantra"
            };

            _mockVehicleDataAccess.Setup(x => x.SaveVehicle(It.IsAny<Vehicle>())).Returns(true);
            _mockUserLogic.Setup(x => x.AddUserAccessToVehicle(1, It.IsAny<int>())).Returns(true);
            _mockConfigHelper.Setup(x => x.GetWebHookUrl()).Returns("");

            // Act
            var result = _apiController.AddVehicle(vehicleInput);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var operationResponse = Assert.IsType<OperationResponse>(jsonResult.Value);
            Assert.True(operationResponse.Success);

            // Verify all defaults are applied correctly
            _mockVehicleDataAccess.Verify(x => x.SaveVehicle(It.Is<Vehicle>(v =>
                v.Make == "Hyundai" &&
                v.Model == "Elantra" &&
                v.Year == DateTime.Now.Year && 
                v.LicensePlate == "" &&
                string.IsNullOrEmpty(v.PurchaseDate) &&
                string.IsNullOrEmpty(v.SoldDate) &&
                v.PurchasePrice == 0 &&
                v.SoldPrice == 0 &&
                v.IsElectric == false &&
                v.IsDiesel == false &&
                v.UseHours == false &&
                v.OdometerOptional == false &&
                v.HasOdometerAdjustment == false &&
                v.OdometerMultiplier == "1" &&
                v.OdometerDifference == "0" &&
                v.VehicleIdentifier == "LicensePlate" &&
                v.Tags.Count == 0 &&
                v.ExtraFields.Count == 0 &&
                v.ImageLocation == "/defaults/noimage.png" &&
                v.MapLocation == ""
            )), Times.Once);
        }
    }
}