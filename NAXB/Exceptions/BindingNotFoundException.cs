using NAXB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Exceptions
{
    public class BindingNotFoundException : Exception
    {
        public BindingNotFoundException(Type type)
            : base(String.Format("Binding not found for type '{0}'.", type.FullName))
        { }
        public BindingNotFoundException(IXmlProperty property)
            : base(String.Format("Binding not found for Property '{0}'.", property.PropertyInfo.FullName))
        { }
    }
}
