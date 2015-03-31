using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    /// <summary>
    /// Marks a property/field to be bound to a direct XML child element of the current context.
    /// </summary>
    public class XmlElementAttribute : Attribute, INAXBPropertyBinding
    {
        /// <summary>
        /// Marks a property/field to be bound to a direct XML child element of the current context.
        /// </summary>
        /// <param name="elementName">Element name</param>
        public XmlElementAttribute(string elementName)
        {
            ElementName = elementName;
            XPaths = null;
        }
        public string RootXPath
        {
            get
            {
                return ElementName;
            }
        }

        public string[] XPaths
        {
            get;
            private set;
        }
        public string ElementName { get; protected set; }

        public int ArgumentCount
        {
            get { return 1; }
        }
    }
}
