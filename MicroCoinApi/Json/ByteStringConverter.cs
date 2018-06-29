using MicroCoin.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi.Json
{
    /// <summary>
    /// ByteString to Json converter for swagger
    /// </summary>
    public class ByteStringConverter : JsonConverter<ByteString>
    {
        public override ByteString ReadJson(JsonReader reader, Type objectType, ByteString existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return jo.Value<string>("Value");            
        }
        public override void WriteJson(JsonWriter writer, ByteString value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            jo.Add("isReadable", value.IsReadable);
            jo.Add("Length", value.Length);
            jo.Add("Value", value.IsReadable?value.ToString():"");
            jo.WriteTo(writer);            
        }
    }
}