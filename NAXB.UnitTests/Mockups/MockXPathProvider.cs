using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests.Mockups
{

    public class MockXPathProvider
    {
        public List<XPathTest> NestedSingleElement
        {
            get;
            set;
        }
        public List<XPathTest> NestedSingleAttribute
        {
            get;
            set;
        }
        public List<XPathTest> NestedMultipleElements
        {
            get;
            set;
        }
        public List<XPathTest> NestedMultipleAttributes
        {
            get;
            set;
        }

        public XPathTest SingleElement
        {
            get;
            set;
        }

        public XPathTest SingleAttribute
        {
            get;
            set;
        }

        public XPathTest MultipleElements
        {
            get;
            set;
        }

        public XPathTest MultipleAttributes
        {
            get;
            set;
        }

        //public XPathTest SingleComplexElement
        //{
        //    get;
        //    set;
        //}

        //public XPathTest SingleComplexAttribute
        //{
        //    get;
        //    set;
        //}

        //public XPathTest MultipleComplexElements
        //{
        //    get;
        //    set;
        //}

        //public XPathTest MultipleComplexAttributes
        //{
        //    get;
        //    set;
        //}
    }
}
