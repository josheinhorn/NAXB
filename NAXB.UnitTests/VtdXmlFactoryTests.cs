using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.VtdXml;
using NAXB.Xml;
using NAXB.UnitTests.Mockups;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.UnitTests
{
    [TestClass]
    public class VtdXmlFactoryTests : XmlFactoryTestBase
    {
        //Change path to be relative
        protected MockXmlProvider provider =
            new MockXmlProvider(MockConstants.PersonXmlFilePath, Encoding.UTF8);
        IXmlFactory factory = new VtdXmlFactory();
        
        protected override Interfaces.IXmlFactory GetXmlFactory()
        {
            return factory;
        }

        protected override Interfaces.INamespace[] GetNamespaces()
        {
            return new XmlNamespace[0]; //put something here?
        }

        protected override Mockups.MockXmlProvider XmlProvider
        {
            get { return provider; }
        }
    }
}
