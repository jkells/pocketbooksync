using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PocketBookSync.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Config> Config { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }

        public async Task<Config> GetConfigAsync()
        {
            var config = await Config.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new Config();
            }
            return config;
        }

        public async Task SetConfigAsync(Config config)
        {
            if (await Config.AnyAsync())
            {
                Config.Update(config);
            }
            else
            {
                Config.Add(config);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var filename = Path.Combine(DataPath.Path, "data.sqlite");
            optionsBuilder.UseSqlite($"Filename={filename}");
        }
    }
}