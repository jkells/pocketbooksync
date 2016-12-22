using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{
    public interface IExporter : IDisposable
    {
        Task<IEnumerable<Transaction>> ExportRecent();
    }
}