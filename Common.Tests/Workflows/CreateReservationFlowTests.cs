using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;
using Common.Workflows;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class CreateReservationFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<ITourService> _tourServiceMock;
        private CreateReservationFlow _createReservationFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _tourServiceMock = new Mock<ITourService>();


            _createReservationFlow = new CreateReservationFlow(
                _contextMock.Object,
                _localizationServiceMock.Object,
                _ticketServiceMock.Object,
                _tourServiceMock.Object);
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

            // Set up mocks for dependencies  
            _ticketServiceMock.Setup(x => x.GetTicket(testTicketNumber))
                .Returns(new Ticket() { Id = testTicketNumber });

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(testTicketNumber))
                .Returns((Tour)null);
            
            _contextMock.Setup(x => x.Groups.Add(group));

            // Assume that Localization.Get always returns a valid message  
            _localizationServiceMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Test message");

            // Act  
            var setTicket = _createReservationFlow.SetTicket(new Ticket() { Id = testTicketNumber });
            var addTicket = _createReservationFlow.AddTicket(testTicketNumber);
            _createReservationFlow.SetTour(new Tour());
            var commit = _createReservationFlow.Commit();

            // Assert  
            Assert.IsTrue(setTicket.Success);
            Assert.IsTrue(addTicket.Success);
            Assert.IsNotNull(_createReservationFlow.Tour);
            Assert.IsTrue(commit.Succeeded);
        }
    }
}
