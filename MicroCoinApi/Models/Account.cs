using MicroCoin.Util;
using MicroCoinApi.Json;
using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Models
{
    /// <summary>
    /// MicroCoin account entity
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Account number
        /// </summary>
        [JsonSchema(NJsonSchema.JsonObjectType.String)]
        [JsonConverter(typeof(AccountNumberConverter))]
        public AccountNumber AccountNumber { get; set; }
        /// <summary>
        /// Account balance
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// Account name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Account type
        /// </summary>
        public uint Type { get; set; }
        /// <summary>
        /// Account status
        /// Listed = for sale
        /// Normal = normal account
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Account price if account for sale
        /// </summary>
        public decimal Price { get; set; }
        public SimpleKey PublicKey { get; internal set; }
    }
}
