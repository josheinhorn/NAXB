using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAXB.UnitTests.Mockups
{
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName;

        public List<Email> Emails { get; set; }

    }

    public class Email
    {
        public string Domain { get; set; }
        public string Name { get; set; }

        public string FullAddress
        {
            get
            {
                return Name + "@" + Domain;
            }
        }
    }
}
