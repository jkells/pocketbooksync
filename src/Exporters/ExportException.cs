using System;

namespace PocketBookSync.Exporters
{
    public class ExportException : Exception
    {
        public ExportException(string message) : base(message)
        {
        }
    }
}