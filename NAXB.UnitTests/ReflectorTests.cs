using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.Build;
using NAXB.Interfaces;
using NAXB.UnitTests.Mockups;
using NAXB.UnitTests.Mockups.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Web;

namespace NAXB.UnitTests
{
    [TestClass]
    public class ReflectorUnitTestSuite
    {
        IReflector reflector = null;
        public ReflectorUnitTestSuite()
        {
            reflector = new Reflector();
        }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            //anything to initialize?
        }

        protected IEnumerable<Email> GetEmailEnumerable()
        {
            return new List<Email> {
                new Email 
                {
                    Name="josh",
                    Domain = "josheinhorn.com"
                },
                new Email
                {
                    Name="josh.einhorn",
                    Domain = "gmail.com"
                }
            };
        }


        [TestMethod]
        public void Test_BuildDefaultConstructor_StringList()
        {
            Type expectedType = typeof(List<string>);

            var ctor = reflector.BuildDefaultConstructor(expectedType);
            var instance = ctor();

            Assert.IsInstanceOfType(instance, expectedType);
        }

        [TestMethod]
        public void Test_BuildConstructor_StringFromCharArray()
        {
            Type expectedType = typeof(String);
            var ctorInfo = expectedType.GetConstructor(new Type[] { typeof(char[]) });

            var ctor = reflector.BuildConstructor(ctorInfo);
            var instance = ctor(new object[] { new char[] { 'a', 'b', 'c' } });

            Assert.IsInstanceOfType(instance, expectedType);
        }

        [TestMethod]
        public void Test_BuildConstructor_BigIntegerFromDecimal()
        {
            Type expectedType = typeof(BigInteger);
            var ctorInfo = expectedType.GetConstructor(new Type[] { typeof(decimal) });

            decimal arg = 2;
            var ctor = reflector.BuildConstructor(ctorInfo);
            var instance = ctor(new object[] { arg });

            Assert.AreEqual(new BigInteger(arg), instance);
        }

        [TestMethod]
        public void Test_BuildSetter_Person_FirstName()
        {
            string testValue = "Joe";
            var person = new Person();
            var firstName = typeof(Person).GetProperty("FirstName");

            var setter = reflector.BuildSetter(firstName);
            setter(person, testValue);

            Assert.AreEqual(testValue, person.FirstName);
        }

        [TestMethod]
        public void Test_BuildGetter_Person_FirstName()
        {
            var expected = "Josh";
            var person = new Person { FirstName = expected };

            var get = reflector.BuildGetter(person.GetType().GetProperty("FirstName"));
            var actual = get(person);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_BuildGetField_Person_MiddleName()
        {
            var expected = "Samuel";
            var person = new Person { MiddleName = expected };

            var get = reflector.BuildGetField(person.GetType().GetField("MiddleName"));
            var actual = get(person);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_BuildSetField_Person_MiddleName()
        {
            var expected = "Samuel";
            var person = new Person();

            var set = reflector.BuildSetField(person.GetType().GetField("MiddleName"));
            set(person, expected);

            Assert.AreEqual(expected, person.MiddleName);
        }

        [TestMethod]
        public void Test_BuildSetField_Person_DateOfBirth()
        {
            var expected = DateTime.Now;
            var person = new Person();

            var set = reflector.BuildSetField(person.GetType().GetField("DateOfBirth"));
            set(person, expected);

            Assert.AreEqual(expected, person.DateOfBirth);
        }

        [TestMethod]
        public void Test_GetPropertyInfo_Person_FirstName()
        {
            var expected = typeof(Person).GetProperty("FirstName");

            var actual = reflector.GetPropertyOrFieldInfo<Person, string>(person => person.FirstName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_BuildPopulateCollection_EmailList()
        {
            var emails = GetEmailEnumerable();
            var emailList = new List<Email>();

            var buildCollection = reflector.BuildPopulateCollection(emailList.GetType());
            buildCollection(emails, emailList);

            Assert.IsTrue(TestUtils.EnumerablesAreEqual(emails, emailList));
        }

        [TestMethod]
        public void Test_BuildToArray_EmailArray()
        {
            var emails = GetEmailEnumerable();

            var toArray = reflector.BuildToArray(typeof(Email));
            var emailArr = toArray(emails);

            Assert.IsTrue(TestUtils.EnumerablesAreEqual(emails, (Email[])emailArr));
        }

        [TestMethod]
        public void Test_IsGenericCollection_EmailList_True()
        {
            var emails = GetEmailEnumerable().ToList();

            Type generic;
            var result = reflector.IsGenericCollection(emails.GetType(), out generic);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_IsGenericCollection_EmailList_OutParam()
        {
            var emails = GetEmailEnumerable().ToList();

            Type generic;
            var result = reflector.IsGenericCollection(emails.GetType(), out generic);

            Assert.AreEqual(typeof(Email), generic);
        }
        [TestMethod]
        public void Test_IsGenericCollection_EmailArr_False()
        {
            var emails = GetEmailEnumerable().ToArray();

            Type elementType;
            var result = reflector.IsGenericCollection(emails.GetType(), out elementType);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_IsArray_EmailArr_OutParam()
        {
            var emails = GetEmailEnumerable().ToArray();

            Type elementType;
            var result = reflector.IsArray(emails.GetType(), out elementType);

            Assert.AreEqual(typeof(Email), elementType);
        }

        [TestMethod]
        public void Test_IsArray_EmailArr_True()
        {
            var emails = GetEmailEnumerable().ToArray();

            Type elementType;
            var result = reflector.IsArray(emails.GetType(), out elementType);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_IsArray_EmailList_False()
        {
            var emails = GetEmailEnumerable().ToList();

            Type elementType;
            var result = reflector.IsArray(emails.GetType(), out elementType);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_IsEnumerable_String_False()
        {
            var test = "This is a test";

            var result = reflector.IsEnumerable(test.GetType());

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Test_IsEnumerable_EmailEnumerable_True()
        {
            var test = GetEmailEnumerable();

            var result = reflector.IsEnumerable(test.GetType());

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_IsNullableType_NullableDateTime_True()
        {
            var test = typeof(DateTime?);
            Type temp;
            var result = reflector.IsNullableType(test, out temp);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_IsNullableType_NullableDateTime_Out_DateTime()
        {
            var test = typeof(DateTime?);
            Type actual;
            var result = reflector.IsNullableType(test, out actual);

            Assert.AreEqual(typeof(DateTime), actual);
        }

        [TestMethod]
        public void Test_IsNullableType_Int_False()
        {
            var test = typeof(int);
            Type actual;
            var result = reflector.IsNullableType(test, out actual);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_BuildSingleParser_Int()
        {
            PropertyType temp;
            var parser = reflector.BuildSingleParser(typeof(int), null, out temp);

            Assert.AreEqual((int)2, parser("2"));
        }

        [TestMethod]
        public void Test_BuildSingleParser_Double()
        {
            PropertyType temp;
            var parser = reflector.BuildSingleParser(typeof(double), null, out temp);

            Assert.AreEqual((double)2, parser("2"));
        }

        [TestMethod]
        public void Test_BuildSingleParser_HtmlString()
        {
            PropertyType temp;
            var parser = reflector.BuildSingleParser(typeof(HtmlString), null, out temp);

            Assert.IsInstanceOfType(parser("2"), typeof(HtmlString));
        }

        [TestMethod]
        public void Test_BuildSingleParser_DateTime()
        {
            PropertyType temp;
            var parser = reflector.BuildSingleParser(typeof(DateTime), null, out temp);
            
            var result = parser("02-02-2002");
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void Test_BuildMultiParser_Tuple_Decimal_Byte_String()
        {
            var expected = typeof(Tuple<decimal, byte, string>);
            PropertyType[] temp;
            var parser = reflector.BuildMultiParser(expected, null, 3, out temp);

            var result = parser(new string[] { "2", "2", "Hello world!" });

            Assert.IsInstanceOfType(result, expected);
        }

        [TestMethod]
        public void Test_IsGenericDictionary_Dictionary_Int_String()
        {
            var expected = typeof(Dictionary<int, string>);
            Type v;
            Type k;
            bool result = reflector.IsGenericDictionary(expected, out k, out v);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PopulateDictionary_Dictionary_Int_String()
        {
            var expected = typeof(Dictionary<int, string>);

            var populate = reflector.BuildPopulateDictionary(expected);

            var kvps = new List<KeyValuePair<object, object>>
            {
                new KeyValuePair<object,object>((int)2, "2"),
                new KeyValuePair<object,object>((int)3, "3")
            };
            var dict = new Dictionary<int, string>();
            populate(kvps, dict);

            Assert.AreEqual(2, dict.Count);
        }
    }
}
