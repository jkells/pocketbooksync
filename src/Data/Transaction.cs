using System;

namespace PocketBookSync.Data
{
    public class Transaction
    {
        public int AccountId { get; set; }
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool Pending { get; set; }
        public bool Processed { get; set; }
    }
}