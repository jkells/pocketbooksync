using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using PocketBookSync.Data;

namespace PocketBookSync.Exporters
{
    public sealed class CbaExporter : IExporter
    {
        private const string LoginUrl = "https://www.my.commbank.com.au";
        private static int _screenshot;
        private readonly RemoteWebDriver _webdriver;

        public CbaExporter(Account account, WebDriverFactory webDriverFactory)
        {
            _webdriver = webDriverFactory.CreateFor(account.Type + account.Username);
        }

        public Task<IEnumerable<Transaction>> ExportRecentAsync(Account account)
        {
            //return MockAsync(account);

            if (!_webdriver.Url.Contains("commbank.com.au"))
            {
                Login(account);
                Log("Logged in");
            }
            else
            {
                ReturnToHome();
                Log("Navigated home");
            }

            SelectAccount(account);
            Log($"Selected Account {account.AccountReference}");
            var source = _webdriver.PageSource;
            var transactions =
                SelectTransactionRows(source).SelectMany(row => ParseTransactionRow(account, row)).ToList();
            Log($"Read {transactions.Count} transactions");

            return Task.FromResult((IEnumerable<Transaction>) transactions);
        }


        /// <summary>
        ///     For testing
        /// </summary>
        private Task<IEnumerable<Transaction>> MockAsync(Account account)
        {
            var filename = Path.Combine(DataPath.Path, $"{account.AccountReference}.html");
            //if (account.AccountReference.StartsWith("Master"))
            //filename = "test.html";
            string source;
            if (File.Exists(filename))
            {
                using (var fs = new StreamReader(new FileStream(filename, FileMode.Open)))
                {
                    source = fs.ReadToEnd();
                }
            }
            else
            {
                Login(account);
                SelectAccount(account);
                source = _webdriver.PageSource;

                using (var fs = new StreamWriter(new FileStream(filename, FileMode.Create)))
                {
                    fs.Write(source);
                }
            }
            return Task.FromResult(SelectTransactionRows(source).SelectMany(row => ParseTransactionRow(account, row)));
        }

        private void ReturnToHome()
        {
            _webdriver.FindElementByCssSelector(".nav-primary .first-child a").Click();
            TakeScreenshot();
        }

        private void TakeScreenshot()
        {
            var filename = Path.Combine(DataPath.Path, $"cba-exporter-{_screenshot++}.png");
            _webdriver.GetScreenshot().SaveAsFile(filename, ImageFormat.Png);
        }

        private void SelectAccount(Account account)
        {
            var accounts = _webdriver.FindElementsByClassName("main_group_account_row");
            foreach (var webElement in accounts)
            {
                var link = webElement.FindElement(By.CssSelector(".NicknameField a"));
                var accountNumber = webElement.FindElement(By.CssSelector(".AccountNumberField .field"));

                if (link.Text.StartsWith(account.AccountReference, StringComparison.OrdinalIgnoreCase) ||
                    accountNumber.Text.StartsWith(account.AccountReference, StringComparison.OrdinalIgnoreCase))
                {
                    link.Click();
                    TakeScreenshot();
                    return;
                }
            }

            throw new AppException($"Account {account.AccountReference} not found.");
        }

        private static IEnumerable<HtmlNode> SelectTransactionRows(string source)
        {
            var document = new HtmlDocument();
            document.LoadHtml(source);
            return document.DocumentNode.SelectNodes("//table[contains(@class, 'cba_transactions_table')]/tbody/tr");
        }

        private static IEnumerable<Transaction> ParseTransactionRow(Account account, HtmlNode row)
        {
            var descriptionNodeParser = new CbaDescriptionNodeParser(row);

            if (!descriptionNodeParser.IsValid)
                yield break;

            var transaction = new Transaction();
            try
            {
                transaction.AccountId = account.Id;
                transaction.Description = descriptionNodeParser.Description;
                transaction.Pending = descriptionNodeParser.IsPending;

                var dateNode = row.SelectSingleNode(".//td[contains(@class, 'date')]");
                dateNode.SelectNodes(".//em")?.FirstOrDefault()?.Remove();
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

        private void Login(Account account)
        {
            _webdriver.Url = LoginUrl;
            _webdriver.Navigate();
            TakeScreenshot();
            var clientNumberField = _webdriver.FindElementByName("txtMyClientNumber$field");
            var passwordField = _webdriver.FindElementByName("txtMyPassword$field");
            var loginButton = _webdriver.FindElementByName("btnLogon$field");
            clientNumberField.SendKeys(account.Username);
            passwordField.SendKeys(account.Password);
            loginButton.Click();
            TakeScreenshot();
        }

        private static void Log(string message)
        {
            Console.WriteLine($"CBAExporter:        {message}");
        }
    }
}