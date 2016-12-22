using System;
using System.Linq;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{
    public class ExporterFactory
    {
        public static IExporter Create(Account account)
        {
            switch (account.Type.ToLowerInvariant())
            {
                case "cba":
                    return new CbaExporter(account);
                default:
                    throw new ExportException($"Exporter not found for type: {account.Type}");
            }
        }
    }
}
