using Depot.Common.Workflow;
using Depot.DAL;
using Depot.DAL.Models;

namespace Depot.Common.Tests.Workflow;

[TestClass]
public class CancelReservationFlowTests
{
    private DepotContext depotContext = new DepotContext();
    private CancelReservationFlow? flow;

    [TestMethod]
    public void TestCanInvalidateNullTour()
    {
        // Arrange
        depotContext.Purge();
        depotContext.LoadContext();
        flow = new CancelReservationFlow(depotContext, 10);

        // Act
        var result = flow.Validate(out bool valid);

        // Assert
        Assert.AreEqual(Localization.Aanmelding_niet_gevonden, result);
        Assert.AreEqual(false, valid);
    }

    [TestMethod]
    public void TestCanInvalidateTicketNotInTour()
    {
        // Arrange
        depotContext.Purge();
        depotContext.LoadContext();
        flow = new CancelReservationFlow(depotContext, 11, 99999999999);
        flow.Tour = depotContext.Tours.FirstOrDefault();

        // Act
        var result = flow.Validate(out bool valid);

        // Assert
        Assert.AreEqual(Localization.Ticket_hoort_niet_bij_deze_rondleiding, result);
        Assert.AreEqual(false, valid);
    }
    
    [TestMethod]
    public void TestCanInvalidateDepartedTour()
    {
        // Arrange
        depotContext.Purge();
        depotContext.LoadContext();
        flow = new CancelReservationFlow(depotContext, 12, 122);
        flow.Tour = depotContext.Tours.FirstOrDefault(t => t.Id == 122);
        flow.Tour!.Departed = true;
        flow.Tour.RegisteredTickets.Add(12);

        // Act
        var result = flow.Validate(out bool valid);

        // Assert
        Assert.AreEqual(Localization.Uw_kunt_uw_reservering_niet_meer_aanpassen, result);
        Assert.AreEqual(false, valid);
    }
    
    [TestMethod]
    public void TestCanValidateGroup()
    {
        // Arrange
        depotContext.Purge();
        depotContext.LoadContext();
        flow = new CancelReservationFlow(depotContext, 13, 133);
        flow.Tour = depotContext.Tours.FirstOrDefault(t => t.Id == 133);
        flow.Tour.Departed = false;
        flow.Tour!.RegisteredTickets.Add(13);
        
        // Create group
        var group = new Group()
        {
            GroupOwnerId = 2,
            TicketIds = [13, 2]
        };
        depotContext.Groups.Add(group);
        depotContext.SaveChanges();

        // Act
        var result = flow.Validate(out bool valid);

        // Assert
        Assert.AreEqual(Localization.Uw_kunt_uw_reservering_niet_annuleren, result);
        Assert.AreEqual(false, valid);
    }
    
    [TestMethod]
    public void TestCanValidate()
    {
        // Arrange
        depotContext.Purge();
        depotContext.LoadContext();
        flow = new CancelReservationFlow(depotContext, 14, 144);
        flow.Tour = depotContext.Tours.FirstOrDefault(t => t.Id == 144);
        flow.Tour.Departed = false;
        flow.Tour!.RegisteredTickets.Add(14);

        // Act
        var result = flow.Validate(out bool valid);

        // Assert
        string expectedString = $"{Localization.Uw_rondleiding_is_om} {flow.Tour.Start:HH:mm}.";
        Assert.AreEqual(expectedString, result);
        Assert.AreEqual(true, valid);
    }
}