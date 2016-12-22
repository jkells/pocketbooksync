using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.PlatformAbstractions;

namespace PocketBookSync.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Config> Config { get; set; }

        public async Task<Config> GetConfigAsync(){
            var config = await Config.FirstOrDefaultAsync();
            if(config == null){
                config = new Config();                                             
            }
            return config;            
        }

        public async Task SetConfigAsync(Config config){
            if(await Config.AnyAsync()){
                Config.Update(config);
            }else{
                Config.Add(config);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var basePath = Environment.GetEnvironmentVariable("APPDATA");
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = PlatformServices.Default.Application.ApplicationBasePath;
            }
            var filename = Path.Combine(basePath, "pocketbooksync.db");            
            optionsBuilder.UseSqlite($"Filename={filename}");            
        }
    }
}