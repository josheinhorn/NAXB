
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using NAXB.Interfaces;

namespace NAXB.Navigator
{
    public class NavigatorXmlData : IXmlData
    {
        public NavigatorXmlData(XPathNavigator navigator)
        {
            if (navigator == null) throw new ArgumentNullException("navigator");
            Init(navigator);
        }

        public NavigatorXmlData(XPathDocument doc)
        {
            if (doc == null) throw new ArgumentNullException("doc");
            Init(doc.CreateNavigator());
        }

        protected void Init(XPathNavigator navigator)
        {
            BaseData = navigator;
            XmlAsString = navigator.OuterXml;
            Encoding = Encoding.UTF8;
            XmlAsByteArray = this.Encoding.GetBytes(XmlAsString);
            Value = navigator.Value;
        }
        public object BaseData
        {
            get; private set;
        }

        public System.IO.Stream XmlAsStream
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public byte[] XmlAsByteArray
        {
            get; private set;
        }

        public string XmlAsString
        {
            get; private set;
        }

        public string Value
        {
            get; private set;
        }

        public Encoding Encoding
        {
            get; private set;
        }
    }
}
