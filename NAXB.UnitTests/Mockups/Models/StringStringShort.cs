using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests.Mockups.Models
{
    public struct StringStringShort
    {
        public string StringItem;
        public string StringItem2;
        public short ShortItem;

        public StringStringShort(string a, string b, short c)
        {
            StringItem = a;
            StringItem2 = b;
            ShortItem = c;
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && obj is StringStringShort)
            {
                var that = (StringStringShort)obj;
                result = this.StringItem == that.StringItem
                    && this.StringItem2 == that.StringItem2
                    && this.ShortItem == that.ShortItem;
            }
            return result;
        }
    }
}
