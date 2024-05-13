using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services.Interfaces;
using Common.Workflows;
using Moq;
using Moq.EntityFrameworkCore;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class CreateUserFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private CreateUserFlow _createUserFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _userServiceMock = new Mock<IUserService>();
            _ticketServiceMock = new Mock<ITicketService>();


            _createUserFlow = new CreateUserFlow(
                _contextMock.Object,
                _localizationServiceMock.Object,
                _ticketServiceMock.Object,
                _userServiceMock.Object);

            // Mocking Localization returns
            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_username_set"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("username_set");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_role_set"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("role_set");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_username_not_set"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("username_not_set");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_role_not_set"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("role_not_set");
            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Commit_successful"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("succes");


        }

        [TestMethod]
        [DataRow(null, true, "username_set")] // Username is empty, should still work
        [DataRow("testUsername", true, "username_set")] // Username is not empty
        public void TestSetUsername(string username, bool setSuccess, string validationMessage)
        {
            // Arrange
            var name = username;
            _contextMock.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(new List<User>());


            // Act
            var userNameSet = _createUserFlow.SetUsername(name);

            // Assert validation and localization message
            Assert.AreEqual(setSuccess, userNameSet.Succeeded);
            Assert.AreEqual(validationMessage, userNameSet.Message);
        }

        [TestMethod]
        [DataRow(1, true, "role_set")] // Role is 1 - guide
        [DataRow(2, true, "role_set")] // Role is 2 - manager
        public void TestSetRole(int roleNum, bool setSuccess, string validationMessage)
        {
            // Arrange
            Role role = (Common.Enums.Role)roleNum;

            _contextMock.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(new List<User>());

            // Act
            var roleSet = _createUserFlow.SetRole(role);

            // Assert validation and localization message
            Assert.AreEqual(setSuccess, roleSet.Succeeded);
            Assert.AreEqual(validationMessage, roleSet.Message);
        }

        [TestMethod]
        public void Commit_WhenUsernameNotSet()
        {
            // Arrange
            _createUserFlow.SetRole(Role.Manager);

            // Act
            var result = _createUserFlow.Commit();

            // Assert
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("username_not_set", result.Message);
        }

        [TestMethod]
        public void Commit_WhenRoleNotSet()
        {
            // Arrange
            _createUserFlow.SetUsername("UsernameHere");

            // Act
            var result = _createUserFlow.Commit();

            // Assert
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("role_not_set", result.Message);
        }

        [TestMethod]
        public void Commit_WhenUsernameAndRoleSet()
        {
            // Arrange
            _createUserFlow.SetUsername("SomeUsername");
            _createUserFlow.SetRole(Role.Guide);

            // Act
            var result = _createUserFlow.Commit();

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("succes", result.Message);
        }
    }

}