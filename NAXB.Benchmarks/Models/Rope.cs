using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NET = System.Xml.Serialization;
using NAXB.Attributes;

namespace NAXB.Benchmarks.Models
{
    [Serializable]
    [XmlModelBinding]
    [NET.XmlRoot("Rope")]
    public class Rope
    {
        [XmlAttribute("TensileStrength")]
        [NET.XmlAttribute]
        public long TensileStrength;

        [XmlAttribute("FallRating")]
        [NET.XmlAttribute]
        public byte FallRating;

        [XmlAttribute("Length")]
        [NET.XmlAttribute]
        public int Length { get; set; }

        [XmlAttribute("Diameter")]
        [NET.XmlAttribute]
        public byte Diameter { get; set; }
    }
}
