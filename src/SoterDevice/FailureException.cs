using System;
namespace SoterDevice
{
    [Serializable]
    public class FailureException<T> : Exception
    {
        public T Failure { get; }

        public FailureException(string message, T failure) : base(message)
        {
            Failure = failure;
        }
    }
}
