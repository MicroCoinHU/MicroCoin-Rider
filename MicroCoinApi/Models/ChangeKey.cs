﻿using MicroCoin.Util;
using MicroCoinApi.Json;
using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Models
{
    public class ChangeKey : ChangeKeyRequest
    {
        public SimpleKey AccountKey { get; set; }

        /// <summary>
        /// Signer (sender) acount number
        /// </summary>
        [JsonConverter(typeof(AccountNumberConverter))]
        [JsonSchema(NJsonSchema.JsonObjectType.String)]
        public AccountNumber Signer { get; internal set; }
        /// <summary>
        /// Transaction type
        /// </summary>
        public string Type { get; internal set; }
        /// <summary>
        /// Transaction confirmations (how many blocks)
        /// </summary>
        public uint? Confirmations { get; internal set; }
        /// <summary>
        /// Transaction subtype
        /// </summary>
        public string SubType { get; internal set; }
        /// <summary>
        /// The new balance after the transaction
        /// </summary>
        public decimal Balance { get; internal set; }
        /// <summary>
        /// The transaction hash
        /// </summary>
        public string OpHash { get; internal set; }
        /// <summary>
        /// Number of transactions
        /// </summary>
        public uint NumberOfOperations { get; set; }
    }
}
