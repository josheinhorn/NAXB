using NAXB.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Web;

namespace NAXB.UnitTests.Mockups.Models
{
    [XmlModelBinding]
    [NamespaceDeclaration(Uri = "http://example.com", Prefix = "ex")]
    public class Person
    {
        [XPath("contacts/person", "firstName", "count(emails/email)")]
        public Dictionary<string, decimal> EmailCountsByName { get; private set; }

        [XPath("contacts/person", "concat(firstName, ' ', lastName)", "emails/email")]
        public Dictionary<string, Email> EmailsByName;

        [XPath("count(contacts/person)")]
        public int NumberOfContacts { get; set; }

        [XPath("count(contacts/person)")]
        public BigInteger BigNumberOfContacts { get; set; }

        [XPath("count(contacts/person)=count(emails/email)")]
        public bool? ContactsEqualToEmails;

        [XPath("ex:firstName")]
        public string FirstName { get; set; }

        [XPath("ex:firstName")]
        public HtmlString FirstNameAsHtml { get; set; }

        [XPath("lastName")]
        public string LastName { get; set; }

        [XmlElement("middleName")]
        public string MiddleName;

        [XPath("emails/email")]
        public List<Email> Emails { get; set; }

        [XPath("address[@type=\"mailing\"]/line")]
        public List<string> MailingAddressLines { get; set; }

        [XPath("address[@type=\"billing\"]/line")]
        public List<string> BillingAddressLines { get; set; }

        [XPath("contacts/person")]
        public Person[] Contacts { get; set; }

        [XmlAttribute("role")]
        [CustomFormat(IgnoreCase=true)]
        public Role? Role { get; set; }

        [XPath("contacts/person[@relationship=\"brother\"]")]
        public List<Person> Brothers { get; set; }

        [XmlElement("DOB")]
        [CustomFormat(DateTimeFormat = "dd-MM-yyyy")]
        public DateTime? DateOfBirth;
    }

   
}
