using System;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{
    public sealed class ExporterFactory : IDisposable
    {
        private readonly WebDriverFactory _webDriverFactory;

        public ExporterFactory(bool useChrome)
        {
            _webDriverFactory = new WebDriverFactory(useChrome);
        }

        public void Dispose()
        {
            using (_webDriverFactory)
            {
            }
        }

        public IExporter Create(Account account)
        {
            switch (account.Type.ToLowerInvariant())
            {
                case "cba":
                    return new CbaExporter(account, _webDriverFactory);
                default:
                    throw new AppException($"Exporter not found for type: {account.Type}");
            }
        }
    }
}