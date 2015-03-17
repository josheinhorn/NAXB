using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Xml
{
    public class XmlNamespace : INamespace
    {

        public string Prefix
        {
            get;
            set;
        }

        public string Uri
        {
            get;
            set;
        }
    }
}
