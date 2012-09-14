using System;

namespace StrategyServer
{
    [Serializable]
    class KickOutException : Exception
    {
        public KickOutException(string message)
            : base(message)
        {
        }
    }
}