using NAXB.Benchmarks.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NAXB.Benchmarks
{
    public class VersusLinqToXml
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please enter temporary XML file path and number of repetitions.");
                return;
            }
            var naxb = new NAXBDeserializer { Name = "NAXB" };
            naxb.Load(typeof(FlattenedClimber));
            var linq = new LinqToXmlClimberDeserializer { Name = "Linq to XML" };
            linq.Load(typeof(FlattenedClimber));
            IDeserializer[] deserializers = new IDeserializer[] { naxb, linq };
            string fileName = args[0];
            int repetitions = int.Parse(args[1]);
            
            BenchmarkHelper.RunTests(deserializers, repetitions, fileName);
        }

    }
}
