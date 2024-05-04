using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;
using Moq;
using Moq.EntityFrameworkCore;

namespace Common.Tests.Services
{
    [TestClass]
    public class TicketServiceTests
    {
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
            _service = new TicketService(
                _mockContext.Object,
                _mockSettings.Object,
                _mockLocalization.Object
            );
        }

        [TestMethod]
        [DataRow(12345, 12345, -1, true, false)]
        [DataRow(12345, 12345, -1, false, true)]
        [DataRow(12345, 12345, 0, false, true)]
        [DataRow(12345, 0, 0, false, false)]

        public void TestValidateTicketNumber(int id, int idToCheck, int dateTimeModifier, bool expires, bool expected)
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
        }
    }
}