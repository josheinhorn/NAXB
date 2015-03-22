using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests
{
    public static class TestUtils
    {
        public static bool EnumerablesAreEqual<T>(IEnumerable<T> x, IEnumerable<T> y)
        {
            bool equal = true;
            //try
            //{
                var arr1 = x.ToArray();
                var arr2 = y.ToArray();
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (!arr2[i].Equals(arr1[i]))
                    {
                        equal = false;
                        break;
                    }
                }
            //}
            //catch (Exception)
            //{
            //    equal = false;
            //}
            return equal;
        }
    }
}
