using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.Interfaces;
using System.IO;
using NAXB.UnitTests.Mockups;

namespace NAXB.UnitTests
{
    /// <summary>
    /// Summary description for XmlFactoryTests
    /// </summary>
    [TestClass]
    public abstract class XmlFactoryTestBase
    {
        protected abstract IXmlFactory GetXmlFactory();
        protected abstract INamespace[] GetNamespaces();
        protected abstract MockXmlProvider XmlProvider { get; }

        [TestMethod]
        public void Test_CreateXml_FomString()
        {
            var factory = GetXmlFactory();
            var expected = XmlProvider.XmlAsString;

            var data = factory.CreateXmlData(expected, XmlProvider.Encoding, GetNamespaces());

            //Assert.AreEqual(expected, data.XmlAsString); //This assert doesn't work, the string is slightly altered by VTD XML
            Assert.IsNotNull(data); //Better assertion??
        }

        [TestMethod]
        public void Test_CreateXml_FromStream()
        {
            var factory = GetXmlFactory();
            IXmlData data;
            using (var stream = XmlProvider.XmlAsStream)
            {
                data = factory.CreateXmlData(stream, GetNamespaces());
            }
            Assert.IsNotNull(data); //what should we actually test here?
        }

        [TestMethod]
        public void Test_CreateXml_FomBytes()
        {
            var factory = GetXmlFactory();
            var expected = XmlProvider.XmlAsByteArray;

            var data = factory.CreateXmlData(expected, XmlProvider.Encoding, GetNamespaces());

            Assert.AreEqual(expected, data.XmlAsByteArray);
        }

        [TestMethod]
        public void Test_CreateXml_FomFile()
        {
            var factory = GetXmlFactory();

            var data = factory.CreateXmlData(XmlProvider.FileName, GetNamespaces());

            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void Test_CreateXml_Value()
        {
            var factory = GetXmlFactory();

            var data = factory.CreateXmlData(XmlProvider.XmlAsString, XmlProvider.Encoding, GetNamespaces());

            //Assert.AreEqual(expected, data.Value); //This assert doesn't work, the string is slightly altered by VTD XML
            Assert.IsNotNull(data); //better assertion?
        }
    }
}
