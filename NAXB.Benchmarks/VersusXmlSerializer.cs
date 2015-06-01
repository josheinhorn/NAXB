using NAXB.Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Ploeh.AutoFixture;
using System.Timers;
using System.Diagnostics;

namespace NAXB.Benchmarks
{
    class VersusXmlSerializer
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please enter temporary XML file path and number of repetitions.");
                return;
            }
            string fileName = args[0];
            int repetitions = int.Parse(args[1]);
            

            
            IDeserializer naxb = new NAXBDeserializer { Name = "NAXB" };
            IDeserializer ms = new MSXmlDeserializer { Name = "MS XML Serializer" };
            naxb.Load(typeof(Climber));
            ms.Load(typeof(Climber));
            IDeserializer[] deserializers = new IDeserializer[] { naxb, ms };
            //TODO: add more deserializers
            BenchmarkHelper.RunTests(deserializers, repetitions, fileName);

            Stopwatch timer = new Stopwatch();
            naxb.Load(typeof(FlattenedClimber));
            timer.Reset();
            timer.Start();
            for (int i = 0; i < repetitions; i++)
            {
                var n = naxb.DeserializeXMLFile(fileName);
                //var a = ((FlattenedClimber)n).BelayerName;
            }
            timer.Stop();
            naxb.ElapsedMilliseconds = timer.ElapsedMilliseconds;

            timer.Reset();
            timer.Start();
            for (int i = 0; i < repetitions; i++)
            {
                var n = ms.DeserializeXMLFile(fileName);
                var f = FlattenClimber((Climber)n);
                //var a = ((FlattenedClimber)n).BelayerName;
            }
            timer.Stop();
            ms.ElapsedMilliseconds = timer.ElapsedMilliseconds;

            Console.WriteLine("Using NAXB specialized Flattened model:");
            foreach (var deserializer in deserializers)
            {
                Console.WriteLine("Total Elapsed time for {0} repetitions using {1}: {2} ms", repetitions, deserializer.Name, deserializer.ElapsedMilliseconds);
                Console.WriteLine("Average time for each repetition using {0}: {1} ms", deserializer.Name, ((double)deserializer.ElapsedMilliseconds) / repetitions);
            }
        }

        private static FlattenedClimber FlattenClimber(Climber climber)
        {
            FlattenedClimber result = new FlattenedClimber();
            result.BelayerName = climber.Belayer.Name;
            result.ApeIndex = climber.ApeIndex;
            result.DateOfBirth = climber.DateOfBirth;
            result.QuickdrawCarabiners = new List<Carabiner>();
            foreach (var qd in climber.Gear.Quickdraws)
            {
                result.QuickdrawCarabiners.AddRange(qd.Carabiners.Where(x => x.CarabinerType == Carabiner.Type.Quickdraw));
            }
            result.HarnessCapacities = climber.Gear.Harnesses.Select(x => x.WeightCapacity).ToList();
            result.Height = climber.Height;
            result.Name = climber.Name;
            result.GatedCarabiners = climber.Gear.Carabiners.Where(x => x.CarabinerType == Carabiner.Type.Gated).ToList();
            result.RopeCount = climber.Gear.Ropes.Count;
            result.RopeStrengthByFallRating = climber.Gear.Ropes.ToDictionary(x => x.FallRating, x => x.TensileStrength);
            result.ShoeSizes = climber.Gear.Shoes.Select(x => x.Size).ToList();
            result.ShoeStrengthByManufacturer = climber.Gear.Shoes.ToDictionary(x => x.Manufacturer, x => x.RubberStrength);
            return result;
        }

      
    }
}
