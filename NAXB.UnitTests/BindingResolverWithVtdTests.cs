using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAXB.UnitTests.Mockups.Models;
using NAXB.VtdXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NAXB.UnitTests
{
    [TestClass]
    public class BindingResolverWithVtdTests : BindingResolverTestBase
    {
        public BindingResolverWithVtdTests()
            : base(new VtdXPathProcessor(), new Assembly[] {typeof(Person).Assembly})
        { }

    }
}
