using Newtonsoft.Json;
using NSwag.Annotations;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi
{
    public class ReDocCodeSampleAttribute : SwaggerOperationProcessorAttribute
    {
        public ReDocCodeSampleAttribute(string language, string source)
            : base(typeof(ReDocCodeSampleAppender), language, source)
        {
        }

        internal class ReDocCodeSampleAppender : IOperationProcessor
        {
            private readonly string _language;
            private readonly string _source;
            private const string ExtensionKey = "x-code-samples";

            public ReDocCodeSampleAppender(string language, string source)
            {
                _language = language;
                _source = source;
            }

            public Task<bool> ProcessAsync(OperationProcessorContext context)
            {
                if (context.OperationDescription.Operation.ExtensionData == null)
                    context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();
                
                var data = context.OperationDescription.Operation.ExtensionData;
                if (!data.ContainsKey(ExtensionKey))
                    data[ExtensionKey] = new List<ReDocCodeSample>();                
                var samples = (List<ReDocCodeSample>)data[ExtensionKey];
                samples.Add(new ReDocCodeSample
                {
                    Language = _language,
                    Source = _source,
                });

                return Task.FromResult(true);
            }
        }

        internal class ReDocCodeSample
        {
            [JsonProperty("lang")]
            public string Language { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }
        }
    }

}
