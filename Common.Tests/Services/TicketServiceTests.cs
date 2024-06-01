using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.EntityFrameworkCore;

namespace Common.Tests.Services
{
    [TestClass]
    public class TicketServiceTests
    {
        private ServiceProvider testServiceProvider = TestServices.BuildServices();
        private IDateTimeService testDateTimeService;
        private Mock<IDepotContext> _mockContext;
        private Mock<ISettingsService> _mockSettings;
        private Mock<ILocalizationService> _mockLocalization;
        private TicketService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockContext = new Mock<IDepotContext>();
            _mockSettings = new Mock<ISettingsService>();
            _mockLocalization = new Mock<ILocalizationService>();
            testDateTimeService = testServiceProvider.GetService<IDateTimeService>()!;
            _service = new TicketService(
                _mockContext.Object,
                _mockSettings.Object,
                _mockLocalization.Object,
                testDateTimeService
            );

            _mockLocalization.Setup(x => x.Get(
                It.Is<string>(i => i == "Ticket_does_not_exist"),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("ticket_no_exist");

            _mockLocalization.Setup(x => x.Get(
                It.Is<string>(i => i == "Ticket_not_valid_today"),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("ticket_invalid_today");

            _mockLocalization.Setup(x => x.Get(
                It.Is<string>(i => i == "Ticket_is_valid"),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("ticket_valid");
        }

        [TestMethod]
        [DataRow(12345, 12345, -1, true, false, "ticket_invalid_today")]
        [DataRow(12345, 12345, -1, false, true, "ticket_valid")]
        [DataRow(12345, 12345, 0, false, true, "ticket_valid")]
        [DataRow(12345, 0, 0, false, false, "ticket_no_exist")]

        public void TestValidateTicketNumber(int id, int idToCheck, int dateTimeModifier, bool expires, bool expected, string expectedMessage)
        {
            // Arrange
            DateTime validOn = DateTime.Now.AddDays(dateTimeModifier);
            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket()
                {
                    Id = id, ValidOn = validOn, Expires = expires
                }
            };

            _mockContext.Setup(x => x.GetDbSet<Ticket>()).ReturnsDbSet(tickets);

            // Act
            var result = _service.ValidateTicketNumber(idToCheck);

            // Assert
            Assert.AreEqual(expected, result.Valid);
            Assert.AreEqual(expectedMessage, result.Message);
        }
    }
}