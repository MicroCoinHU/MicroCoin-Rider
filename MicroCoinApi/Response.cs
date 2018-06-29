using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public int ErrorCode { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }

        public static Response<T> ErrorResponse(int code, string message)
        {
            return new Response<T>()
            {
                ErrorCode = code,
                Error = message
            };
        }

        public static Response<T> SuccessResponse(T data)
        {
            return new Response<T>()
            {
                ErrorCode = 0,
                Error = null,
                Data = data   ,
                Success = true
            };
        }
    }
}
