using Depot.DAL;
using Depot.DAL.Models;

namespace Depot.Common.Workflow.Tests
{
    [TestClass]
    public class CreateReservationFlowTest
    {
        [DataRow(null, false)]
        [DataRow(0, false)]
        [DataRow(1, true)]
        [DataRow(2, true)]
        [DataTestMethod]
        public void SetTicketAmountTest(int? ticketAmount, bool expected)
        {
            // Arrange
            DepotContext context = new DepotContext();
            CreateReservationFlow create = new CreateReservationFlow(context, 1);

            // Act
            bool result = create.SetTicketAmount(ticketAmount);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AddTicketTest()
        {
            // Arrange
            DepotContext context = new DepotContext();
            CreateReservationFlow create = new CreateReservationFlow(context, 1);
            CreateReservationFlow createDupe = new CreateReservationFlow(context, 11);

            //Act
            string reservationResult = create.AddTicket(2);
            string reservationDuplicate = create.AddTicket(2);
            string reservationGroupDuplicate = create.AddTicket(1);

            //Assert
            Assert.AreEqual(Localization.Ticket_is_toegevoegd, reservationResult);
            Assert.AreEqual(Localization.Ticket_zit_al_in_uw_groep, reservationDuplicate);
            Assert.AreEqual(Localization.Ticket_zit_al_in_uw_groep, reservationGroupDuplicate);
        }

        [TestMethod]
        public void CopyTicketsFromGroupTest()
        {
            // Arrange
            DepotContext context = new DepotContext();
            CreateReservationFlow create = new CreateReservationFlow(context, 1);
            Group group = new() { TicketIds = new List<long> { 2, 3 } };
            Group? groupNull = null;

            // Act
            bool result = create.CopyTicketsFromGroup(group);
            bool resultNull = create.CopyTicketsFromGroup(groupNull);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(resultNull);
            Assert.AreEqual(group.TicketIds, create.TicketNumbers);
        }

        [TestMethod]
        public void SetTourTest()
        {
            // Arrange
            DepotContext context = new DepotContext();
            CreateReservationFlow create = new CreateReservationFlow(context, 1);
            Tour tour = new() { Id = 1 };

            // Act
            bool result = create.SetTour(tour);
            bool resultNull = create.SetTour(2);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(resultNull);
            Assert.AreEqual(tour, create.Tour);
        }

        [TestMethod]
        public void ValidateTest()
        {
            // Arrange
            DepotContext context = new DepotContext();
            CreateReservationFlow create = new CreateReservationFlow(context, 1);
            Tour tour = new() { Id = 1 };
            Group group = new() { TicketIds = new List<long> { 2, 3 } };

            // Act
            create.SetTour(tour);
            create.CopyTicketsFromGroup(group);
            create.SetTicketAmount(2);
            string result = create.Validate(out bool valid);

            // Assert
            Assert.AreEqual(Localization.Uw_rondleiding_is_gereserveerd, result);
            Assert.IsTrue(valid);
        }
    }
}