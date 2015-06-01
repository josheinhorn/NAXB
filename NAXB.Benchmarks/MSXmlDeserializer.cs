using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace NAXB.Benchmarks
{
    public class MSXmlDeserializer : IDeserializer
    {
        private XmlSerializer serializer;

        public void Load(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            serializer = new XmlSerializer(type);
        }

        public object DeserializeXMLFile(string fileName)
        {
            object result = null;
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                result = serializer.Deserialize(fs);
            };
            return result;
        }
        public string Name
        {
            get;
            set;
        }
        public long ElapsedMilliseconds { get; set; }
    }
}
