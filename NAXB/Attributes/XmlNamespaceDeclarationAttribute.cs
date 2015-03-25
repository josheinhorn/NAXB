using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    /// <summary>
    /// An XML Namespace Declaration for an XML Model Binding
    /// </summary>
    public class NamespaceDeclarationAttribute : Attribute, INamespaceBinding
    {
        /// <summary>
        /// Namespace URI
        /// </summary>
        public string Uri
        {
            get; set;
        }
        /// <summary>
        /// Namespace Prefix
        /// </summary>
        public string Prefix
        {
            get; set;
        }
    }
}
