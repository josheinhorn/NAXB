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
                
                while (ap.evalXPath() != -1) //Evaluated relative to the current cursor of the VTDNav object
                {
                    //nav.push(); //push the new cursor position onto the internal stack
                    BookMark bookMark = new BookMark(nav);
                    bookMark.recordCursorPosition(); //Which cursor position is it getting here? Theoretically should be the position navigated to by the AutoPilot
                    result.Add(new VtdXmlData(bookMark));
                    //nav.pop(); //reset the cursor position (is this even necessary since the VtdXmlData Ctor isn't actually doing any navigation?)
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
        public IXPath CompileXPath(string xpath, INamespace[] namespaces)
        {
            var ap = new AutoPilot();
            AddNamespaces(ap, namespaces);
            ap.selectXPath(xpath);
            return new DefaultXPath
            {
               UnderlyingObject = ap,
               XPathAsString = xpath,
               Namespaces = namespaces
            };
        }
    }
}
