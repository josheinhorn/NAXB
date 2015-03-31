using NAXB.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests.Mockups.Models
{
    [XmlModelBinding()]
    public class EmailProvider
    {
        [XPath("@name")]
        public string Name { get; protected set; }
    }
}
