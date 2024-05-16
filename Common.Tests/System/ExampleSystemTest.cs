using Common.Clients;
using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests.System;

[TestClass]
public class ExampleSystemTest
{
    private IDateTimeService DateTime;
    
    [TestMethod]
    [TestCategory("LocalOnly")]
    public void TestCanEnterTicketAndQuit()
    {
        // Set up the client
        var serviceCollection = TestServices.ConfigureServices();
        var serviceProvider = serviceCollection.BuildServiceProvider(); 
        ConsoleClient client = new KioskClient(serviceProvider);
        client.RunsContained();
        
        // Set up the mock data providers
        var testConsole = (TestConsole)serviceProvider.GetService<IAnsiConsole>()!;
        var testContext = (TestDepotContext)serviceProvider.GetService<IDepotContext>()!;
        testContext.Purge();
        testContext.Initialize();
        DateTime = serviceProvider.GetService<IDateTimeService>()!;
        
        // Set up data
        var validTickets = new List<Ticket>()
        {
            new() { Id = 12938476 },
            new() { Id = 28371946 },
            new() { Id = 39421587 },
        };
        
        // Add data to context
        testContext.SetDbSet(validTickets);
        
        // Prepare the script for this test case
        // The testConsole will dequeue and enter an input every time one is requested
        testConsole.Input.PushTextWithEnter("39421587");
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        
        // Use this to debug the state of the console
        // Especially useful for when you get a "No Input Available" error and need to navigate the script.
        try
        {
            client.Run();
        }
        catch (InvalidOperationException ex)
        {
            // Print the console output at the moment of the error
            Console.WriteLine("Error: " + ex.Message);
            Console.WriteLine("Console output at the moment of the error:");
            Console.WriteLine(testConsole.Output.NormalizeLineEndings());
        
            // Re-throw the exception to fail the test
            throw;
        }

        // Read the output after the application has shut down
        string output = testConsole.Output.NormalizeLineEndings();
        string[] outputLines = output.Split('\n');
        
        // This asserts the last entry is the user picking to exit the program
        Assert.IsTrue(outputLines.Last().Contains("> Programma sluiten"));
        
        // Print the output for debugging and visual confirmation
        Console.WriteLine(output);
    }
}

