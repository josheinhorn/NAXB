using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.VtdXml
{
    public class VtdXmlFactory : IXmlFactory
    {
        public IXmlData CreateXmlData(string xml, Encoding encoding, INamespace[] namespaces = null)
        {
            var bytes = encoding.GetBytes(xml);
            return new VtdXmlData(bytes);
        }
        public IXmlData CreateXmlData(byte[] byteArray, Encoding encoding, INamespace[] namespaces = null)
        {
            return new VtdXmlData(byteArray);
        }
        public IXmlData CreateXmlData(System.IO.Stream xmlStream, INamespace[] namespaces = null)
        {
            return new VtdXmlData(GetByteArrayFromStream(xmlStream));
        }
        public IXmlData CreateXmlData(string fileName, INamespace[] namespaces = null)
        {
            IXmlData result = null;
            using (var xmlStream = new FileStream(fileName, FileMode.Open))
            {
                result = new VtdXmlData(GetByteArrayFromStream(xmlStream));
            }
            return result;
        }

        protected byte[] GetByteArrayFromStream(Stream stream)
        {
            var byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);
            return byteArray;
        }
    }
}
