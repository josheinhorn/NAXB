﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using NAXB.BindingModel;
using System.Reflection;

namespace NAXB.Build
{
    public class XmlBindingResolver : IXmlBindingResolver
    {
        protected readonly IReflector reflector;
        protected readonly IXPathProcessor xPathProcessor;
        protected readonly Dictionary<Type, IXmlModelBinding> bindings = new Dictionary<Type, IXmlModelBinding>(); //Is Dictionary the most effective data structure?
        public XmlBindingResolver(IReflector reflector, IXPathProcessor xPathProcessor
            , Assembly[] assemblies)
        {
            if (reflector == null) throw new ArgumentNullException("reflector");
            if (xPathProcessor == null) throw new ArgumentNullException("xPathProcessor");
            this.reflector = reflector;
            this.xPathProcessor = xPathProcessor;
            LoadBindings(assemblies);
        }

        protected virtual void LoadBindings(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    //It is an XML Model and it hasn't yet been added
                    if (type.GetCustomAttributes(typeof(IXmlModelDescription), false).Any()
                        && !bindings.ContainsKey(type)) 
                    {
                        lock (bindings)
                        {
                            if (!bindings.ContainsKey(type))
                            {
                                var result = new XmlModelBinding(type, reflector);
                                bindings.Add(type, result);
                            }
                        }
                    }

                }
            }
            //Initialize all bindings -- must happen after all the bindings were loaded so they're available when requested
            foreach (var entry in bindings)
            {
                foreach (var prop in entry.Value.Properties)
                {
                    //Set complex binding, compile xpath, create delegates, etc.
                    prop.Initialize(this, xPathProcessor, entry.Value.Namespaces); 
                }
            }
        }

        public IXmlModelBinding ResolveBinding<T>()
        {
            var type = typeof(T);
            return ResolveBinding(type);
        }

        public IXmlModelBinding ResolveBinding(Type type)
        {
            IXmlModelBinding result = null;
            bindings.TryGetValue(type, out result);
            return result;
        }
    }
}
