using NAXB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Build
{
    public interface IDynamicModelBinder<T>
    {
        T GetModel(IXmlData xml);
    }

    class DynamicAssemblyTesting
    {
    }
}
