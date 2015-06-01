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
    [NET.XmlRoot("Quickdraw")]
    public class Quickdraw
    {
        [XmlElement("Carabiners")]
        [NET.XmlElement]
        public Carabiner[] Carabiners;

        [XmlAttribute("WeightCapacity")]
        [NET.XmlAttribute]
        public int WeightCapacity;

        [XmlAttribute("FallRating")]
        [NET.XmlAttribute]
        public int FallRating;
    }
}
