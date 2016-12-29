using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Data;

namespace PocketBookSync.Commands
{
    public class ConfigCommand
    {
        public void Build(CommandLineApplication application)
        {
            application.Command("configure", c =>
            {
                c.Description = "Configure pocket book credentials";
                var username = c.Option("--username", "Pocketbook username", CommandOptionType.SingleValue);

                c.HelpOption("-? | -h | --help");
                c.OnExecute(() =>
                {
                    if (!username.HasValue())
                    {
                        c.ShowHelp();
                        return 1;
                    }

                    Console.WriteLine("Enter Password: ");
                    var password = ReadPassword();
                    ConfigureAsync(username.Value(), password).Wait();
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

        private static async Task ConfigureAsync(string username, string password)
        {
            using (var db = new AppDbContext())
            {
                await Migrations.MigrateAsync(db);
                var config = await db.GetConfigAsync();
                config.PocketBookUsername = username;
                config.PocketBookPassword = password;
                await db.SetConfigAsync(config);
                await db.SaveChangesAsync();
            }
        }
    }
}