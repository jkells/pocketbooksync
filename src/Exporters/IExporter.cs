using System.Collections.Generic;
using System.Threading.Tasks;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{
    public interface IExporter
    {
        Task<IEnumerable<Transaction>> ExportRecent(Account account);
    }
}