using System.Security.Cryptography.X509Certificates;
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
        }

        [TestMethod] 
        public void HappyFlow() 
        {
            // Arrange

            var name = "testUsername";

            

            // Set up mocks for dependencies
            _contextMock.Setup(x => x.GetDbSet<User>()).ReturnsDbSet(new List<User>());

            _contextMock.Setup(x => x.SaveChanges())
                .Returns(1);



            // Act
            var userNameSet = _createUserFlow.SetUsername(name);
            var roleSet = _createUserFlow.SetRole(Role.Manager);
            var result = _createUserFlow.Commit();

            // Assert
            Assert.IsTrue(userNameSet.Succeeded);
            Assert.IsTrue(roleSet.Succeeded);
            Assert.IsTrue(result.Succeeded);
            
        }    
        
    }
}