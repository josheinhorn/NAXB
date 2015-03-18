using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    public class XPathAttribute : Attribute, INAXBPropertyBinding
    {
        public XPathAttribute(string xpath)
        {
            XPath = xpath;
        }
        public string XPath
        {
            get; set;
        }
    }
}
