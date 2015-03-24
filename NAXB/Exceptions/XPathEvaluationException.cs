using NAXB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Exceptions
{
    public class XPathEvaluationException : Exception
    {
        public XPathEvaluationException(IXmlProperty property, IXPath xpath, Exception innerException) 
            : base(String.Format("Error evaluating XPath '{0}' for Property '{1}'. See inner exception for more details."
            , xpath.XPathAsString, property.PropertyInfo.FullName), innerException)
        { }
    }
}
