﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(Guid id) : base($"Product with id {id} was not found.")
        {
        }
    }
}
