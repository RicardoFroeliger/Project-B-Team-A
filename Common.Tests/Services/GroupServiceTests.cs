using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Common.Tests.Service
{
    [TestClass]
    public class GroupServiceTests
    {
        private Mock<IDepotContext> _contextMock;
        private Mock<ISettingsService> _settingsServiceMock;
        private GroupService _groupService;

        [TestInitialize]
        public void TestInitialize()
        {
            _contextMock = new Mock<IDepotContext>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _groupService = new GroupService(_contextMock.Object, _settingsServiceMock.Object);
        }

        public Group CreateTestGroup(int ticketOwnerID, List<int>? groupMembers = null)
        {
            var group = new Group { GroupOwnerId = ticketOwnerID, GroupTickets = new List<int> { ticketOwnerID } };
            // Not used, but if we want to test for group members later it's nice to have.
            if (groupMembers != null) group.GroupTickets.AddRange(groupMembers);
            var groups = new List<Group> { group };

            // Loading group into context
            // Mocking DbSet is not fun
            var queryableGroups = groups.AsQueryable();
            var mockDbSet = new Mock<DbSet<Group>>();
            mockDbSet.As<IQueryable<Group>>().Setup(m => m.Provider).Returns(queryableGroups.Provider);
            mockDbSet.As<IQueryable<Group>>().Setup(m => m.Expression).Returns(queryableGroups.Expression);
            mockDbSet.As<IQueryable<Group>>().Setup(m => m.ElementType).Returns(queryableGroups.ElementType);
            mockDbSet.As<IQueryable<Group>>().Setup(m => m.GetEnumerator()).Returns(queryableGroups.GetEnumerator());
            _contextMock.Setup(c => c.Groups).Returns(mockDbSet.Object);

            return group;
        }

        [TestMethod]
        public void TestCanGetGroupFromTicket()
        {
            // Arrange
            var testTicketNumber = 13548424;
            var group = CreateTestGroup(testTicketNumber);

            // This is where the magic actually happens
            var returnedGroup = _groupService.GetGroupForTicket(testTicketNumber);

            // Assert
            Assert.AreSame(group, returnedGroup);
        }

        // DeleteGroup and AddGroup can't be tested without unit testing the underlying units.
    }
}