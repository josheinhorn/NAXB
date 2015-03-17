using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Attributes
{
    public class NamespaceDeclarationAttribute : Attribute, INamespaceBinding
    {

        public string Uri
        {
            get; set;
        }

        public string Prefix
        {
            get; set;
        }
    }
}
