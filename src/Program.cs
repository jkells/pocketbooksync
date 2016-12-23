using Microsoft.Extensions.CommandLineUtils;
using PocketBookSync.Commands;

namespace PocketBookSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(false);
            var addCommand = new AddCommand();
            var deleteCommand = new DeleteAccountCommand();
            var listCommand = new ListAccountsCommand();
            var syncAllCommand = new SyncAllCommand();
            var syncAccountCommand = new SyncAccountCommand();

            addCommand.Build(commandLineApplication);
            deleteCommand.Build(commandLineApplication);
            listCommand.Build(commandLineApplication);
            syncAllCommand.Build(commandLineApplication);
            syncAccountCommand.Build(commandLineApplication);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                commandLineApplication.ShowHelp();
                return 0;
            });
            commandLineApplication.Execute(args);
        }
    }
}