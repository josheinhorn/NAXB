using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using System.Globalization;
using System.Reflection;

namespace NAXB.Attributes
{
    public class CustomFormatAttribute : Attribute, ICustomFormatBinding
    {
        protected Func<ICustomBindingResolver> createResolver = null;
        protected Type customBindingResolverType = null;
        bool isInitialized = false;
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

        public virtual ICustomBindingResolver GetCustomResolver() //Perhaps turn this into an initialize method?
        {
            //lazy load createResolver delegate
            if (!isInitialized) throw new InvalidOperationException("GetCustomResolver cannot be called before the Initialize method is called on this object.");
            else if (createResolver != null)
                return createResolver();
            else return null;
        }

        public virtual void Initialize(IReflector reflector)
        {
            if (!isInitialized && CustomBindingResolverType != null)
            {
                var defaultCtor = reflector.BuildDefaultConstructor(CustomBindingResolverType);
                createResolver = () => (ICustomBindingResolver)defaultCtor();
            }
            isInitialized = true;
        }
    }
}
