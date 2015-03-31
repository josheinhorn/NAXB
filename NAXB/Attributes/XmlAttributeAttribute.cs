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
            RootXPath = "@" + attributeName;
            XPaths = null;
        }
        public string RootXPath
        {
           get; private set;
        }

        public string[] XPaths
        {
            get;
            private set;
        }

        public string AttributeName { get; protected set; }

        public int ArgumentCount
        {
            get { return 1; }
        }
    }
}
