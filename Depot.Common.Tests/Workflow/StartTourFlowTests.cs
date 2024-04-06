using Depot.Common.Workflow;
using Depot.DAL;

namespace Depot.Common.Tests.Workflow;

[TestClass]
public class StartTourFlowTests
{
    private DepotContext depotContext = new DepotContext();
    private StartTourFlow? startTourFlow;

    [TestInitialize]
    public void TestInitialize()
    {
        // Arrange
        depotContext.Purge(); // BIG OOF
        depotContext.LoadContext();
        startTourFlow = new StartTourFlow(depotContext, 1);
    }

    [TestMethod]
    public void TestCanValidate()
    {
        // Act
        startTourFlow!.Validate(out bool valid);

        // Assert
        Assert.IsTrue(valid);
        Assert.IsTrue(startTourFlow.Tour!.Departed);
    }

    [DataRow(null, false, null)] // Ticketnumber is null
    [DataTestMethod]
    public void TestAddNullTicket(long? ticketNumber, bool expectedReturn, string? expectedMsg)
    {
        // Act
        var result = startTourFlow!.AddTicket(ticketNumber, out string? message);

        // Assert
        Assert.AreEqual(expectedReturn, result);
        Assert.AreEqual(expectedMsg, message);
    }

    [DataRow((long)1025836947, null, true, Localization.Ticket_bevestigd)] // Ticket has reservation and will be confirmed
    [DataRow(8105723946, null, true, Localization.Ticket_niet_in_reserveringen)] // Ticket can be added to non-full tour
    [DataRow(9483752601, 8105723943, false, Localization.Rondleiding_Vol)] // Ticket can't be added because full tour
    [DataTestMethod]
    public void TestAddTicketOnNonFullTour(long? ticketNumber, long? extraTicket, bool expectedReturn, string? expectedMsg)
    {
        // Arrange
        var tickets = new List<long>
        {
            1025836947, 1058394270, 2019485736, 2019584270, 2948105723, 2948105724, 
            2948105725, 2948105727, 2948105728, 2948105729, 2948105735, 2948105762
        };
        if (extraTicket != null) tickets.Add((long)extraTicket);

        // Act
        startTourFlow!.Tour!.RegisteredTickets = tickets;
        var result = startTourFlow!.AddTicket(ticketNumber, out string? message);

        // Assert
        Assert.AreEqual(expectedReturn, result);
        Assert.AreEqual(expectedMsg, message);
    }

    [TestMethod]
    public void TestCleanup()
    {
        // Arrange
        var registeredTickets = new List<long> { 1, 2, 3, 4, 5 };
        var confirmedTickets = new List<long> { 2, 4 };

        startTourFlow!.Tour!.RegisteredTickets = registeredTickets;
        startTourFlow!.ConfirmedTickets = confirmedTickets;

        // Act
        startTourFlow!.Cleanup();

        // Assert
        CollectionAssert.AreEquivalent(confirmedTickets, startTourFlow!.Tour!.RegisteredTickets);
    }
}