using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    public class XmlModelAttribute : Attribute, IXmlModelDescription
    {
        public XmlModelAttribute()
        { }

        public virtual string RootXPath
        {
            get;
            set;
        }
    }
}
