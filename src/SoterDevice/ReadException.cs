namespace SoterDevice
{
    public class ReadException : DeviceException
    {
        public byte[] ReadData;
        public object LastWrittenMessage;

        public ReadException(string message, byte[] readData, object lastWrittenMessage) : base(message)
        {
            ReadData = readData;
        }
    }
}
