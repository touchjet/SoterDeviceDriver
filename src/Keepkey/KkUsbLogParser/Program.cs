using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ProtoBuf;
using SoterDevice;
using SoterDevice.Contracts;
using SoterDevice.Utilities;
using Touchjet.BinaryUtils;

namespace KkUsbLogParser
{
#pragma warning disable RECS0060 // Warns when a culture-aware 'IndexOf' call is used by default.
#pragma warning disable RECS0061 // Warns when a culture-aware 'IndexOf' call is used by default.
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

        static void ParseFile(string logFileName)
        {
            bool isHeader = true;
            bool isOutput = true;
            var logLines = File.ReadAllLines(logFileName).Select(line => line.Replace(" ", "")).ToList();

            while (!logLines[0].Contains("---"))
            {
                logLines.RemoveAt(0);
            }
            logLines.RemoveAt(0);

            ByteBuffer buffer = null;
            byte messageId = 0;
            uint messageLength = 0;
            int lineIndex = 0;
            var resultLines = new List<string>();
            var setting = new JsonSerializerSettings { Converters = { new JsonByteArrayHexConverter() } };

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
                    if (isOutput)
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
                    var sb = new StringBuilder();
                    sb.Append(isOutput ? "-->" : "<--");
                    sb.Append(messageType.ToString().Substring(11));
                    sb.Append(" ");
                    sb.Append(JsonConvert.SerializeObject(obj, setting));
                    resultLines.Add(sb.ToString());
                }
                else
                {
                    isHeader = false;
                }
            }
            File.WriteAllLines(logFileName + ".json", resultLines);
        }
        static void Main(string[] args)
        {
            string logFolder = @"./logs/";
            if (args.Length>=1)
            {
                logFolder = args[0];
            }
            foreach(var fileName in Directory.EnumerateFiles(logFolder))
            {
                if (fileName.EndsWith(".txt"))
                {
                    ParseFile(fileName);
                }
            }
        }
    }
#pragma warning restore RECS0061 // Warns when a culture-aware 'IndexOf' call is used by default.
#pragma warning restore RECS0060 // Warns when a culture-aware 'IndexOf' call is used by default.
}
