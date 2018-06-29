using MicroCoin.Util;
using MicroCoinApi.Json;
using Newtonsoft.Json;
using NJsonSchema.Annotations;
using NJsonSchema.Converters;

namespace MicroCoinApi.Models
{
    public class TransactionRequest
    {
        /// <summary>
        /// Number of coins to send
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Fee, if any
        /// Otherwise zero
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Optional payload string
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// The sender account
        /// </summary>
        [JsonConverter(typeof(AccountNumberConverter))]
        [JsonSchema(NJsonSchema.JsonObjectType.String)]        
        public AccountNumber Sender { get; set; }

        /// <summary>
        /// The target (receiver) account
        /// </summary>
        [JsonConverter(typeof(AccountNumberConverter))]
        [JsonSchema(NJsonSchema.JsonObjectType.String)]
        public AccountNumber Target { get; set; }

        /// <summary>
        /// Transaction hash to sign
        /// </summary>
        public string Hash { get; set; }

        public Signature Signature { get; set; }
    }
}
