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
        private readonly Mock<IOdometerLogic> _mockOdometerLogic;
        private readonly Mock<IFileHelper> _mockFileHelper;
        private readonly Mock<IMailHelper> _mockMailHelper;
        private readonly Mock<IConfigHelper> _mockConfigHelper;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly APIController _apiController;

        public APIControllerTests()
        {
            var mockVehicleDataAccess = new Mock<IVehicleDataAccess>();
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
            var mockVehicleLogic = new Mock<IVehicleLogic>();
            _mockOdometerLogic = new Mock<IOdometerLogic>();
            _mockFileHelper = new Mock<IFileHelper>();
            _mockMailHelper = new Mock<IMailHelper>();
            _mockConfigHelper = new Mock<IConfigHelper>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            _apiController = new APIController(
                mockVehicleDataAccess.Object,
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
                mockVehicleLogic.Object,
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

        private readonly UserConfig _testUserConfig = new UserConfig
        {
            UseDefaultCaseSensitiveSearch = false, // Default to case insensitive
            VisibleTabs = new List<ImportMode> { ImportMode.ServiceRecord, ImportMode.RepairRecord, ImportMode.GasRecord }
        };

        [Fact]
        public void SearchRecords_CaseSensitiveTrue_ExactMatch()
        {
            // Arrange
            var vehicleId = 1;
            var searchQuery = "Oil";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now },
                new ServiceRecord { Id = 2, Description = "oil filter", Date = DateTime.Now }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());
            
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - explicitly set case sensitive to true
            var result = _apiController.SearchRecords(vehicleId, searchQuery, caseSensitive: true);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should only find "Oil Change" (exact case match), not "oil filter"
            Assert.Single(model);
            Assert.Equal(1, model.First().Id);
        }

        [Fact]
        public void SearchRecords_CaseSensitiveFalse_CaseInsensitiveMatch()
        {
            // Arrange
            var vehicleId = 1;
            var searchQuery = "OIL";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now },
                new ServiceRecord { Id = 2, Description = "oil filter", Date = DateTime.Now },
                new ServiceRecord { Id = 3, Description = "Brake Service", Date = DateTime.Now }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());
            
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - explicitly set case sensitive to false
            var result = _apiController.SearchRecords(vehicleId, searchQuery, caseSensitive: false);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should find both "Oil Change" and "oil filter" (case insensitive)
            Assert.Equal(2, model.Count);
            Assert.Contains(model, x => x.Id == 1);
            Assert.Contains(model, x => x.Id == 2);
        }

        [Fact]
        public void SearchRecords_NullCaseSensitive_UsesUserConfig()
        {
            // Arrange
            var vehicleId = 1;
            var searchQuery = "OIL";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now },
                new ServiceRecord { Id = 2, Description = "oil filter", Date = DateTime.Now }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());

            // User config is set to case insensitive (false)
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - don't provide caseSensitive parameter, should use user config
            var result = _apiController.SearchRecords(vehicleId, searchQuery, caseSensitive: null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should find both records because user config is case insensitive
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void SearchRecords_NullCaseSensitiveWithUserConfigCaseSensitive_UsesUserConfig()
        {
            // Arrange
            var vehicleId = 1;
            var searchQuery = "Oil";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now },
                new ServiceRecord { Id = 2, Description = "oil filter", Date = DateTime.Now }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());

            // Set user config to case sensitive (true)
            _testUserConfig.UseDefaultCaseSensitiveSearch = true;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - don't provide caseSensitive parameter, should use user config
            var result = _apiController.SearchRecords(vehicleId, searchQuery, caseSensitive: null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should only find "Oil Change" because user config is case sensitive
            Assert.Single(model);
            Assert.Equal(1, model.First().Id);
        }

        [Fact]
        public void SearchRecordsByTags_CaseSensitiveTrue_ExactTagMatch()
        {
            // Arrange
            var vehicleId = 1;
            var tags = "Maintenance";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now, Tags = new List<string> { "Maintenance", "Oil" } },
                new ServiceRecord { Id = 2, Description = "Filter Replace", Date = DateTime.Now, Tags = new List<string> { "maintenance", "filter" } }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());
            
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - explicitly set case sensitive to true
            var result = _apiController.SearchRecordsByTags(vehicleId, tags, caseSensitive: true);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should only find exact case match "Maintenance", not "maintenance"
            Assert.Single(model);
            Assert.Equal(1, model.First().Id);
        }

        [Fact]
        public void SearchRecordsByTags_CaseSensitiveFalse_CaseInsensitiveTagMatch()
        {
            // Arrange
            var vehicleId = 1;
            var tags = "MAINTENANCE";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now, Tags = new List<string> { "Maintenance", "Oil" } },
                new ServiceRecord { Id = 2, Description = "Filter Replace", Date = DateTime.Now, Tags = new List<string> { "maintenance", "filter" } },
                new ServiceRecord { Id = 3, Description = "Brake Service", Date = DateTime.Now, Tags = new List<string> { "brake", "safety" } }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());
            
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - explicitly set case sensitive to false
            var result = _apiController.SearchRecordsByTags(vehicleId, tags, caseSensitive: false);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should find both "Maintenance" and "maintenance" (case insensitive)
            Assert.Equal(2, model.Count);
            Assert.Contains(model, x => x.Id == 1);
            Assert.Contains(model, x => x.Id == 2);
        }

        [Fact]
        public void SearchRecordsByTags_NullCaseSensitive_UsesUserConfig()
        {
            // Arrange
            var vehicleId = 1;
            var tags = "MAINTENANCE";
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now, Tags = new List<string> { "Maintenance", "Oil" } },
                new ServiceRecord { Id = 2, Description = "Filter Replace", Date = DateTime.Now, Tags = new List<string> { "maintenance", "filter" } }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());

            // User config is set to case insensitive (false)
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - don't provide caseSensitive parameter, should use user config
            var result = _apiController.SearchRecordsByTags(vehicleId, tags, caseSensitive: null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should find both records because user config is case insensitive
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void SearchRecordsByTags_MultipleTags_HandlesSpacesCorrectly()
        {
            // Arrange
            var vehicleId = 1;
            var tags = "  maintenance   oil  "; // Extra spaces
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Oil Change", Date = DateTime.Now, Tags = new List<string> { "maintenance", "oil" } },
                new ServiceRecord { Id = 2, Description = "Filter Replace", Date = DateTime.Now, Tags = new List<string> { "maintenance", "filter" } },
                new ServiceRecord { Id = 3, Description = "Oil Filter", Date = DateTime.Now, Tags = new List<string> { "oil", "filter" } }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            
            // Setup empty collections for other record types to prevent null reference exceptions
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(new List<GasRecord>());
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());
            
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - case insensitive search
            var result = _apiController.SearchRecordsByTags(vehicleId, tags, caseSensitive: false);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should find records that have either "maintenance" or "oil" tags
            Assert.Equal(3, model.Count); // All records have at least one matching tag
        }

        [Fact]
        public void SearchRecords_EmptySearchQuery_ReturnsEmptyResults()
        {
            // Arrange
            var vehicleId = 1;
            var searchQuery = "";

            // Act
            var result = _apiController.SearchRecords(vehicleId, searchQuery);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            Assert.Empty(model);
        }

        [Fact]
        public void SearchRecordsByTags_EmptyTags_ReturnsEmptyResults()
        {
            // Arrange
            var vehicleId = 1;
            var tags = "";

            // Act
            var result = _apiController.SearchRecordsByTags(vehicleId, tags);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            Assert.Empty(model);
        }

        [Fact]
        public void SearchRecords_MultipleRecordTypes_ReturnsResultsFromAllTypes()
        {
            // Arrange
            var vehicleId = 1;
            var searchQuery = "test";
            
            var serviceRecords = new List<ServiceRecord>
            {
                new ServiceRecord { Id = 1, Description = "Test Service", Date = DateTime.Now }
            };
            
            var gasRecords = new List<GasRecord>
            {
                new GasRecord { Id = 2, Notes = "Test Gas Fill", Date = DateTime.Now }
            };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId))
                .Returns(serviceRecords);
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId))
                .Returns(gasRecords);

            // Setup empty collections for other record types to prevent null reference exceptions
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(new List<CollisionRecord>());
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(new List<UpgradeRecord>());
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(new List<TaxRecord>());
            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordsByVehicleId(vehicleId)).Returns(new List<SupplyRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());
            _mockNoteDataAccess.Setup(x => x.GetNotesByVehicleId(vehicleId)).Returns(new List<Note>());
            
            // Setup user config to show both service and gas records
            _testUserConfig.VisibleTabs = new List<ImportMode> { ImportMode.ServiceRecord, ImportMode.GasRecord };
            _testUserConfig.UseDefaultCaseSensitiveSearch = false;
            _mockConfigHelper.Setup(x => x.GetUserConfig(It.IsAny<ClaimsPrincipal>())).Returns(_testUserConfig);

            // Act - case insensitive search
            var result = _apiController.SearchRecords(vehicleId, searchQuery, caseSensitive: false);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<List<SearchResult>>(jsonResult.Value);
            
            // Should find results from both record types
            Assert.Equal(2, model.Count);
            Assert.Contains(model, x => x.RecordType == ImportMode.ServiceRecord);
            Assert.Contains(model, x => x.RecordType == ImportMode.GasRecord);
        }
    }
}