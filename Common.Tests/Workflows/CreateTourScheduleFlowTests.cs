using System;
using System.Collections.Generic;
using System.Linq;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;
using Common.Workflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            _contextMock.Setup(x => x.SaveLocalChanges()).Returns(1);
            

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




//using Common.DAL;
//using Common.DAL.Interfaces;
//using Common.DAL.Models;
//using Common.Services;
//using Common.Services.Interfaces;
//using Common.Workflows;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;


//namespace Common.Tests.Workflows
//{
//    [TestClass]
//    public class CreateTourScheduleFlowTests
//    {
//        private Mock<IDepotContext> _contextMock;
//        private Mock<ILocalizationService> _localizationServiceMock;
//        private Mock<ITicketService> _ticketServiceMock;
//        private Mock<ISettingsService> _settingsServiceMock;
//        private CreateTourScheduleFlow _createTourScheduleFlow;

//        [TestInitialize]
//        public void TestInitialize()
//        {
//            _contextMock = new Mock<IDepotContext>();
//            _localizationServiceMock = new Mock<ILocalizationService>();
//            _ticketServiceMock = new Mock<ITicketService>();
//            _settingsServiceMock = new Mock<ISettingsService>();

//            _createTourScheduleFlow = new CreateTourScheduleFlow(
//                _contextMock.Object,
//                _localizationServiceMock.Object,
//                _ticketServiceMock.Object,
//                _settingsServiceMock.Object);
//        }

//        [TestMethod]
//        public void HappyFlow()
//        {
//            // Arrange  
//            DateTime startDate = DateTime.Now.Date;
//            DateTime endDate = DateTime.Now.AddDays(1).Date;
//            TimeSpan startTime = new TimeSpan(11, 0, 0);
//            TimeSpan endTime = new TimeSpan(17, 0, 0);
//            int interval = 40;

//            _settingsServiceMock.Setup(x => x.GetValueAsInt("Tour_duration")).Returns(40);


//            // Act  
//            var setDateSpanResult = _createTourScheduleFlow.SetDateSpan(startDate, endDate);
//            var setTimeSpanResult = _createTourScheduleFlow.SetTimeSpan(startTime, endTime);
//            var setIntervalResult = _createTourScheduleFlow.SetInterval(interval);
//            var previewChanges = _createTourScheduleFlow.GetPreviewChanges();
//            var disposePlanning = _createTourScheduleFlow.DisposePlanning(new Dictionary<DateTime, List<Tour>>());
//            var commit = _createTourScheduleFlow.Commit();

//            // Assert  
//            Assert.IsTrue(setDateSpanResult.Success);
//            Assert.IsTrue(setTimeSpanResult.Success);
//            Assert.IsTrue(setIntervalResult.Success);
//            Assert.IsNotNull(previewChanges);
//            Assert.IsTrue(disposePlanning.Succeeded);
//            Assert.IsTrue(commit.Succeeded);
//        }
//    }
//}
