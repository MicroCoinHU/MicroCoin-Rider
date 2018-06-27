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
    public class AccountNumberConverter : JsonConverter<AccountNumber>
    {
        public override AccountNumber ReadJson(JsonReader reader, Type objectType, AccountNumber existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JToken jt = JToken.Load(reader);
            String value = jt.Value<String>();
            return value;
        }

        public override void WriteJson(JsonWriter writer, AccountNumber value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
