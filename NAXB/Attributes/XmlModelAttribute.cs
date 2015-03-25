using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    /// <summary>
    /// Marks a Model that can be bound to XML
    /// </summary>
    public class XmlModelBindingAttribute : Attribute, IXmlModelDescription
    {
        /// <summary>
        /// Marks a Model that can be bound to XML
        /// </summary>
        public XmlModelBindingAttribute()
        { }

        /// <summary>
        /// The root XPath for all properties/fields of this model
        /// </summary>
        public virtual string RootXPath //TODO: Figure out how to actually use this - problem is that it will break XPath Functions (e.g. count)
        {
            get;
            set;
        }
    }
}
