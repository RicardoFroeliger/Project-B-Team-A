using Common.Clients;
using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests.System;

[TestClass]
public class GuideClientTests
{
    private IDateTimeService DateTime;

    [TestMethod]
    [TestCategory("LocalOnly")]
    public void TestGuideRemovesTicket()
    {
        // Set up the client
        var serviceCollection = TestServices.ConfigureServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        ConsoleClient client = new GuideClient(serviceProvider);
        client.RunsContained();

        // Set up the mock data providers
        var testConsole = (TestConsole)serviceProvider.GetService<IAnsiConsole>()!;
        var testContext = (TestDepotContext)serviceProvider.GetService<IDepotContext>()!;
        testContext.CleanSlateContext();
        DateTime = serviceProvider.GetService<IDateTimeService>()!;

        // Set up Data
        var testGuideId = 1000100;
        var testUserToRemove = 27639481;
        var testUsers = new List<User>()
        {
            new User()
            {
                Id = testGuideId,
                Role = 1
            }
        };
        var testTicketNumbers = new List<int>()
        {
            testUserToRemove,
            39421587,
            46827391,
            57936214,
            78439215
        };

        var testTickets = testTicketNumbers
            .Select(id => new Ticket() { Id = id }).ToList();
        
        var testGroup = new List<Group>()
        {
            new()
            {
                GroupOwnerId = testUserToRemove,
                GroupTickets = testTicketNumbers
            }
        };
        
        var tourList = new List<Tour>()
        {
            new Tour() { GuideId = testGuideId, Start = DateTime.Now.AddMinutes(10) },
            new Tour() { GuideId = testGuideId, Start = DateTime.Now.AddMinutes(11), RegisteredTickets = testTicketNumbers },
            new Tour() { GuideId = testGuideId, Start = DateTime.Now.AddMinutes(12) },
            new Tour() { GuideId = testGuideId, Start = DateTime.Now.AddMinutes(13) },
            new Tour() { GuideId = testGuideId, Start = DateTime.Now.AddMinutes(14) },
            new Tour() { GuideId = testGuideId, Start = DateTime.Now.AddMinutes(15) }
        };
        testContext.SetDbSet(testTickets);
        testContext.SetDbSet(tourList);
        testContext.SetDbSet(testUsers);
        testContext.SetDbSet(testGroup);

        // Uitgangspunt 1           Gids is ingelogd
        // Uitgangspunt 2 	        De gids is de rondleiding nog niet begonnen
        // Uitgangspunt 3	        Het gidsmenu is zichtbaar
        // Uitgangspunt 4          	De bezoeker is niet op de aangewezen plek voor de rondleiding
        
        // Prepare the script for this test case
        // Here you create a "script" where all inputs are stored
        // The test console will dequeue and enter an input every time an input is requested

        // Stap 1	Selecteer de juiste rondleiding
        testConsole.Input.PushTextWithEnter(testGuideId.ToString());
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);

        // Stap 2	Kies "Bezoeker verwijderen"
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        
        // Stap 3	Scan of typ het ticketnummer van de te verwijderen bezoeker
        testConsole.Input.PushTextWithEnter(testUserToRemove.ToString());
        
        // Stap 4	Bevestig dat je de bezoeker(slijst) wil verwijderen
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        
        // Stap 5	Beantwoord of je nog een persoon wil verwijderen
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        
        // Stap 6 Escape the menu
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.Enter);
        
        // Stap 7 Escape the application
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
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

        // This asserts the ticket has been removed from the tour
        var negativeIndex = outputLines.Length - 5;
        Assert.IsTrue(outputLines[negativeIndex].Contains("(4/13)"));

        // Print the output for debugging and visual confirmation
        Console.WriteLine(output);
    }
}