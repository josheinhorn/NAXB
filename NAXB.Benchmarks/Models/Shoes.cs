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
    [NET.XmlRoot("Shoes")]
    public class Shoes
    {
        [XmlAttribute("ToeAngle")]
        [NET.XmlAttribute]
        public int ToeAngle;

        [XmlAttribute("RubberStrength")]
        [NET.XmlAttribute]
        public long RubberStrength { get; set; }

        [XmlAttribute("Manufacturer")]
        [NET.XmlAttribute]
        public string Manufacturer;

        [XmlAttribute("Size")]
        [NET.XmlAttribute]
        public byte Size { get; set; }

        public enum Type
        {
            Bouldering, Hybrid, Sport
        }
    }
}
