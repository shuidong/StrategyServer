using System;

namespace StrategyServer
{
    class KickOutException : Exception
    {
        public KickOutException(string message)
            : base(message)
        {
        }
    }
}