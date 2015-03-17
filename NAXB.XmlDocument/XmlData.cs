
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NAXB.Interfaces;

namespace NAXB.XmlDOM
{
    public class XmlData : IXmlData
    {
        public XmlData(XmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            BaseData = node;
            XmlAsString = node.OuterXml;
            Encoding = Encoding.UTF8; //Unknown unless we search the Node's OwnerDocument for it
            XmlAsByteArray = this.Encoding.GetBytes(XmlAsString);
            Value = node.Value;
        }

        public XmlData(XmlDocument doc)
        {
            if (doc == null) throw new ArgumentNullException("doc");
            BaseData = doc;
            XmlAsString = doc.OuterXml;
            //Move Encoding stuff to lazy loaded property
            XmlDeclaration declaration = null;
            if (doc.DocumentType != null && doc.DocumentType.NextSibling is XmlDeclaration)
            {
                declaration = doc.DocumentType.NextSibling as XmlDeclaration;
            }
            else if (doc.FirstChild is XmlDeclaration)
            {
                declaration = doc.FirstChild as XmlDeclaration;
            }
            if (declaration != null)
            {
                switch (declaration.Encoding)
                {
                    case "UTF-8":
                        this.Encoding = Encoding.UTF8;
                        break;
                    case "ASCII":
                        this.Encoding = Encoding.ASCII;
                        break;
                    case "Unicode":
                    case "UTF-16LE":
                    case "UTF-16":
                        this.Encoding = Encoding.Unicode;
                        break;
                    case "BigEndianUnicode":
                    case "UTF-16BE":
                        this.Encoding = Encoding.BigEndianUnicode;
                        break;
                    default:
                        this.Encoding = Encoding.UTF8;
                        break;
                }
            }
            XmlAsByteArray = this.Encoding.GetBytes(XmlAsString); //Move to lazy loaded property
            Value = doc.Value;
        }

        public object BaseData
        {
            get;
            private set;
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
            get;
            private set;
        }

        public string XmlAsString
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }

        public Encoding Encoding
        {
            get;
            private set;
        }
    }
}
