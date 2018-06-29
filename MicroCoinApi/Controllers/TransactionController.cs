using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using MicroCoin;
using MicroCoin.Cryptography;
using MicroCoin.DTO;
using MicroCoin.RPC;
using MicroCoin.Util;
using MicroCoinApi.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace MicroCoinApi.Controllers
{
    /// <summary>
    /// Manage transactions
    /// </summary>
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/Transaction")]
    public class TransactionController : Controller
    {
        /// <summary>
        /// Retrieve single transaction by hash
        /// </summary>
        /// <remarks>
        /// If you know the transaction hash (ophash), you can retreive the transaction details
        /// </remarks>
        /// <param name="ophash">Transaction hash</param>
        /// <returns>Transaction object</returns>
        /// <response code="200">Transaction</response>
        /// <response code="400">Invalid data</response>
        /// <response code="404">Transaction not found</response>
        [HttpGet("{ophash}")]
        [ProducesResponseType(200, Type = typeof(Transaction))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [ProducesResponseType(404, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Transaction), Description = "The transaction details")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid transaction hash")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MicroCoinError), Description = "Transaction not found")]
        [SwaggerOperation("GetTransaction")]
        public ActionResult<Transaction> GetTransaction(string ophash)
        {
            var microCoinClient = new MicroCoinClient();
            OperationDTO resp;
            try
            {
                resp = microCoinClient.FindOperation(ophash);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            if (resp == null)
                return NotFound( new MicroCoinError(ErrorCode.NotFound, "Transaction not found","No transaction found with the requested ophash"));
            var response = new Transaction
            {
                Amount = resp.Amount,
                Balance = resp.Balance,
                Confirmations = resp.Maturation,
                Fee = resp.Fee,
                OpHash = resp.Ophash,
                Payload = (Hash)resp.PayLoad,
                Sender = resp.SenderAccount,
                Signer = (uint)resp.SignerAccount,
                SubType = resp.SubType.ToString(),
                Target = resp.DestAccount,
                Type = resp.Type.ToString()
            };
            return Ok(response);


        }

        /// <summary>
        /// Create transaction for sign
        /// </summary>
        /// <remarks>
        /// To send coins you need to create and send transactions. Using this method you can validate your transaction
        /// and you will get a transaction hash. This is the hash what you need to sign using your private key, then you
        /// can commit your transaction with your valid signature
        /// </remarks>
        /// <param name="data">Basic transaction data</param>
        /// <response code="200">Transaction created</response>
        /// <response code="400">Invalid data</response>        
        [HttpPost("start")]
        [ProducesResponseType(200, Type = typeof(TransactionRequest))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(TransactionRequest), Description = "Transaction to sign")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid data")]
        [SwaggerOperation("StartTransaction")]
        public ActionResult<TransactionRequest> StartTransaction([FromBody] TransactionRequest data)
        {
            var microCoinClient = new MicroCoinClient();
            AccountDTO account;
            try
            {
                account = microCoinClient.GetAccount(data.Sender);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            string pubkey = account.EncPubKey;
            PublicKeyDTO key = null;
            try
            {
                key = microCoinClient.DecodePubKey(pubkey, null);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            Hash X = key.X;
            Hash Y = key.Y;
            try
            {
                var transaction = new MicroCoin.Transactions.TransferTransaction
                {
                    Amount = (ulong)(data.Amount * 10000),
                    Fee = (ulong)(data.Fee * 10000M),
                    Payload = data.Payload,
                    SignerAccount = data.Sender,
                    TargetAccount = data.Target,
                    TransactionStyle = MicroCoin.Transactions.TransferTransaction.TransferType.Transaction,
                    TransactionType = MicroCoin.Transactions.TransactionType.Transaction,
                    AccountKey = new ECKeyPair
                    {
                        CurveType = CurveType.Secp256K1,
                        PublicKey = new ECPoint
                        {
                            X = X,
                            Y = Y,
                        }
                    }
                };
                transaction.NumberOfOperations = microCoinClient.GetAccount(transaction.SignerAccount).NumOperations + 1;
                Hash hash = transaction.GetHash();
                data.Hash = hash;
                return Ok(data);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            catch(Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new MicroCoinError(ErrorCode.UnknownError, e.Message, ""));
            }
        }
        /// <summary>
        /// Commit a signed transaction
        /// </summary>
        /// <remarks>
        /// After you created and signed your transaction, you need to commit it.
        /// </remarks>
        /// <param name="data">The signed transaction</param>
        /// <returns>Transaction</returns>
        /// <response code="200">Transaction commited</response>
        /// <response code="400">Invalid data</response>
        /// <response code="403">Invalid signature</response>
        [HttpPost("commit")]        
        [ProducesResponseType(200, Type = typeof(Transaction))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [ProducesResponseType(403, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Transaction), Description = "Transaction committed")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid transaction")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MicroCoinError), Description = "Invalid signature")]
        [SwaggerOperation("CommitTransaction")]
        public ActionResult<Transaction> CommitTransaction([FromBody] TransactionRequest data)
        {
            var microCoinClient = new MicroCoinClient();
            string pubkey = microCoinClient.GetAccount(data.Sender).EncPubKey;
            var key = microCoinClient.DecodePubKey(pubkey, null);
            Hash X = key.X;
            Hash Y = key.Y;
            var transaction = new MicroCoin.Transactions.TransferTransaction
            {
                Amount = (ulong)(data.Amount * 10000),
                Fee = (ulong)(data.Fee * 10000M),
                Payload = data.Payload,
                SignerAccount = data.Sender,
                TargetAccount = data.Target,
                TransactionStyle = MicroCoin.Transactions.TransferTransaction.TransferType.Transaction,
                TransactionType = MicroCoin.Transactions.TransactionType.Transaction,
                AccountKey = new ECKeyPair
                {
                    CurveType = CurveType.Secp256K1,
                    PublicKey = new ECPoint
                    {
                        X = X,
                        Y = Y,
                    }
                }
            };
            transaction.NumberOfOperations = microCoinClient.GetAccount(transaction.SignerAccount).NumOperations + 1;
            Hash hash = transaction.GetHash();
            if (data.Signature != null)
            {
                var Signature = new ECSignature();
                Hash R = data.Signature.R.ToUpper().PadLeft(64, '0');
                Hash S = data.Signature.S.ToUpper().PadLeft(64, '0');
                Signature.R = R;
                Signature.S = S;
                transaction.Signature = Signature;
                var b = Utils.ValidateSignature(hash, transaction.Signature, transaction.AccountKey);
                if (!b)
                {
                    return StatusCode(403, MicroCoinError.Error(ErrorCode.InvalidSignature));
                }
                using (var ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms, System.Text.Encoding.Default, true))
                    {
                        bw.Write(1);
                        bw.Write(1);
                    }
                    transaction.SaveToStream(ms);
                    ms.Position = 0;
                    Hash h = ms.ToArray();
                    var resp = microCoinClient.ExecuteOperations(h);
                    if (resp.First().Errors == null)
                    {
                        var op = resp.First();
                        var tr =  new Transaction
                        {
                            Amount = op.Amount,
                            Balance = op.Balance,
                            Confirmations = op.Maturation,
                            Fee = op.Fee,
                            OpHash = op.Ophash,
                            Sender = op.SenderAccount,
                            Target = op.DestAccount,
                            Payload = new ByteString(op.PayLoad),
                            Signer = (uint)op.SignerAccount,
                            SubType = op.SubType.ToString(),
                            Type = op.Type.ToString()
                        };
                        return Ok(tr);
                    }
                    else
                    {
                        return BadRequest(new MicroCoinError(ErrorCode.InvalidData, resp.First().Errors, "Check your data"));
                    }
                }
            }
            return BadRequest(new MicroCoinError(ErrorCode.InvalidData, "Missing signature","Please sign your transaction hash"));
        }
    }
}

