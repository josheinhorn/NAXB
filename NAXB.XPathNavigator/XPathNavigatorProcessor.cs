using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using NAXB.Interfaces;
using System.IO;
using System.Xml;
using NAXB.Xml;

namespace NAXB.Navigator
{
    public class XPathNavigatorProcessor : IXPathProcessor
    {
        public IEnumerable<IXmlData> ProcessXPath(IXmlData data, IXPath xpath)
        {
            IEnumerable<IXmlData> result = null;
            if (data != null && xpath != null && data.BaseData is XPathNavigator)
            {
                var nav = data.BaseData as XPathNavigator;
                XPathNodeIterator nodeIt = null;
                if (xpath.UnderlyingObject is XPathExpression)
                {
                    nodeIt = nav.Select(xpath.UnderlyingObject as XPathExpression);
                }
                else
                {
                    nodeIt = nav.Select(GetCompiledXPathExpression(xpath.XPathAsString, xpath.Namespaces));
                }
                result = nodeIt.Cast<XPathNavigator>().Select(node => new NavigatorXmlData(node));
            }
            return result;
        }

        public IXPath CompileXPath(string xpath, INamespace[] namespaces, PropertyType propertyType, bool isMultiValue)
        {
            var xpe = GetCompiledXPathExpression(xpath, namespaces);
            return new DefaultXPath
            {
                UnderlyingObject = xpe,
                XPathAsString = xpath,
                Namespaces = namespaces
            };
        }

        protected XPathExpression GetCompiledXPathExpression(string xpath, INamespace[] namespaces)
        {
            //XmlNamespaceManager nsMgr = new XmlNamespaceManager(??); //How to get an instance of XmlNameTable?
            //foreach (var ns in namespaces)
            //{
            //    nsMgr.AddNamespace(ns.Prefix, ns.Uri);
            //}
            var xpe = XPathExpression.Compile(xpath);
            //xpe.SetContext(nsMgr);
            return xpe;
        }
    }
}
