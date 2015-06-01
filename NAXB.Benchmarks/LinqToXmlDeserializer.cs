using NAXB.Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NAXB.Benchmarks
{
    public class LinqToXmlClimberDeserializer : IDeserializer
    {
        public string Name
        {
            get;
            set;
        }

        public void Load(Type type)
        {
            //
        }

        public object DeserializeXMLFile(string fileName)
        {
            return GetClimberWithLinq(fileName);
        }

        public long ElapsedMilliseconds
        {
            get;
            set;
        }

        private FlattenedClimber GetClimberWithLinq(string fileName)
        {
            XDocument doc = XDocument.Load(fileName);
            FlattenedClimber result = new FlattenedClimber();
            byte tempByte;
            result.ApeIndex = byte.TryParse(doc.Element("Climber").Attribute("ApeIndex").Value, out tempByte) ? tempByte : default(byte);
            result.BelayerName = doc.Element("Climber").Element("Belayer").Attribute("Name").Value;
            DateTime date;
            result.DateOfBirth = DateTime.TryParse(doc.Element("Climber").Attribute("DateOfBirth").Value, out date) ? date : default(DateTime);
            Carabiner.Type ctype;
            int tempInt;
            long tempLong;
            result.GatedCarabiners = doc.Element("Climber").Element("Gear").Elements("Carabiners").Where(x => x.Attribute("CarabinerType").Value == "Gated")
                .Select(x => new Carabiner
                {
                    CarabinerType = Enum.TryParse<Carabiner.Type>(x.Attribute("CarabinerType").Value, out ctype)
                    ? ctype : Carabiner.Type.Gated,
                    Height = int.TryParse(x.Attribute("Height").Value, out tempInt) ? tempInt : default(int),
                    Length = int.TryParse(x.Attribute("Length").Value, out tempInt) ? tempInt : default(int),
                    TensileStrength = long.TryParse(x.Attribute("Length").Value, out tempLong) ? tempLong : default(long),
                    Width = int.TryParse(x.Attribute("Width").Value, out tempInt) ? tempInt : default(int)
                }).ToList();
            result.HarnessCapacities = doc.Element("Climber").Element("Gear").Elements("Harnesses")
                .Select(x => int.TryParse(x.Attribute("WeightCapacity").Value, out tempInt)
                    ? tempInt : default(int)).ToList();
            result.Height = int.TryParse(doc.Element("Climber").Attribute("Height").Value, out tempInt) ?
                tempInt : default(int);
            result.Name = doc.Element("Climber").Attribute("Name").Value;
            result.QuickdrawCarabiners = doc.Element("Climber").Element("Gear").Elements("Quickdraws").Elements("Carabiners")
                .Where(x => x.Attribute("CarabinerType").Value == "Quickdraw")
                .Select(x => new Carabiner
                {
                    CarabinerType = Enum.TryParse<Carabiner.Type>(x.Attribute("CarabinerType").Value, out ctype)
                    ? ctype : Carabiner.Type.Gated,
                    Height = int.TryParse(x.Attribute("Height").Value, out tempInt) ? tempInt : default(int),
                    Length = int.TryParse(x.Attribute("Length").Value, out tempInt) ? tempInt : default(int),
                    TensileStrength = long.TryParse(x.Attribute("Length").Value, out tempLong) ? tempLong : default(long),
                    Width = int.TryParse(x.Attribute("Width").Value, out tempInt) ? tempInt : default(int)
                }).ToList();
            result.RopeCount = doc.Element("Climber").Element("Gear").Elements("Ropes").Count();
            result.RopeStrengthByFallRating = doc.Element("Climber").Element("Gear").Elements("Ropes")
                .ToDictionary(x => byte.TryParse(x.Attribute("FallRating").Value, out tempByte) ? tempByte : default(byte),
                x => long.TryParse(x.Attribute("TensileStrength").Value, out tempLong) ? tempLong : default(long));
            result.ShoeSizes = doc.Element("Climber").Element("Gear").Elements("Shoes").Where(x => byte.Parse(x.Attribute("Size").Value) < 100)
                .Select(x => byte.TryParse(x.Attribute("Size").Value, out tempByte) ? tempByte : default(byte))
                .ToList();
            result.ShoeStrengthByManufacturer = doc.Element("Climber").Element("Gear").Elements("Shoes")
                .ToDictionary(x => x.Attribute("Manufacturer").Value,
                x => long.TryParse(x.Attribute("RubberStrength").Value, out tempLong) ? tempLong : default(long));
            return result;
        }
    }
}
