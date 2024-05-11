using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;
using Moq;
using Moq.EntityFrameworkCore;

namespace Common.Tests.Services
{
    [TestClass]
    public class TourServiceTests
    {
        
        private Mock<IDepotContext> _mockContext;
        private Mock<ISettingsService> _mockSettings;
        private TourService _service;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _mockContext = new Mock<IDepotContext>();
            _mockSettings = new Mock<ISettingsService>();
            _service = new TourService(
                _mockContext.Object, 
                _mockSettings.Object
                );
        }
        
        
        [TestMethod]
        public void TestGetTourForTicket()
        {
            // Arrange
            var tours = new List<Tour>
            {
                new Tour {RegisteredTickets = {1, 2, 3}},
                new Tour {RegisteredTickets = {4, 5, 6}}
            };

            _mockContext.Setup(x => x.GetDbSet<Tour>()).ReturnsDbSet(tours);

            var foundTour = _service.GetTourForTicket(6);
            var containsTicket = foundTour!.RegisteredTickets.Contains(4);
            var containsTicket2 = foundTour!.RegisteredTickets.Contains(2);
            
            Assert.IsTrue(containsTicket);
            Assert.IsFalse(containsTicket2);
        }
        
        [TestMethod]
        [DataRow(0, 2, 0, 2, 0)]    // If recentTours > 0, return N tours in the past
        [DataRow(0, -1, 0, 4, 0)]   // If recentTours == -1, return all recent tours
        [DataRow(0, 0 , 2 , 0 , 2)] // If upcomingTours > 0, return N tours in the future
        [DataRow(2, 0 , 4 , 0 , 3)] // If upcomingTours > 0, return N tours in the future, but mincap is 1
        [DataRow(0, 0, -1, 0, 4)]   // If upcomingTours == -1, return all tours in the future
        [DataRow(0, 2, 2, 2, 2 )]   // If upcomingTours > 0 && recentTours > 0, return both N Tours
        [DataRow(2, 4, 4, 4, 3 )]   // If upcomingTours > 0 && recentTours > 0, return both N Tours
        public void TestGetToursForToday(int minimumCap, int recentTours, 
            int upcomingTours, int expectedRecent, int expectedUpcoming)
        {
            // Arrange
            var tours = new List<Tour>
            {
                new Tour {Start = DateTime.Now.AddHours(-1)},
                new Tour {Start = DateTime.Now.AddHours(-1)},
                new Tour {Start = DateTime.Now.AddHours(-1)},
                new Tour {Start = DateTime.Now.AddMinutes(-1)},
                new Tour {Start = DateTime.Now.AddMinutes(1)},
                new Tour {Start = DateTime.Now.AddHours(1)},
                new Tour {Start = DateTime.Now.AddHours(1)},
                new Tour
                {
                    Start = DateTime.Now.AddHours(1),
                    RegisteredTickets = {1}
                },
            };
            
            _mockContext.Setup(x => x.GetDbSet<Tour>()).ReturnsDbSet(tours);
            _mockSettings.Setup(x => x.GetValueAsInt(It.IsAny<string>())).Returns(2);
            
            // Act
            var foundTours = _service.GetToursForToday(minimumCap, recentTours, upcomingTours);
            var now = DateTime.Now;
            List<Tour> pastTours = [];
            List<Tour> comingTours = [];

            foreach (var tour in foundTours)
            {
                if (tour.Start < now)
                {
                    pastTours.Add(tour);
                }
                else if (tour.Start > now)
                {
                    comingTours.Add(tour);
                }
                else
                {
                    Assert.Fail();
                }
            }
            
            Assert.AreEqual(expectedRecent, pastTours.Count);
            Assert.AreEqual(expectedUpcoming, comingTours.Count);
        }
        
        [TestMethod]
        public void TestGetToursForTimespan()
        {
            // Arrange
            var tours = new List<Tour>
            {
                new Tour {Start = DateTime.Now.AddDays(-3)},
                new Tour {Start = DateTime.Now.AddDays(-1)},
                new Tour {Start = DateTime.Now},
                new Tour {Start = DateTime.Now.AddDays(1)},
                new Tour {Start = DateTime.Now.AddDays(3)},
            };
            
            _mockContext.Setup(x => x.GetDbSet<Tour>()).ReturnsDbSet(tours);
            var now = DateTime.Now;
            
            // Act
            var before = DateTime.Now.AddDays(-2);
            var after = DateTime.Now.AddDays(2);
            var selectedTours = _service.GetToursForTimespan(before , after);

            foreach (var tourEntry in selectedTours)
            {
                var selectedDate = tourEntry.Key;
                Assert.IsTrue((before < selectedDate && selectedDate < after));
            }
            
            Assert.AreEqual(3, selectedTours.Count);
        }
        
    }
}