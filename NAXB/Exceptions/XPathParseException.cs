using NAXB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Exceptions
{
    public class XPathCompilationException : Exception
    {
        public XPathCompilationException(IXmlProperty property) 
            : base(String.Format("Failed to compile XPath '{0}' on Property '{1}'."
            , property.Binding.RootXPath
            , property.PropertyInfo.FullName))
        { }
    }
}
