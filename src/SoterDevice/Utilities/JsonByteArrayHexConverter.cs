using System;
using System.Linq;
using Newtonsoft.Json;

namespace SoterDevice.Utilities
{
    public class JsonByteArrayHexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        private readonly string _separator;

        public JsonByteArrayHexConverter(string separator = "") => _separator = separator;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var hexString = string.Join(_separator, ((byte[])value).Select(p => p.ToString("X2")));
            writer.WriteValue(hexString);
        }
    }
}
