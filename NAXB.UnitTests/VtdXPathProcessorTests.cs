using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.Interfaces;
using NAXB.UnitTests.Mockups;
using NAXB.VtdXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests
{
    [TestClass]
    public class VtdXPathProcessorTests : XPathProcessorTestBase
    {
        private IXmlData vtdXmlData;
        private IXPathProcessor processor;
        private MockXPathProvider provider;
        
        public VtdXPathProcessorTests()
        {
            vtdXmlData = new VtdXmlFactory()
            .CreateXmlData(MockConstants.PersonXmlFilePath, Namespaces);
            processor = new VtdXPathProcessor();
            provider = new MockXPathProvider
                {
                    SingleElement = new XPathTest
                    {
                        XPath = "firstName",
                        ExpectedValue = "Josh"
                    },
                    SingleAttribute = new XPathTest
                    {
                        XPath = "contacts/person[@role=\"bodyguard\"]/@relationship",
                        ExpectedValue = "brother"
                    },
                    MultipleElements= new XPathTest
                    {
                        XPath = "address[@type=\"mailing\"]/line",
                        ExpectedValue = new List<string>
                        {
                            "123 Any Lane",
                            "Anywhere, USA 12345"
                        }
                    },
                    MultipleAttributes= new XPathTest
                    {
                        XPath = "emails/email/@domain",
                        ExpectedValue = new List<string>
                        {
                            "josheinhorn.com", "gmail.com"
                        }
                    },
                    NestedSingleElement = new List<XPathTest>
                    {
                        new XPathTest { XPath = "contacts"},
                        new XPathTest { XPath = "person[@role=\"assistant\"]" },
                        new XPathTest { XPath = "firstName", ExpectedValue = "Joe" }
                    },
                    NestedSingleAttribute = new List<XPathTest>
                    {
                        new XPathTest { XPath = "contacts"},
                        new XPathTest { XPath = "person[firstName=\"Jimmy\"]" },
                        new XPathTest { XPath = "contacts"},
                        new XPathTest { XPath = "person[@role=\"boss\"]" },
                        new XPathTest { XPath = "@firstName", ExpectedValue = "Josh" }
                    },
                    NestedMultipleElements = new List<XPathTest>
                    {
                        new XPathTest { XPath = "address"},
                        new XPathTest { XPath = "line[@num=\"2\"]"
                            , ExpectedValue = new List<string>
                            {
                                "Anywhere, USA 12345",
                                "Moneysville, USA 12345"
                            }
                        }
                    }
                };
        }

        protected override IXPathProcessor Processor
        {
            get { return processor; }
        }

        protected override INamespace[] Namespaces
        {
            get { return new INamespace[0]; }
        }

        protected override MockXPathProvider XPathProvider
        {
            get { return provider; }
        }

        protected override IXmlData XmlData
        {
            get { return vtdXmlData; }
        }
    }
}
