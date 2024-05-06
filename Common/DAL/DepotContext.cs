using Common.DAL.Interfaces;
using Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Common.DAL
{
    public class DepotContext : DbContext, IDepotContext
    {
        private DbSet<User> Users { get; set; }
        private DbSet<Tour> Tours { get; set; }
        private DbSet<Group> Groups { get; set; }
        private DbSet<Ticket> Tickets { get; set; }
        private DbSet<Translation> Translations { get; set; }
        private DbSet<Setting> Settings { get; set; }
        private DbSet<DataSet> DataSets { get; set; }

        private bool _isLoaded { get; set; }

        private const string UsersPath = @"Json\users.json";
        private const string ToursPath = @"Json\tours.json";
        private const string GroupsPath = @"Json\groups.json";
        private const string TicketsPath = @"Json\tickets.json";
        private const string TranslationsPath = @"Json\translations.json";
        private const string SettingsPath = @"Json\settings.json";
        private const string DataSetsPath = @"Json\datasets.json";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "Depot");
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

        public async void LoadContext()
        {
            if (_isLoaded) return;

            LoadJson(Users, UsersPath);
            LoadJson(Tours, ToursPath);
            LoadJson(Groups, GroupsPath);
            LoadJson(Tickets, TicketsPath);
            LoadJson(Translations, TranslationsPath);
            LoadJson(Settings, SettingsPath);
            LoadJson(DataSets, DataSetsPath);

            _isLoaded = true;

            await SaveChangesAsync();
        }

        /// <summary>
        /// Do not run this in production, this is for testing purposes only.
        /// </summary>
        public void Purge()
        {
            if (!_isLoaded) return;

            Users.RemoveRange(Users);
            Tours.RemoveRange(Tours);
            Groups.RemoveRange(Groups);
            Tickets.RemoveRange(Tickets);
            Translations.RemoveRange(Translations);
            Settings.RemoveRange(Settings);
            DataSets.RemoveRange(DataSets);

            _isLoaded = false;
        }

        public override int SaveChanges()
        {
            int changes = base.SaveChanges();

            File.WriteAllText(UsersPath, JsonSerializer.Serialize(Users.ToList()));
            File.WriteAllText(TicketsPath, JsonSerializer.Serialize(Tickets.ToList()));
            File.WriteAllText(ToursPath, JsonSerializer.Serialize(Tours.ToList()));
            File.WriteAllText(GroupsPath, JsonSerializer.Serialize(Groups.ToList()));
            File.WriteAllText(TranslationsPath, JsonSerializer.Serialize(Translations.ToList()));
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Settings.ToList()));
            File.WriteAllText(DataSetsPath, JsonSerializer.Serialize(DataSets.ToList()));

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

        public DbSet<T>? GetDbSet<T>() where T : DbEntity
        {
            if (typeof(T) == typeof(User))
                return Users as DbSet<T>;
            if (typeof(T) == typeof(Tour))
                return Tours as DbSet<T>;
            if (typeof(T) == typeof(Group))
                return Groups as DbSet<T>;
            if (typeof(T) == typeof(Ticket))
                return Tickets as DbSet<T>;
            if (typeof(T) == typeof(Translation))
                return Translations as DbSet<T>;
            if (typeof(T) == typeof(Setting))
                return Settings as DbSet<T>;
            if (typeof(T) == typeof(DataSet))
                return DataSets as DbSet<T>;

            throw new ArgumentException("Invalid type");
        }
    }
}
