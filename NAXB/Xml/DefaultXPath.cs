using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Xml
{
    public class DefaultXPath : IXPath
    {

        public string XPathAsString
        {
            get; set;
        }

        public object UnderlyingObject
        {
            get; set;
        }

        public INamespace[] Namespaces
        {
            get; set;
        }
    }
}
