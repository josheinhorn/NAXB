using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Attributes;

namespace NAXB.UnitTests.Mockups.Models
{
    [XmlModelBinding()]
    [NamespaceDeclaration(Uri = "http://example.com", Prefix = "ex")]
    public class EmailAddress
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

        //Override equals for testing purposes only
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && obj is Email)
            {
                var email = obj as Email;
                result = this.Domain == email.Domain
                    && this.Name == email.Name;
            }
            return result;
        }

        public override int GetHashCode()
        {
            return FullAddress.GetHashCode();
        }
    }
}
