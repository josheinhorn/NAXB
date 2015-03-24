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
    }
}
