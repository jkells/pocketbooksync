using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;
using PocketBookSync.Data;
using System.IO;

namespace PocketBookSync.Exporters
{
    public class CBAExporterOld
    {
        private const string BaseUrl = "https://www.my.commbank.com.au";
        private static readonly Uri LoginUrl = new Uri(BaseUrl + "/netbank/Logon/Logon.aspx?Embedded=true");        
        private static readonly string ExportUri = BaseUrl + "/netbank/TransactionHistory/Exports.aspx?RID={0}&SID={1}&ExportType=OFX";
        private bool _isLoggedIn = false;
        private string _rid;
        private string _sid;

        private readonly HttpClient _client;
        private readonly Account _account;
        public CBAExporterOld(Account account)
        {
            _account = account;
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, sdch, br");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-AU,en;q=0.8,en-US;q=0.6");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");
        }


        /*
        private async Task PerformAdvancedSearch(DateTime start, DateTime end)
        {
            TakeScreenshot();
            _webdriver.FindElementById("cba_advanced_search_trigger").Click();
            TakeScreenshot();
            (await WaitForDisplay(_webdriver.FindElementById("ctl00_BodyPlaceHolder_radioSwitchDateRange_field_1_label"))).Click();
            TakeScreenshot();

            var from = _webdriver.FindElementById("ctl00_BodyPlaceHolder_fromCalTxtBox_field");
            var to = _webdriver.FindElementById("ctl00_BodyPlaceHolder_toCalTxtBox_field");

            from.SendKeys(start.ToString("dd/MM/yyyy"));
            TakeScreenshot();
            to.SendKeys(end.ToString("dd/MM/yyyy"));
            TakeScreenshot();

            _webdriver.FindElementById("ctl00_BodyPlaceHolder_lbSearch").Click();
            TakeScreenshot();
            var spinners = _webdriver.FindElementsByClassName(".Spinner80");
            var wait = 0;
            while (spinners.Any(x => x.Displayed))
            {
                await Task.Delay(500);
                if (wait++ == 60)
                    throw new ExportException("Waited too long for transaction search");
            }

            TakeScreenshot();
            _webdriver.FindElementByCssSelector(".txnExport .export>a>i").Click();
            TakeScreenshot();
            (await WaitForDisplay(_webdriver.FindElementByCssSelector("#ctl00_CustomFooterContentPlaceHolder_updatePanelExport1 input.ddl_selected"))).Click();
            TakeScreenshot();
            (await WaitForDisplay(_webdriver.FindElementByCssSelector("#ctl00_CustomFooterContentPlaceHolder_updatePanelExport1 li.option1 a"))).Click();
            TakeScreenshot();
            (await WaitForDisplay(_webdriver.FindElementByCssSelector("#ctl00_CustomFooterContentPlaceHolder_lbExport1"))).Click();
            TakeScreenshot();
        }
        */

        private IEnumerable<KeyValuePair<string, string>> PopulateLoginForm(IEnumerable<KeyValuePair<string, string>> fields)
        {
            return fields.Select(field =>
            {
                if (field.Key.Equals("txtMyClientNumber$field", StringComparison.OrdinalIgnoreCase))
                    return new KeyValuePair<string, string>(field.Key, _account.Username);

                if (field.Key.Equals("txtMyPassword$field", StringComparison.OrdinalIgnoreCase))
                    return new KeyValuePair<string, string>(field.Key, _account.Password);

                if (field.Key.Equals("chkRemember$field", StringComparison.OrdinalIgnoreCase))
                    return new KeyValuePair<string, string>(field.Key, "on");

                if (field.Key.Equals("JS", StringComparison.OrdinalIgnoreCase))
                    return new KeyValuePair<string, string>(field.Key, "E");

                return field;
            });
        }

        private void ExtractRidSid(IEnumerable<KeyValuePair<string, string>> fields)
        {
            _sid = fields.FirstOrDefault(x=>x.Key.Equals("SID", StringComparison.OrdinalIgnoreCase)).Value;
            _rid = fields.FirstOrDefault(x=>x.Key.Equals("RID", StringComparison.OrdinalIgnoreCase)).Value;

            if(string.IsNullOrWhiteSpace(_sid))
                throw new ExportException("SID not set");
            
            if(string.IsNullOrWhiteSpace(_rid))
                throw new ExportException("RID not set");
        }

        public async Task ExportTest()
        {
            if(!_isLoggedIn)
                throw new ExportException("Not logged in");

            var url = string.Format(ExportUri, _rid, _sid);
            using(var stream = await _client.GetStreamAsync(url)){
                using(var fs = new FileStream("export.ofx", FileMode.Create)){
                    await stream.CopyToAsync(fs);
                }
            }
        }

        public async Task Login()
        {
            var response = await _client.GetAsync(LoginUrl);
            if (!response.IsSuccessStatusCode)
                throw new ExportException($"Error loading login page: {response.StatusCode}");

            Console.WriteLine($"CBA Exporter: Retrieved login form: {response.StatusCode}");
            using (var content = await response.Content.ReadAsStreamAsync())
            {                
                var doc = new HtmlDocument();
                doc.Load(content);
                var formInputs = doc.DocumentNode.SelectNodes("//input");
                if (formInputs == null)
                    throw new Exception("Cannot parse the login page. No inputs found");

                var formFields = formInputs.Select(x => new KeyValuePair<string, string>(x.GetAttributeValue("name", ""), x.GetAttributeValue("value", "")));
                ExtractRidSid(formFields);
                var populatedForm = PopulateLoginForm(formFields);
                var loginResponse = await _client.PostAsync(LoginUrl, new FormUrlEncodedContent(populatedForm));                
                
                if (!response.IsSuccessStatusCode)
                    throw new ExportException($"Error logging in: {response.StatusCode}");

                Console.WriteLine($"CBA Exporter: Logged in: {response.StatusCode}");
                _isLoggedIn = true;
            }            
        }
    }
}