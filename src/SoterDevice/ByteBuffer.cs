using System;
using System.Linq;
using System.Text;
using Touchjet.BinaryUtils;

namespace SoterDevice
{
    public class ByteBuffer
    {
        readonly Endianness _endianness;

        public byte[] Value { get; }

        public int Position { get; private set; }

        public ByteBuffer(uint size, Endianness endianness)
        {
            Value = Enumerable.Repeat((byte)0, (int)size).ToArray();
            _endianness = endianness;
        }

        public void PutHex(string hexString)
        {
            Put(hexString.ToBytes());
        }

        public void PutASCII(string asciiString)
        {
            Put(Encoding.ASCII.GetBytes(asciiString));
        }

        public void Put(UInt16 value)
        {
            Put(value.ToBytes(_endianness));
        }

        public void Put(UInt32 value)
        {
            Put(value.ToBytes(_endianness));
        }

        public void Put(UInt64 value)
        {
            Put(value.ToBytes(_endianness));
        }

        public void Put(byte theByte)
        {
            if (Position >= Value.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            Value[Position] = theByte;
            Position++;
        }

        public void Put(byte[] bytes, int startIndex, int length)
        {
            if (Position + length >= Value.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            Buffer.BlockCopy(bytes, startIndex, Value, Position, length);
            Position += length;
        }

        public void Put(byte[] bytes)
        {
            Put(bytes, 0, bytes.Length);
        }
    }
}
