using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Services.Interfaces;
using Moq;
using Moq.EntityFrameworkCore;

namespace Common.Tests.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IDepotContext> _mockContext;
        private Mock<ISettingsService> _mockSettings;
        private Mock<ILocalizationService> _mockLocalization;
        private UserService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockContext = new Mock<IDepotContext>();
            _mockSettings = new Mock<ISettingsService>();
            _mockLocalization = new Mock<ILocalizationService>();
            _service = new UserService(_mockContext.Object, _mockSettings.Object, _mockLocalization.Object);
        }

        [TestMethod]
        [DataRow(1, true)]  // User is not null, validates
        [DataRow(2, false)] // User is null, doesn't validate
        public void TestValidateUserpass(int userId, bool expectedOutcome)
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 }
            };

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);

            // Act
            var result = _service.ValidateUserpass(userId);

            // Assert
            Assert.AreEqual(expectedOutcome, result.Valid);
        }

        [TestMethod]
        [DataRow(1, Role.Manager, Role.Manager, true)]   // User must be at least manager, is manager - validates
        [DataRow(2, Role.Manager, Role.Visitor, true)]   // User must be at least manager, is visitor - validates
        [DataRow(3, Role.Visitor, Role.Manager, false)]  // User must be at least manager, is visitor - doesn't validate
        public void TestValidateUserForRole(int id, Role actualRole, Role expectedRole, bool expectedReturn)
        {
            // Arrange
            var user = new User { Id = id, Role = (int)actualRole };
            var users = new List<User>
            {
                user
            };

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);

            // Act
            var result = _service.ValidateUserForRole(user, expectedRole);

            // Assert
            Assert.AreEqual(expectedReturn, result.Valid);
        }

        [TestMethod]
        public void TestGetUser()
        {
            // Nothing in this function can be tested without testing outside of the unit
        }

        [TestMethod]
        public void TestGetAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 }
            };

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);

            // Act
            var result = _service.GetAll();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void TestGetUsersOfRole()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Role = (int)Role.Manager },
                new User { Id = 2, Role = (int)Role.Guide }
            };

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);

            // Act
            var result = _service.GetUsersOfRole(Role.Guide);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void TestAddUser()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1000400 }
            };

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);
            _mockContext.Setup(m => m.GetDbSet<User>()!.Add(It.IsAny<User>())).Callback<User>(users.Add);

            // Act
            var newUser = new User();
            _service.AddOne(newUser);

            // Assert
            var expectedId = 1000500;
            var user = _service.GetAll().FirstOrDefault(x => x.Id == expectedId);
            Assert.AreEqual(expectedId, user!.Id);
        }
    }
}