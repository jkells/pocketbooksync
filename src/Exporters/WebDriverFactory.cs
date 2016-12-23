using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;

namespace PocketBookSync.Exporters
{
    public sealed class WebDriverFactory : IDisposable
    {
        private readonly Dictionary<string, RemoteWebDriver> _drivers = new Dictionary<string, RemoteWebDriver>();
        private readonly bool _useChrome;

        public WebDriverFactory(bool useChrome)
        {
            _useChrome = useChrome;
        }

        public void Dispose()
        {
            foreach (var driver in _drivers.Values)
            {
                using (driver)
                {
                }
            }
            _drivers.Clear();
        }

        public RemoteWebDriver CreateFor(string key)
        {
            if (_drivers.ContainsKey(key))
                return _drivers[key];

            RemoteWebDriver driver;
            if (_useChrome)
            {
                driver = new ChromeDriver();
            }
            else
            {
                var options = new PhantomJSOptions();
                options.AddAdditionalCapability("phantomjs.page.settings.userAgent",
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");
                driver =
                    new PhantomJSDriver(PhantomJSDriverService.CreateDefaultService(Directory.GetCurrentDirectory()),
                        options);
            }

            _drivers[key] = driver;
            return _drivers[key];
        }
    }
}