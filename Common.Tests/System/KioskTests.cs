using Common.Clients;
using Common.Statics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests.System;

[TestClass]
public class KioskTests
{
    [TestMethod]
    public void TestCanEnterTicketAndQuit()
    {
        // Set up the client
        var serviceCollection = Setup.ConfigureServices();
        var testConsole = new TestConsole().Interactive(); // Must be interactive or won't process inputs
        serviceCollection.Replace(ServiceDescriptor.Singleton<IAnsiConsole>(testConsole));
        var serviceProvider = serviceCollection.BuildServiceProvider(); 
        ConsoleClient client = new KioskClient(serviceProvider);
        
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

