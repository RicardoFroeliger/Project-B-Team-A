using Common.Clients;
using Common.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests.System;

[TestClass]
public class KioskClientTests
{
    private static List<Tour> createTours()
    {
        return new List<Tour>()
        {
            new Tour()
            {
                GuideId = 1000100, 
                RegisteredTickets = new List<int>() { 81479625, 62839174 },
                Start = DateTime.Now.AddMinutes(1)
            },
            new Tour() { 
                GuideId = 1000100, 
                Start = DateTime.Now.AddMinutes(2) 
            },
            new Tour()
            {
                GuideId = 1000100, 
                Start = DateTime.Now.AddMinutes(3)
            }
        };
    }

    [TestMethod]
    [TestCategory("LocalOnly")]
    public void TestCanEnterTicketAndQuit()
    {
        // Set up the client
        var serviceCollection = Mocks.ConfigureServices();
        var serviceProvider = serviceCollection.BuildServiceProvider(); 
        ConsoleClient client = new KioskClient(serviceProvider);
        var testConsole = (TestConsole)serviceProvider.GetService<IAnsiConsole>()!;
        
        // Prepare the script for this test case
        // Here you create a "script" where all inputs are stored
        // The test console will dequeue and enter an input every time an input is requested
        testConsole.Input.PushTextWithEnter("39457821");
        testConsole.Input.PushKey(ConsoleKey.Enter);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        
        // Run the client and let your script be executed
        client.Run();

        // Read the output after the application has shut down
        string output = testConsole.Output.NormalizeLineEndings();
        string[] outputLines = output.Split('\n');
        
        // This asserts the last entry is the user picking to exit the program
        Assert.IsTrue(outputLines.Last().Contains("> Programma sluiten"));
    }
}

