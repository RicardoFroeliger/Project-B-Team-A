using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;
using Common.Workflows;
using Moq;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class CancelReservationFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<ITourService> _tourServiceMock;
        private Mock<IGroupService> _groupServiceMock;
        private CancelReservationFlow _cancelReservationFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _tourServiceMock = new Mock<ITourService>();
            _groupServiceMock = new Mock<IGroupService>();

            _cancelReservationFlow = new CancelReservationFlow(
                _contextMock.Object,
                _localizationServiceMock.Object,
                _ticketServiceMock.Object,
                _tourServiceMock.Object,
                _groupServiceMock.Object);
        }


        [TestMethod]
        public void HappyFlow()
        {
            // Arrange  
            int testTicketNumber = 13548424;

            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };
            var tour = new Tour();

            // Set up mocks for dependencies  
            _ticketServiceMock.Setup(x => x.GetTicket(testTicketNumber))
                .Returns(new Ticket() { Id = testTicketNumber });

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(testTicketNumber))
                .Returns(new Tour(){Departed = false});

            _contextMock.Setup(x => x.Groups.Add(group));

            _groupServiceMock.Setup(service => service.GetGroupForTicket(new Ticket())).Returns(group);

            // Assume that Localization.Get always returns a valid message  
            _localizationServiceMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Test message");
            
            

            // Act  
            var setTicket = _cancelReservationFlow.SetTicket(new Ticket() { Id = testTicketNumber });
            var commit = _cancelReservationFlow.Commit();

            // Assert  
            Assert.IsTrue(setTicket.Success);
            Assert.IsTrue(commit.Succeeded);
        }
    }
}