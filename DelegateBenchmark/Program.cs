using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Build;
using NAXB.Interfaces;
using System.Diagnostics;
using System.Globalization;

namespace ParsingDelegateBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            int iter = int.Parse(args[0]);
            var type = typeof(short);
            PropertyType temp;
            IReflector reflector = new Reflector();
            var builtParser = reflector.BuildSingleParser(type, null, out temp);
            Func<string, object> delegateParser = (string value) =>
                {
                    short s;
                    return short.TryParse(value, out s) ? s : default(short);
                };

            Stopwatch sw = new Stopwatch();


            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                short a = (short)Parseshort(i.ToString());
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using static Parser: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                short a = (short)delegateParser(i.ToString());
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using delegate Parser: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                short a = (short)builtParser(i.ToString());
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using Built Parser: {1} ms", iter, sw.ElapsedMilliseconds);

        }

        static object Parseshort(string value)
        {
            short s = 0;
            return short.TryParse(value, out s) ? s : default(short);
        }
    }
}
