using MicroCoin.Cryptography;
using MicroCoin.Util;
using MicroCoinApi.Json;
using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace MicroCoinApi.Models
{
    /// <summary>
    /// Purchase account request
    /// </summary>
    public class PurchaseAccountRequest
    {
        /// <summary>
        /// Account to purchase
        /// </summary>
        [JsonConverter(typeof(AccountNumberConverter))]
        [JsonSchema(NJsonSchema.JsonObjectType.String)]
        public AccountNumber AccountNumber { get; set; }
        /// <summary>
        /// Founder account, who will pay the price
        /// </summary>
        [JsonConverter(typeof(AccountNumberConverter))]
        [JsonSchema(NJsonSchema.JsonObjectType.String)]
        public AccountNumber FounderAccount { get; set; }
        public SimpleKey NewKey { get; set; }
        public Signature Signature { get; set; }
        /// <summary>
        /// Transaction hash to sign the transaction
        /// </summary>
        public string Hash { get; internal set; }
        /// <summary>
        /// Transaction fee, if any, otherwise zero
        /// </summary>
        public decimal Fee { get; set; }
    }
}
