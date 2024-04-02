using Depot.Common.Workflow;
using Depot.DAL;

namespace Depot.Common.Tests.Workflow;

[TestClass]
public class StartTourFlowTests
{
    private DepotContext? depotContext;
    private StartTourFlow? startTourFlow;

    [TestInitialize]
    public void TestInitialize()
    {
        // Arrange
        depotContext = new DepotContext();
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
    [DataRow(8105723946, true, Localization.Ticket_niet_in_reserveringen)] // Ticket can be added to non-full tour
    [DataRow(2948105735, false, Localization.Rondleiding_Vol)]
    [DataTestMethod]
    public void TestAddTicket(long? ticketNumber, bool expectedReturn, string? expectedMsg)
    {
        // Act
        var result = startTourFlow!.AddTicket(ticketNumber, out string? message);
        
        // Assert
        Assert.AreEqual(expectedReturn, result);
        Assert.AreEqual(expectedMsg, message);
    }
}