using MicroCoin.Cryptography;
using MicroCoin.Util;
using MicroCoinApi.Json;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Annotations;
using NJsonSchema.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Models
{
    /// <summary>
    /// Change key transaction model
    /// </summary>
    public class ChangeKeyRequest
    {
        /// <summary>
        /// The account number of the account
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws when Account number is in bad format
        /// </exception>
        [JsonConverter(typeof(AccountNumberConverter))]
        [JsonSchema(JsonObjectType.String)]
        public AccountNumber AccountNumber { get; set; }
        [JsonRequired]
        public SimpleKey NewOwnerPublicKey { get; set; }
        public Signature Signature { get; set; }
        /// <summary>
        /// The transaction hash for signature generation. Filled by the API 
        /// </summary>        
        public string Hash { get; internal set; }
        /// <summary>
        /// Transaction fee, if any, otherwise zero
        /// </summary>
        public decimal Fee { get; set; }
    }
}
