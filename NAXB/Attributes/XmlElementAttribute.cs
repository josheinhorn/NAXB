using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    public class XmlElementAttribute : Attribute, INAXBPropertyBinding
    {
        public XmlElementAttribute(string elementName)
        {
            ElementName = elementName;
        }
        public string XPath
        {
            get
            {
                return ElementName;
            }
        }

        public string ElementName { get; protected set; }

        //public bool IsAttribute
        //{
        //    get { return false; }
        //}

        //public bool IsElement
        //{
        //    get { return true; }
        //}
    }
}
