using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Storage.Azure
{
    /// <summary>
    ///     HACK: This is a clone of the CosmosJsonDotNetSerializer found at:
    ///     https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos/src/Serializer/CosmosJsonDotNetSerializer.cs
    ///     It adds the configuration of JSON parsing which is required to fix the bug in parsing
    ///     <see cref="DateTimeOffset" /> that exists currently in this class
    /// </summary>
    internal sealed class CosmosJsonDotNetSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
        private readonly JsonSerializerSettings serializerSettings;

        internal CosmosJsonDotNetSerializer()
        {
            this.serializerSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            };
        }

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T) (object) stream;
                }

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var jsonSerializer = GetSerializer();
                        return jsonSerializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, DefaultEncoding, 1024, true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.None;
                    var jsonSerializer = GetSerializer();
                    jsonSerializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }

        private JsonSerializer GetSerializer()
        {
            return JsonSerializer.Create(this.serializerSettings);
        }
    }
}