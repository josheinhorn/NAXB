using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Build;
using System.Collections;
using System.Diagnostics;
namespace PopulateCollectionBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            int iter = int.Parse(args[0]);
            int listLength = int.Parse(args[1]);
            var builtDel = new Reflector().BuildPopulateCollection(typeof(List<string>));

            Action<IEnumerable, object> del = (IEnumerable items, object collection) =>
                {
                    foreach (var item in items)
                    {
                        ((List<string>)collection).Add((string)item);
                    }
                };
            string[] strings = new string[listLength];
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = new Guid().ToString();
            }
            var coll = new List<string>();
            sw.Start();

            for (int i = 0; i < iter; i++)
            {
                PopulateCollection(strings, coll);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using static method: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            coll = new List<string>();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                del(strings, coll);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using delegate: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            coll = new List<string>();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                builtDel(strings, coll);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using compiled Delegate: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            coll = new List<string>();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                PopulateCollectionNoCast(strings, coll);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using No casts : {1} ms", iter, sw.ElapsedMilliseconds);
            Console.ReadLine(); //Stop so we can read
        }

        private static void PopulateCollection(IEnumerable items, object collection)
        {
            foreach (var item in items)
            {
                ((List<string>)collection).Add((string)item);
            }
        }

        private static void PopulateCollectionNoCast(string[] strings, List<string> list)
        {
            foreach (var str in strings)
            {
                list.Add(str);
            }
        }
    }
}
