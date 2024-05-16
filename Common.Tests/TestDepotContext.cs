using System.ComponentModel.DataAnnotations;
using Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Common.DAL;

namespace Common.Tests
{
    public class TestDepotContext : DbContext, IDepotContext
    {
        private DbSet<User> Users { get; set; }
        private DbSet<Tour> Tours { get; set; }
        private DbSet<Group> Groups { get; set; }
        private DbSet<Ticket> Tickets { get; set; }
        private DbSet<Translation> Translations { get; set; }
        private DbSet<Setting> Settings { get; set; }
        private DbSet<DataSet> DataSets { get; set; }

        private bool IsInitialized { get; set; }
        
        private const string TranslationsPath = @"Json\translations.json";
        private const string SettingsPath = @"Json\settings.json";
        private const string DataSetsPath = @"Json\datasets.json";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "TestDepot");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(b => b.Id);
            modelBuilder.Entity<Ticket>().HasKey(b => b.Id);
            modelBuilder.Entity<Schedule>().HasKey(b => b.Id);
            modelBuilder.Entity<Tour>().HasKey(b => b.Id);
            modelBuilder.Entity<Group>().HasKey(b => b.Id);
            modelBuilder.Entity<Translation>().HasKey(b => b.Id);
            modelBuilder.Entity<Setting>().HasKey(b => b.Id);
            modelBuilder.Entity<DataSet>().HasKey(b => b.Id);
            modelBuilder.Entity<DataEntry>().HasKey(b => b.Id);
        }

        public async void Initialize()
        {
            if (IsInitialized) return;
            
            LoadJson(Translations, TranslationsPath);
            LoadJson(Settings, SettingsPath);
            LoadJson(DataSets, DataSetsPath);
            
            IsInitialized = true;

            await SaveChangesAsync();
        }

        /// <summary>
        /// Do not run this in production, this is for testing purposes only.
        /// </summary>
        public void Purge()
        {
            if (!IsInitialized) return;

            Users.RemoveRange(Users);
            Tours.RemoveRange(Tours);
            Groups.RemoveRange(Groups);
            Tickets.RemoveRange(Tickets);
            Translations.RemoveRange(Translations);
            Settings.RemoveRange(Settings);
            DataSets.RemoveRange(DataSets);

            IsInitialized = false;
        }

        public override int SaveChanges()
        {
            
            int changes = base.SaveChanges();
            
            return changes;
        }

        private void LoadJson<T>(DbSet<T> dbSet, string jsonFile) where T : DbEntity
        {
            if (File.Exists(jsonFile))
            {
                var objs = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(jsonFile));
                if (objs != null)
                {
                    dbSet.AddRange(objs);
                }
            }
        }

        public DbSet<T>? GetDbSet<T>() where T : DbEntity => typeof(T) switch
        {
            var type when type == typeof(User) => Users as DbSet<T>,
            var type when type == typeof(Tour) => Tours as DbSet<T>,
            var type when type == typeof(Group) => Groups as DbSet<T>,
            var type when type == typeof(Ticket) => Tickets as DbSet<T>,
            var type when type == typeof(Translation) => Translations as DbSet<T>,
            var type when type == typeof(Setting) => Settings as DbSet<T>,
            var type when type == typeof(DataSet) => DataSets as DbSet<T>,
            _ => throw new ArgumentException("Invalid type"),
        };
        
        public void SetDbSet<T>(List<T> data) where T : DbEntity
        {
            GetDbSet<T>()!.AddRange(data);
            var changes = SaveChanges();
        }
        
        public void CleanSlateContext()
        {
            // Remove any persistent data from the in-memory database
            // This way we don't contaminate between tests.
            Users.RemoveRange(Users);
            Tours.RemoveRange(Tours);
            Groups.RemoveRange(Groups);
            Tickets.RemoveRange(Tickets);
        }
    }
}
