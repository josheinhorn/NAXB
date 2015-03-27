using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using System.Globalization;
using System.Reflection;

namespace NAXB.Attributes
{
    /// <summary>
    /// Marks a Property/Field with a custom format or custom value resolver
    /// </summary>
    public class CustomFormatAttribute : Attribute, ICustomFormatBinding
    {
        protected Func<ICustomBindingResolver> createResolver = null;
        protected Type customBindingResolverType = null;
        bool isInitialized = false;
        /// <summary>
        /// The type of Custom Binding Resolver to use for this Property/Field. If this is specified, the XPath will only be evaluated as a node set.
        /// Must implement ICustomBindingResolver.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the Type does not implement ICustomBindingResolver</exception>
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

        /// <summary>
        /// Culture name used for the Format Provider. See System.Globalization.CultureInfo for valid values.
        /// </summary>
        public string CultureName { get; set; }

        /// <summary>
        /// Date Time Format string.
        /// </summary>
        /// <example>"dd-MM-yyyy"</example>
        public virtual string DateTimeFormat
        {
            get;
            set;
        }
        /// <summary>
        /// Ignore casing when parsing the string value into an Enum
        /// </summary>
        public virtual bool IgnoreCase
        {
            get;
            set;
        }
        /// <summary>
        /// Get a Format Provider for parsing the property value based on CultureName (if specified).
        /// </summary>
        /// <returns>Format Provider</returns>
        public virtual IFormatProvider GetFormatProvider()
        {
            if (!String.IsNullOrEmpty(CultureName)) return CultureInfo.GetCultureInfo(CultureName);
            else return null;
        }
        /// <summary>
        /// Gets a custom resolver based on CustomResolverType (if specified).
        /// </summary>
        /// <returns>Custom resolver</returns>
        public virtual ICustomBindingResolver GetCustomResolver() //Perhaps turn this into an initialize method?
        {
            //lazy load createResolver delegate
            if (!isInitialized) throw new InvalidOperationException("GetCustomResolver cannot be called before the Initialize method is called on this object.");
            else if (createResolver != null)
                return createResolver();
            else return null;
        }
        /// <summary>
        /// Initialize the custom formatting
        /// </summary>
        /// <param name="reflector">Reflector</param>
        public virtual bool InitializeResolver(IReflector reflector, out PropertyType propertyType)
        {
            propertyType = PropertyType.Text; //default
            bool result = false;
            if (reflector == null) throw new ArgumentNullException("reflector");
            if (!isInitialized && CustomBindingResolverType != null)
            {
                var defaultCtor = reflector.BuildDefaultConstructor(CustomBindingResolverType);
                createResolver = () => (ICustomBindingResolver)defaultCtor();
                result = true;
                propertyType = PropertyType.XmlFragment;
            }
            isInitialized = true;
            return result;
        }
    }
}
