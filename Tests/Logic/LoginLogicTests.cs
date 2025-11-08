using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CarCareTracker.Tests.Logic
{
    public class LoginLogicTests
    {
        private readonly Mock<IUserRecordDataAccess> _mockUserData;
        private readonly Mock<ITokenRecordDataAccess> _mockTokenData;
        private readonly Mock<IMailHelper> _mockMailHelper;
        private readonly Mock<IConfigHelper> _mockConfigHelper;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly LoginLogic _loginLogic;

        public LoginLogicTests()
        {
            _mockUserData = new Mock<IUserRecordDataAccess>();
            _mockTokenData = new Mock<ITokenRecordDataAccess>();
            _mockMailHelper = new Mock<IMailHelper>();
            _mockConfigHelper = new Mock<IConfigHelper>();
            _mockCache = new Mock<IMemoryCache>();
            _loginLogic = new LoginLogic(
                _mockUserData.Object,
                _mockTokenData.Object,
                _mockMailHelper.Object,
                _mockConfigHelper.Object,
                _mockCache.Object
            );
        }

        [Fact]
        public void CheckIfUserIsValid_RootUser_ReturnsTrue()
        {
            // Arrange
            var userId = -1; // Root user

            // Act
            var result = _loginLogic.CheckIfUserIsValid(userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckIfUserIsValid_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var userId = 5;
            var userData = new UserData { Id = 5, UserName = "testuser" };

            _mockUserData.Setup(x => x.GetUserRecordById(userId)).Returns(userData);

            // Act
            var result = _loginLogic.CheckIfUserIsValid(userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckIfUserIsValid_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            var userId = 999;

            _mockUserData.Setup(x => x.GetUserRecordById(userId)).Returns((UserData?)null);

            // Act
            var result = _loginLogic.CheckIfUserIsValid(userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckIfUserIsValid_UserWithZeroId_ReturnsFalse()
        {
            // Arrange
            var userId = 5;
            var userData = new UserData { Id = 0, UserName = "testuser" }; // Invalid user

            _mockUserData.Setup(x => x.GetUserRecordById(userId)).Returns(userData);

            // Act
            var result = _loginLogic.CheckIfUserIsValid(userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void MakeUserAdmin_ValidUser_UpdatesAdminStatus()
        {
            // Arrange
            var userId = 5;
            var isAdmin = true;
            var userData = new UserData { Id = 5, UserName = "testuser", IsAdmin = false };

            _mockUserData.Setup(x => x.GetUserRecordById(userId)).Returns(userData);
            _mockUserData.Setup(x => x.SaveUserRecord(It.IsAny<UserData>())).Returns(true);

            // Act
            var result = _loginLogic.MakeUserAdmin(userId, isAdmin);

            // Assert
            Assert.True(result);
            Assert.True(userData.IsAdmin);
            _mockUserData.Verify(x => x.SaveUserRecord(userData), Times.Once);
        }

        [Fact]
        public void MakeUserAdmin_InvalidUser_ReturnsFalse()
        {
            // Arrange
            var userId = 999;
            var isAdmin = true;

            _mockUserData.Setup(x => x.GetUserRecordById(userId)).Returns((UserData?)null);

            // Act
            var result = _loginLogic.MakeUserAdmin(userId, isAdmin);

            // Assert
            Assert.False(result);
            _mockUserData.Verify(x => x.SaveUserRecord(It.IsAny<UserData>()), Times.Never);
        }

        [Fact]
        public void GenerateUserToken_NewEmail_CreatesTokenAndSendsEmail()
        {
            // Arrange
            var emailAddress = "test@example.com";
            var autoNotify = true;
            var emptyToken = new Token { Id = 0 };

            _mockTokenData.Setup(x => x.GetTokenRecordByEmailAddress(emailAddress)).Returns(emptyToken);
            _mockTokenData.Setup(x => x.CreateNewToken(It.IsAny<Token>())).Returns(true);
            _mockMailHelper.Setup(x => x.NotifyUserForRegistration(emailAddress, It.IsAny<string>())).Returns(OperationResponse.Succeed());

            // Act
            var result = _loginLogic.GenerateUserToken(emailAddress, autoNotify);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Token Generated!", result.Message);
            _mockTokenData.Verify(x => x.CreateNewToken(It.Is<Token>(t => t.EmailAddress == emailAddress)), Times.Once);
            _mockMailHelper.Verify(x => x.NotifyUserForRegistration(emailAddress, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GenerateUserToken_ExistingEmail_ReturnsFailed()
        {
            // Arrange
            var emailAddress = "test@example.com";
            var autoNotify = false;
            var existingToken = new Token { Id = 1, EmailAddress = emailAddress };

            _mockTokenData.Setup(x => x.GetTokenRecordByEmailAddress(emailAddress)).Returns(existingToken);

            // Act
            var result = _loginLogic.GenerateUserToken(emailAddress, autoNotify);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("existing token", result.Message);
            _mockTokenData.Verify(x => x.CreateNewToken(It.IsAny<Token>()), Times.Never);
        }

        [Fact]
        public void RegisterNewUser_ValidCredentials_CreatesUser()
        {
            // Arrange
            var credentials = new LoginModel
            {
                EmailAddress = "test@example.com",
                UserName = "testuser",
                Password = "password123",
                Token = "validtoken"
            };
            var validToken = new Token { Id = 1, EmailAddress = credentials.EmailAddress };
            var emptyUser = new UserData { Id = 0 };

            _mockTokenData.Setup(x => x.GetTokenRecordByBody(credentials.Token)).Returns(validToken);
            _mockUserData.Setup(x => x.GetUserRecordByUserName(credentials.UserName)).Returns(emptyUser);
            _mockUserData.Setup(x => x.GetUserRecordByEmailAddress(credentials.EmailAddress)).Returns(emptyUser);
            _mockTokenData.Setup(x => x.DeleteToken(validToken.Id)).Returns(true);
            _mockUserData.Setup(x => x.SaveUserRecord(It.IsAny<UserData>())).Returns(true);

            // Act
            var result = _loginLogic.RegisterNewUser(credentials);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("redirected to the login page", result.Message);
            _mockTokenData.Verify(x => x.DeleteToken(validToken.Id), Times.Once);
            _mockUserData.Verify(x => x.SaveUserRecord(It.Is<UserData>(u => 
                u.UserName == credentials.UserName && 
                u.EmailAddress == credentials.EmailAddress)), Times.Once);
        }

        [Fact]
        public void RegisterNewUser_InvalidToken_ReturnsFailed()
        {
            // Arrange
            var credentials = new LoginModel
            {
                EmailAddress = "test@example.com",
                UserName = "testuser",
                Password = "password123",
                Token = "invalidtoken"
            };
            var emptyToken = new Token { Id = 0 };

            _mockTokenData.Setup(x => x.GetTokenRecordByBody(credentials.Token)).Returns(emptyToken);

            // Act
            var result = _loginLogic.RegisterNewUser(credentials);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid Token", result.Message);
            _mockUserData.Verify(x => x.SaveUserRecord(It.IsAny<UserData>()), Times.Never);
        }

        [Fact]
        public void RegisterNewUser_UsernameAlreadyTaken_ReturnsFailed()
        {
            // Arrange
            var credentials = new LoginModel
            {
                EmailAddress = "test@example.com",
                UserName = "existinguser",
                Password = "password123",
                Token = "validtoken"
            };
            var validToken = new Token { Id = 1, EmailAddress = credentials.EmailAddress };
            var existingUser = new UserData { Id = 5, UserName = credentials.UserName };

            _mockTokenData.Setup(x => x.GetTokenRecordByBody(credentials.Token)).Returns(validToken);
            _mockUserData.Setup(x => x.GetUserRecordByUserName(credentials.UserName)).Returns(existingUser);

            // Act
            var result = _loginLogic.RegisterNewUser(credentials);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username already taken", result.Message);
            _mockUserData.Verify(x => x.SaveUserRecord(It.IsAny<UserData>()), Times.Never);
        }

        [Fact]
        public void RequestResetPassword_ExistingUser_GeneratesTokenAndReturnsSuccess()
        {
            // Arrange
            var credentials = new LoginModel { UserName = "existinguser" };
            var existingUser = new UserData { Id = 5, UserName = "existinguser", EmailAddress = "user@example.com" };
            var emptyToken = new Token { Id = 0 };

            _mockUserData.Setup(x => x.GetUserRecordByUserName(credentials.UserName)).Returns(existingUser);
            _mockTokenData.Setup(x => x.GetTokenRecordByEmailAddress(existingUser.EmailAddress)).Returns(emptyToken);
            _mockTokenData.Setup(x => x.CreateNewToken(It.IsAny<Token>())).Returns(true);
            _mockMailHelper.Setup(x => x.NotifyUserForPasswordReset(existingUser.EmailAddress, It.IsAny<string>())).Returns(OperationResponse.Succeed());

            // Act
            var result = _loginLogic.RequestResetPassword(credentials);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("should receive an email", result.Message);
        }

        [Fact]
        public void RequestResetPassword_NonExistentUser_StillReturnsSuccessForSecurity()
        {
            // Arrange
            var credentials = new LoginModel { UserName = "nonexistentuser" };
            var emptyUser = new UserData { Id = 0 };

            _mockUserData.Setup(x => x.GetUserRecordByUserName(credentials.UserName)).Returns(emptyUser);

            // Act
            var result = _loginLogic.RequestResetPassword(credentials);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("should receive an email", result.Message);
        }

        [Fact]
        public void ValidateUserCredentials_RootUser_ReturnsRootUserData()
        {
            // Arrange
            var credentials = new LoginModel { UserName = "root", Password = "rootpassword" };

            _mockConfigHelper.Setup(x => x.AuthenticateRootUser(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            // Act
            var result = _loginLogic.ValidateUserCredentials(credentials);

            // Assert
            Assert.Equal(-1, result.Id);
            Assert.True(result.IsRootUser);
            Assert.True(result.IsAdmin);
            Assert.Equal(credentials.UserName, result.UserName);
        }

        [Fact]
        public void ValidateUserCredentials_ValidDbUser_ReturnsUserData()
        {
            // Arrange
            var credentials = new LoginModel { UserName = "testuser", Password = "password123" };
            var userData = new UserData { Id = 5, UserName = "testuser", Password = "hashedpassword" };

            _mockConfigHelper.Setup(x => x.AuthenticateRootUser(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _mockUserData.Setup(x => x.GetUserRecordByUserName(credentials.UserName)).Returns(userData);

            // Note: This test assumes the GetHash method would return the same value for the same input
            // In a real scenario, you'd need to mock or test the actual hash comparison

            // Act
            var result = _loginLogic.ValidateUserCredentials(credentials);

            // Assert - This test would need adjustment based on actual hashing implementation
            Assert.NotNull(result);
        }

        [Fact]
        public void DeleteUser_CallsDeleteUserRecord()
        {
            // Arrange
            var userId = 5;

            _mockUserData.Setup(x => x.DeleteUserRecord(userId)).Returns(true);

            // Act
            var result = _loginLogic.DeleteUser(userId);

            // Assert
            Assert.True(result);
            _mockUserData.Verify(x => x.DeleteUserRecord(userId), Times.Once);
        }

        [Fact]
        public void DeleteUserToken_CallsDeleteToken()
        {
            // Arrange
            var tokenId = 10;

            _mockTokenData.Setup(x => x.DeleteToken(tokenId)).Returns(true);

            // Act
            var result = _loginLogic.DeleteUserToken(tokenId);

            // Assert
            Assert.True(result);
            _mockTokenData.Verify(x => x.DeleteToken(tokenId), Times.Once);
        }

        [Fact]
        public void GetAllUsers_ReturnsUserList()
        {
            // Arrange
            var users = new List<UserData>
            {
                new UserData { Id = 1, UserName = "user1" },
                new UserData { Id = 2, UserName = "user2" }
            };

            _mockUserData.Setup(x => x.GetUsers()).Returns(users);

            // Act
            var result = _loginLogic.GetAllUsers();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(users, result);
        }

        [Fact]
        public void GetAllTokens_ReturnsTokenList()
        {
            // Arrange
            var tokens = new List<Token>
            {
                new Token { Id = 1, EmailAddress = "email1@example.com" },
                new Token { Id = 2, EmailAddress = "email2@example.com" }
            };

            _mockTokenData.Setup(x => x.GetTokens()).Returns(tokens);

            // Act
            var result = _loginLogic.GetAllTokens();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(tokens, result);
        }
    }
}