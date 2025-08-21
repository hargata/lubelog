using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CarCareTracker.Tests.Logic
{
    public class VehicleLogicTests
    {
        private readonly Mock<IServiceRecordDataAccess> _mockServiceRecordDataAccess;
        private readonly Mock<IGasRecordDataAccess> _mockGasRecordDataAccess;
        private readonly Mock<ICollisionRecordDataAccess> _mockCollisionRecordDataAccess;
        private readonly Mock<IUpgradeRecordDataAccess> _mockUpgradeRecordDataAccess;
        private readonly Mock<ITaxRecordDataAccess> _mockTaxRecordDataAccess;
        private readonly Mock<IOdometerRecordDataAccess> _mockOdometerRecordDataAccess;
        private readonly Mock<IReminderRecordDataAccess> _mockReminderRecordDataAccess;
        private readonly Mock<IPlanRecordDataAccess> _mockPlanRecordDataAccess;
        private readonly Mock<IReminderHelper> _mockReminderHelper;
        private readonly Mock<IVehicleDataAccess> _mockVehicleDataAccess;
        private readonly Mock<ISupplyRecordDataAccess> _mockSupplyRecordDataAccess;
        private readonly Mock<ILogger<VehicleLogic>> _mockLogger;
        private readonly VehicleLogic _vehicleLogic;

        public VehicleLogicTests()
        {
            _mockServiceRecordDataAccess = new Mock<IServiceRecordDataAccess>();
            _mockGasRecordDataAccess = new Mock<IGasRecordDataAccess>();
            _mockCollisionRecordDataAccess = new Mock<ICollisionRecordDataAccess>();
            _mockUpgradeRecordDataAccess = new Mock<IUpgradeRecordDataAccess>();
            _mockTaxRecordDataAccess = new Mock<ITaxRecordDataAccess>();
            _mockOdometerRecordDataAccess = new Mock<IOdometerRecordDataAccess>();
            _mockReminderRecordDataAccess = new Mock<IReminderRecordDataAccess>();
            _mockPlanRecordDataAccess = new Mock<IPlanRecordDataAccess>();
            _mockReminderHelper = new Mock<IReminderHelper>();
            _mockVehicleDataAccess = new Mock<IVehicleDataAccess>();
            _mockSupplyRecordDataAccess = new Mock<ISupplyRecordDataAccess>();
            _mockLogger = new Mock<ILogger<VehicleLogic>>();

            _vehicleLogic = new VehicleLogic(
                _mockServiceRecordDataAccess.Object,
                _mockGasRecordDataAccess.Object,
                _mockCollisionRecordDataAccess.Object,
                _mockUpgradeRecordDataAccess.Object,
                _mockTaxRecordDataAccess.Object,
                _mockOdometerRecordDataAccess.Object,
                _mockReminderRecordDataAccess.Object,
                _mockPlanRecordDataAccess.Object,
                _mockReminderHelper.Object,
                _mockVehicleDataAccess.Object,
                _mockSupplyRecordDataAccess.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public void GetVehicleRecords_ReturnsCorrectRecords()
        {
            // Arrange
            var vehicleId = 1;
            var serviceRecords = new List<ServiceRecord> { new ServiceRecord { Id = 1, Cost = 100 } };
            var gasRecords = new List<GasRecord> { new GasRecord { Id = 1, Cost = 50 } };
            var collisionRecords = new List<CollisionRecord> { new CollisionRecord { Id = 1, Cost = 200 } };
            var taxRecords = new List<TaxRecord> { new TaxRecord { Id = 1, Cost = 75 } };
            var upgradeRecords = new List<UpgradeRecord> { new UpgradeRecord { Id = 1, Cost = 300 } };
            var odometerRecords = new List<OdometerRecord> { new OdometerRecord { Id = 1, Mileage = 1000 } };

            _mockServiceRecordDataAccess.Setup(x => x.GetServiceRecordsByVehicleId(vehicleId)).Returns(serviceRecords);
            _mockGasRecordDataAccess.Setup(x => x.GetGasRecordsByVehicleId(vehicleId)).Returns(gasRecords);
            _mockCollisionRecordDataAccess.Setup(x => x.GetCollisionRecordsByVehicleId(vehicleId)).Returns(collisionRecords);
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(taxRecords);
            _mockUpgradeRecordDataAccess.Setup(x => x.GetUpgradeRecordsByVehicleId(vehicleId)).Returns(upgradeRecords);
            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(odometerRecords);

            // Act
            var result = _vehicleLogic.GetVehicleRecords(vehicleId);

            // Assert
            Assert.Equal(serviceRecords, result.ServiceRecords);
            Assert.Equal(gasRecords, result.GasRecords);
            Assert.Equal(collisionRecords, result.CollisionRecords);
            Assert.Equal(taxRecords, result.TaxRecords);
            Assert.Equal(upgradeRecords, result.UpgradeRecords);
            Assert.Equal(odometerRecords, result.OdometerRecords);
        }

        [Fact]
        public void GetVehicleTotalCost_CalculatesCorrectTotal()
        {
            // Arrange
            var vehicleRecords = new VehicleRecords
            {
                ServiceRecords = new List<ServiceRecord> { new ServiceRecord { Cost = 100 }, new ServiceRecord { Cost = 150 } },
                GasRecords = new List<GasRecord> { new GasRecord { Cost = 50 }, new GasRecord { Cost = 75 } },
                CollisionRecords = new List<CollisionRecord> { new CollisionRecord { Cost = 200 } },
                TaxRecords = new List<TaxRecord> { new TaxRecord { Cost = 80 } },
                UpgradeRecords = new List<UpgradeRecord> { new UpgradeRecord { Cost = 300 } }
            };

            // Act
            var result = _vehicleLogic.GetVehicleTotalCost(vehicleRecords);

            // Assert
            Assert.Equal(955, result); // 250 + 125 + 200 + 80 + 300
        }

        [Fact]
        public void GetMaxMileage_WithVehicleRecords_ReturnsCorrectMaxMileage()
        {
            // Arrange
            var vehicleRecords = new VehicleRecords
            {
                ServiceRecords = new List<ServiceRecord> { new ServiceRecord { Mileage = 1000 }, new ServiceRecord { Mileage = 1500 } },
                GasRecords = new List<GasRecord> { new GasRecord { Mileage = 1200 } },
                CollisionRecords = new List<CollisionRecord> { new CollisionRecord { Mileage = 800 } },
                UpgradeRecords = new List<UpgradeRecord> { new UpgradeRecord { Mileage = 2000 } },
                OdometerRecords = new List<OdometerRecord> { new OdometerRecord { Mileage = 1800 } }
            };

            // Act
            var result = _vehicleLogic.GetMaxMileage(vehicleRecords);

            // Assert
            Assert.Equal(2000, result);
        }

        [Fact]
        public void GetMaxMileage_WithVehicleRecords_EmptyRecords_ReturnsZero()
        {
            // Arrange
            var vehicleRecords = new VehicleRecords
            {
                ServiceRecords = new List<ServiceRecord>(),
                GasRecords = new List<GasRecord>(),
                CollisionRecords = new List<CollisionRecord>(),
                UpgradeRecords = new List<UpgradeRecord>(),
                OdometerRecords = new List<OdometerRecord>()
            };

            // Act
            var result = _vehicleLogic.GetMaxMileage(vehicleRecords);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetMinMileage_WithVehicleRecords_ReturnsCorrectMinMileage()
        {
            // Arrange
            var vehicleRecords = new VehicleRecords
            {
                ServiceRecords = new List<ServiceRecord> { new ServiceRecord { Mileage = 1000 }, new ServiceRecord { Mileage = 1500 } },
                GasRecords = new List<GasRecord> { new GasRecord { Mileage = 1200 } },
                CollisionRecords = new List<CollisionRecord> { new CollisionRecord { Mileage = 800 } },
                UpgradeRecords = new List<UpgradeRecord> { new UpgradeRecord { Mileage = 2000 } },
                OdometerRecords = new List<OdometerRecord> { new OdometerRecord { Mileage = 1800 } }
            };

            // Act
            var result = _vehicleLogic.GetMinMileage(vehicleRecords);

            // Assert
            Assert.Equal(800, result);
        }

        [Fact]
        public void GetMinMileage_WithVehicleRecords_IgnoresDefaultMileage()
        {
            // Arrange
            var vehicleRecords = new VehicleRecords
            {
                ServiceRecords = new List<ServiceRecord> { new ServiceRecord { Mileage = 0 }, new ServiceRecord { Mileage = 1500 } },
                GasRecords = new List<GasRecord> { new GasRecord { Mileage = 1200 } },
                CollisionRecords = new List<CollisionRecord> { new CollisionRecord { Mileage = 800 } },
                UpgradeRecords = new List<UpgradeRecord> { new UpgradeRecord { Mileage = 0 } },
                OdometerRecords = new List<OdometerRecord> { new OdometerRecord { Mileage = 1800 } }
            };

            // Act
            var result = _vehicleLogic.GetMinMileage(vehicleRecords);

            // Assert
            Assert.Equal(800, result);
        }

        [Fact]
        public void GetOwnershipDays_WithYearSpecified_CalculatesCorrectDays()
        {
            // Arrange
            var purchaseDate = "2023-01-01";
            var soldDate = "";
            var year = 2023;
            var serviceRecords = new List<ServiceRecord>();
            var collisionRecords = new List<CollisionRecord>();
            var gasRecords = new List<GasRecord>();
            var upgradeRecords = new List<UpgradeRecord>();
            var odometerRecords = new List<OdometerRecord>();
            var taxRecords = new List<TaxRecord>();

            // Act
            var result = _vehicleLogic.GetOwnershipDays(purchaseDate, soldDate, year, serviceRecords, collisionRecords, gasRecords, upgradeRecords, odometerRecords, taxRecords);

            // Assert
            Assert.True(result >= 0);
            Assert.True(result <= 365);
        }

        [Fact]
        public void GetVehicleHasUrgentOrPastDueReminders_WithUrgentReminders_ReturnsTrue()
        {
            // Arrange
            var vehicleId = 1;
            var currentMileage = 1000;
            var reminders = new List<ReminderRecord> { new ReminderRecord { Id = 1 } };
            var reminderViewModels = new List<ReminderRecordViewModel>
            {
                new ReminderRecordViewModel { Urgency = ReminderUrgency.VeryUrgent }
            };

            _mockReminderRecordDataAccess.Setup(x => x.GetReminderRecordsByVehicleId(vehicleId)).Returns(reminders);
            _mockReminderHelper.Setup(x => x.GetReminderRecordViewModels(reminders, currentMileage, It.IsAny<DateTime>())).Returns(reminderViewModels);

            // Act
            var result = _vehicleLogic.GetVehicleHasUrgentOrPastDueReminders(vehicleId, currentMileage);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetVehicleHasUrgentOrPastDueReminders_WithoutUrgentReminders_ReturnsFalse()
        {
            // Arrange
            var vehicleId = 1;
            var currentMileage = 1000;
            var reminders = new List<ReminderRecord> { new ReminderRecord { Id = 1 } };
            var reminderViewModels = new List<ReminderRecordViewModel>
            {
                new ReminderRecordViewModel { Urgency = ReminderUrgency.NotUrgent }
            };

            _mockReminderRecordDataAccess.Setup(x => x.GetReminderRecordsByVehicleId(vehicleId)).Returns(reminders);
            _mockReminderHelper.Setup(x => x.GetReminderRecordViewModels(reminders, currentMileage, It.IsAny<DateTime>())).Returns(reminderViewModels);

            // Act
            var result = _vehicleLogic.GetVehicleHasUrgentOrPastDueReminders(vehicleId, currentMileage);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UpdateRecurringTaxes_VehicleSold_ReturnsFalse()
        {
            // Arrange
            var vehicleId = 1;
            var vehicle = new Vehicle { Id = vehicleId, SoldDate = "2023-12-31" };

            _mockVehicleDataAccess.Setup(x => x.GetVehicleById(vehicleId)).Returns(vehicle);

            // Act
            var result = _vehicleLogic.UpdateRecurringTaxes(vehicleId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UpdateRecurringTaxes_NoOutdatedRecurringTaxes_ReturnsFalse()
        {
            // Arrange
            var vehicleId = 1;
            var vehicle = new Vehicle { Id = vehicleId, SoldDate = "" };
            var taxRecords = new List<TaxRecord>
            {
                new TaxRecord { IsRecurring = false, Date = DateTime.Now.AddDays(-30) },
                new TaxRecord { IsRecurring = true, Date = DateTime.Now.AddDays(-1), RecurringInterval = ReminderMonthInterval.OneMonth }
            };

            _mockVehicleDataAccess.Setup(x => x.GetVehicleById(vehicleId)).Returns(vehicle);
            _mockTaxRecordDataAccess.Setup(x => x.GetTaxRecordsByVehicleId(vehicleId)).Returns(taxRecords);

            // Act
            var result = _vehicleLogic.UpdateRecurringTaxes(vehicleId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RestoreSupplyRecordsByUsage_ValidSupply_UpdatesSupplyRecord()
        {
            // Arrange
            var supplyUsage = new List<SupplyUsageHistory>
            {
                new SupplyUsageHistory { Id = 1, Quantity = 5, Cost = 25 }
            };
            var usageDescription = "Test restoration";
            var existingSupply = new SupplyRecord
            {
                Id = 1,
                Quantity = 10,
                Cost = 50,
                RequisitionHistory = new List<SupplyUsageHistory>()
            };

            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordById(1)).Returns(existingSupply);
            _mockSupplyRecordDataAccess.Setup(x => x.SaveSupplyRecordToVehicle(It.IsAny<SupplyRecord>())).Returns(true);

            // Act
            _vehicleLogic.RestoreSupplyRecordsByUsage(supplyUsage, usageDescription);

            // Assert
            _mockSupplyRecordDataAccess.Verify(x => x.SaveSupplyRecordToVehicle(It.Is<SupplyRecord>(s =>
                s.Quantity == 15 && s.Cost == 75 && s.RequisitionHistory.Count == 1)), Times.Once);
        }

        [Fact]
        public void RestoreSupplyRecordsByUsage_InvalidSupplyId_SkipsRecord()
        {
            // Arrange
            var supplyUsage = new List<SupplyUsageHistory>
            {
                new SupplyUsageHistory { Id = 0, Quantity = 5, Cost = 25 } // Default ID
            };
            var usageDescription = "Test restoration";

            // Act
            _vehicleLogic.RestoreSupplyRecordsByUsage(supplyUsage, usageDescription);

            // Assert
            _mockSupplyRecordDataAccess.Verify(x => x.GetSupplyRecordById(It.IsAny<int>()), Times.Never);
            _mockSupplyRecordDataAccess.Verify(x => x.SaveSupplyRecordToVehicle(It.IsAny<SupplyRecord>()), Times.Never);
        }

        [Fact]
        public void RestoreSupplyRecordsByUsage_SupplyNotFound_DoesNotSaveRecord()
        {
            // Arrange
            var supplyUsage = new List<SupplyUsageHistory>
            {
                new SupplyUsageHistory { Id = 999, Quantity = 5, Cost = 25 }
            };
            var usageDescription = "Test restoration";

            _mockSupplyRecordDataAccess.Setup(x => x.GetSupplyRecordById(999)).Returns((SupplyRecord)null);

            // Act
            _vehicleLogic.RestoreSupplyRecordsByUsage(supplyUsage, usageDescription);

            // Assert
            _mockSupplyRecordDataAccess.Verify(x => x.GetSupplyRecordById(999), Times.Once);
            _mockSupplyRecordDataAccess.Verify(x => x.SaveSupplyRecordToVehicle(It.IsAny<SupplyRecord>()), Times.Never);
        }
    }
}