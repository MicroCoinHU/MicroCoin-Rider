using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Models
{
    public class PurchaseAccount : PurchaseAccountRequest
    {
        public string Type { get; internal set; }
        public string SubType { get; internal set; }
        public uint? Confirmations { get; internal set; }
        public string OpHash { get; internal set; }
        public decimal Balance { get; internal set; }
    }
}
