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

        public VtdXmlData(BookMark bookMark)
        {
            this.nav = bookMark.Nav;
            this.VTDBookMark = bookMark;
        }
        public VtdXmlData(byte[] xml)
        {
            VTDGen gen = new VTDGen();
            gen.setDoc(xml); //is this the right method? what about the Encoding?
            gen.parse(true);
            nav = gen.getNav();
            byteArray = xml;
            VTDBookMark = new BookMark(nav);
            VTDBookMark.recordCursorPosition(); //Should be equivalent to .getCurrentIndex()
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
                    if (VTDBookMark.setCursorPosition())
                    {
                        var elementToken = nav.getElementFragment();
                        int length = (int)elementToken >> 32;
                        int offset = (int)elementToken;
                        nav.getXML().getBytes(offset, length);
                    }
                    else byteArray = new byte[0];
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
                    if (VTDBookMark.setCursorPosition())
                        xmlString = nav.toString(nav.getCurrentIndex());
                    else xmlString = string.Empty;
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
                    if (VTDBookMark.setCursorPosition())
                    {
                        var innerTextToken = nav.getContentFragment(); //TODO: How to deal with Attributes? This method seems to be for elements?
                        int length = (int)innerTextToken >> 32;
                        int offset = (int)innerTextToken;
                        value = nav.toString(offset, length); //or .toRawString ??
                    }
                    else value = String.Empty;
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
