using Newtonsoft.Json;
using NJsonSchema.Annotations;

namespace MicroCoinApi.Models
{   
    /// <summary>
    /// Simple ECDSA public key
    /// </summary>
    public class SimpleKey 
    {
        /// <summary>
        /// The curve type of the key.
        /// Eg: secp256k1
        /// </summary>
        public string CurveType { get; set; }
        /// <summary>
        /// Public key X coordinate in hexadecimal
        /// </summary>
        public string X { get; set; }
        /// <summary>
        /// Public key Y coordinate in hexadecimal
        /// </summary>
        public string Y { get; set; }
    }

}
