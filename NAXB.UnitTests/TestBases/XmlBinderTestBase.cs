using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Build;
using NAXB.Interfaces;
using NAXB.UnitTests.Mockups.Models;
using NAXB.Exceptions;
using System.Numerics;

namespace NAXB.UnitTests
{
    [TestClass]
    public abstract class XmlBinderTestBase
    {
        protected abstract IXmlModelBinder Binder { get; }        
        protected abstract IXmlData PersonXmlData { get; }

        [TestMethod]
        public void Test_BindToModel_TypeAndData()
        {
            var type = typeof(Person);

            var model = Binder.BindToModel(type, PersonXmlData);
            
            Assert.IsInstanceOfType(model, type);
        }

        [TestMethod]
        [ExpectedException(typeof(BindingNotFoundException))]
        public void Test_BindToModel_Throw_BindingNotFoundException()
        {
            var type = typeof(XmlBinderTestBase);

            var model = Binder.BindToModel(type, PersonXmlData);
        }


        [TestMethod]
        public void Test_SetPropertyValue_DateOfBirth()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.DateOfBirth);

            Assert.IsNotNull(person.DateOfBirth);
        }

        [TestMethod]
        public void Test_SetPropertyValue_MiddleName()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.MiddleName);

            Assert.AreEqual("Samuel", person.MiddleName);
        }


        [TestMethod]
        public void Test_SetPropertyValue_WithNamespace_FirstName()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.FirstName);

            Assert.AreEqual("Josh", person.FirstName);
        }

        [TestMethod]
        public void Test_SetPropertyValue_StringCtor_FirstNameAsHtml()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.FirstNameAsHtml);

            Assert.AreEqual("Josh", person.FirstNameAsHtml.ToHtmlString());
        }

        [TestMethod]
        public void Test_SetPropertyValue_MailingAddressLines()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.MailingAddressLines);

            Assert.IsTrue(person.MailingAddressLines.Count == 2);
        }

        [TestMethod]
        public void Test_SetPropertyValue_Brothers()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.Brothers);

            Assert.IsTrue(person.Brothers.Count == 1);
        }

        [TestMethod]
        public void Test_SetPropertyValue_Contacts()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.Contacts);

            Assert.IsTrue(person.Contacts.Length == 2);
        }

        [TestMethod]
        public void Test_SetPropertyValue_XPathFunction_NumberOfContacts()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.NumberOfContacts);

            Assert.AreEqual(2, person.NumberOfContacts);
        }

        [TestMethod]
        public void Test_SetPropertyValue_DecimalCtor_BigNumberOfContacts()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.BigNumberOfContacts);

            Assert.AreEqual(new BigInteger(2), person.BigNumberOfContacts);
        }

        
        [TestMethod]
        public void Test_SetPropertyValue_XPathFunction_ContactsEqualToEmails()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.ContactsEqualToEmails);

            Assert.IsTrue((bool)person.ContactsEqualToEmails);
        }

        [TestMethod]
        public void Test_SetPropertyValue_Enum_Role()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.Role);

            Assert.AreEqual(Role.Boss, person.Role);
        }
        
        [TestMethod]
        public void Test_SetProperty_Dictionary_string_decimal()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.EmailCountsByName);

            Assert.AreEqual(2, person.EmailCountsByName.Count);
        }

        [TestMethod]
        public void Test_SetProperty_Dictionary_string_Email()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.EmailsByName);

            Assert.AreEqual(2, person.EmailsByName.Count);
        }

        [TestMethod]
        public void Test_SetProperty_Emails()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.Emails);

            Assert.IsTrue(TestUtils.EnumerablesAreEqual(
                new List<Email>
                {
                    new Email { Domain = "josheinhorn.com", Name = "josh" },
                    new Email { Domain = "gmail.com", Name = "josh.einhorn"}
                },
                person.Emails
                ));
        }

        [TestMethod]
        public void Test_SetProperty_Dictionary_EmailAddress_Provider()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.PersonByEmail);

            Assert.AreEqual(2, person.PersonByEmail.Count);
        }

        [TestMethod]
        public void Test_SetProperty_Tuple_string_string_short()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.ConstructorTest);

            Assert.AreEqual(new StringStringShort("Joe", "Schmo", 1), person.ConstructorTest);
        }

        [TestMethod]
        public void Test_SetProperty_GenericTuple()
        {
            var person = new Person();

            Binder.SetPropertyValue(person, PersonXmlData, p => p.GenericTupleTest);

            Assert.AreEqual(new Tuple<string, string, short>("Joe", "Schmo", 1), person.GenericTupleTest);
        }
    }
}
