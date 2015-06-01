using NAXB.Build;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PopulateDictionaryBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            int iter = int.Parse(args[0]);
            int listLength = int.Parse(args[1]);
            var builtDel = new Reflector().BuildPopulateDictionary(typeof(Dictionary<string,string>));


            Action<IEnumerable<KeyValuePair<object, object>>, object> del = (IEnumerable<KeyValuePair<object, object>> kvps, object dict) =>
            {
                foreach (var kvp in kvps)
                {
                    if (!((Dictionary<string,string>)dict).ContainsKey((string)kvp.Key))
                    ((Dictionary<string, string>)dict).Add((string)kvp.Key, (string)kvp.Value);
                }
            };
            KeyValuePair<object, object>[] objectKvps = new KeyValuePair<object, object>[listLength];
            KeyValuePair<string, string>[] stringKvps = new KeyValuePair<string, string>[listLength];

            Stopwatch sw = new Stopwatch();
            Random r = new Random();
            for (int i = 0; i < objectKvps.Length; i++)
            {
                var k = r.Next(listLength).ToString();
                var v = r.Next(listLength).ToString();

                objectKvps[i] = new KeyValuePair<object, object>(k, v);
                stringKvps[i] = new KeyValuePair<string, string>(k, v);
            }
            var dictionary = new Dictionary<string,string>();
            sw.Start();

            for (int i = 0; i < iter; i++)
            {
                PopulateDictionary(objectKvps, dictionary);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using static method: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            dictionary = new Dictionary<string,string>();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                del(objectKvps, dictionary);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using delegate: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            dictionary = new Dictionary<string,string>();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                builtDel(objectKvps, dictionary);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using compiled Delegate: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            dictionary = new Dictionary<string,string>();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                PopulateDictNoCast(stringKvps, dictionary);
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using No casts : {1} ms", iter, sw.ElapsedMilliseconds);
        }

        private static void PopulateDictionary(IEnumerable<KeyValuePair<object,object>> kvps, object dict)
        {
            foreach (var kvp in kvps)
            {
                if (!((Dictionary<string, string>)dict).ContainsKey((string)kvp.Key))
                ((Dictionary<string, string>)dict).Add((string)kvp.Key, (string)kvp.Value);
            }
        }

        private static void PopulateDictNoCast(IEnumerable<KeyValuePair<string,string>> kvps, Dictionary<string,string> dict)
        {
            foreach (var kvp in kvps)
            {
                if (!dict.ContainsKey(kvp.Key))
                (dict).Add(kvp.Key, kvp.Value);
            }
        }
    }
}
