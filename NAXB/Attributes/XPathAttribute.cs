using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{

    /// <summary>
    /// Marks a Property/Field to be bound to an XPath expression evaluated on the current context. 
    /// The XPath may be a function expression or a node set selection.
    /// </summary>
    public class XPathAttribute : Attribute, INAXBPropertyBinding
    {
        /// <summary>
        /// Marks a Property/Field to be bound to an XPath expression evaluated on the current context. 
        /// </summary>
        /// <param name="childXPaths">XPath expression. This may be a function expression or a node set selection.</param>
        public XPathAttribute(string xpath, params string[] childXPaths)
        {
            RootXPath = xpath;
            XPaths = childXPaths;
        }
        /// <summary>
        /// XPath expression
        /// </summary>
        public string RootXPath
        {
            get; set;
        }
        public string[] XPaths
        {
            get;
            private set;
        }
    }
}
