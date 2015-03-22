using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using System.Globalization;

namespace NAXB.Attributes
{
    public class CustomFormatAttribute : Attribute, ICustomFormatBinding
    {
        protected Func<ICustomBindingResolver> createResolver = null;
        protected Type customBindingResolverType = null;

        public Type CustomBindingResolverType
        {
            get
            {
                return customBindingResolverType;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!typeof(ICustomBindingResolver).IsAssignableFrom(value))
                    throw new ArgumentException("CustomBindingResolverType must implement ICustomBindingResolver");
                customBindingResolverType = value;
            }
        }

        public string CultureName { get; set; }

        public virtual string DateTimeFormat
        {
            get;
            set;
        }

        public virtual bool IgnoreCase
        {
            get;
            set;
        }

        public virtual IFormatProvider GetFormatProvider()
        {
            if (!String.IsNullOrEmpty(CultureName)) return CultureInfo.GetCultureInfo(CultureName);
            else return null;
        }

        public virtual ICustomBindingResolver GetCustomResolver(IReflector reflector)
        {
            //lazy load createResolver delegate
            if (createResolver == null)
            {
                var defaultCtor = reflector.BuildDefaultConstructor(CustomBindingResolverType);
                createResolver = () => (ICustomBindingResolver)defaultCtor();
            }
            return createResolver();
        }
    }
}
