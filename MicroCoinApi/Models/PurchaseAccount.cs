using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Models
{
    public class PurchaseAccount : PurchaseAccountRequest
    {
        /// <summary>
        /// Transaction Type
        /// </summary>
        public string Type { get; internal set; }
        /// <summary>
        /// Transaction subtype
        /// </summary>
        public string SubType { get; internal set; }
        /// <summary>
        /// Confirmations (how many blocks included)
        /// </summary>
        public uint? Confirmations { get; internal set; }
        /// <summary>
        /// Transaction operation hash
        /// You can find the transaction with this
        /// </summary>
        public string OpHash { get; internal set; }
        /// <summary>
        /// Balance after the transaction
        /// </summary>
        public decimal Balance { get; internal set; }
    }
}
