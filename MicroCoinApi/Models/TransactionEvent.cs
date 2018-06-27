namespace MicroCoinApi.Models
{
    public class TransactionEvent
    {
        public string from { get; set; }
        public string to { get; set; }
        public decimal amount { get; set; }
        public decimal fee { get; set; }
        public string ophash { get; set; }
        public decimal balance { get; set; }
        public string payload { get; set; }
    }
}