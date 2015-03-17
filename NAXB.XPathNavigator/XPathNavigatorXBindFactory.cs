using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using NAXB.Interfaces;

namespace NAXB.Navigator
{
    public class XPathNavigatorNAXBFactory : IXmlFactory
    {
        public IXPathProcessor CreateXPathProcessor(IEnumerable<IXmlProperty> properties, IEnumerable<INamespace> namespaces)
        {
            return new XPathNavigatorProcessor();
        }

        public IXmlData CreateXmlData(string xml, Encoding encoding, INamespace[] namespaces = null)
        {
            var doc = new XPathDocument(new StringReader(xml));
            return new NavigatorXmlData(doc);
        }

        public IXmlData CreateXmlData(byte[] byteArray, Encoding encoding, INamespace[] namespaces = null)
        {
            string xml = encoding.GetString(byteArray);
            return CreateXmlData(xml, encoding, namespaces);
        }

        public IXmlData CreateXmlData(System.IO.Stream xmlStream, INamespace[] namespaces = null)
        {
            var doc = new XPathDocument(xmlStream);
            return new NavigatorXmlData(doc);
        }

        public IXmlData CreateXmlData(string fileName, INamespace[] namespaces = null)
        {
            var doc = new XPathDocument(fileName);
            return new NavigatorXmlData(doc);
        }
    }
}
