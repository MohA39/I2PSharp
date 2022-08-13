using System;
using System.Collections.Generic;
using System.Text;

namespace I2PSharp
{
    public class SAMBadResultException : Exception
    {
        public SAMBadResultException( string message) : base(message) { }
    }
}
