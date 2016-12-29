using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using PocketBookSync.Data;

namespace PocketBookSync.PocketBook
{
    public sealed class PocketBookUploader : IDisposable
    {
        private const string LoginUrl = "https://getpocketbook.com/login";
        private const string SignInUrl = "https://getpocketbook.com/signin";
        private const string UploadUrl = "https://getpocketbook.com/upload";
        private const string SettingsUrl = "https://getpocketbook.com/settings";

        private readonly AppDbContext _db;

        private HttpClient _httpClient;
        private bool _loggedIn;        

        public PocketBookUploader(AppDbContext db)
        {
            _db = db;
        }

        public void Dispose()
        {
            using (_httpClient)
            {
            }
        }

        private async Task LoginAsync()
        {
            if (_loggedIn)
                return;

            var config = await _db.GetConfigAsync();
            if (config.PocketBookUsername == null)
                throw new AppException("Please configure your pocketbook credentials first using the configure command");

            var cookieContainer = new CookieContainer();
            _httpClient =
                new HttpClient(new HttpClientHandler {AllowAutoRedirect = true, CookieContainer = cookieContainer});

            var csrfToken = await GetCsrfToken(SignInUrl);

            var loginResponse = await _httpClient.PostAsync(LoginUrl, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", config.PocketBookUsername),
                new KeyValuePair<string, string>("password", config.PocketBookPassword),
                new KeyValuePair<string, string>("_csrf", csrfToken)
            }));

            if (!loginResponse.IsSuccessStatusCode)
                throw new AppException(
                    $"Error logging in to PocketBook {loginResponse.StatusCode} {loginResponse.ReasonPhrase}");
            Log("Logged in");
            _loggedIn = true;
        }

        private async Task<string> GetCsrfToken(string url)
        {
            var signInPage = await _httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(signInPage);
            var csrfNode = document.DocumentNode.SelectSingleNode("//input[@name='_csrf']");
            var csrfToken = csrfNode.GetAttributeValue("value", "");
            return csrfToken;
        }

        public async Task UploadAsync(Account account)
        {
            var unprocessed = await _db.Transactions.Where(x => !x.Processed && x.AccountId == account.Id).ToListAsync();
            if (!unprocessed.Any())
                return;

            await LoginAsync();
            var qif = new Qif(unprocessed);
            var csrfToken = await GetCsrfToken(SignInUrl);

            using (var stream = await qif.GenerateAsync())
            {
                await UploadAsync(account.PocketBookAccountNumber, stream, csrfToken);
            }

            Log($"Uploaded {unprocessed.Count} transactions to PocketBook account: {account.AccountReference}");

            foreach (var transaction in unprocessed)
            {
                transaction.Processed = true;
            }
        }

        private async Task UploadAsync(string accountNumber, Stream qifFile, string csrfToken)
        {
            var random = new Random();
            var boundary = $"-------------------------{random.Next()}";
            var filePart = new StreamContent(qifFile);
            var formPart = new StringContent(accountNumber);
            var body = new MultipartFormDataContent(boundary)
            {
                {filePart, "Filedata[]", "export.qif"},
                {formPart, "accountId"}
            };
            body.Headers.Add("X-CSRF-TOKEN", csrfToken);

            var uploadResponse = await _httpClient.PostAsync(UploadUrl, body);

            if (!uploadResponse.IsSuccessStatusCode)
                throw new AppException(
                    $"Error uploading transactions to PocketBook {uploadResponse.StatusCode} {uploadResponse.ReasonPhrase}");
        }

        private static void Log(string message)
        {
            Console.WriteLine($"PocketBookUploader: {message}");
        }
    }
}