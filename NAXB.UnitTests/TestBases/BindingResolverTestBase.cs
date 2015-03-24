using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using NAXB.Build;
using System.Reflection;
using NAXB.UnitTests.Mockups.Models;
using NAXB.Exceptions;

namespace NAXB.UnitTests
{
    [TestClass]
    public abstract class BindingResolverTestBase
    {
        private IXmlBindingResolver resolver;

        public BindingResolverTestBase(IXPathProcessor processor, Assembly[] bindingAssemblies)
        {
            resolver = new XmlBindingResolver(new Reflector(), processor, bindingAssemblies);
        }

        [TestMethod]
        public void Test_ResolveBinding_Person()
        {
            var expectedType = typeof(Person);
            var binding = resolver.ResolveBinding(expectedType);

            Assert.AreEqual(expectedType.Name, binding.Name); //find a better assertion
        }

    }
}
