using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Common.Workflows.Guide;
using Moq;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class AddTicketTourGuideFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<ITourService> _tourServiceMock;
        private Mock<ISettingsService> _settingsServiceMock;
        private Mock<IGroupService> _groupServiceMock;
        private AddTicketTourGuideFlow _addTicketTourGuideFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _tourServiceMock = new Mock<ITourService>();
            _groupServiceMock = new Mock<IGroupService>();

            _addTicketTourGuideFlow = new AddTicketTourGuideFlow(
                _contextMock.Object,
                _localizationServiceMock.Object,
                _ticketServiceMock.Object,
                _tourServiceMock.Object,
                _settingsServiceMock.Object,
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

            _settingsServiceMock.Setup(x => x.GetValueAsInt("Max_capacity_per_tour"))
                .Returns(13);

            _contextMock.Setup(x => x.SaveChanges()).Returns(1);

            // Assume that Localization.Get always returns a valid message  
            _localizationServiceMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Test message");

            // Act  
            var addTour = _addTicketTourGuideFlow.SetTour(new Tour() { });
            var addTicket = _addTicketTourGuideFlow.AddTicket(testTicketNumber);
            var beforeTicketBuffer = _addTicketTourGuideFlow.TicketBuffer.ToList();
            var commit = _addTicketTourGuideFlow.Commit();
            var afterTicketBuffer = _addTicketTourGuideFlow.TicketBuffer.ToList();

            // Assert  
            // Since we've set up the mocks to return values indicating a valid ticket,  
            // the method should return success  
            Assert.IsTrue(addTour.Success);
            Assert.IsTrue(addTicket.Success);
            Assert.IsTrue(beforeTicketBuffer.Count == 1);
            Assert.IsTrue(commit.Succeeded);
            Assert.IsTrue(afterTicketBuffer.Count == 0);
            Assert.IsTrue(_addTicketTourGuideFlow.Tour!.RegisteredTickets.Count == 1);
        }
    }
}