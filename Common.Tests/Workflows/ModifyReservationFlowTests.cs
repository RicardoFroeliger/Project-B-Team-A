using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Moq;
using Moq.EntityFrameworkCore;
using Common.Workflows.Kiosk;

namespace Common.Tests.Workflows
{
    [TestClass]
    public class ModifyReservationFlowTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ILocalizationService> _localizationServiceMock;
        private Mock<ITicketService> _ticketServiceMock;
        private Mock<ITourService> _tourServiceMock;
        private Mock<IGroupService> _groupServiceMock;
        private ModifyReservationFlow _modifyReservationFlow;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _localizationServiceMock = new Mock<ILocalizationService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _tourServiceMock = new Mock<ITourService>();
            _groupServiceMock = new Mock<IGroupService>();

            _modifyReservationFlow = new ModifyReservationFlow(
                _contextMock.Object,
                _localizationServiceMock.Object,
                _ticketServiceMock.Object,
                _tourServiceMock.Object,
                _groupServiceMock.Object);

            // Mocking Localization returns
            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_no_tour"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("no_tour");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_tour_departed"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("tour_departed");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_no_group"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("no_group");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_not_group_owner"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("not_group_owner");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_same_tour"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("same_tour");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_new_tour_set"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("new_tour_set");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Flow_no_new_tour"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("no_new_tour");

            _localizationServiceMock.Setup(x => x.Get(
                It.Is<string>(i => i == "Commit_successful"),
                It.IsAny<string?>(),
                It.IsAny<List<string>?>())).Returns("succes");


            _contextMock.Setup(x => x.GetDbSet<Ticket>()).ReturnsDbSet(new List<Ticket>());
            _contextMock.Setup(x => x.GetDbSet<Tour>()).ReturnsDbSet(new List<Tour>());
            _contextMock.Setup(x => x.GetDbSet<Group>()).ReturnsDbSet(new List<Group>());

        }
        [TestMethod]
        public void TestSetTicket_WhenTourIsNull()
        {
            // Arrange
            int testTicketNumber = 13548424;

            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };

            var ticket = new Ticket() { Id = testTicketNumber };

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            // Act
            var result = _modifyReservationFlow.SetTicket(ticket);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("no_tour", result.Message);
        }

        [TestMethod]
        public void TestSetTicket_WhenTourIsDeparted()
        {
            // Arrange
            int testTicketNumber = 13548424;

            var tour = new Tour { Departed = true };
            var ticket = new Ticket() { Id = testTicketNumber };

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket)).Returns(tour);

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));


            // Act
            var result = _modifyReservationFlow.SetTicket(ticket);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("tour_departed", result.Message);
        }

        [TestMethod]
        public void TestSetTicket_WhenGroupIsNull()
        {
            // Arrange
            int testTicketNumber = 13548424;

            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };
            var tour = new Tour();
            var ticket = new Ticket() { Id = testTicketNumber };

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket)).Returns(tour);


            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(testTicketNumber))
                .Returns(tour);

            _groupServiceMock.Setup(service => service.GetGroupForTicket(ticket)).Returns((Group)null);

            // Act
            var result = _modifyReservationFlow.SetTicket(ticket);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("no_group", result.Message);
        }
        [TestMethod]
        public void TestSetTicket_NotGroupOwner()
        {
            // Arrange
            int testTicketNumber = 13548424;
            int otherTicketNumber = 13548425;

            var group = new Group()
            {
                GroupOwnerId = otherTicketNumber,
                GroupTickets = new List<int>() { otherTicketNumber, testTicketNumber }
            };
            var tour = new Tour();
            var ticket = new Ticket() { Id = testTicketNumber };

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket)).Returns(tour);


            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(testTicketNumber))
                .Returns(tour);

            _groupServiceMock.Setup(service => service.GetGroupForTicket(ticket)).Returns((group));

            // Act
            var result = _modifyReservationFlow.SetTicket(ticket);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("not_group_owner", result.Message);
        }

        //Set Tour tests
        [TestMethod]
        public void TestSetTour_WhenTourIsNull()
        {
            // Arrange
            int testTicketNumber = 13548424;
            var tour = new Tour();
            var ticket = new Ticket() { Id = testTicketNumber };

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket)).Returns(tour);

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));


            // Act
            var result = _modifyReservationFlow.SetTour(tour);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("no_tour", result.Message);
        }

        [TestMethod]
        public void TestSetTour_WhenTourIsDeparted()
        {
            // Arrange
            int testTicketNumber = 13548424;

            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };
            var tour = new Tour { Departed = true };
            var ticket = new Ticket() { Id = testTicketNumber };

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket)).Returns(tour);

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));


            // Act
            _modifyReservationFlow.SetTicket(ticket);
            var result = _modifyReservationFlow.SetTour(tour);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("tour_departed", result.Message);
        }

        [TestMethod]
        public void TestSetTour_TourSame()
        {
            // Arrange
            int testTicketNumber = 13548424;

            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };

            var ticket = new Ticket() { Id = testTicketNumber };
            var tour = new Tour { Id = 166 };


            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));
            _tourServiceMock.Setup(x => x.GetTourForTicket(testTicketNumber)).Returns(tour);


            // Act
            _modifyReservationFlow.SetTicket(ticket);
            var result = _modifyReservationFlow.SetTour(tour);
        }

        [TestMethod]
        public void TestCommit_WhenTourIsNull()
        {
            // Arrange
            int testTicketNumber = 13548424;
            var tour = new Tour();
            var ticket = new Ticket() { Id = testTicketNumber };
            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };


            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _groupServiceMock.Setup(service => service.GetGroupForTicket(ticket)).Returns((group));

            // Act
            _modifyReservationFlow.SetTicket(ticket);

            var result = _modifyReservationFlow.Commit();

            // Assert
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("no_tour", result.Message);
        }

        [TestMethod]
        public void TestCommit_WhenNoGroup()
        {
            // Arrange
            int testTicketNumber = 13548424;
            var tour = new Tour();
            var ticket = new Ticket() { Id = testTicketNumber };

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket)).Returns(tour);

            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));


            // Act
            _modifyReservationFlow.SetTicket(ticket);

            var result = _modifyReservationFlow.Commit();

            // Assert
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("no_group", result.Message);
        }

        [TestMethod]
        public void TestCommit_WhenNoNewTour()
        {
            // Arrange  
            int testTicketNumber = 13548424;

            var group = new Group()
            {
                GroupOwnerId = testTicketNumber,
                GroupTickets = new List<int>() { testTicketNumber }
            };
            var tour = new Tour { Id = 166 };
            var ticket = new Ticket() { Id = testTicketNumber };



            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket))
                .Returns(tour);


            _groupServiceMock.Setup(service => service.GetGroupForTicket(ticket)).Returns(group);



            // Act  
            var setTicket = _modifyReservationFlow.SetTicket(ticket);
            _modifyReservationFlow.SetTicket(ticket);
            var setTour = _modifyReservationFlow.SetTour(tour);
            _modifyReservationFlow.SetTour(tour);
            var commit = _modifyReservationFlow.Commit();

            var result = _modifyReservationFlow.Commit();

            // Assert
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("no_new_tour", result.Message);
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
            var tour = new Tour { Id = 166 };
            var ticket = new Ticket() { Id = testTicketNumber };
            var tour2 = new Tour { Id = 167 };


            _ticketServiceMock.Setup(x => x.ValidateTicketNumber(testTicketNumber))
                .Returns((true, ""));

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket))
                .Returns(tour);

            _tourServiceMock.Setup(x => x.GetTourForTicket(ticket))
                .Returns(tour2);

            _groupServiceMock.Setup(service => service.GetGroupForTicket(ticket)).Returns(group);



            // Act  
            var setTicket = _modifyReservationFlow.SetTicket(ticket);
            _modifyReservationFlow.SetTicket(ticket);
            var setTour = _modifyReservationFlow.SetTour(tour);
            _modifyReservationFlow.SetTour(tour);
            var commit = _modifyReservationFlow.Commit();

            // Assert  
            Assert.IsTrue(setTicket.Success);
            Assert.AreEqual("Flow_new_tour_set", setTour.Message);
            Assert.IsTrue(setTour.Success);
            Assert.IsTrue(commit.Succeeded);
            Assert.AreEqual("succes", commit.Message);

        }
    }
}