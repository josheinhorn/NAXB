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
    [NET.XmlRoot("Equipment")]
    public class Equipment
    {
        [XmlElement("Shoes")]
        [NET.XmlElement]
        public List<Shoes> Shoes { get; set; }

        [XmlElement("Ropes")]
        [NET.XmlElement]
        public List<Rope> Ropes { get; set; }

        [XmlElement("Quickdraws")]
        [NET.XmlElement]
        public List<Quickdraw> Quickdraws { get; set; }

        [XmlElement("Carabiners")]
        [NET.XmlElement]
        public List<Carabiner> Carabiners { get; set; }

        [XmlElement("Harnesses")]
        [NET.XmlElement]
        public List<Harness> Harnesses { get; set; }
    }
}
