using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Moq;
using Moq.EntityFrameworkCore;


namespace Common.Tests.Services
{
    [TestClass]
    public class SettingsServiceTests
    {
        private Mock<IDepotContext> _mockContext;
        private SettingsService _service;


        [TestInitialize]
        public void TestInitialize()
        {
            _mockContext = new Mock<IDepotContext>();
            _service = new SettingsService(_mockContext.Object);
        }

        [TestMethod]
        public void TestGetValueAsInt()
        {
            // Arrange
            var settings = new List<Setting>
            {
                new Setting { Key = "TestKey", Value = "123" }
            };

            _mockContext.Setup(x => x.GetDbSet<Setting>()).ReturnsDbSet(settings);

            // Act
            var result = _service.GetValueAsInt("TestKey");

            // Assert
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void TestGetValue()
        {
            // Arrange
            var settings = new List<Setting>
            {
                new Setting { Key = "TestKey", Value = "TestValue" }
            };

            _mockContext.Setup(x => x.GetDbSet<Setting>()).ReturnsDbSet(settings);

            // Act
            var result = _service.GetValue("TestKey");

            // Assert
            Assert.AreEqual("TestValue", result);
        }
    }
}