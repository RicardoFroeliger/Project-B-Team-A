using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Common.Workflows.Guide;
using Moq;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class RemoveTicketTourGuideFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<ITourService> _tourServiceMock;
        private Mock<IGroupService> _groupServiceMock;
        private RemoveTicketTourGuideFlow _removeTicketTourGuideFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _tourServiceMock = new Mock<ITourService>();
            _groupServiceMock = new Mock<IGroupService>();

            _removeTicketTourGuideFlow = new RemoveTicketTourGuideFlow(
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

            // Set up mocks for dependencies  
            _ticketServiceMock.Setup(x => x.GetOne(testTicketNumber))
                .Returns(new Ticket() { Id = testTicketNumber, ValidOn = DateTime.Today });

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(testTicketNumber))
                .Returns((Tour)null);

            _contextMock.Setup(x => x.SaveChanges()).Returns(1);

            _groupServiceMock.Setup(x => x.GetGroupForTicket(testTicketNumber))
                .Returns(new Group() { GroupTickets = { testTicketNumber } });

            // Assume that Localization.Get always returns a valid message  
            _localizationServiceMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Test message");

            // Act
            var setTour = _removeTicketTourGuideFlow.SetTour(new Tour() { RegisteredTickets = { testTicketNumber } });
            var removeTicket = _removeTicketTourGuideFlow.RemoveTicket(testTicketNumber, true);
            var beforeTicketBuffer = _removeTicketTourGuideFlow.TicketBuffer.ToList();
            var commit = _removeTicketTourGuideFlow.Commit();
            var afterTicketBuffer = _removeTicketTourGuideFlow.TicketBuffer.ToList();

            // Assert  
            // Since we've set up the mocks to return values indicating a valid ticket,  
            // the method should return success  
            Assert.IsTrue(setTour.Success);
            Assert.IsTrue(removeTicket.Success);
            Assert.IsTrue(beforeTicketBuffer.Count == 1);
            Assert.IsTrue(commit.Succeeded);
            Assert.IsTrue(afterTicketBuffer.Count == 0);
            Assert.IsTrue(_removeTicketTourGuideFlow.Tour!.RegisteredTickets.Count == 0);
        }
    }
}