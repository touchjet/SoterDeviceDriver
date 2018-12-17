using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProtoBuf;
using SoterDevice;
using SoterDevice.Contracts;
using Touchjet.BinaryUtils;

namespace KkUsbLogParser
{
#pragma warning disable RECS0060 // Warns when a culture-aware 'IndexOf' call is used by default.
    class Program
    {
        static Type MessageTypeType => typeof(MessageType);
        static object Deserialize(MessageType messageType, byte[] data)
        {
            try
            {
                var contractType = ContractUtility.GetContractType(messageType);

                return Deserialize(contractType, data);
            }
            catch
            {
                throw new Exception("InvalidProtocolBufferException");
            }
        }

        static object Deserialize(Type type, byte[] data)
        {
            using (var writer = new MemoryStream(data))
            {
                return Serializer.NonGeneric.Deserialize(type, writer);
            }
        }

        static void Main(string[] args)
        {
            bool isHeader = true;
            bool isOutput = true;
            string logFileName = args.Length >= 1 ? args[0] : $"/Users/Zhen/Documents/Keepkey_Init.txt";
            var logLines = File.ReadAllLines(logFileName).Skip(26).Select(line => line.Replace(" ", "")).ToArray();
            ByteBuffer buffer = null;
            byte messageId = 0;
            uint messageLength = 0;
            uint lineIndex = 0;

            var setting = new JsonSerializerSettings { Converters = { new ByteArrayHexConverter() } };

            while (lineIndex < logLines.Count())
            {
                string line = logLines[lineIndex];
                int lineStart = 0;
                if (isHeader)
                {
                    if (line.Contains("OUT"))
                    {
                        isOutput = true;
                        lineStart = line.IndexOf("OUT") + 3;
                    }
                    else if (line.Contains("IN"))
                    {
                        isOutput = false;
                        lineStart = line.IndexOf("IN") + 2;
                    }
                    else
                    {
                        throw new Exception("WRONG LINE!!!");
                    }
                    //Console.WriteLine(line.Substring(lineStart, 64));
                    var bytes = line.Substring(lineStart, 64).ToBytes();
                    messageId = bytes[4];
                    messageLength = (uint)bytes[8] + ((uint)bytes[7] << 8);
                    buffer = new ByteBuffer(messageLength, Endianness.BigEndian);
                    for (int i = 9; i < bytes.Length; i++)
                    {
                        if (messageLength == 0) break;
                        buffer.Put(bytes[i]);
                        messageLength--;
                    }
                    lineIndex++;
                    line = logLines[lineIndex];
                    bytes = line.Substring(0, 64).ToBytes();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (messageLength == 0) break;
                        buffer.Put(bytes[i]);
                        messageLength--;
                    }
                    lineIndex++;
                }
                else
                {
                    if(isOutput)
                    {
                        lineStart = line.IndexOf("OUT") + 3;
                    }
                    else
                    {
                        lineStart = line.IndexOf("IN") + 2;
                    }
                    var bytes = line.Substring(lineStart, 64).ToBytes();
                    for (int i = 1; i < bytes.Length; i++)
                    {
                        if (messageLength == 0) break;
                        buffer.Put(bytes[i]);
                        messageLength--;
                    }
                    lineIndex++;
                    line = logLines[lineIndex];
                    bytes = line.Substring(0, 64).ToBytes();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (messageLength == 0) break;
                        buffer.Put(bytes[i]);
                        messageLength--;
                    }
                    lineIndex++;
                }
                if (messageLength == 0)
                {
                    isHeader = true;
                    var messageTypeValueName = Enum.GetName(MessageTypeType, messageId);

                    var messageType = (MessageType)Enum.Parse(MessageTypeType, messageTypeValueName);
                    var obj = Deserialize(messageType, buffer.Value);
                    if (isOutput)
                    {
                        Console.Write("-->");
                    }
                    else
                    {
                        Console.Write("<--");
                    }

                    Console.WriteLine($"{messageType.ToString()} {JsonConvert.SerializeObject(obj, setting)}");

                }
                else
                {
                    isHeader = false;
                }
            }
        }
    }

    class ByteArrayHexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        private readonly string _separator;

        public ByteArrayHexConverter(string separator = ",") => _separator = separator;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var hexString = string.Join(_separator, ((byte[])value).Select(p => p.ToString("X2")));
            writer.WriteValue(hexString);
        }
    }

#pragma warning restore RECS0060 // Warns when a culture-aware 'IndexOf' call is used by default.
}
