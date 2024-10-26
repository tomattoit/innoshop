using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class NoProductsException : Exception
    {
        public NoProductsException() : base($"No products listed.")
        {
        }
    }
}
