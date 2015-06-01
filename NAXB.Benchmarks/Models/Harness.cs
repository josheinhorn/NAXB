using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NET = System.Xml.Serialization;
using NAXB.Attributes;

namespace NAXB.Benchmarks.Models
{
    [Serializable]
    [XmlModelBinding()]
    [NET.XmlRoot("Harness")]
    public class Harness
    {
        [XmlAttribute("TensileStrength")]
        [NET.XmlAttribute]
        public int WeightCapacity;

        [XmlAttribute("TensileStrength")]
        [NET.XmlAttribute]
        public string Manufacturer { get; set; }

        [XmlAttribute("TensileStrength")]
        [NET.XmlAttribute]
        public string Size { get; set; }
    }
}
