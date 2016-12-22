using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{

    public interface IExporter
    {
        Task<IEnumerable<Transaction>> ExportRecent(Account account);
    }

    public class ExporterFactory
    {
        public IExporter Create(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "cba":
                    return new CbaExporter();
                default:
                    throw new ExportException($"Exporter not found for type: {type}");
            }
        }
    }
}
