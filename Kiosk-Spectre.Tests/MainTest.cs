namespace Kiosk_Spectre.Tests
{
    [TestClass]
    public class MainTest : IntegrationTest
    {
        [TestMethod]
        public void TestMain()
        {
            //Arrange
            SetKeyboard("63821974");
            SetKeboard(new ConsoleKeyInfo(ConsoleKey.DownArrow) "");
            InitScreen();

            //Act
            Kiosk_Spectre.Program.Main();

            //Assert
            Assert.IsTrue(ReadScreen().Contains($"\r\n{expected}\r\n"));
        }

        /*
        [DataTestMethod]
        [DataRow("3 + 5", "8")]
        [DataRow("4 + 8", "12")]
        public void TestMain(string expression, string expected)
        {
            //Arrange
            SetKeyboard(expression);
            InitScreen();

            //Act
            Program.Main();

            //Assert
            Assert.IsTrue(ReadScreen().Contains($"\r\n{expected}\r\n"));
        }

        [TestMethod]
        [DataRow("3 + 6", "9")]
        [DataRow("4 * 8", "32")]
        public void TestFromFile(string expression, string expected)
        {
            //Arrange
            WriteTemporaryTextFile("expression.txt", expression);

            //Act
            Program.FromFile();

            //Assert
            string actual = ReadTemporaryTextFile("result.txt");
            Assert.AreEqual(actual, expected);
        }
        */
    }
}