
using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Common.Workflows.Manager;
using Moq;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class CreateTourScheduleFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<ITourService> _tourServiceMock;
        private Mock<ISettingsService> _settingsServiceMock;
        private CreateTourScheduleFlow _createTourScheduleFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _tourServiceMock = new Mock<ITourService>();
            _settingsServiceMock = new Mock<ISettingsService>();

            _createTourScheduleFlow = new CreateTourScheduleFlow(
                _contextMock.Object,
                _localizationServiceMock.Object,
                _ticketServiceMock.Object,
                _tourServiceMock.Object,
                _settingsServiceMock.Object);
        }

        [TestMethod]
        public void HappyFlow()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = startDate.AddDays(4);
            TimeSpan startTime = new TimeSpan(11, 0, 0);
            TimeSpan endTime = new TimeSpan(16, 0, 0);
            int interval = 40;


            // Set up mocks
            _settingsServiceMock.Setup(x => x.GetValueAsInt("Tour_duration")).Returns(40);


            // Act
            var setDateSpanResult = _createTourScheduleFlow.SetDateSpan(startDate, endDate);
            var setTimeSpanResult = _createTourScheduleFlow.SetTimeSpan(startTime, endTime);
            var setIntervalResult = _createTourScheduleFlow.SetInterval(interval);
            var previewChanges = _createTourScheduleFlow.GetPreviewChanges();
            var disposePlanning = _createTourScheduleFlow.DisposePlanning(new Dictionary<DateTime, List<Tour>>());


            // Assert
            Assert.IsTrue(setDateSpanResult.Success);
            Assert.IsTrue(setTimeSpanResult.Success);
            Assert.IsTrue(setIntervalResult.Success);
            Assert.IsNotNull(previewChanges);
            Assert.IsTrue(disposePlanning.Succeeded);
            Assert.AreEqual((endDate - startDate).Days + 1, previewChanges.Count);
        }

    }
}