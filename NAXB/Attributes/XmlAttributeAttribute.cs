using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    /// <summary>
    /// Marks a Property/Field to be bound to an XML Attribute of the current node.
    /// </summary>
    public class XmlAttributeAttribute : Attribute, INAXBPropertyBinding
    {
        /// <summary>
        /// Marks a Property/Field to be bound to an XML Attribute of the current node.
        /// </summary>
        /// <param name="attributeName">Attribute Name</param>
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
        public bool IsFunction
        {
            get { return false; }
        }
    }
}
