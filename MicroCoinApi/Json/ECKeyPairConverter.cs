using MicroCoin.Cryptography;
using MicroCoin.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Json
{
    public class ECKeyPairConverter : JsonConverter<ECKeyPair>
    {
        public override ECKeyPair ReadJson(JsonReader reader, Type objectType, ECKeyPair existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            JObject jo = JObject.Load(reader);
            Hash X = jo.Value<string>("X");
            Hash Y = jo.Value<string>("Y");            
            return new ECKeyPair
            {               
                CurveType = (CurveType) Enum.Parse(typeof(CurveType), jo.Value<string>("CurveType"), true),
                PublicKey = new System.Security.Cryptography.ECPoint
                {
                    X = X,
                    Y = Y
                }
            };
        }

        public override void WriteJson(JsonWriter writer, ECKeyPair value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            jo.Add("CurveType", value.CurveType.ToString());
            Hash X = value.PublicKey.X;
            Hash Y = value.PublicKey.Y;
            jo.Add("X", X.ToString());
            jo.Add("Y", Y.ToString());
            jo.WriteTo(writer);
        }
    }
}
