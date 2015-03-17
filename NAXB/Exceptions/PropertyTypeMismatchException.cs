using NAXB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Exceptions
{
    public class PropertyTypeMismatchException : Exception
    {
        public PropertyTypeMismatchException(string message) : base(message) { }
        public PropertyTypeMismatchException(IXmlProperty property, object value, Exception innerException) 
            : base(String.Format("A mismatch occured assigning Property '{0}' to value '{1}'. Property is of type '{2}' and value is of type '{3}'. See inner exception for more details.",
            property.PropertyInfo.Name, value ?? "", property.PropertyInfo.PropertyType.FullName, value != null ? value.GetType().FullName : "unknown")
            , innerException)
        {

        }
    }
}
