using System.Runtime.CompilerServices;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services.Interfaces;
using Common.Workflows;

using Moq;
using Spectre.Console.Testing;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class StartTourGuideFlowTests
    {
        private Mock<IDepotContext> _mockContext;
        private Mock<ILocalizationService> _mockLocalizationService;
        private Mock<ITicketService> _mockTicketService;
        private Mock<ITourService> _mockTourService;
        private Mock<ISettingsService> _mockSettingsService;
        private Mock<IUserService> _mockUserService;
        private StartTourGuideFlow _startTourGuideFlow;
        private TestConsole _console;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockContext = new Mock<IDepotContext>();
            _mockLocalizationService = new Mock<ILocalizationService>();
            _mockTicketService = new Mock<ITicketService>();
            _mockTourService = new Mock<ITourService>();
            _mockSettingsService = new Mock<ISettingsService>();
            _mockUserService = new Mock<IUserService>();

            ConsoleWrapper.LoadTestConsole();
            _console = (TestConsole)ConsoleWrapper.Console;

            _startTourGuideFlow = new StartTourGuideFlow(
                _mockContext.Object,
                _mockLocalizationService.Object,
                _mockTicketService.Object,
                _mockTourService.Object,
                _mockSettingsService.Object,
                _mockUserService.Object);

            // Mocking Localization returns
            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_ticket_not_in_tour"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("ticket_not_in_tour");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_ticket_already_added_to_list"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("ticket_already_added");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_tour_no_space_for_tickets_in_tour"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("no_space_in_tour");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_ticket_added_to_list"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("great_success");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_next_step"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("next_step");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_no_tickets_scanned"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("no_ticket_scanned");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_ticket_invalid"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("ticket_invalid");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_no_tickets_scanned"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("no_ticket_scanned");

            _mockLocalizationService.Setup(x => x.Get(
                It.Is<string>(i => i == "Commit_successful"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("great_commit_success");


            // Mock Non valid ticket numbers
            _mockTicketService.Setup(service => service.ValidateTicketNumber(
                It.Is<int>(i => i == 110))).Returns((Valid: false, Message: ""));


            // Mock Valid ticket numbers in service
            var validTicketNumbers = new List<int> { 111, 112, 113, 114 };

            _mockTicketService.Setup(service => service.ValidateTicketNumber(
                It.Is<int>(i => validTicketNumbers.Contains(i)))).Returns((Valid: true, Message: ""));

            // Mock Valid ticket lookup in context
            _mockTicketService.Setup(x => x.GetOne(
                    It.Is<int>(i => validTicketNumbers.Contains(i))))
                .Returns((int i) => new Ticket() { Id = i });
        }

        [TestMethod]
        [DataRow(110, false, false, "ticket_invalid", 999)]      // Didn't validate ticket
        [DataRow(111, false, false, "ticket_not_in_tour", 999)]  // Ticket not in tour && no extra room
        [DataRow(112, true, false, "ticket_already_added", 999)] // Didn't add ticket because it's already added
        [DataRow(113, true, false, "no_space_in_tour", 0)]       // Can't add ticket because tour capicity is reached
        [DataRow(114, true, true, "great_success", 999)]         // Happy flow, no validating if statements are triggered
        public void TestAddScannedTicket(int ticketNumber, bool extra, bool addSucces, string validationMessage,
            int maxAmount)
        {
            // Arrange
            // Setup registered tickets
            _startTourGuideFlow.SetTour(new Tour()
            {
                RegisteredTickets = new List<int>() { 112 }
            });

            // Mock max amount of people in tour
            _mockSettingsService.Setup(x => x.GetValueAsInt(It.IsAny<string>())).Returns(maxAmount);

            // Setup TicketBuffer
            _startTourGuideFlow.TicketBuffer.Add(112, extra);

            // Act
            var result = _startTourGuideFlow.AddScannedTicket(ticketNumber, extra);

            // Assert validation and localization message
            Assert.AreEqual(addSucces, result.Success);
            Assert.AreEqual(validationMessage, result.Message);
        }

        [TestMethod]
        [DataRow(1, 1, true)]   // Validates
        [DataRow(2, 0, false)]  // Doesn't Validate
        public void TestScanBadge_ValidUser_ScansBadge(int userId, int guideId, bool expectedResult)
        {
            // Arrange
            _mockUserService.Setup(x => x.ValidateUserpass(It.Is<int>(i => i == 1))).Returns((true, ""));
            _mockUserService.Setup(x => x.ValidateUserpass(It.Is<int>(i => i == 2))).Returns((false, ""));

            // Act
            var result = _startTourGuideFlow.ScanBadge(userId);

            // Assert
            Assert.AreEqual(expectedResult, result.Success);
            Assert.AreEqual(guideId, _startTourGuideFlow.GuideId);
        }

        [TestMethod]
        [DataRow(FlowStep.ScanRegistration, FlowStep.ScanExtra)]
        [DataRow(FlowStep.ScanExtra, FlowStep.Finalize)]
        public void TestProgressStep_ProgressesStep(FlowStep currentStep, FlowStep expectedStep)
        {
            // Arrange
            _startTourGuideFlow.Step = currentStep;

            // Act
            _startTourGuideFlow.ProgressStep();

            // Assert
            Assert.AreEqual(_startTourGuideFlow.Step, expectedStep);
        }

        [TestMethod]
        [DataRow(null, false, false, "no_ticket_scanned")]  // No TicketBuffer, does not succeed
        [DataRow(1, true, true, "great_commit_success")]    // TicketBuffer contains a ticket
        public void TestCommit(int? ticketNumber, bool extra, bool expectedResult, string expectedMsg)
        {
            // Arrange
            if (ticketNumber.HasValue)
            {
                _startTourGuideFlow.TicketBuffer.Add(ticketNumber.Value, extra);
            }

            // Mock internal Tour
            _startTourGuideFlow.SetTour(new Tour());

            _mockContext.Setup(x => x.SaveChanges());

            // Act
            var result = _startTourGuideFlow.Commit();

            // Assert
            Assert.AreEqual(expectedResult, result.Succeeded);
            Assert.AreEqual(expectedMsg, result.Message);
        }
    }
}