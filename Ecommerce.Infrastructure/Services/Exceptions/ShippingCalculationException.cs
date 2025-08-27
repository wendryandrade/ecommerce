using System;
using System.Runtime.Serialization;

namespace Ecommerce.Infrastructure.Services.Exceptions
{
    [Serializable] // necess�rio para serializa��o
    public class ShippingCalculationException : Exception
    {
        public ShippingCalculationException()
        {
        }

        public ShippingCalculationException(string message) 
            : base(message)
        {
        }

        public ShippingCalculationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        // Construtor para serializa��o
        protected ShippingCalculationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
