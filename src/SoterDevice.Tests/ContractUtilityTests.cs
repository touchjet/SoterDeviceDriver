using System;
using Serilog;
using SoterDevice.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace SoterDevice.Tests
{
    public class ContractUtilityTests
    {
        public ContractUtilityTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger();
        }

        [Fact]
        public void SupportAllMessageTypes()
        {
            bool allSupported = true;
            foreach (var messageType in ((MessageType[])Enum.GetValues(typeof(MessageType))))
            {
                try
                {
                    ContractUtility.GetContractType(messageType);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    allSupported = false;
                }
                Assert.True(allSupported, "Not all message types supported.");
            }
        }

    }
}
