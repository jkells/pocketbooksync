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
            await db.Database.ExecuteSqlCommandAsync("CREATE TABLE IF NOT EXISTS Transactions (TransactionId INTEGER PRIMARY KEY, AccountId INTEGER, Date TEXT, Description TEXT, Amount REAL, Pending INTEGER, Processed INTEGER);");
            await db.Database.ExecuteSqlCommandAsync("CREATE INDEX IF NOT EXISTS transactiondate ON Transactions(AccountId, Date); ");
        }
    }
}