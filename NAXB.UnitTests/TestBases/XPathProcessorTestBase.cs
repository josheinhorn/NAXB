using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.Interfaces;
using NAXB.UnitTests.Mockups;
using System.Linq;
using System.Collections.Generic;

namespace NAXB.UnitTests
{
    [TestClass]
    public abstract class XPathProcessorTestBase
    {
        protected abstract IXPathProcessor Processor { get; }
        protected abstract INamespace[] Namespaces { get; }
        //protected abstract MockXmlProvider XmlProvider { get; }
        protected abstract MockXPathProvider XPathProvider { get; }
        protected abstract IXmlData XmlData { get; }

        [TestMethod]
        public void Test_XPathProcessor_CompileXPath_ElementXPath()
        {
            var compiled = Processor.CompileXPath(XPathProvider.SingleElement.XPath, Namespaces, PropertyType.Text);

            Assert.AreEqual(XPathProvider.SingleElement.XPath, compiled.XPathAsString);
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_SingleElement()
        {
            var compiled = Processor.CompileXPath(XPathProvider.SingleElement.XPath, Namespaces, PropertyType.Text);

            var result = Processor.ProcessXPath(XmlData, compiled);

            Assert.AreEqual(XPathProvider.SingleElement.ExpectedValue, result.FirstOrDefault().Value);
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_SingleAttribute()
        {
            var compiled = Processor.CompileXPath(XPathProvider.SingleAttribute.XPath, Namespaces, PropertyType.Text);

            var result = Processor.ProcessXPath(XmlData, compiled);

            Assert.AreEqual(XPathProvider.SingleAttribute.ExpectedValue, result.FirstOrDefault().Value);
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_MultipleElements()
        {
            var compiled = Processor.CompileXPath(XPathProvider.MultipleElements.XPath, Namespaces, PropertyType.Text);

            var result = Processor.ProcessXPath(XmlData, compiled);
            Assert.IsTrue(TestUtils.EnumerablesAreEqual((IEnumerable<string>)XPathProvider.MultipleElements.ExpectedValue
                , result.Select(x => x.Value)));
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_MultipleAttributes()
        {
            var compiled = Processor.CompileXPath(XPathProvider.MultipleAttributes.XPath, Namespaces, PropertyType.Text);

            var result = Processor.ProcessXPath(XmlData, compiled);
            Assert.IsTrue(TestUtils.EnumerablesAreEqual((IEnumerable<string>)XPathProvider.MultipleAttributes.ExpectedValue
                , result.Select(x => x.Value)));
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_NestedSingleElement()
        {
            IXmlData parent = XmlData;
            IEnumerable<IXmlData> result = null;
            foreach (var xpt in XPathProvider.NestedSingleElement)
            {
                var compiled = Processor.CompileXPath(xpt.XPath, Namespaces, PropertyType.Text);
                
                result = Processor.ProcessXPath(parent, compiled);

                parent = result.FirstOrDefault();
            }
            Assert.AreEqual(XPathProvider.NestedSingleElement.LastOrDefault().ExpectedValue
                , result.FirstOrDefault().Value);
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_NestedSingleAttribute()
        {
            IXmlData parent = XmlData;
            IEnumerable<IXmlData> result = null;
            foreach (var xpt in XPathProvider.NestedSingleAttribute)
            {
                var compiled = Processor.CompileXPath(xpt.XPath, Namespaces, PropertyType.Text);

                result = Processor.ProcessXPath(parent, compiled);

                parent = result.FirstOrDefault();
            }
            Assert.AreEqual(XPathProvider.NestedSingleAttribute.LastOrDefault().ExpectedValue
                , result.FirstOrDefault().Value);
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_NestedMultipleElements()
        {
            IXmlData parent = XmlData;
            List<string> results = new List<string>();
            RecurseXPaths(results, parent, new Queue<XPathTest>(XPathProvider.NestedMultipleElements));
            Assert.IsTrue(TestUtils.EnumerablesAreEqual(
                (IEnumerable<string>)XPathProvider.NestedMultipleElements.LastOrDefault().ExpectedValue
                , results));
        }

        [TestMethod]
        public void Test_XPathProcessor_ProcessXPath_NestedMultipleAttributes()
        {
            IXmlData parent = XmlData;
            List<string> results = new List<string>();
            RecurseXPaths(results, parent, new Queue<XPathTest>(XPathProvider.NestedMultipleAttributes));
            Assert.IsTrue(TestUtils.EnumerablesAreEqual(
                (IEnumerable<string>)XPathProvider.NestedMultipleAttributes.LastOrDefault().ExpectedValue
                , results));
        }

        protected void RecurseXPaths(List<string> results, IXmlData parent, Queue<XPathTest> xpts)
        {
            if (xpts.Count == 1)
            {
                //the last one
                var xpt = xpts.Dequeue();
                var compiled = Processor.CompileXPath(xpt.XPath, Namespaces, PropertyType.Text);
                var processed = Processor.ProcessXPath(parent, compiled);
                results.AddRange(processed.Select(x => x.Value));
                //Stop recursing
            }
            else if (xpts.Count != 0)
            {
                var xpt = xpts.Dequeue();
                var compiled = Processor.CompileXPath(xpt.XPath, Namespaces, PropertyType.Text);
                var processed = Processor.ProcessXPath(parent, compiled);
                foreach (var xml in processed)
                {
                    RecurseXPaths(results, xml, new Queue<XPathTest>(xpts));
                }
            }
        }
    }
}
