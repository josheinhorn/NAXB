using NAXB.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetSetBenchmarks
{


    class Program
    {
        static void Main(string[] args)
        {
            int iter = int.Parse(args[0]);
            var type = typeof(TestClass);
            var reflector = new Reflector();
            var dels = new List<KeyValuePair<string, Action<object,object>>>
            {
                new KeyValuePair<string, Action<object,object>>("Get String Property", reflector.BuildSetter(type.GetProperty("StringProp"))),
            var delSetStringField = reflector.BuildSetField(type.GetField("StringField"));
            var delSetIntProp = reflector.BuildSetter(type.GetProperty("IntProp"));
            var delSetIntField = reflector.BuildSetField(type.GetField("IntField"));
            var delSetObjectProp = reflector.BuildSetter(type.GetProperty("ObjectProp"));
            var delSetObjectField = reflector.BuildSetField(type.GetField("ObjectField"));
        };

        }

        private static void SetStringProp(TestClass model, string value)
        {
            model.StringProp = value;
        }
        private static void SetObjectProp(TestClass model, TestClass value)
        {
            model.ObjectProp = value;
        }
        private static void SetStringField(TestClass model, string value)
        {
            model.StringField = value;
        }
        private static void SetObjectField(TestClass model, TestClass value)
        {
            model.ObjectField = value;
        }

        private static void SetIntProp(TestClass model, int value)
        {
            model.IntProp = value;
        }
        private static void SetIntField(TestClass model, int value)
        {
            model.IntField = value;
        }
        //private static string GetStringProp(TestClass model)
        //{
        //    return model.StringProp;
        //}
        //private static TestClass GetObjectProp(TestClass model)
        //{
        //    return model.ObjectProp;
        //}

        public class TestClass
        {
            public string StringProp { get; set; }
            public string StringField;
            public TestClass ObjectProp { get; set; }
            public TestClass ObjectField;
            public int IntProp { get; set; }
            public int IntField;
        }
    }
}
