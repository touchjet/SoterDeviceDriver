using System;
namespace SoterDevice.Hid.Tests
{
    public static class AddressUtilities
    {
        const uint HardeningConstant = 0x80000000;

        public static uint HardenNumber(uint softNumber)
        {
            return (softNumber | HardeningConstant) >> 0;
        }

        public static uint UnhardenNumber(uint hardNumber)
        {
            return hardNumber ^ HardeningConstant;
        }

        public static string[] Split(this string path, char splitter)
        {
            return path.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
