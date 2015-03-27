using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using com.ximpleware;
using com.ximpleware.xpath;
using System.IO;

namespace NAXB.VtdXml
{
    public class VtdXmlData : IXmlData
    {
        private VTDNav nav;
        private byte[] byteArray;
        private string xmlString;
        private string value;
        private Encoding encoding;
        public VtdXmlData(string evaluatedValue)
        {
            this.value = evaluatedValue;
            this.xmlString = evaluatedValue;
        }

        public VtdXmlData(BookMark bookMark)
        {
            this.nav = bookMark.Nav;
            this.VTDBookMark = bookMark;
        }
        public VtdXmlData(byte[] xml)
        {
            VTDGen gen = new VTDGen();
            gen.setDoc(xml); 
            gen.parse(true);
            nav = gen.getNav();
            byteArray = xml;
            VTDBookMark = new BookMark(nav);
            VTDBookMark.recordCursorPosition();
            //Lazy load other properties...
        }

        //public int VTDIndex { get; protected set; }
        public BookMark VTDBookMark { get; protected set; }
        /// <summary>
        /// Returns the underlying VTDNav object with the Cursor position set to the point specified when this VtdXmlData object was created
        /// </summary>
        public VTDNav Navigator
        {
            get
            {
                VTDBookMark.setCursorPosition();
                return nav;
            }
        }
        public object BaseData
        {
            get 
            { 
                return Navigator;
            }
        }

        public System.IO.Stream XmlAsStream
        {
            get
            {
                return new MemoryStream(XmlAsByteArray);
            }
        }

        public byte[] XmlAsByteArray
        {
            get
            {
                if (byteArray == null)
                {
                    lock (nav) //Lock on the VTDNav for thread safety if we use multi-threading in future
                    {
                        if (VTDBookMark.setCursorPosition())
                        {
                            var elementToken = nav.getElementFragment();
                            int length = (int)(elementToken >> 32);
                            int offset = (int)elementToken;
                            nav.getXML().getBytes(offset, length);
                        }
                        else byteArray = new byte[0];
                    }
                }
                return byteArray;
            }
        }

        public string XmlAsString
        {
            get
            {
                if (xmlString == null)
                {
                    lock (nav) //Lock on the VTDNav for thread safety if we use multi-threading in future
                    {
                        if (VTDBookMark.setCursorPosition())
                        {
                            long elementToken = nav.getElementFragment();
                            int length = (int)(elementToken >> 32);
                            int offset = (int)elementToken;
                            xmlString = nav.toString(offset, length);
                        }
                        else xmlString = string.Empty;
                    }
                }
                return xmlString;
            }
        }

        public string Value
        {
            get
            {
                if (value == null)
                {
                    lock (nav) //Lock on the VTDNav for thread safety if we use multi-threading in future
                    {
                        if (VTDBookMark.setCursorPosition())
                        {
                            int index = -1;
                            if ((index = nav.getText()) == -1) //no text, assumed an attribute
                            {
                                var name = nav.toString(nav.getCurrentIndex());
                                index = nav.getAttrVal(name);
                            }
                            //If index is still -1, there is no value (e.g. self closing element)
                            if (index != -1) value = nav.toNormalizedString(index).Trim();
                            else value = string.Empty;
                        }
                        else value = String.Empty;
                    }
                }
                return value;
            }
        }

        public Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    int encodingIndex = nav.getEncoding();
                    if (encodingIndex == VTDNav.FORMAT_UTF8)
                    {
                        encoding = Encoding.UTF8;
                    }
                    else if (encodingIndex == VTDNav.FORMAT_ASCII)
                    {
                        encoding = Encoding.ASCII;
                    }
                    else if (encodingIndex == VTDNav.FORMAT_UTF_16LE)
                    {
                        encoding = Encoding.Unicode;
                    }
                    else if (encodingIndex == VTDNav.FORMAT_UTF_16BE)
                    {
                        encoding = Encoding.BigEndianUnicode;
                    }
                    else encoding = Encoding.UTF8; //just guess it's UTF8...
                }
                return encoding;
            }
        }
    }
}
