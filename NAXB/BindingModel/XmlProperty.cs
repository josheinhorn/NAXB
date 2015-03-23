﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NAXB.Interfaces;
using NAXB.Exceptions;

namespace NAXB.BindingModel
{
    public class XmlProperty : IXmlProperty
    {
        protected IXPath compiledXPath;
        protected Func<IEnumerable<IXmlData>, IXmlModelBinder, IEnumerable> convertXmlListToEnumerable = null;
        protected Func<string, object> parseXmlValue = null;
        protected Func<IEnumerable, object> convertEnumerableToProperty = null;
        protected Func<IEnumerable<IXmlData>, IXmlModelBinder, object> convertXmlToProperty = null;
        protected readonly Action<object, object> set;
        protected readonly Func<object, object> get;
        protected bool isInitialized = false;
        protected IReflector reflector;

        public XmlProperty(MemberInfo property, IReflector reflector)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (reflector == null) throw new ArgumentNullException("reflector");
            this.reflector = reflector;
            PropertyInfo = new XPropertyInfo(property, reflector);
            if (property is PropertyInfo)
            {
                var propInfo = property as PropertyInfo;
                set = reflector.BuildSetter(propInfo);
                get = reflector.BuildGetter(propInfo);
            }
            else if (property is FieldInfo)
            {
                var fieldInfo = property as FieldInfo;
                set = reflector.BuildSetField(fieldInfo);
                get = reflector.BuildGetField(fieldInfo);
            }
            //Get the first Property Binding attribute
            Binding = property.GetCustomAttributes(typeof(INAXBPropertyBinding), true).Cast<INAXBPropertyBinding>().FirstOrDefault();
            CustomFormatBinding = property.GetCustomAttributes(typeof(ICustomFormatBinding), true).Cast<ICustomFormatBinding>().FirstOrDefault();
        }

        public INAXBPropertyBinding Binding
        {
            get;
            protected set;
        }

        public IPropertyInfo PropertyInfo 
        {
            get; protected set;
        }

        public virtual object Get(object obj)
        {
            return get(obj);
        }
        public virtual void Set(object obj, object value)
        {
            set(obj, value);
        }
      

        public virtual void Initialize(IXmlBindingResolver resolver, IXPathProcessor xPathProcessor, INamespace[] namespaces) //Must be called before ComplexBinding property can be used
        {
            ComplexBinding = resolver.ResolveBinding(PropertyInfo.ElementType);
            compiledXPath = xPathProcessor.CompileXPath(this.Binding.XPath, namespaces); //We should pass in the Root Element Name from the Model here!! just a simple concat? modelDescription.RootElementName + this.Binding.XPath ??
            //Build up the delegates that are used to get property value
            BuildParseXmlValue();
            BuildConvertEnumerableToProperty();
            BuildConvertXmlListToEnumerable();
            BuildConvertXmlToProperty();
            isInitialized = true;
        }

        public IXmlModelBinding ComplexBinding
        {
            get; protected set;
        }

        public virtual object GetPropertyValue(IXmlData data, IXPathProcessor xPathProcessor, IXmlModelBinder binder)
        {
            if (!isInitialized) throw new InvalidOperationException("GetPropertyValue cannot be called before the Initialize method is called on this object.");
            object result = null;
            if (xPathProcessor != null && data != null && binder != null)
            {
                var xml = xPathProcessor.ProcessXPath(data, compiledXPath);
                result = convertXmlToProperty(xml, binder);
            }
            return result;
        }

        public ICustomFormatBinding CustomFormatBinding
        {
            get;
            protected set;
        }

        protected void BuildConvertXmlToProperty()
        {
            ICustomBindingResolver resolver = null;
            if (CustomFormatBinding != null &&  (resolver = CustomFormatBinding.GetCustomResolver(reflector)) != null)
            {
                convertXmlToProperty = (IEnumerable<IXmlData> data, IXmlModelBinder binder) =>
                    {
                        object result;
                        //use the Try version here?
                        resolver.TryGetPropertyValue(data, binder, out result);
                        return result;
                    };
            }
            else
            {
                convertXmlToProperty = (IEnumerable<IXmlData> data, IXmlModelBinder binder) =>
                    convertEnumerableToProperty(convertXmlListToEnumerable(data, binder));
            }
        }
        
        protected void BuildConvertXmlListToEnumerable()
        {
            if (ComplexBinding != null) //it's a complex type
            {
                convertXmlListToEnumerable = (IEnumerable<IXmlData> data, IXmlModelBinder binder) =>
                    data.Select(xml => binder.BindToModel(ComplexBinding, xml))
                    .Where(value => value != null);
            }
            else //simple type
            {
                convertXmlListToEnumerable = (IEnumerable<IXmlData> data, IXmlModelBinder binder) =>
                    data.Select(xml => 
                        {
                            try { return parseXmlValue(xml.Value); }
                            catch (Exception) { return null; } //Just return null if the value couldn't be parsed
                        })
                        .Where(value => value != null); //filter out any values that couldn't be parsed
            }
        }
        protected void BuildParseXmlValue()
        {
            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            string dateTimeFormat = null;
            bool ignoreCase = false;
            if (CustomFormatBinding !=null)
            {
                formatProvider = CustomFormatBinding.GetFormatProvider() ?? CultureInfo.InvariantCulture;
                dateTimeFormat = CustomFormatBinding.DateTimeFormat;
                ignoreCase = CustomFormatBinding.IgnoreCase;
            }
            var elementType = PropertyInfo.ElementType;
            if (elementType == typeof(string))
                parseXmlValue = (string value) => value;
            else if (PropertyInfo.IsEnum)
                parseXmlValue = (string value) => Enum.Parse(elementType, value, ignoreCase);
            else if (elementType == typeof(bool))
                parseXmlValue = (string value) => bool.Parse(value); //no format
            else if (elementType == typeof(byte))
                parseXmlValue = (string value) => byte.Parse(value, formatProvider);
            else if (elementType == typeof(sbyte))
                parseXmlValue = (string value) => sbyte.Parse(value, formatProvider);
            else if (elementType == typeof(short))
                parseXmlValue = (string value) => short.Parse(value, formatProvider);
            else if (elementType == typeof(ushort))
                parseXmlValue = (string value) => ushort.Parse(value, formatProvider);
            else if (elementType == typeof(int))
                parseXmlValue = (string value) => int.Parse(value, formatProvider);
            else if (elementType == typeof(uint))
                parseXmlValue = (string value) => uint.Parse(value, formatProvider);
            else if (elementType == typeof(long))
                parseXmlValue = (string value) => long.Parse(value, formatProvider);
            else if (elementType == typeof(ulong))
                parseXmlValue = (string value) => ulong.Parse(value, formatProvider);
            else if (elementType == typeof(float))
                parseXmlValue = (string value) => float.Parse(value, formatProvider);
            else if (elementType == typeof(double))
                parseXmlValue = (string value) => double.Parse(value, formatProvider);
            else if (elementType == typeof(DateTime))
            {
                if (String.IsNullOrEmpty(dateTimeFormat))
                    parseXmlValue = (string value) => DateTime.Parse(value, formatProvider);
                else parseXmlValue = (string value) => DateTime.ParseExact(value, dateTimeFormat, formatProvider);
            }
            else if (elementType == typeof(DateTimeOffset))
            {
                if (String.IsNullOrEmpty(dateTimeFormat))
                    parseXmlValue = (string value) => DateTimeOffset.Parse(value, formatProvider);
                else parseXmlValue = (string value) => DateTimeOffset.ParseExact(value, dateTimeFormat, formatProvider);
            }
            else if (elementType == typeof(Guid))
                parseXmlValue = (string value) => new Guid(value); //No format for this

        }
        protected void BuildConvertEnumerableToProperty()
        {
            var elementType = PropertyInfo.ElementType;
            if (PropertyInfo.IsEnumerable)
            {
                convertEnumerableToProperty = (IEnumerable values) => values; //Almost certainly results in an invalid cast exception
            }
            else if (PropertyInfo.IsArray)
            {
                convertEnumerableToProperty = (IEnumerable values) => PropertyInfo.ToArray(values);
            }
            else if (PropertyInfo.IsGenericCollection)
            {
                convertEnumerableToProperty = (IEnumerable values) =>
                    {
                        object result = PropertyInfo.CreateInstance(); //create a new collection object of the property type
                        PropertyInfo.PopulateCollection(values, result);
                        return result;
                    };
            }
            else //it's a single value, just return the first one (should really only be one thing but you never know)
            {
                convertEnumerableToProperty = (IEnumerable values) => values.Cast<object>().FirstOrDefault();
            }
        }

        
    }
}
