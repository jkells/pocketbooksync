using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;

namespace PocketBookSync.Commands
{
    public class AddCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("add-account", c =>
            {
                c.Description = "Add a bank account";
                var clientNumber = c.Option("--client-number", "Client Number", CommandOptionType.SingleValue);
                var accountReference = c.Option("--account-reference", "Account Reference",
                    CommandOptionType.SingleValue);
                var pocketBookAccountNumber = c.Option("--pocket-book-account", "Pocket Book Account Number",
                    CommandOptionType.SingleValue);
                var type = c.Option("--type", "Type", CommandOptionType.SingleValue);
                c.HelpOption("-? | -h | --help");
                c.OnExecute(() =>
                {
                    if (!clientNumber.HasValue() || !accountReference.HasValue() || !type.HasValue() ||
                        !pocketBookAccountNumber.HasValue())
                    {
                        c.ShowHelp();
                        return 1;
                    }

                    Console.WriteLine("Enter Password: ");
                    var password = ReadPassword();
                    AddAccountAsync(clientNumber.Value(), password, accountReference.Value(),
                        pocketBookAccountNumber.Value(), type.Value()).Wait();
                    return 0;
                });
            });
        }

        private static string ReadPassword()
        {
            var password = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return password;
                }
                else
                {
                    password += key.KeyChar;
                }
            }
        }

        private static async Task AddAccountAsync(string clientNumber, string password, string accountReference,
            string pocketBookAccountNumber, string type)
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                if (db.Accounts.Any(x => x.AccountReference == accountReference))
                {
                    Console.WriteLine("Account already added");
                    return;
                }

                var account = new Account
                {
                    Username = clientNumber,
                    AccountReference = accountReference,
                    PocketBookAccountNumber = pocketBookAccountNumber,
                    Password = password,
                    Type = type
                };
                await db.AddAsync(account);
                await db.SaveChangesAsync();
            }
        }
    }
}