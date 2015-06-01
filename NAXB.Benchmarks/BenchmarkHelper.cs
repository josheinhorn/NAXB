using NAXB.Benchmarks.Models;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NAXB.Benchmarks
{
    public static class BenchmarkHelper
    {
        private static void CreateTestXml(string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Climber));
            var file = new FileInfo(fileName);
            if (file.Exists) file.Delete();
            using (var fs = new FileStream(fileName, FileMode.CreateNew))
            {
                xs.Serialize(fs, GetTestData()); //Create our test XML file
            }
        }
        public static void RunTests(IDeserializer[] deserializers, int repetitions, string fileName)
        {
            CreateTestXml(fileName);
            Stopwatch timer = new Stopwatch();

            foreach (var deserializer in deserializers)
            {
                timer.Reset();
                timer.Start();
                object n;
                for (int i = 0; i < repetitions; i++)
                {
                    n = deserializer.DeserializeXMLFile(fileName);
                    var a = ((FlattenedClimber)n).ApeIndex;
                }
                timer.Stop();
                deserializer.ElapsedMilliseconds = timer.ElapsedMilliseconds;
            }

            foreach (var deserializer in deserializers)
            {
                Console.WriteLine("Total Elapsed time for {0} repetitions using {1}: {2} ms", repetitions, deserializer.Name, deserializer.ElapsedMilliseconds);
                Console.WriteLine("Average time for each repetition using {0}: {1} ms", deserializer.Name, ((double)deserializer.ElapsedMilliseconds) / repetitions);
            }
        }
        private static Climber GetTestData()
        {
            var fixture = new Fixture();
            var gear = fixture.Build<Equipment>()
                .With(g => g.Carabiners, fixture.CreateMany<Carabiner>(20).ToList())
                .With(g => g.Harnesses, fixture.CreateMany<Harness>(20).ToList())
                .With(g => g.Quickdraws, fixture.CreateMany<Quickdraw>(20).ToList())
                .With(g => g.Ropes, fixture.CreateMany<Rope>(20).ToList())
                .With(g => g.Shoes, fixture.CreateMany<Shoes>(20).ToList())
                .Create();
            var bGear = fixture.Build<Equipment>()
                .With(g => g.Carabiners, fixture.CreateMany<Carabiner>(20).ToList())
                .With(g => g.Harnesses, fixture.CreateMany<Harness>(20).ToList())
                .With(g => g.Quickdraws, fixture.CreateMany<Quickdraw>(20).ToList())
                .With(g => g.Ropes, fixture.CreateMany<Rope>(20).ToList())
                .With(g => g.Shoes, fixture.CreateMany<Shoes>(20).ToList())
                .Create();
            var climber = fixture.Build<Climber>()
                .WithAutoProperties()
                .With(c => c.Gear, gear)
                .Without(c => c.Belayer)
                .Create();
            var belayer = fixture.Build<Climber>()
                .WithAutoProperties()
                .Without(c => c.Belayer)
                .With(c => c.Gear, bGear)
                .Create();
            climber.Belayer = belayer;
            return climber;
        }

    }
}
