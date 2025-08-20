using CarCareTracker.External.Interfaces;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CarCareTracker.Tests.Logic
{
    public class OdometerLogicTests
    {
        private readonly Mock<IOdometerRecordDataAccess> _mockOdometerRecordDataAccess;
        private readonly Mock<ILogger<IOdometerLogic>> _mockLogger;
        private readonly OdometerLogic _odometerLogic;

        public OdometerLogicTests()
        {
            _mockOdometerRecordDataAccess = new Mock<IOdometerRecordDataAccess>();
            _mockLogger = new Mock<ILogger<IOdometerLogic>>();
            _odometerLogic = new OdometerLogic(_mockOdometerRecordDataAccess.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetLastOdometerRecordMileage_WithProvidedRecords_ReturnsMaxMileage()
        {
            // Arrange
            var vehicleId = 1;
            var odometerRecords = new List<OdometerRecord>
            {
                new OdometerRecord { Mileage = 1000 },
                new OdometerRecord { Mileage = 1500 },
                new OdometerRecord { Mileage = 1200 }
            };

            // Act
            var result = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, odometerRecords);

            // Assert
            Assert.Equal(1500, result);
        }

        [Fact]
        public void GetLastOdometerRecordMileage_WithEmptyProvidedRecords_FetchesFromDatabase()
        {
            // Arrange
            var vehicleId = 1;
            var emptyRecords = new List<OdometerRecord>();
            var dbRecords = new List<OdometerRecord>
            {
                new OdometerRecord { Mileage = 2000 },
                new OdometerRecord { Mileage = 2500 }
            };

            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(dbRecords);

            // Act
            var result = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, emptyRecords);

            // Assert
            Assert.Equal(2500, result);
            _mockOdometerRecordDataAccess.Verify(x => x.GetOdometerRecordsByVehicleId(vehicleId), Times.Once);
        }

        [Fact]
        public void GetLastOdometerRecordMileage_WithNoRecords_ReturnsZero()
        {
            // Arrange
            var vehicleId = 1;
            var emptyRecords = new List<OdometerRecord>();

            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(vehicleId)).Returns(new List<OdometerRecord>());

            // Act
            var result = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, emptyRecords);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void AutoInsertOdometerRecord_WithDefaultMileage_ReturnsFalse()
        {
            // Arrange
            var odometer = new OdometerRecord { Mileage = 0, VehicleId = 1 };

            // Act
            var result = _odometerLogic.AutoInsertOdometerRecord(odometer);

            // Assert
            Assert.False(result);
            _mockOdometerRecordDataAccess.Verify(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>()), Times.Never);
        }

        [Fact]
        public void AutoInsertOdometerRecord_WithValidMileage_SetsInitialMileageAndSaves()
        {
            // Arrange
            var odometer = new OdometerRecord { Mileage = 1500, VehicleId = 1 };
            var existingRecords = new List<OdometerRecord>
            {
                new OdometerRecord { Mileage = 1200 }
            };

            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(1)).Returns(existingRecords);
            _mockOdometerRecordDataAccess.Setup(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>())).Returns(true);

            // Act
            var result = _odometerLogic.AutoInsertOdometerRecord(odometer);

            // Assert
            Assert.True(result);
            Assert.Equal(1200, odometer.InitialMileage);
            _mockOdometerRecordDataAccess.Verify(x => x.SaveOdometerRecordToVehicle(odometer), Times.Once);
        }

        [Fact]
        public void AutoInsertOdometerRecord_WithNoExistingRecords_SetsInitialMileageToCurrentMileage()
        {
            // Arrange
            var odometer = new OdometerRecord { Mileage = 1500, VehicleId = 1 };

            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(1)).Returns(new List<OdometerRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>())).Returns(true);

            // Act
            var result = _odometerLogic.AutoInsertOdometerRecord(odometer);

            // Assert
            Assert.True(result);
            Assert.Equal(1500, odometer.InitialMileage);
            _mockOdometerRecordDataAccess.Verify(x => x.SaveOdometerRecordToVehicle(odometer), Times.Once);
        }

        [Fact]
        public void AutoInsertOdometerRecord_SaveFails_ReturnsFalse()
        {
            // Arrange
            var odometer = new OdometerRecord { Mileage = 1500, VehicleId = 1 };

            _mockOdometerRecordDataAccess.Setup(x => x.GetOdometerRecordsByVehicleId(1)).Returns(new List<OdometerRecord>());
            _mockOdometerRecordDataAccess.Setup(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>())).Returns(false);

            // Act
            var result = _odometerLogic.AutoInsertOdometerRecord(odometer);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AutoConvertOdometerRecord_OrdersRecordsAndSetsInitialMileage()
        {
            // Arrange
            var odometerRecords = new List<OdometerRecord>
            {
                new OdometerRecord { Id = 3, Date = DateTime.Parse("2023-03-01"), Mileage = 1500 },
                new OdometerRecord { Id = 1, Date = DateTime.Parse("2023-01-01"), Mileage = 1000 },
                new OdometerRecord { Id = 2, Date = DateTime.Parse("2023-02-01"), Mileage = 1200 }
            };

            _mockOdometerRecordDataAccess.Setup(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>())).Returns(true);

            // Act
            var result = _odometerLogic.AutoConvertOdometerRecord(odometerRecords);

            // Assert
            Assert.Equal(3, result.Count);
            
            // Verify ordering by date and then by mileage
            Assert.Equal(1, result[0].Id); // January record
            Assert.Equal(2, result[1].Id); // February record
            Assert.Equal(3, result[2].Id); // March record

            // Verify initial mileage settings
            Assert.Equal(1000, result[0].InitialMileage); // First record uses its own mileage
            Assert.Equal(1000, result[1].InitialMileage); // Second record uses previous mileage
            Assert.Equal(1200, result[2].InitialMileage); // Third record uses previous mileage

            // Verify all records were saved
            _mockOdometerRecordDataAccess.Verify(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>()), Times.Exactly(3));
        }

        [Fact]
        public void AutoConvertOdometerRecord_SingleRecord_SetsInitialMileageToOwnMileage()
        {
            // Arrange
            var odometerRecords = new List<OdometerRecord>
            {
                new OdometerRecord { Id = 1, Date = DateTime.Parse("2023-01-01"), Mileage = 1000 }
            };

            _mockOdometerRecordDataAccess.Setup(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>())).Returns(true);

            // Act
            var result = _odometerLogic.AutoConvertOdometerRecord(odometerRecords);

            // Assert
            Assert.Single(result);
            Assert.Equal(1000, result[0].InitialMileage);
            _mockOdometerRecordDataAccess.Verify(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>()), Times.Once);
        }

        [Fact]
        public void AutoConvertOdometerRecord_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var odometerRecords = new List<OdometerRecord>();

            // Act
            var result = _odometerLogic.AutoConvertOdometerRecord(odometerRecords);

            // Assert
            Assert.Empty(result);
            _mockOdometerRecordDataAccess.Verify(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>()), Times.Never);
        }

        [Fact]
        public void AutoConvertOdometerRecord_SameDateDifferentMileage_OrdersByMileage()
        {
            // Arrange
            var samDate = DateTime.Parse("2023-01-01");
            var odometerRecords = new List<OdometerRecord>
            {
                new OdometerRecord { Id = 2, Date = samDate, Mileage = 1200 },
                new OdometerRecord { Id = 1, Date = samDate, Mileage = 1000 }
            };

            _mockOdometerRecordDataAccess.Setup(x => x.SaveOdometerRecordToVehicle(It.IsAny<OdometerRecord>())).Returns(true);

            // Act
            var result = _odometerLogic.AutoConvertOdometerRecord(odometerRecords);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id); // Lower mileage comes first
            Assert.Equal(2, result[1].Id); // Higher mileage comes second
            Assert.Equal(1000, result[1].InitialMileage); // Second record uses first record's mileage
        }
    }
}