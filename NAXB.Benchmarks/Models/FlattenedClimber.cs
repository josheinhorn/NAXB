using NAXB.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.Benchmarks.Models
{
    [XmlModelBinding]
    public class FlattenedClimber
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [XmlAttribute("Height")]
        public int Height { get; set; }

        [XmlAttribute("ApeIndex")]
        public byte ApeIndex;

        [XPath("Gear/Shoes[@Size < 100]/@Size")]
        public List<byte> ShoeSizes;

        [XPath("Gear/Shoes", "@Manufacturer", "@RubberStrength")]
        public Dictionary<string, long> ShoeStrengthByManufacturer;

        [XPath("Gear/Ropes", "@FallRating", "@TensileStrength")]
        public Dictionary<byte, long> RopeStrengthByFallRating { get; set; }

        [XPath("count(Gear/Ropes)")]
        public int RopeCount;

        [XPath("Gear/Quickdraws/Carabiners[@CarabinerType='Quickdraw']")]
        public List<Carabiner> QuickdrawCarabiners;

        [XPath("Gear/Carabiners[@CarabinerType='Gated']")]
        public List<Carabiner> GatedCarabiners;

        [XPath("Belayer/@Name")]
        public string BelayerName;

        [XPath("Gear/Harnesses/@WeightCapacity")]
        public List<int> HarnessCapacities;
    }
}
