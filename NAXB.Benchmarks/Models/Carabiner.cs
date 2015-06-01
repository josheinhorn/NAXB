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
    [NET.XmlRoot("Carabiner")]
    public class Carabiner
    {
        [XmlAttribute("TensileStrength")]
        [NET.XmlAttribute]
        public long TensileStrength { get; set; }

        [XmlAttribute("CarabinerType")]
        [NET.XmlAttribute]
        public Type CarabinerType;

        [XmlAttribute("Height")]
        [NET.XmlAttribute]
        public int Height;

        [XmlAttribute("Length")]
        [NET.XmlAttribute]
        public int Length;

        [XmlAttribute("Width")]
        [NET.XmlAttribute]
        public int Width;

        public enum Type
        {
            Quickdraw, Gated
        }
    }


}
