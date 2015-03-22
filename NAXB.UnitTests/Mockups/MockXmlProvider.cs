﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NAXB.UnitTests.Mockups
{
    public class MockXmlProvider
    {
        public MockXmlProvider(string fileName, Encoding encoding)
        {
            FileName = fileName;
            Encoding = encoding;
            using (var fs = new FileStream(FileName, FileMode.Open))
            {
                XmlAsByteArray = new byte[fs.Length];
                fs.Read(XmlAsByteArray, 0, (int)fs.Length);
            }
            XmlAsString = Encoding.GetString(XmlAsByteArray);

            //Is this even necessary?
            var doc = new XmlDocument();
            doc.Load(FileName);
            ExpectedValue = doc.Value; //Better way to get this? maybe don't even need it?
        }

        public string ExpectedValue
        {
            get;
            private set;
        }

        public string FileName { get; private set; }

        public Encoding Encoding
        {
            get;
            private set;
        }

        public string XmlAsString
        {
            get;
            private set;
        }

        public Stream XmlAsStream
        {
            get
            {
                return new MemoryStream(XmlAsByteArray);
            }
        }

        public byte[] XmlAsByteArray
        {
            get;
            private set;
        }

    }
}
