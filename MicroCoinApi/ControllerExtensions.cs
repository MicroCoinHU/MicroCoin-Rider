using MicroCoin;
using MicroCoin.RPC;
using MicroCoinApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroCoinApi
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Handle MicroCoin RPC error
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="e">RPC Exception</param>
        /// <returns>API Response</returns>
        public static ObjectResult HandlerError(this Controller controller, MicroCoinRPCException e)
        {
            MicroCoinError error = new MicroCoinError(e.Error.ErrorCode, e.Message, "");
            switch (e.Error.ErrorCode)
            {
                
                case ErrorCode.NotFound: return controller.NotFound(error);
                case ErrorCode.InvalidAccount: return controller.BadRequest(error);
                case ErrorCode.InternalError: return controller.StatusCode(500, error);
                case ErrorCode.InvalidBlock: return controller.BadRequest(error);
                case ErrorCode.InvalidData: return controller.BadRequest(error);
                case ErrorCode.InvalidOperation: return controller.BadRequest(error);
                case ErrorCode.InvalidPubKey: return controller.BadRequest(error);
                case ErrorCode.MethodNotFound: return controller.BadRequest(error);
                case ErrorCode.WalletPasswordProtected: return controller.BadRequest(error);
                default: return controller.BadRequest(error);
            }
        }
    }
}
