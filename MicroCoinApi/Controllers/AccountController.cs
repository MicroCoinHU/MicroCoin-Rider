using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MicroCoin;
using MicroCoin.Cryptography;
using MicroCoin.DTO;
using MicroCoin.RPC;
using MicroCoin.Util;
using MicroCoinApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSwag.Annotations;

namespace MicroCoinApi.Controllers
{
    /// <summary>
    /// Operations to manage accounts
    /// </summary>
    [Produces("application/json")]    
    [Consumes("application/json")]    
    [Route("api/Account")]
    public class AccountController : Controller
    {
        private MicroCoinClient client = new MicroCoinClient(MicroCoinClientConfiguration.DefaultMainNet);
        private IHubContext<MicroCoinHub> hubContext;

        public AccountController(IHubContext<MicroCoinHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        /// <summary>
        /// Account detils
        /// </summary>
        /// <remarks>
        /// You can retrieve details (balance, name, type, etc.) of any account, if you know the account number.
        /// </remarks>
        /// <param name="AccountNumber">Account number, example: 1-22, or 1</param>
        /// <response code="200">Returns the account object</response>
        /// <response code="400">Invalid account number</response>
        /// <response code="404">Account not exists</response>
        /// <response code="500">Internal error</response>
        /// <returns>Account details</returns>
        [HttpGet("{AccountNumber}")]
        [ProducesResponseType(200, Type = typeof(Account))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [ProducesResponseType(404, Type = typeof(MicroCoinError))]
        [ProducesResponseType(500, Type = typeof(MicroCoinError))]
        [SwaggerOperation("GetAccount")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Account), Description = "Account details")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid account number specified")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(MicroCoinError), Description = "Account not exists")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(MicroCoinError), Description = "Internal error")]
        [ReDocCodeSample("CURL", "curl - X GET--header 'Accept: application/json' 'https://rider.microcoin.hu/api/Account/0-10'")]
        public ActionResult<Account> GetAccount(string AccountNumber)
        {
            AccountNumber number;
            try
            {                
                number = AccountNumber;
            }
            catch (InvalidCastException e)
            {
                try
                {
                    var acc = client.FindAccounts(AccountNumber);
                    if (acc.Count() > 0)
                    {
                        number = acc.First().AccountNumber;
                    }
                    else throw new Exception("Invalid account number");
                } catch (Exception)
                {
                    return BadRequest(new MicroCoinError(ErrorCode.InvalidAccount, e.Message, $"Account number ({AccountNumber}) not valid. You can specify account numbers in two way: number-checksum, or single number"));
                }
            }
            AccountDTO account = null;
            try
            {                
                account = client.GetAccount(number);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }

            if (account == null)
                return NotFound(new MicroCoinError(ErrorCode.NotFound,$"Account {number} not found","Your account number is valid, but no accounts exists with this number"));
            var key = client.DecodePubKey(account.EncPubKey, null);
            return new Account
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Name = account.Name,
                Status = account.State.ToString(),
                Type = account.Type,
                Price = account.Price / 10000M,
                PublicKey = new SimpleKey
                {
                    X = key.X,
                    Y = key.Y
                }
            };
        }

        /// <summary>
        /// Transaction event receiver
        /// </summary>
        /// <param name="transactionEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("event")]
        [SwaggerIgnore]
        public Response<string> Event([FromBody] TransactionEvent transactionEvent)
        {
            hubContext.Clients.All.SendAsync(transactionEvent.to, transactionEvent).Wait();
            hubContext.Clients.All.SendAsync(transactionEvent.from, transactionEvent).Wait();
            return Response<string>.SuccessResponse("ok");
        }

        /// <summary>
        /// Get list of accounts for sale
        /// </summary>
        /// <remarks>
        /// <para>This is the list of accounts for sale.</para>
        /// <para>
        /// You can purchase accounts if you have enough MicroCoin in your founder account.
        /// The account price will be deducted from the founder account balance.
        /// You must sign the transaction with the founder account private key.
        /// </para>
        /// </remarks>
        /// <returns>List of accounts for sale</returns>
        /// <response code="200">List of accounts for sale</response>
        [HttpGet]
        [Route("offers")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Account>))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Account>), Description = "List of accounts for sale")]
        [SwaggerOperation("GetOffers")]
        public ActionResult<IEnumerable<Account>> GetOffers()
        {            
            var result = new List<Account>();
            try
            {
                foreach (var account in client.FindAccounts(null, null, 2).Where(p => p.PrivateSale == false))
                {
                    result.Add(new Account
                    {
                        AccountNumber = account.AccountNumber,
                        Balance = account.Balance,
                        Name = account.Name,
                        Price = account.Price / 10000M,
                        Status = account.State.ToString(),
                        Type = account.Type
                    });
                }
                return result.OrderBy(p => p.Price - p.Balance).ToList();
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
        }

        /// <summary>
        /// Get a list of accounts belonging to the key
        /// </summary>
        /// <remarks>
        /// Every account belongs to a public key. If you know the public key you can retrieve the list of accounts belonging to the key.
        /// </remarks>
        /// <param name="key">The public key</param>
        /// <response code="200">Account list</response>
        /// <returns>Account list</returns>
        /// <response code="400">Bad key</response>
        [HttpPost("list")]        
        [ProducesResponseType(typeof(IEnumerable<Account>), 200)]
        [ProducesResponseType(typeof(MicroCoinError), 400)]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Account>), Description = "Account list")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Bad key - error message")]
        [SwaggerOperation("MyAccounts")]
        public ActionResult<IEnumerable<Account>> MyAccounts([FromBody] SimpleKey key)
        {
            var result = new List<Account>();
            try
            {
                string keyPair;
                if (!Enum.TryParse(key.CurveType, true, out KeyType curveType))
                    return BadRequest(new MicroCoinError(ErrorCode.InvalidPubKey, "Invalid key type", "Valid types are: secp256k1, secp384k1"));
                try
                {
                    keyPair = client.EncodePubKey(Enum.Parse<KeyType>(key.CurveType, true),
                        key.X.PadLeft(64, '0'), key.Y.PadLeft(64, '0'));
                }
                catch (MicroCoinRPCException e)
                {
                    return this.HandlerError(e);
                }                
                foreach (var account in client.FindMyAccounts(keyPair))
                {
                    result.Add(new Account
                    {
                        AccountNumber = account.AccountNumber,
                        Balance = account.Balance,
                        Name = account.Name,
                        Price = account.Price / 10000M,
                        Status = account.State.ToString(),
                        Type = account.Type
                    });
                }
                return result;
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
        }

        /// <summary>
        /// Create new change key transaction
        /// </summary>
        /// <param name="changeKey">Initial transaction data</param>
        /// <remarks>
        /// With the change key transaction you can transfer your account to a new owner.<br />
        /// To send a new changekey transaction you need to create a transaction using this this method, 
        /// then sign it with your private key.
        /// You can send your transaction using the CommitChangeKey method.
        /// </remarks>        
        /// <response code="200">ChangeKey transaction successfully created</response>
        /// <response code="400">Invalid data sent</response>
        /// <returns>New ChangeKey transaction object for signing</returns>
        /// <example>
        /// <code>
        ///     client.ChangeKey(tr);
        /// </code>
        /// </example>
        [HttpPost]
        [Route("change-key/start")]
        [ProducesResponseType(200, Type = typeof(ChangeKeyRequest))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(ChangeKeyRequest), Description = "Transaction created. You can sign it")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid data")]
//        [SwaggerTag("Change account owner", AddToDocument = true, Description = @"Transfer account to a new owner")]
        [SwaggerOperation("StartChangeKey")]
        public ActionResult StartChangeKey([FromBody] ChangeKeyRequest changeKey)
        {
            try
            {
                var transaction = ChangeKeyRequestToTransaction(changeKey);
                changeKey.Hash = (Hash)transaction.GetHash();
                return Ok(changeKey);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
        }

        private MicroCoin.Transactions.ChangeKeyTransaction ChangeKeyRequestToTransaction(ChangeKeyRequest changeKey)
        {
            var account = client.GetAccount(changeKey.AccountNumber);
            string pubkey = account.EncPubKey;
            var key = client.DecodePubKey(pubkey, null);
            return new MicroCoin.Transactions.ChangeKeyTransaction
            {
                NewAccountKey = new ECKeyPair
                {
                    CurveType = Enum.Parse<CurveType>(changeKey.NewOwnerPublicKey.CurveType, true),
                    PublicKey = new ECPoint
                    {
                        X = (Hash)changeKey.NewOwnerPublicKey.X.PadLeft(64, '0'),
                        Y = (Hash)changeKey.NewOwnerPublicKey.Y.PadLeft(64, '0')
                    }
                },
                TargetAccount = changeKey.AccountNumber,
                SignerAccount = changeKey.AccountNumber,
                TransactionType = MicroCoin.Transactions.TransactionType.ChangeKeySigned,
                NumberOfOperations = account.NumOperations + 1,
                Fee = (ulong)(changeKey.Fee * 10000M),
                AccountKey = new ECKeyPair
                {
                    CurveType = (CurveType)key.KeyType,
                    PublicKey = new ECPoint
                    {
                        X = (Hash)key.X,
                        Y = (Hash)key.Y
                    }
                }
            };
        }

        /// <summary>
        /// Commit change key transaction
        /// </summary>
        /// <remarks>
        /// With the change key transaction you can transfer your account to a new owner.<br />
        /// To send a new changekey transaction you need to create a transaction using the StartChangeKey method, 
        /// then sign it with your private key.
        /// You can send your transaction using this method.
        /// </remarks>
        /// <param name="changeKey">The signed transaction</param>
        /// <returns>Transaction</returns>
        /// <response code="200">Transaction committed</response>
        /// <response code="400">Invalid data</response>
        /// <response code="403">Invalid signature</response>
        [HttpPost]
        [Route("change-key/commit")]
        [ProducesResponseType(200, Type = typeof(ChangeKey))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [ProducesResponseType(403, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(ChangeKey), Description = "Transaction committed")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description ="Invalid transaction")]
        [SwaggerResponse(HttpStatusCode.Forbidden,  typeof(MicroCoinError), Description = "Invalid signature")]
        [SwaggerOperation("CommitChangeKey")]
        public ActionResult CommitChangeKey([FromBody] ChangeKeyRequest changeKey)
        {
            MicroCoin.Transactions.ChangeKeyTransaction transaction = null;
            try
            {
                transaction = ChangeKeyRequestToTransaction(changeKey);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            if (changeKey.Signature == null)
            {
                return BadRequest(new MicroCoinError(ErrorCode.InvalidOperation, "Missing signature", "You must sign the hash and set the signature"));
            }
            transaction.Signature = new ECSignature
            {
                R = (Hash)changeKey.Signature.R.PadLeft(64, '0'),
                S = (Hash)changeKey.Signature.S.PadLeft(64, '0')
            };
            if (Utils.ValidateSignature(transaction.GetHash(), transaction.Signature, transaction.AccountKey))
            {
                using (var ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms, System.Text.Encoding.Default, true))
                    {
                        bw.Write(1);
                        bw.Write(7);
                    }
                    transaction.SaveToStream(ms);
                    ms.Position = 0;
                    Hash h = ms.ToArray();
                    try
                    {
                        var resp = client.ExecuteOperations(h);
                        var r = resp.First();
                        var tr = new ChangeKey
                        {
                            AccountNumber = (uint)r.Account,
                            Balance = r.Balance,
                            OpHash = r.Ophash,
                            Signer = (uint)r.SignerAccount,
                            SubType = r.SubType.ToString(),
                            Type = r.Type.ToString(),
                            Confirmations = r.Maturation,
                            NewOwnerPublicKey = changeKey.NewOwnerPublicKey,
                            Signature = changeKey.Signature
                        };
                        return Ok(tr);
                    }
                    catch (MicroCoinRPCException e)
                    {
                        return this.HandlerError(e);
                    }
                }
            }
            else
            {
                return StatusCode(403, new MicroCoinError(ErrorCode.InvalidData, "Invalid signature", "Your signature is invalid"));
            }
        }

        /// <summary>
        /// Create purchase account transaction
        /// </summary>
        /// <remarks>
        /// If you want to purchase an account you need to create a new transaction, sign it then send it into the newtwork.
        /// This method creates a new transaction and a hash for you. You need to sign the hash then commit your transaction
        /// with the CommitPurchaseAccount method
        /// </remarks>
        /// <param name="data">Transaction data</param>
        /// <returns>Transaction</returns>
        /// <response code="200">Transaction to sign</response>
        /// <response code="400">Invalid data</response>
        [HttpPost("purchase/start")]
        [ProducesResponseType(200, Type = typeof(PurchaseAccountRequest))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(PurchaseAccountRequest), Description = "Transaction to sign")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid data")]
        [SwaggerOperation("StartPurchaseAccount")]
        public ActionResult<PurchaseAccountRequest> StartPurchaseAccount([FromBody] PurchaseAccountRequest data)
        {
            try
            {
                MicroCoin.Transactions.TransferTransaction transaction = PurchaseAccountRequestToTransaction(data);
                data.Hash = (Hash)transaction.GetHash();
                return Ok(data);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            catch (Exception e)
            {
                return StatusCode(500, new MicroCoinError(ErrorCode.UnknownError, e.Message, ""));
            }
        }

        private MicroCoin.Transactions.TransferTransaction PurchaseAccountRequestToTransaction(PurchaseAccountRequest data)
        {
            var account = client.GetAccount(data.AccountNumber);
            var founder = client.GetAccount(data.FounderAccount);
            string pubkey = founder.EncPubKey;
            PublicKeyDTO key = client.DecodePubKey(pubkey, null);
            return new MicroCoin.Transactions.TransferTransaction
            {
                Amount = (ulong)(account.Price),
                Fee = (ulong)(data.Fee * 10000M),
                SignerAccount = founder.AccountNumber,
                SellerAccount = account.SellerAccount,
                TargetAccount = account.AccountNumber,
                TransactionStyle = MicroCoin.Transactions.TransferTransaction.TransferType.BuyAccount,
                TransactionType = MicroCoin.Transactions.TransactionType.BuyAccount,
                NumberOfOperations = founder.NumOperations + 1,
                AccountKey = new ECKeyPair
                {
                    CurveType = (CurveType)key.KeyType,
                    PublicKey = new ECPoint
                    {
                        X = (Hash)key.X,
                        Y = (Hash)key.Y
                    }
                },
                AccountPrice = (ulong)(account.Price),
                NewAccountKey = new ECKeyPair
                {
                    CurveType = Enum.Parse<CurveType>(data.NewKey.CurveType, true),
                    PublicKey = new ECPoint
                    {
                        X = (Hash)data.NewKey.X.PadLeft(64, '0'),
                        Y = (Hash)data.NewKey.Y.PadLeft(64, '0')
                    }
                }
            };
        }

        /// <summary>
        /// Commit signed Purchase account transaction
        /// </summary>
        /// <remarks>
        /// If you created and signed your "Purchase account" transaction you need to send it into the network.
        /// Call this method to send and commit your signed "Purchase account" transaction.
        /// </remarks>
        /// <param name="data">Signed transaction</param>
        /// <returns>Transaction</returns>        
        /// <response code="200">Transaction committed</response>
        /// <response code="400">Invalid data</response>
        /// <response code="403">Invalid signature</response>
        [HttpPost("purchase/commit")]
        [ProducesResponseType(200, Type=typeof(PurchaseAccount))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [ProducesResponseType(403, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(PurchaseAccount), Description = "Transaction committed")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid transaction")]
        [SwaggerResponse(HttpStatusCode.Forbidden, typeof(MicroCoinError), Description = "Invalid signature")]
        [SwaggerOperation("CommitPurchaseAccount")]
        public async Task<IActionResult> CommitPurchaseAccount([FromBody] PurchaseAccountRequest data)
        {
            MicroCoin.Transactions.TransferTransaction transaction = null;
            try
            {
                transaction = PurchaseAccountRequestToTransaction(data);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            catch (Exception e)
            {
                return StatusCode(500, new MicroCoinError(ErrorCode.UnknownError, e.Message, ""));
            }
            Hash hash = transaction.GetHash();
            if (data.Signature != null)
            {
                transaction.Signature = new ECSignature
                {
                    R = (Hash) data.Signature.R.PadLeft(64, '0'),
                    S = (Hash) data.Signature.S.PadLeft(64, '0')
                };
                if (Utils.ValidateSignature(hash, transaction.Signature, transaction.AccountKey))
                {
                    using (var ms = new MemoryStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms, System.Text.Encoding.Default, true))
                        {
                            bw.Write(1);
                            bw.Write(6);
                        }
                        transaction.SaveToStream(ms);
                        ms.Position = 0;
                        Hash h = ms.ToArray();
                        var resp = await client.ExecuteOperationsAsync(h);
                        if (resp.Count() > 0 && resp.First().Errors == null)
                        {
                            var r = resp.First();
                            var tr = new PurchaseAccount
                            {
                                FounderAccount = (uint)r.Account,
                                AccountNumber = (uint)r.DestAccount,
                                Type = r.Type.ToString(),
                                SubType = r.SubType.ToString(),
                                Fee = r.Fee,
                                Confirmations = r.Maturation,
                                OpHash = r.Ophash,
                                Balance = r.Balance
                            };
                            return Ok(tr);
                        }
                        else
                        {
                            return BadRequest(new MicroCoinError(ErrorCode.InvalidOperation,
                                resp.Count() > 0 ? resp.First().Errors : "Invalid transaction",
                                "Your transaction is invalid"
                                ));
                        }                        
                    }
                }
                else
                {
                    return StatusCode(403, MicroCoinError.Error(ErrorCode.InvalidSignature));
                }
            }
            return BadRequest("Missing signature");
        }

        /// <summary>
        /// Retrieve account pendig transactions
        /// </summary>
        /// <param name="AccountNumber">Account number to </param>       
        /// <returns>List of the account history</returns>
        /// <response code="200">Pendign transactions</response>
        /// <response code="400">Invalid account number</response>
        [HttpGet("{AccountNumber}/pending")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Models.Transaction>))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Transaction>), Description = "List of the pending transactions")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid account number")]
        [SwaggerOperation("GetPendings")]
        public ActionResult<IEnumerable<Models.Transaction>> GetTransactions(string AccountNumber)
        {
            AccountNumber number;
            try
            {
                number = AccountNumber;
            }
            catch (InvalidCastException)
            {
                return BadRequest(new MicroCoinError(ErrorCode.InvalidAccount, "Invalid account", $"Account number ({AccountNumber}) not valid. You can specify account numbers in two way: number-checksum, or single number"));
            }
            try
            {
                var ops = client.GetPendings();
                var filtered = new List<OperationDTO>();
                foreach (var o in ops)
                {                                            
                    if(o.Account == number || o.SenderAccount == number || o.SignerAccount == number || o.DestAccount == number)
                    {
                        filtered.Add(o);
                    }
                }
                var result = new List<Transaction>();
                foreach (var op in filtered)
                {
                    ByteString payload = op.PayLoad;
                    //if (!payload.IsReadable)
                    //    payload = (Hash)payload;
                    result.Add(new Transaction
                    {
                        Block = op.Block.HasValue ? op.Block.Value : 0,
                        Timestamp = op.Time.HasValue ? op.Time.Value : 0,
                        Amount = op.Amount,
                        Fee = op.Fee,
                        Payload = (Hash)op.PayLoad,
                        Sender = op.SenderAccount,
                        Target = (uint)op.DestAccount,
                        Signer = (uint)op.SignerAccount,
                        Type = op.Type.ToString(),
                        Confirmations = op.Maturation.HasValue ? op.Maturation.Value : 0,
                        SubType = op.SubType.ToString(),
                        Balance = op.Balance,
                        OpHash = op.Ophash
                    });
                }
                return Ok(result);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            catch (Exception e)
            {
                return BadRequest(new MicroCoinError(ErrorCode.InvalidData, e.Message, e.Message));
            }
        }



        /// <summary>
        /// Retrieve account transaction history
        /// </summary>
        /// <param name="AccountNumber">Account number to </param>
        /// <param name="start">Start from</param>
        /// <param name="max">Maximum lines to receive</param>
        /// <returns>List of the account history</returns>
        /// <response code="200">Transaction history</response>
        /// <response code="400">Invalid account number</response>
        [HttpGet("{AccountNumber}/history")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Models.Transaction>))]
        [ProducesResponseType(400, Type = typeof(MicroCoinError))]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<Transaction>), Description = "List of the account history")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(MicroCoinError), Description = "Invalid account number")]
        [SwaggerOperation("GetTransactions")]
        public ActionResult<IEnumerable<Models.Transaction>> GetTransactions(string AccountNumber, [FromQuery] int? start=null, [FromQuery] int? max=null)
        {
            AccountNumber number;
            try
            {
                number = AccountNumber;
            }
            catch (InvalidCastException) {
                return BadRequest(new MicroCoinError(ErrorCode.InvalidAccount,"Invalid account",$"Account number ({AccountNumber}) not valid. You can specify account numbers in two way: number-checksum, or single number"));
            }
            try
            {
                var ops = client.GetAccountOperations(number, null, start, max);
                var result = new List<Transaction>();
                foreach (var op in ops)
                {
                    ByteString payload = op.PayLoad;
                    //if (!payload.IsReadable)
                    //    payload = (Hash)payload;
                    result.Add(new Transaction
                    {
                        Block = op.Block.HasValue?op.Block.Value:0,
                        Timestamp = op.Time.HasValue?op.Time.Value:0,
                        Amount = op.Amount,
                        Fee = op.Fee,
                        Payload = (Hash) op.PayLoad,
                        Sender = op.SenderAccount,
                        Target = (uint)op.DestAccount,
                        Signer = (uint)op.SignerAccount,
                        Type = op.Type.ToString(),
                        Confirmations = op.Maturation.HasValue?op.Maturation.Value:0,
                        SubType = op.SubType.ToString(),
                        Balance = op.Balance,
                        OpHash = op.Ophash
                    });
                }
                return Ok(result);
            }
            catch (MicroCoinRPCException e)
            {
                return this.HandlerError(e);
            }
            catch (Exception e)
            {
                return BadRequest(new MicroCoinError(ErrorCode.InvalidData, e.Message, e.Message));
            }
        }
    }
}