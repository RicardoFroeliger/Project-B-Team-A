using Common.Clients;
using Common.DAL;
using Common.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests;

[TestClass]
public class GuideClientTests
{
    [TestMethod]
    [TestCategory("LocalOnly")]
    public void TestGuideLoggedIn()
    {
        // Set up the client
        var serviceCollection = TestServices.ConfigureServices();
        var serviceProvider = serviceCollection.BuildServiceProvider(); 
        var client = new GuideClient(serviceProvider);
        var testConsole = (TestConsole)serviceProvider.GetService<IAnsiConsole>()!;
        var testContext = (TestDepotContext)serviceProvider.GetService<IDepotContext>()!;

        // Set up Data
        var testGuideId = 1222100; //1000100
        var tourList = new List<Tour>()
        {
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(10)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(11)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(12)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(13)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(14)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(15)}
        };
        testContext.SetDbSet(tourList);

        
        // Uitgangspunt 1	        Gids is ingelogd
        // Uitgangspunt 2 	        De gids is de rondleiding nog niet begonnen
        // Uitgangspunt 3	        Het gidsmenu is zichtbaar
        // Uitgangspunt 4          	De bezoeker is niet op de aangewezen plek voor de rondleiding
        
        // Stap 1	Selecteer de juiste rondleiding
        // Stap 2	Kies "Bezoeker verwijderen"
        // Stap 3	Scan of typ het ticketnummer van de te verwijderen bezoeker
        // Stap 4	Beantwoord of je nog een persoon wil verwijderen
        // Stap 5	Bevestig dat je de bezoeker(slijst) wil verwijderen

        // Resultaat 1	Zie dat je een de bezoeker uit de rondleiding is verwijderd
        // Resultaat 2	In het JSON bestand staat de bezoeker niet meer gereserveerd op een rondleiding.
        // Systeemtest	TestGuideRemovesVisitorSuccesful
        // Populate the Context with data, so we can interact with a controlled environment
        
        
        // // Prepare the script for this test case
        // // Here you create a "script" where all inputs are stored
        // // The test console will dequeue and enter an input every time an input is requested
        // testConsole.Input.PushTextWithEnter(testGuideId.ToString());
        // testConsole.Input.PushKey(ConsoleKey.DownArrow);
        // testConsole.Input.PushKey(ConsoleKey.DownArrow);
        // testConsole.Input.PushKey(ConsoleKey.Enter);
        //
        // // Run the client and let your script be executed
        // client.Run();
        //
        // // Read the output after the application has shut down
        // string output = testConsole.Output.NormalizeLineEndings();
        // string[] outputLines = output.Split('\n');
        //
        // // This asserts the last entry is the user picking to exit the program
        // Assert.IsTrue(outputLines.Last().Contains("> Programma sluiten"));
    }
}