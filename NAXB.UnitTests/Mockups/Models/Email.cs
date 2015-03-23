using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Attributes;

namespace NAXB.UnitTests.Mockups.Models
{
    [XmlModel]
    [NamespaceDeclaration(Uri = "http://josheinhorn.com", Prefix = "jse")]
    public class Email
    {
        [XPath("@domain")]
        public string Domain { get; set; }
        [XmlAttribute("name")]
        internal string Name { get; set; }

        public string FullAddress
        {
            get
            {
                return Name + "@" + Domain;
            }
        }
    }
}
