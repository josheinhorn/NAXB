using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using NAXB.Xml;

namespace NAXB.BindingModel
{
    public class XmlModelBinding : IXmlModelBinding
    {
        protected Func<object> defaultConstructor = null;

        public XmlModelBinding(Type type, IReflector reflector)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (reflector == null) throw new ArgumentNullException("reflector");
            ModelType = type;
            Name = ModelType.Name;
            Properties = ModelType.GetProperties().Select(property => new XmlProperty(property, reflector) as IXmlProperty).ToList();
            Description = ModelType.GetCustomAttributes(typeof(IXmlModelDescription), true).Cast<IXmlModelDescription>().FirstOrDefault();
            defaultConstructor = reflector.BuildDefaultConstructor(type);
            //Get namespaces from Type's Custom Attributes
            Namespaces = ModelType.GetCustomAttributes(typeof(INamespaceBinding), true)
                .Cast<INamespaceBinding>()
                .Select(binding => new XmlNamespace { Prefix = binding.Prefix, Uri = binding.Uri })
                .ToArray();
        }
        public IList<IXmlProperty> Properties
        {
            get; protected set;
        }

        public virtual object CreateInstance()
        {
            return defaultConstructor();
        }

        public Type ModelType
        {
            get; protected set;
        }

        public string Name
        {
            get; protected set;
        }

        public IXmlModelDescription Description //TODO: Actually USE the root element name when evaluating XPaths
        {
            get; protected set;
        }

        public INamespace[] Namespaces
        {
            get;
            protected set;
        }


        public virtual object CreateInstance(params object[] ctorArgs)
        {
            throw new NotImplementedException(); //Even if we have Ctors that accept args, how to decide which ctor to use? Type comparisons?
        }
    }
}
