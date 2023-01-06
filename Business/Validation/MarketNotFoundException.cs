using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Business.Validation
{
    [Serializable]
    public class MarketNotFoundException:Exception
    {
        public MarketNotFoundException()
        {

        }
        public MarketNotFoundException(string message) : base(message)
        {

        }

        public MarketNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MarketNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
