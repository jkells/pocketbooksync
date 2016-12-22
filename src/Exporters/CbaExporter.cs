using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{
    public sealed class CbaExporter : IDisposable
    {
        private const string BaseUrl = "https://www.my.commbank.com.au";
        private static readonly Uri LoginUrl = new Uri(BaseUrl + "/netbank/Logon/Logon.aspx?Embedded=true");
        
        private readonly Account _account;
        private readonly RemoteWebDriver _webdriver;
        private int _screenshot;

        public CbaExporter(Account account)
        {
            _account = account;
            var options = new PhantomJSOptions();
            options.AddAdditionalCapability("phantomjs.page.settings.userAgent",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");
            _webdriver = new PhantomJSDriver(PhantomJSDriverService.CreateDefaultService(Directory.GetCurrentDirectory()), options);            
        }

        public void Dispose()
        {
            using (_webdriver)
            {
            }
        }

        private void TakeScreenshot()
        {
            _webdriver.GetScreenshot().SaveAsFile($"exporter-{_screenshot++}.png", ImageFormat.Png);
        }

        private async Task SelectAccount()
        {
            var accounts = _webdriver.FindElementsByClassName("main_group_account_row");
            foreach (var webElement in accounts)
            {
                var link = webElement.FindElement(By.CssSelector(".NicknameField a"));
                var accountNumber = webElement.FindElement(By.CssSelector(".AccountNumberField .field"));

                if (link.Text.StartsWith(_account.AccountReference, StringComparison.OrdinalIgnoreCase) ||
                    accountNumber.Text.StartsWith(_account.AccountReference, StringComparison.OrdinalIgnoreCase))
                {
                    link.Click();
                    return;
                }
            }

            throw new ExportException($"Account {_account.AccountReference} not found.");
        }


        public async Task<IEnumerable<Transaction>> ExportRecentAsync()
        {
            //await Login();
            //await SelectAccount();
            //var source = _webdriver.PageSource;
            string source;
            using (var fs = new StreamReader(new FileStream("test.html", FileMode.Open)))
            {
                source = fs.ReadToEnd();
            }

            //using (var fs = new StreamWriter(new FileStream("test.html", FileMode.Create)))
            //{
            //fs.Write(source);
            //}

            
            return SelectTransactionRows(source).SelectMany(ParseTransactionRow);            
        }

        private static IEnumerable<HtmlNode> SelectTransactionRows(string source)
        {
            var document = new HtmlDocument();
            document.LoadHtml(source);
            return document.DocumentNode.SelectNodes("//table[contains(@class, 'cba_transactions_table')]/tbody/tr");
        }

        private static IEnumerable<Transaction> ParseTransactionRow(HtmlNode row)
        {
            var descriptionNode = row.SelectSingleNode(".//span[contains(@class, 'merchant')]//a");
            if (descriptionNode == null)
                yield break;

            Transaction transaction = new Transaction();
            try
            {
                descriptionNode.SelectNodes("./i")?.FirstOrDefault()?.Remove();
                transaction.Description = Regex.Replace(descriptionNode.InnerText, @"[\s-]+", " ", RegexOptions.Multiline);

                if (transaction.Description.StartsWith("PENDING", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Description = transaction.Description.Substring(8);
                    transaction.Pending = true;
                }
                
                var dateNode = row.SelectSingleNode(".//td[contains(@class, 'date')]");
                dateNode.SelectNodes("./em")?.FirstOrDefault()?.Remove();
                var date = dateNode.InnerText.Trim();
                transaction.Date = DateTime.Parse(date);

                var ammountNode = row.SelectSingleNode(".//td[contains(@class, 'amount')]");
                var ammount = ammountNode.InnerText.Replace("$", "");
                transaction.Amount = decimal.Parse(ammount);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing row: {ex}");
                yield break;
            }

            yield return transaction;
        }

        private async Task Login()
        {
            _webdriver.Url = BaseUrl;
            _webdriver.Navigate();
            TakeScreenshot();
            var clientNumberField = _webdriver.FindElementByName("txtMyClientNumber$field");
            var passwordField = _webdriver.FindElementByName("txtMyPassword$field");
            var loginButton = _webdriver.FindElementByName("btnLogon$field");
            clientNumberField.SendKeys(_account.Username);
            passwordField.SendKeys(_account.Password);
            loginButton.Click();
            TakeScreenshot();
        }
    }
}