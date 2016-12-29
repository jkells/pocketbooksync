using System;
using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Commands;

namespace PocketBookSync
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(false);
            var addCommand = new AddCommand();
            var deleteCommand = new DeleteAccountCommand();
            var listCommand = new ListAccountsCommand();
            var syncAllCommand = new SyncAllCommand();
            var syncAccountCommand = new SyncAccountCommand();
            var configCommand = new ConfigCommand();

            addCommand.Build(commandLineApplication);
            deleteCommand.Build(commandLineApplication);
            listCommand.Build(commandLineApplication);
            syncAllCommand.Build(commandLineApplication);
            syncAccountCommand.Build(commandLineApplication);
            configCommand.Build(commandLineApplication);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                commandLineApplication.ShowHelp();
                return 0;
            });

            try
            {
                try
                {
                    commandLineApplication.Execute(args);
                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten().InnerException;
                }
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (AppException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}