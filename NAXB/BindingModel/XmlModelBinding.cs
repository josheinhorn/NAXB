using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using NAXB.Xml;
using System.Reflection;

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
            //Get all Field and Properties with a NAXB Property Attribute and creating Property objects
            Description = ModelType.GetCustomAttributes(typeof(IXmlModelDescription), true).Cast<IXmlModelDescription>().FirstOrDefault();
            string xpath = Description == null ? null : Description.RootXPath;
            Properties = ModelType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(property => (property is PropertyInfo || property is FieldInfo) && property.GetCustomAttributes(typeof(INAXBPropertyBinding), true).Any())
                .Select(property => new XmlProperty(property, reflector, xpath) as IXmlProperty).ToList();
            //Try to create the ctor -- will throw exception if doesn't work
            defaultConstructor = reflector.BuildDefaultConstructor(type);
            //Get namespaces from Type's Custom Attributes
             Namespaces = ModelType.GetCustomAttributes(typeof(INamespaceBinding), true)
                .Cast<INamespaceBinding>()
                .Select(binding => new XmlNamespace { Prefix = binding.Prefix, Uri = binding.Uri })
                .ToArray();
        }
        public IList<IXmlProperty> Properties
        {
            get;
            protected set;
        }

        public virtual object CreateInstance()
        {
            return defaultConstructor();
        }

        public Type ModelType
        {
            get;
            protected set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public IXmlModelDescription Description //TODO: Actually USE the root element name when evaluating XPaths
        {
            get;
            protected set;
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
