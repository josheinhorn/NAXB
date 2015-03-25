using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using NAXB;
using com.ximpleware;
using NAXB.Xml;

namespace NAXB.VtdXml
{
    public class VtdXPathProcessor : IXPathProcessor
    {
        public IEnumerable<IXmlData> ProcessXPath(IXmlData data, IXPath xpath)
        {
            var result = new List<IXmlData>();
            if (data != null && xpath != null && data is VtdXmlData)
            {
                var vtdData = data as VtdXmlData;
                var nav = vtdData.Navigator;
                AutoPilot ap = null;
                if (xpath.UnderlyingObject is AutoPilot)
                {
                    ap = xpath.UnderlyingObject as AutoPilot;
                }
                else
                {
                    ap = new AutoPilot();
                    AddNamespaces(ap, xpath.Namespaces);
                    ap.selectXPath(xpath.XPathAsString);
                }
                ap.bind(nav);
                //Question -- is the XPath evaluated relative to the current Cursor location or relative to the entire document?
                //Answer -- it is relative to the current Cursor position:
                //"If the navigation you want to perform is more complicated, you can in fact nest XPath queries" - http://www.codeproject.com/Articles/28237/Programming-XPath-with-VTD-XML
                
                //Note: attempted to just use IsMultiValue and only use evalXPathToString when not multivalue instead of try/catch, 
                //but this breaks for single nested types where a BookMark is needed
                if (!xpath.IsFunction) //It's a node set
                {
                    //try
                    //{
                        while (ap.evalXPath() != -1) //Evaluated relative to the current cursor of the VTDNav object
                        {
                            BookMark bookMark = new BookMark(nav);
                            //Record the current position navigated to by the AutoPilot
                            bookMark.recordCursorPosition();
                            //Add new XML Data with the recorded position
                            result.Add(new VtdXmlData(bookMark));
                        }
                    //}
                    //catch (XPathEvalException) 
                    //{
                    //    //We assume it failed because it's actually a function expression
                    //    //There doesn't appear to be any way to tell if it is a function without trying to eval
                    //    xpath.IsFunction = true; //Set to true so all future attempts will go straight to function evaluation
                    //}
                }
                //if (xpath.IsFunction)
                else //it is a function!
                {
                    //XPath is actually a function, evaluate to single string
                    string evaluatedValue = ap.evalXPathToString(); //Always evaluate to string, parse to real property type later

                    //Switch isn't necessary because we parse the value later, a string will suffice for now!
                    //switch (xpath.Type) 
                    //{
                    //    case XPathType.Text:
                    //        evaluatedValue = ap.evalXPathToString();
                    //        break;
                    //    case XPathType.Boolean:
                    //        evaluatedValue = ap.evalXPathToBoolean();
                    //        break;
                    //    case XPathType.Numeric:
                    //        evaluatedValue = ap.evalXPathToNumber();
                    //        break;
                    //    default:
                    //        evaluatedValue = string.Empty;
                    //        break;
                    //}
                    result.Add(new VtdXmlData(evaluatedValue));
                }
                ap.resetXPath();
            }
            return result;
        }
        protected void AddNamespaces(AutoPilot ap, INamespace[] namespaces)
        {
            foreach (var ns in namespaces)
            {
                ap.declareXPathNameSpace(ns.Prefix, ns.Uri);
            }
        }
        public IXPath CompileXPath(string xpath, INamespace[] namespaces, PropertyType propertyType, bool isFunction)
        {
            var ap = new AutoPilot();
            AddNamespaces(ap, namespaces);
            ap.selectXPath(xpath);
            //XPath type not actually necessary, just using Text always works!
            XPathType type;
            switch (propertyType)
            {
                case PropertyType.Text:
                    type = XPathType.Text;
                    break;
                case PropertyType.Number:
                    type = XPathType.Numeric;
                    break;
                case PropertyType.Bool:
                    type = XPathType.Boolean;
                    break;
                case PropertyType.DateTime:
                    type = XPathType.Text;
                    break;
                case PropertyType.Enum:
                    type = XPathType.Text;
                    break;
                case PropertyType.Complex:
                    type = XPathType.Text;
                    break;
                case PropertyType.XmlFragment:
                    type = XPathType.Text;
                    break;
                default:
                    type = XPathType.Text;
                    break;
            }
            return new DefaultXPath
            {
                UnderlyingObject = ap,
                XPathAsString = xpath,
                Namespaces = namespaces,
                Type = type,
                IsFunction = isFunction
            };
        }
    }
}
