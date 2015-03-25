using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Build;
using NAXB.Interfaces;
using NAXB.UnitTests.Mockups.Models;
using NAXB.Exceptions;

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
        
    }
}
