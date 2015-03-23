using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.Build;
using NAXB.Interfaces;
using NAXB.UnitTests.Mockups;
using NAXB.UnitTests.Mockups.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public void Test_Reflector_BuildDefaultConstructor_StringList()
        {
            Type expectedType = typeof(List<string>);

            var ctor = reflector.BuildDefaultConstructor(expectedType);
            var instance = ctor();

            Assert.IsInstanceOfType(instance, expectedType);
        }

        [TestMethod]
        public void Test_Reflector_BuildSetter_Person_FirstName()
        {
            string testValue = "Joe";
            var person = new Person();
            var firstName = typeof(Person).GetProperty("FirstName");

            var setter = reflector.BuildSetter(firstName);
            setter(person, testValue);

            Assert.AreEqual(testValue, person.FirstName);
        }

        [TestMethod]
        public void Test_Reflector_BuildGetter_Person_FirstName()
        {
            var expected =  "Josh";
            var person = new Person { FirstName = expected };

            var get = reflector.BuildGetter(person.GetType().GetProperty("FirstName"));
            var actual = get(person);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_Reflector_BuildGetField_Person_MiddleName()
        {
            var expected = "Samuel";
            var person = new Person { MiddleName = expected };

            var get = reflector.BuildGetField(person.GetType().GetField("MiddleName"));
            var actual = get(person);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_Reflector_BuildSetField_Person_MiddleName()
        {
            var expected = "Samuel";
            var person = new Person();

            var set = reflector.BuildSetField(person.GetType().GetField("MiddleName"));
            set(person, expected);

            Assert.AreEqual(expected, person.MiddleName);
        }

        [TestMethod]
        public void Test_Reflector_GetPropertyInfo_Person_FirstName()
        {
            var expected = typeof(Person).GetProperty("FirstName");

            var actual = reflector.GetPropertyInfo<Person, string>(person => person.FirstName);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_Reflector_BuildPopulateCollection_EmailList()
        {
            var emails = GetEmailEnumerable();
            var emailList = new List<Email>();

            var buildCollection = reflector.BuildPopulateCollection(emailList.GetType());
            buildCollection(emails, emailList);

            Assert.IsTrue(TestUtils.EnumerablesAreEqual(emails, emailList)); 
        }

        [TestMethod]
        public void Test_Reflector_BuildToArray_EmailArray()
        {
            var emails = GetEmailEnumerable();

            var toArray = reflector.BuildToArray(typeof(Email));
            var emailArr = toArray(emails);

            Assert.IsTrue(TestUtils.EnumerablesAreEqual(emails, (Email[])emailArr));
        }

        [TestMethod]
        public void Test_Reflector_IsGenericCollection_EmailList_True()
        {
            var emails = GetEmailEnumerable().ToList();

            Type generic;
            var result = reflector.IsGenericCollection(emails.GetType(), out generic);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Reflector_IsGenericCollection_EmailList_OutParam()
        {
            var emails = GetEmailEnumerable().ToList();

            Type generic;
            var result = reflector.IsGenericCollection(emails.GetType(), out generic);

            Assert.AreEqual(typeof(Email), generic);
        }
        [TestMethod]
        public void Test_Reflector_IsGenericCollection_EmailArr_False()
        {
            var emails = GetEmailEnumerable().ToArray();

            Type elementType;
            var result = reflector.IsGenericCollection(emails.GetType(), out elementType);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_Reflector_IsArray_EmailArr_OutParam()
        {
            var emails = GetEmailEnumerable().ToArray();

            Type elementType;
            var result = reflector.IsArray(emails.GetType(), out elementType);

            Assert.AreEqual(typeof(Email), elementType);
        }

        [TestMethod]
        public void Test_Reflector_IsArray_EmailArr_True()
        {
            var emails = GetEmailEnumerable().ToArray();

            Type elementType;
            var result = reflector.IsArray(emails.GetType(), out elementType);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Reflector_IsArray_EmailList_False()
        {
            var emails = GetEmailEnumerable().ToList();

            Type elementType;
            var result = reflector.IsArray(emails.GetType(), out elementType);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_Reflector_IsEnumerable_String_False()
        {
            var test = "This is a test";

            var result = reflector.IsEnumerable(test.GetType());

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Test_Reflector_IsEnumerable_EmailEnumerable_True()
        {
            var test = GetEmailEnumerable();

            var result = reflector.IsEnumerable(test.GetType());

            Assert.IsTrue(result);
        }
    }
}
