using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Common.Tests.Services
{
    [TestClass]
    public class LocalizationServiceTests
    {
        private ServiceProvider testServiceProvider = TestServices.BuildServices();
        private IDateTimeService testDateTimeService;
        
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
                new Translation { Id = 7, Key = "test_key_with_id", Locale = "nl-NL", Value = "test_key | nl-NL" },
                new Translation { Id = 8, Key = "test_key_with_id", Locale = "en-US", Value = "test_key | en-US" },
                new Translation { Id = 9, Key = "test_key_with_id", Locale = "de-DE", Value = "test_key | de-DE" }
            };

            var replacementStrings = new List<string> { "replacement" };

            var _contextMock = new Mock<IDepotContext>();
            _contextMock.Setup(context => context.GetDbSet<Translation>()).Returns(GetQueryableMockDbSet(_translations));
            _contextMock.Setup(x => x.SaveChanges()).Returns(1);

            testDateTimeService = testServiceProvider.GetService<IDateTimeService>()!;
            var _localizationService = new LocalizationService(_contextMock.Object, testDateTimeService);

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

            Assert.AreEqual("Id: 7 | test_key | nl-NL", resultNL_with_id);
            Assert.AreEqual("Id: 8 | test_key | en-US", resultEN_with_id);
            Assert.AreEqual("Id: 9 | test_key | de-DE", resultDE_with_id);
        }

        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(sourceList.Add);

            return dbSet.Object;
        }
    }
}