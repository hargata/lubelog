using CarCareTracker.External.Interfaces;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Moq;
using Xunit;

namespace CarCareTracker.Tests.Logic
{
    public class UserLogicTests
    {
        private readonly Mock<IUserAccessDataAccess> _mockUserAccess;
        private readonly Mock<IUserRecordDataAccess> _mockUserData;
        private readonly UserLogic _userLogic;

        public UserLogicTests()
        {
            _mockUserAccess = new Mock<IUserAccessDataAccess>();
            _mockUserData = new Mock<IUserRecordDataAccess>();
            _userLogic = new UserLogic(_mockUserAccess.Object, _mockUserData.Object);
        }

        [Fact]
        public void GetCollaboratorsForVehicle_ReturnsCorrectCollaborators()
        {
            // Arrange
            var vehicleId = 1;
            var userAccess = new List<UserAccess>
            {
                new UserAccess { Id = new UserVehicle { UserId = 1, VehicleId = vehicleId } },
                new UserAccess { Id = new UserVehicle { UserId = 2, VehicleId = vehicleId } }
            };
            var userData1 = new UserData { Id = 1, UserName = "user1" };
            var userData2 = new UserData { Id = 2, UserName = "user2" };

            _mockUserAccess.Setup(x => x.GetUserAccessByVehicleId(vehicleId)).Returns(userAccess);
            _mockUserData.Setup(x => x.GetUserRecordById(1)).Returns(userData1);
            _mockUserData.Setup(x => x.GetUserRecordById(2)).Returns(userData2);

            // Act
            var result = _userLogic.GetCollaboratorsForVehicle(vehicleId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("user1", result[0].UserName);
            Assert.Equal("user2", result[1].UserName);
        }

        [Fact]
        public void AddCollaboratorToVehicle_UserExists_ReturnsSuccess()
        {
            // Arrange
            var vehicleId = 1;
            var username = "testuser";
            var existingUser = new UserData { Id = 5, UserName = username };

            _mockUserData.Setup(x => x.GetUserRecordByUserName(username)).Returns(existingUser);
            _mockUserAccess.Setup(x => x.GetUserAccessByVehicleAndUserId(existingUser.Id, vehicleId)).Returns((UserAccess?)null);
            _mockUserAccess.Setup(x => x.SaveUserAccess(It.IsAny<UserAccess>())).Returns(true);

            // Act
            var result = _userLogic.AddCollaboratorToVehicle(vehicleId, username);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Collaborator Added", result.Message);
        }

        [Fact]
        public void AddCollaboratorToVehicle_UserDoesNotExist_ReturnsFailed()
        {
            // Arrange
            var vehicleId = 1;
            var username = "nonexistentuser";
            var emptyUser = new UserData { Id = 0 };

            _mockUserData.Setup(x => x.GetUserRecordByUserName(username)).Returns(emptyUser);

            // Act
            var result = _userLogic.AddCollaboratorToVehicle(vehicleId, username);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Unable to find user {username} in the system", result.Message);
        }

        [Fact]
        public void AddCollaboratorToVehicle_UserAlreadyCollaborator_ReturnsFailed()
        {
            // Arrange
            var vehicleId = 1;
            var username = "testuser";
            var existingUser = new UserData { Id = 5, UserName = username };
            var existingAccess = new UserAccess { Id = new UserVehicle { UserId = 5, VehicleId = vehicleId } };

            _mockUserData.Setup(x => x.GetUserRecordByUserName(username)).Returns(existingUser);
            _mockUserAccess.Setup(x => x.GetUserAccessByVehicleAndUserId(existingUser.Id, vehicleId)).Returns(existingAccess);

            // Act
            var result = _userLogic.AddCollaboratorToVehicle(vehicleId, username);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User is already a collaborator", result.Message);
        }

        [Fact]
        public void AddUserAccessToVehicle_RootUser_ReturnsTrue()
        {
            // Arrange
            var userId = -1; // Root user
            var vehicleId = 1;

            // Act
            var result = _userLogic.AddUserAccessToVehicle(userId, vehicleId);

            // Assert
            Assert.True(result);
            _mockUserAccess.Verify(x => x.SaveUserAccess(It.IsAny<UserAccess>()), Times.Never);
        }

        [Fact]
        public void AddUserAccessToVehicle_RegularUser_CallsSaveUserAccess()
        {
            // Arrange
            var userId = 5;
            var vehicleId = 1;

            _mockUserAccess.Setup(x => x.SaveUserAccess(It.IsAny<UserAccess>())).Returns(true);

            // Act
            var result = _userLogic.AddUserAccessToVehicle(userId, vehicleId);

            // Assert
            Assert.True(result);
            _mockUserAccess.Verify(x => x.SaveUserAccess(It.Is<UserAccess>(ua => 
                ua.Id.UserId == userId && ua.Id.VehicleId == vehicleId)), Times.Once);
        }

        [Fact]
        public void FilterUserVehicles_RootUser_ReturnsAllVehicles()
        {
            // Arrange
            var userId = -1; // Root user
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "Toyota" },
                new Vehicle { Id = 2, Make = "Honda" }
            };

            // Act
            var result = _userLogic.FilterUserVehicles(vehicles, userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(vehicles, result);
        }

        [Fact]
        public void FilterUserVehicles_RegularUserWithAccess_ReturnsAccessibleVehicles()
        {
            // Arrange
            var userId = 5;
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "Toyota" },
                new Vehicle { Id = 2, Make = "Honda" },
                new Vehicle { Id = 3, Make = "Ford" }
            };
            var userAccess = new List<UserAccess>
            {
                new UserAccess { Id = new UserVehicle { UserId = userId, VehicleId = 1 } },
                new UserAccess { Id = new UserVehicle { UserId = userId, VehicleId = 3 } }
            };

            _mockUserAccess.Setup(x => x.GetUserAccessByUserId(userId)).Returns(userAccess);

            // Act
            var result = _userLogic.FilterUserVehicles(vehicles, userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, v => v.Id == 1);
            Assert.Contains(result, v => v.Id == 3);
            Assert.DoesNotContain(result, v => v.Id == 2);
        }

        [Fact]
        public void FilterUserVehicles_RegularUserWithoutAccess_ReturnsEmptyList()
        {
            // Arrange
            var userId = 5;
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "Toyota" },
                new Vehicle { Id = 2, Make = "Honda" }
            };

            _mockUserAccess.Setup(x => x.GetUserAccessByUserId(userId)).Returns(new List<UserAccess>());

            // Act
            var result = _userLogic.FilterUserVehicles(vehicles, userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void UserCanEditVehicle_RootUser_ReturnsTrue()
        {
            // Arrange
            var userId = -1; // Root user
            var vehicleId = 1;

            // Act
            var result = _userLogic.UserCanEditVehicle(userId, vehicleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserCanEditVehicle_UserWithAccess_ReturnsTrue()
        {
            // Arrange
            var userId = 5;
            var vehicleId = 1;
            var userAccess = new UserAccess { Id = new UserVehicle { UserId = userId, VehicleId = vehicleId } };

            _mockUserAccess.Setup(x => x.GetUserAccessByVehicleAndUserId(userId, vehicleId)).Returns(userAccess);

            // Act
            var result = _userLogic.UserCanEditVehicle(userId, vehicleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserCanEditVehicle_UserWithoutAccess_ReturnsFalse()
        {
            // Arrange
            var userId = 5;
            var vehicleId = 1;

            _mockUserAccess.Setup(x => x.GetUserAccessByVehicleAndUserId(userId, vehicleId)).Returns((UserAccess?)null);

            // Act
            var result = _userLogic.UserCanEditVehicle(userId, vehicleId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeleteCollaboratorFromVehicle_CallsDeleteUserAccess()
        {
            // Arrange
            var userId = 5;
            var vehicleId = 1;

            _mockUserAccess.Setup(x => x.DeleteUserAccess(userId, vehicleId)).Returns(true);

            // Act
            var result = _userLogic.DeleteCollaboratorFromVehicle(userId, vehicleId);

            // Assert
            Assert.True(result);
            _mockUserAccess.Verify(x => x.DeleteUserAccess(userId, vehicleId), Times.Once);
        }

        [Fact]
        public void DeleteAllAccessToVehicle_CallsDeleteAllAccessRecordsByVehicleId()
        {
            // Arrange
            var vehicleId = 1;

            _mockUserAccess.Setup(x => x.DeleteAllAccessRecordsByVehicleId(vehicleId)).Returns(true);

            // Act
            var result = _userLogic.DeleteAllAccessToVehicle(vehicleId);

            // Assert
            Assert.True(result);
            _mockUserAccess.Verify(x => x.DeleteAllAccessRecordsByVehicleId(vehicleId), Times.Once);
        }

        [Fact]
        public void DeleteAllAccessToUser_CallsDeleteAllAccessRecordsByUserId()
        {
            // Arrange
            var userId = 5;

            _mockUserAccess.Setup(x => x.DeleteAllAccessRecordsByUserId(userId)).Returns(true);

            // Act
            var result = _userLogic.DeleteAllAccessToUser(userId);

            // Assert
            Assert.True(result);
            _mockUserAccess.Verify(x => x.DeleteAllAccessRecordsByUserId(userId), Times.Once);
        }
    }
}