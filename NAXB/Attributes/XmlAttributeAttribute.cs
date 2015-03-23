using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    public class XmlAttributeAttribute : Attribute, INAXBPropertyBinding
    {
        public XmlAttributeAttribute(string attributeName)
        {
            AttributeName = attributeName;
            XPath = "@" + attributeName;
        }
        public string XPath
        {
           get; private set;
        }

        public string AttributeName { get; protected set; }

        //public bool IsAttribute
        //{
        //    get { return true; }
        //}

        //public bool IsElement
        //{
        //    get { return false; }
        //}
    }
}
