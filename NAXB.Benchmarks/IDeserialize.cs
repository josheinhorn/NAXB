using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Benchmarks
{
    public interface IDeserializer
    {
        string Name { get; set; }
        void Load(Type type);
        object DeserializeXMLFile(string fileName);
        long ElapsedMilliseconds { get; set; }
    }
}
