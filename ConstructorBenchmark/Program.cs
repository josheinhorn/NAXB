using NAXB.Build;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ConstructorBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            int iter = int.Parse(args[0]);
            Func<object> del = () => new StringBuilder();
            var compiledDel = new Reflector().BuildDefaultConstructor(typeof(StringBuilder));

            Stopwatch sw = new Stopwatch();
            object a = null;
            sw.Start();

            for (int i = 0; i < iter; i++)
            {
                a = CreateParameterlessInstance();
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using static method: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                a = del();
            }
            sw.Stop();
            Console.WriteLine("Time for {0} iterations using delegate: {1} ms", iter, sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                a = compiledDel();
            }
            sw.Stop();
            a.GetHashCode();
            Console.WriteLine("Time for {0} iterations using compiled Delegate: {1} ms", iter, sw.ElapsedMilliseconds);

            //sw.Reset();
            //sw.Start();
            //for (int i = 0; i < iter; i++)
            //{
                
            //}
            //sw.Stop();
            //Console.WriteLine("Time for {0} iterations using No casts : {1} ms", iter, sw.ElapsedMilliseconds);
            Console.ReadLine(); //Stop so we can read
        }

        public static object CreateParameterlessInstance()
        {
            return new StringBuilder();
        }
        
    }
}
