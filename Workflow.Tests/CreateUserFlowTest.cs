using Depot.Common.Workflow;
using Depot.DAL;
using Depot.DAL.Models;
using Depot.Common;

namespace Workflow.Tests
{
    [TestClass]
    public class CreateUserFlowTest
    {
        private CreateUsersFlow createUsersFlow;

        [TestCleanup]
        public void Cleanup()
        {
            createUsersFlow = null;
        }

        [TestInitialize]
        public void Initialize()
        {
            createUsersFlow = new CreateUsersFlow(new DepotContext());
        }

        [TestMethod]
        public void SetUserAmountTest()
        {
            Initialize();

            // Act
            bool result1 = createUsersFlow.SetUserAmount(null);
            bool result2 = createUsersFlow.SetUserAmount(5);

            // Assert
            Assert.IsFalse(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual(5, createUsersFlow.Amount);

            createUsersFlow.SetUserAmount(0);

            Cleanup();
        }

        [TestMethod]
        public void AddUserTest()
        {
            Initialize();

            // Arrange
            User bezoeker = new User
            {
                Name = "John Doe",
                Role = (int)Role.Bezoeker
            };

            User gids = new User
            {
                Name = "Jane Doe",
                Role = (int)Role.Gids
            };

            User afdelingshoofd = new User
            {
                Name = "Jack Doe",
                Role = (int)Role.Afdelingshoofd
            };

            // Act
            bool result1 = createUsersFlow.AddUser(bezoeker);
            bool result2 = createUsersFlow.AddUser(gids);
            bool result3 = createUsersFlow.AddUser(afdelingshoofd);


            // Assert
            Assert.AreEqual(true, result1);
            Assert.AreEqual(true, result2);
            Assert.AreEqual(true, result3);

            Cleanup();
        }

        [TestMethod]
        public void ValidateTest()
        {
            Initialize();

            // Arrange
            User bezoeker = new User
            {
                Name = "John Doe",
                Role = (int)Role.Bezoeker
            };

            User gids = new User
            {
                Name = "Jane Doe",
                Role = (int)Role.Gids
            };

            User afdelingshoofd = new User
            {
                Name = "Jack Doe",
                Role = (int)Role.Afdelingshoofd
            };

            createUsersFlow.AddUser(bezoeker);
            createUsersFlow.AddUser(gids);
            createUsersFlow.AddUser(afdelingshoofd);

            // Act
            bool valid;
            string response = createUsersFlow.Validate(out valid);

            // Assert
            Assert.IsTrue(valid);
            Assert.AreEqual($"{Localization.Aangemaakt}: {bezoeker.Role}, {bezoeker.Id}, {bezoeker.Name}.\n{Localization.Aangemaakt}: {gids.Role}, {gids.Id}, {gids.Name}.\n{Localization.Aangemaakt}: {afdelingshoofd.Role}, {afdelingshoofd.Id}, {afdelingshoofd.Name}.\n", response);

            Cleanup();
        }   
    }
}
