using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NAXB.Interfaces;
using System.IO;
using NAXB.Xml;

namespace NAXB.XmlDOM
{
    public class XmlDocumentXPathProcessor : IXPathProcessor
    {        
        public IEnumerable<IXmlData> ProcessXPath(IXmlData data, IXPath xpath)
        {
            IEnumerable<IXmlData> result = null;
            XmlNode nodeToXpath = null;
            if (data.BaseData is XmlDocument) //It's a full-blown XmlDoc, use it
            {
                nodeToXpath = data.BaseData as XmlDocument;
            }
            else if (data.BaseData is XmlNode) //It's a node of a doc, use it
            {
                nodeToXpath = data.BaseData as XmlNode;
            }
            else //Some unknown implementation . . . what to do?
            {
                //??
            }
            if (nodeToXpath != null)
            {
                var nodes = nodeToXpath.SelectNodes(xpath.XPathAsString); //Get the XPath'ed nodes only
                result = nodes.Cast<XmlNode>().Select(node => new XmlData(node));
            }
            return result;
        }

        public IXPath CompileXPath(string xpath, INamespace[] namespaces, PropertyType propertyType)
        {
            return new DefaultXPath
            {
                UnderlyingObject = xpath,
                Namespaces = namespaces,
                XPathAsString = xpath
            };
        }
    }
}
