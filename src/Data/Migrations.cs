using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PocketBookSync.Data
{
    public static class Migrations
    {
        public static async Task MigrateAsync(AppDbContext db)
        {
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlCommandAsync("CREATE TABLE IF NOT EXISTS Config (ID INTEGER PRIMARY KEY, Data TEXT);");            
        }
    }
}