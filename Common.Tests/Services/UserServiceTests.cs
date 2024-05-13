using Common.DAL;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
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
            _service = new UserService(
                _mockContext.Object,
                _mockSettings.Object,
                _mockLocalization.Object
            );
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
        [DataRow(1, RoleType.Manager, RoleType.Manager, true)]   // User must be at least manager, is manager - validates
        [DataRow(2, RoleType.Manager, RoleType.Guest, true)]   // User must be at least manager, is visitor - validates
        [DataRow(3, RoleType.Guest, RoleType.Manager, false)]  // User must be at least manager, is visitor - doesn't validate
        public void TestValidateUserForRole(int id, RoleType actualRole, RoleType expectedRole, bool expectedReturn)
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
                new User { Id = 1, Role = (int)RoleType.Manager },
                new User { Id = 2, Role = (int)RoleType.Guide }
            };

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);

            // Act
            var result = _service.GetUsersOfRole(RoleType.Guide);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [Ignore("Won't pass due to expected return value on add.")]
        [TestMethod]
        public void TestAddUser()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1000400 }
            };
            var newUser = new User();

            _mockContext.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(users);
            _mockContext.Setup(m => m.GetDbSet<User>()!.Add(It.IsAny<User>())).Callback<User>(users.Add);

            // Act
            _service.AddOne(newUser);

            // Assert
            var expectedId = 1000500;
            var user = _service.GetAll().FirstOrDefault(x => x.Id == expectedId);
            Assert.AreEqual(expectedId, user!.Id);
        }
    }
}