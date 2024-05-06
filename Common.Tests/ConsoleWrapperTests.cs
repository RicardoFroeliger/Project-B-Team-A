using Spectre.Console;
using Spectre.Console.Testing;

namespace Common.Tests
{
    [TestClass]
    public class ConsoleWrapperTests
    {
        [TestMethod]
        public void TestConsoleOutput()
        {
            // Arrange
            ConsoleWrapper.LoadTestConsole();
            var testConsole = (TestConsole)ConsoleWrapper.Console;

            // Act
            testConsole.WriteLine("This is a test");

            // Assert
            Assert.AreEqual("This is a test\n", testConsole.Output);
        }
    }
}