using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Common.Tests.Services
{
    [TestClass]
    public class LocalizationServiceTests
    {
        [TestMethod]
        public void TestGet()
        {
            // Arrange
            var _translations = new List<Translation>() {
                new Translation { Id = 1, Key = "test_key", Locale = "nl-NL", Value = "test_key" },
                new Translation { Id = 2, Key = "test_key", Locale = "en-US", Value = "test_key" },
                new Translation { Id = 3, Key = "test_key", Locale = "de-DE", Value = "test_key" },
                new Translation { Id = 4, Key = "test_key_replacement", Locale = "nl-NL", Value = "test_key {0}" },
                new Translation { Id = 5, Key = "test_key_replacement", Locale = "en-US", Value = "test_key {0}" },
                new Translation { Id = 6, Key = "test_key_replacement", Locale = "de-DE", Value = "test_key {0}" },
                new Translation { Id = 7, Key = "test_key_with_id", Locale = "nl-NL", Value = "test_key {0} | en-US" },
                new Translation { Id = 8, Key = "test_key_with_id", Locale = "en-US", Value = "test_key {0} | de-DE" },
                new Translation { Id = 9, Key = "test_key_with_id", Locale = "de-DE", Value = "test_key {0} | en-US" }
            };

            var replacementStrings = new List<string> { "replacement" };

            var _mockSet = new Mock<DbSet<Translation>>();
            _mockSet.As<IQueryable<Translation>>().Setup(m => m.Provider).Returns(_translations.AsQueryable().Provider);
            _mockSet.As<IQueryable<Translation>>().Setup(m => m.Expression).Returns(_translations.AsQueryable().Expression);
            _mockSet.As<IQueryable<Translation>>().Setup(m =>  m.ElementType).Returns(_translations.AsQueryable().ElementType);
            _mockSet.As<IQueryable<Translation>>().Setup(m => m.GetEnumerator()).Returns(_translations.AsQueryable().GetEnumerator());
            _mockSet.Setup(m => m.Add(It.IsAny<Translation>())).Returns();
                
            var _contextMock = new Mock<IDepotContext>();
            _contextMock.Setup(context => context.Translations).Returns(_mockSet.Object);

            var _localizationService = new LocalizationService(_contextMock.Object);

            _contextMock.Setup(x => x.SaveLocalChanges()).Returns(1);

            // Act
            var resultNL = _localizationService.Get("test_key");
            var resultEN = _localizationService.Get("test_key", "en-US");
            var resultDE = _localizationService.Get("test_key", "de-DE");

            var resultNL_replacement = _localizationService.Get("test_key_replacement", replacementStrings: replacementStrings);
            var resultEN_replacement = _localizationService.Get("test_key_replacement", "en-US", replacementStrings: replacementStrings);
            var resultDE_replacement = _localizationService.Get("test_key_replacement", "de-DE", replacementStrings: replacementStrings);

            var resultNL_replacement_unused = _localizationService.Get("test_key_replacement");
            var resultEN_replacement_unused = _localizationService.Get("test_key_replacement", "en-US");
            var resultDE_replacement_unused = _localizationService.Get("test_key_replacement", "de-DE");

            var resultNL_with_id = _localizationService.Get("test_key_with_id");
            var resultEN_with_id = _localizationService.Get("test_key_with_id", "en-US");
            var resultDE_with_id = _localizationService.Get("test_key_with_id", "de-DE");

            var resultNL_nonexistant = _localizationService.Get("test_key_nonexistant");
            var resultEN_nonexistant = _localizationService.Get("test_key_nonexistant", "en-US");
            var resultDE_nonexistant = _localizationService.Get("test_key_nonexistant", "de-DE");


            // Assert
            Assert.AreEqual("test_key", resultNL);
            Assert.AreEqual("test_key", resultEN);
            Assert.AreEqual("test_key", resultDE);

            Assert.AreEqual("test_key replacement", resultNL_replacement);
            Assert.AreEqual("test_key replacement", resultEN_replacement);
            Assert.AreEqual("test_key replacement", resultDE_replacement);

            Assert.AreEqual("test_key {0}", resultNL_replacement_unused);
            Assert.AreEqual("test_key {0}", resultEN_replacement_unused);
            Assert.AreEqual("test_key {0}", resultDE_replacement_unused);

            Assert.AreEqual("7 | test_key | nl-NL", resultNL_with_id);
            Assert.AreEqual("8 | test_key | en-US", resultEN_with_id);
            Assert.AreEqual("9 | test_key | de-DE", resultDE_with_id);

            Assert.IsTrue(resultEN_nonexistant.EndsWith(" | test_key_nonexistant | nl-NL"));
            Assert.IsTrue(resultEN_nonexistant.EndsWith(" | test_key_nonexistant | en-US"));
            Assert.IsTrue(resultDE_nonexistant.EndsWith(" | test_key_nonexistant | de-DE"));
        }
    }
}