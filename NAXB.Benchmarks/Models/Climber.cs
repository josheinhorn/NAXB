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
    [NET.XmlRoot("Climber")]
    public class Climber
    {
        [XmlAttribute("Name")]
        [NET.XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute("DateOfBirth")]
        [NET.XmlAttribute]
        public DateTime DateOfBirth { get; set; }

        [XmlAttribute("Height")]
        [NET.XmlAttribute]
        public int Height { get; set; }

        [XmlAttribute("ApeIndex")]
        [NET.XmlAttribute]
        public byte ApeIndex;

        [XmlElement("Gear")]
        [NET.XmlElement]
        public Equipment Gear { get; set; }

        [XPath("Belayer")]
        [NET.XmlElement]
        public Climber Belayer { get; set; }

    }
}
