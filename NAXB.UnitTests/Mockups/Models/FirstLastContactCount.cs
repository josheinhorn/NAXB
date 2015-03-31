using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests.Mockups.Models
{
    public struct StringStringShort
    {
        public string FirstName;
        public string LastName;
        public short ContactCount;

        public StringStringShort(string a, string b, short c)
        {
            FirstName = a;
            LastName = b;
            ContactCount = c;
        }
    }
}
