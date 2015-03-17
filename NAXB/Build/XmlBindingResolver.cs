using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using NAXB.BindingModel;

namespace NAXB.Build
{
    public class XmlBindingResolver : IXmlBindingResolver
    {
        protected readonly IReflector reflector;
        protected readonly IXPathProcessor xPathProcessor;
        protected readonly Dictionary<Type, IXmlModelBinding> bindings = new Dictionary<Type, IXmlModelBinding>(); //Is Dictionary the most effective data structure?
        public XmlBindingResolver(IReflector reflector, IXPathProcessor xPathProcessor)
        {
            if (reflector == null) throw new ArgumentNullException("reflector");
            if (xPathProcessor == null) throw new ArgumentNullException("xPathProcessor");
            this.reflector = reflector;
            this.xPathProcessor = xPathProcessor;
        }

        public IXmlModelBinding ResolveBinding<T>()
        {
            var type = typeof(T);
            return ResolveBinding(type);
        }

        public IXmlModelBinding ResolveBinding(Type type)
        {
            IXmlModelBinding result = null;
            if (!bindings.TryGetValue(type, out result))
            {
                lock (bindings)
                {
                    if (!bindings.TryGetValue(type, out result))
                    {
                        //lazy load
                        result = new XmlModelBinding(type, reflector); //What if the type is not complex? e.g. string, int, etc.
                        if (result.Description != null)
                        {
                            foreach (var prop in result.Properties)
                            {
                                prop.Initialize(this, xPathProcessor, result.Namespaces); //Set complex binding, compile xpath
                            }
                        }
                        bindings.Add(type, result);
                    }
                }
            }
            return result;
        }
    }
}
