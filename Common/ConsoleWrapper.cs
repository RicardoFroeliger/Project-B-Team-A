using Spectre.Console;
using Spectre.Console.Testing;

namespace Common;

public static class ConsoleWrapper
{
    public static IAnsiConsole Console { get; private set; } = AnsiConsole.Console;

    public static void LoadTestConsole()
    {
        Console = new TestConsole();
    }
}