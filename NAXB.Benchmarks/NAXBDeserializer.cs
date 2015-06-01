using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Build;
using NAXB.VtdXml;
using NAXB.Interfaces;
using System.Reflection;
namespace NAXB.Benchmarks
{
    public class NAXBDeserializer : IDeserializer
    {
        private IXmlFactory factory = new VtdXmlFactory();
        private IReflector reflector = new Reflector();
        private IXPathProcessor xpather = new VtdXPathProcessor();
        private IXmlBindingResolver resolver;
        private IXmlModelBinder binder;
        private Type type;
        public void Load(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            this.type = type;
            resolver = new XmlBindingResolver(reflector, xpather, new Assembly[] { type.Assembly });
            binder = new XmlBinder(resolver, xpather, reflector);
        }

        public object DeserializeXMLFile(string fileName)
        {
            var xml = factory.CreateXmlData(fileName);
            return binder.BindToModel(type, xml);
        }

        public string Name
        {
            get;
            set;
        }
        public long ElapsedMilliseconds { get; set; }
    }
}
