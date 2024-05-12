using Common.Clients;
using Common.DAL.Models;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests.System;

[TestClass]
public class GuideClientTests
{
    [TestMethod]
    [TestCategory("LocalOnly")]
    public void TestGuideLoggedIn()
    {
        // Set up the client
        var serviceCollection = Mocks.ConfigureServices();
        var serviceProvider = serviceCollection.BuildServiceProvider(); 
        var client = new GuideClient(serviceProvider);
        var testConsole = (TestConsole)serviceProvider.GetService<IAnsiConsole>()!;
        
        // Populate the Context with data, so we can interact with a controlled environment
        var testGuideId = 1000100;
        var tourService = serviceProvider.GetService<ITourService>()!;
        var tourList = new List<Tour>()
        {
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(10)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(11)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(12)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(13)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(14)},
            new Tour(){GuideId = testGuideId, Start = DateTime.Now.AddMinutes(15)}
        };
        tourService.AddRange(tourList);
        tourService.SaveChanges();
        
        // Prepare the script for this test case
        // Here you create a "script" where all inputs are stored
        // The test console will dequeue and enter an input every time an input is requested
        testConsole.Input.PushTextWithEnter(testGuideId.ToString());
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
        testConsole.Input.PushKey(ConsoleKey.DownArrow);
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