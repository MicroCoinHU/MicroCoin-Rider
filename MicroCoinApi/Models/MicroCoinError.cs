using MicroCoin;
using MicroCoin.RPC;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Models
{
    /// <summary>
    /// API error response
    /// </summary>
    public class MicroCoinError
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Help test or url, if any
        /// </summary>
        public string Help { get; set; }
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="ErrorCode">Error code</param>
        /// <param name="Message">Error message</param>
        /// <param name="Help">Help text</param>
        public MicroCoinError(ErrorCode ErrorCode, string Message, string Help)
        {
            this.ErrorCode = (int)ErrorCode;
            this.Message = Message;
            this.Help = Help;
        }
        /// <summary>
        /// Creates MicroCoinError from RPC ErrorCode
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <returns></returns>
        public static MicroCoinError Error(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case MicroCoin.ErrorCode.InternalError:
                    return new MicroCoinError(errorCode, "Internal error", "Internal error, Please try again later");
                case MicroCoin.ErrorCode.InvalidBlock:
                    return new MicroCoinError(errorCode, "Invalid block", "Invalid block, or block not exists");
                case MicroCoin.ErrorCode.InvalidAccount:
                    return new MicroCoinError(errorCode, "Invalid account", "Account not found or account state is invalid");
                case MicroCoin.ErrorCode.InvalidData:
                    return new MicroCoinError(errorCode, "Invalid data", "Your data is invalid. Please check your input");
                case MicroCoin.ErrorCode.InvalidOperation:
                    return new MicroCoinError(errorCode, "Invalid transaction", "Please check transaction data");
                case MicroCoin.ErrorCode.InvalidPubKey:
                    return new MicroCoinError(errorCode, "Invalid public key", "Please check your public key.");
                case MicroCoin.ErrorCode.InvalidSignature:
                    return new MicroCoinError(errorCode, "Invalid signature", "The signature is invalid. Please genrate signature using your private key");
                case MicroCoin.ErrorCode.MethodNotFound:
                    return new MicroCoinError(errorCode, "Method not found", "The node server is outdated");
                case MicroCoin.ErrorCode.NotFound:
                    return new MicroCoinError(errorCode, "Not found", "The requested object not found");
                case MicroCoin.ErrorCode.UnknownError:
                    return new MicroCoinError(errorCode, "Unknown error", "Please check your data");
                case MicroCoin.ErrorCode.WalletPasswordProtected:
                    return new MicroCoinError(errorCode, "Wallet protected", "Wallet protected and can't accessed");
                default:
                    return new MicroCoinError(errorCode, "Unknown error", "Please check your data");
            }
        }
    }
}