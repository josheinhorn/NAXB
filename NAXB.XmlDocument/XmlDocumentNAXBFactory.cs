using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NAXB.Interfaces;

namespace NAXB.XmlDOM
{
    public class XmlDocumentNAXBFactory : IXmlFactory
    {
        public IXmlData CreateXmlData(string xml, Encoding encoding, INamespace[] namespaces = null)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            AddNSManager(namespaces, doc);
            return new XmlData(doc);
        }

        protected void AddNamespaces(XmlNamespaceManager nsMgr, INamespace[] namespaces)
        {
            foreach (INamespace ns in namespaces)
            {
                nsMgr.AddNamespace(ns.Prefix, ns.Uri);
            }
        }

        public IXmlData CreateXmlData(byte[] byteArray, Encoding encoding, INamespace[] namespaces = null)
        {
            string xml = encoding.GetString(byteArray);
            return CreateXmlData(xml, encoding, namespaces);
        }

        public IXmlData CreateXmlData(System.IO.Stream xmlStream, INamespace[] namespaces = null)
        {
            var doc = new XmlDocument();
            doc.Load(xmlStream);
            AddNSManager(namespaces, doc);
            return new XmlData(doc);
        }

        public IXmlData CreateXmlData(string fileName, INamespace[] namespaces = null)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            AddNSManager(namespaces, doc);
            return new XmlData(doc);
        }

        protected void AddNSManager(INamespace[] namespaces, XmlDocument doc)
        {
            if (namespaces != null)
            {
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                AddNamespaces(nsMgr, namespaces);
            }
        }
    }
}
