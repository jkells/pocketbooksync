using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PocketBookSync.Data;

namespace PocketBookSync.PocketBook
{
    public class Qif
    {
        private readonly IEnumerable<Transaction> _transactions;

        public Qif(IEnumerable<Transaction> transactions)
        {
            _transactions = transactions;
        }

        public async Task<Stream> GenerateAsync()
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024*1024, true))
            {
                await writer.WriteLineAsync("!Type:Bank");

                foreach (var transaction in _transactions)
                {
                    await writer.WriteAsync('D');
                    await writer.WriteLineAsync(transaction.Date.ToString("dd/MM/yyyy"));
                    await writer.WriteAsync('T');
                    await writer.WriteLineAsync(transaction.Amount.ToString(CultureInfo.InvariantCulture));
                    await writer.WriteAsync('P');
                    await writer.WriteLineAsync(transaction.Description);
                    await writer.WriteLineAsync("^");
                }
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}