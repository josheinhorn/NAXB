using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.Build;
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
    public class VtdXmlBinderTests : XmlBinderTestBase
    {
        private IXPathProcessor processor = new VtdXPathProcessor();
        private IXmlData personXmlData;
        private IXmlModelBinder binder;
        public VtdXmlBinderTests()
        {
            var factory = new VtdXmlFactory();
            personXmlData = factory.CreateXmlData(MockConstants.PersonXmlFilePath);
            var reflector = new Reflector();
            var resolver = new XmlBindingResolver(reflector, processor);
            resolver.LoadBindings(this.GetType().Assembly);
            binder = new XmlBinder(resolver, processor, reflector);
        }
        
        

        protected override IXmlData PersonXmlData
        {
            get
            {
                return personXmlData;
            }
        }

        protected override IXmlModelBinder Binder
        {
            get
            {
                return binder;
            }
        }
    }
}
