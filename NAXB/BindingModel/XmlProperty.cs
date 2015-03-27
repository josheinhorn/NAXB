using System;
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
        protected string xpath = null;
        protected bool isFunction = false;
        private string CombineXPaths(string root, string xpath)
        {
            string result = null;
            if (!String.IsNullOrEmpty(root) && String.IsNullOrEmpty(xpath)) //xpath is empty but root isn't
            {
                result = root;
            }
            else if (String.IsNullOrEmpty(root) && !String.IsNullOrEmpty(xpath)) //root is empty but xpath isn't
            {
                result = xpath;
            }
            else if (!String.IsNullOrEmpty(root) && !String.IsNullOrEmpty(xpath)) //neither are empty
            {
                if (root.Last().Equals('/') && xpath.First().Equals('/'))
                {
                    //Both root ends in / and xpath starts with /
                    result = root + xpath.Substring(1);
                }
                else if (root.Last().Equals('/') || xpath.First().Equals('/'))
                {
                    //Only one starts/ends with /
                    result = root + xpath;
                }
                else
                {
                    //neither starts/ends with /, add it
                    result = root + "/" + xpath;
                }
            }
            //else both are null or empty

            return result;
        }

        public XmlProperty(MemberInfo property, IReflector reflector, string rootXPath)
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
            if (Binding == null) throw new BindingNotFoundException(this);
            CustomFormatBinding = property.GetCustomAttributes(typeof(ICustomFormatBinding), true).Cast<ICustomFormatBinding>().FirstOrDefault();
            PropertyType temp;
            if (CustomFormatBinding != null && CustomFormatBinding.InitializeResolver(reflector, out temp))
            {
                Type = temp;
            }
            else
            {
                Type = PropertyType.XmlFragment; //Initialize Type
            }
            BuildParseXmlValue(); //Build the parse XML delegate and set the Property Type
            //Evaluate as function is it is a single text, number, or boolean value -- could easily break for custom binding resolvers though
            isFunction = !PropertyInfo.IsEnumerable
                && (Type == PropertyType.Text || Type == PropertyType.Number || Type == PropertyType.Bool);

            if (!isFunction && rootXPath != null)
            {
                //Node set and there is a root XPath, combine XPaths
                this.xpath = CombineXPaths(rootXPath, Binding.XPath);
            }
            else
            {
                //It's either a function or there's no root XPath
                this.xpath = Binding.XPath;
            }

        }

        public INAXBPropertyBinding Binding
        {
            get;
            protected set;
        }

        public IPropertyInfo PropertyInfo
        {
            get;
            protected set;
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
            //Get complex binding, if any
            ComplexBinding = resolver.ResolveBinding(PropertyInfo.ElementType);
            //Build up the delegates that are used to get property value
            BuildConvertEnumerableToProperty();
            BuildConvertXmlListToEnumerable();
            BuildConvertXmlToProperty();
            try
            {
                compiledXPath = xPathProcessor.CompileXPath(xpath, namespaces, Type, isFunction);
            }
            catch (Exception)
            {
                throw new XPathCompilationException(this);
            }
            isInitialized = true;
        }

        public IXmlModelBinding ComplexBinding
        {
            get;
            protected set;
        }

        public virtual object GetPropertyValue(IXmlData data, IXPathProcessor xPathProcessor, IXmlModelBinder binder)
        {
            if (!isInitialized) throw new InvalidOperationException("GetPropertyValue cannot be called before the Initialize method is called on this object.");
            object result = null;
            if (xPathProcessor != null && data != null && binder != null)
            {
                IEnumerable<IXmlData> xml;
                try
                {
                    xml = xPathProcessor.ProcessXPath(data, compiledXPath);
                }
                catch (Exception e)
                {
                    throw new XPathEvaluationException(this, compiledXPath, e);
                }
                result = convertXmlToProperty(xml, binder);
            }
            return result;
        }

        public ICustomFormatBinding CustomFormatBinding
        {
            get;
            protected set;
        }

        public PropertyType Type
        {
            get;
            protected set;
        }

        protected void BuildConvertXmlToProperty()
        {
            ICustomBindingResolver resolver = null;
            if (CustomFormatBinding != null && (resolver = CustomFormatBinding.GetCustomResolver()) != null)
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
                    .Where(value => value != null); //filter nulls or not? Seems strange to return null values
                Type = PropertyType.Complex;
            }
            else //simple type
            {
                convertXmlListToEnumerable = (IEnumerable<IXmlData> data, IXmlModelBinder binder) =>
                    {
                        var result = new List<object>();
                        foreach (var xml in data)
                        {
                                try
                                { 
                                    result.Add(parseXmlValue(xml.Value));
                                }
                                catch (Exception) { } 
                        }
                        return result;
                    };
                    //data.Select(xml =>
                    //    {
                    //        try { return parseXmlValue(xml.Value); }
                    //        catch (Exception) { return null; } //Just return null if the value couldn't be parsed
                    //    })
                    //    .Where(value => value != null); //filter out any values that couldn't be parsed
            }
        }
        protected void BuildParseXmlValue()
        {
            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            string dateTimeFormat = null;
            bool ignoreCase = false;
            ConstructorInfo ctor = null;
            if (CustomFormatBinding != null)
            {
                formatProvider = CustomFormatBinding.GetFormatProvider() ?? CultureInfo.InvariantCulture;
                dateTimeFormat = CustomFormatBinding.DateTimeFormat;
                ignoreCase = CustomFormatBinding.IgnoreCase;
            }
            var elementType = PropertyInfo.ElementType;
            if (elementType == typeof(string))
            {
                parseXmlValue = (string value) => value; //no format
                Type = PropertyType.Text;
            }
            else if (elementType == typeof(char))
            {
                parseXmlValue = (string value) => char.Parse(value); //no format
                Type = PropertyType.Text;
            }
            else if (PropertyInfo.IsEnum)
            {
                parseXmlValue = (string value) => Enum.Parse(elementType, value, ignoreCase);
                Type = PropertyType.Enum;
            }
            else if (elementType == typeof(bool))
            {
                parseXmlValue = (string value) => bool.Parse(value); //no format
                Type = PropertyType.Bool;
            }
            else if (elementType == typeof(byte))
            {
                parseXmlValue = (string value) => byte.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(sbyte))
            {
                parseXmlValue = (string value) => sbyte.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(short))
            {
                parseXmlValue = (string value) => short.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(ushort))
            {
                parseXmlValue = (string value) => ushort.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(int))
            {
                parseXmlValue = (string value) => int.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(uint))
            {
                parseXmlValue = (string value) => uint.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(long))
            {
                parseXmlValue = (string value) => long.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(ulong))
            {
                parseXmlValue = (string value) => ulong.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(float))
            {
                parseXmlValue = (string value) => float.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(double))
            {
                parseXmlValue = (string value) => double.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(decimal))
            {
                parseXmlValue = (string value) => decimal.Parse(value, formatProvider);
                Type = PropertyType.Number;
            }
            else if (elementType == typeof(DateTime))
            {
                Type = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    parseXmlValue = (string value) => DateTime.Parse(value, formatProvider);
                else parseXmlValue = (string value) => DateTime.ParseExact(value, dateTimeFormat, formatProvider);
            }
            else if (elementType == typeof(DateTimeOffset))
            {
                Type = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    parseXmlValue = (string value) => DateTimeOffset.Parse(value, formatProvider);
                else parseXmlValue = (string value) => DateTimeOffset.ParseExact(value, dateTimeFormat, formatProvider);
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(string) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { value });
                Type = PropertyType.Text;
            }
            //else if ((ctor = elementType.GetConstructor(new Type[] { typeof(Char) })) != null)
            //{
            //    var ctorDel = reflector.BuildConstructor(ctor);
            //    parseXmlValue = (string value) => ctorDel(new object[] { char.Parse(value) });
            //    Type = PropertyType.Text;
            //}
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(decimal) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { decimal.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(double) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { double.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(float) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { float.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(long) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { long.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(ulong) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { ulong.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(int) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { int.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(uint) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { uint.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(short) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { short.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(ushort) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { ushort.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(byte) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { byte.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(sbyte) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { sbyte.Parse(value, formatProvider) });
                Type = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(bool) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                parseXmlValue = (string value) => ctorDel(new object[] { bool.Parse(value) });
                Type = PropertyType.Bool;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(DateTime) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                Type = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    parseXmlValue = (string value) => ctorDel(new object[] { DateTime.Parse(value, formatProvider) });
                else parseXmlValue = (string value) => ctorDel(new object[] { DateTime.ParseExact(value, dateTimeFormat, formatProvider) });
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(DateTimeOffset) })) != null)
            {
                var ctorDel = reflector.BuildConstructor(ctor);
                Type = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    parseXmlValue = (string value) => ctorDel(new object[] { DateTimeOffset.Parse(value, formatProvider) });
                else parseXmlValue = (string value) => ctorDel(new object[] { DateTimeOffset.ParseExact(value, dateTimeFormat, formatProvider) });
            }
            //else it's some other type and it won't be assigned -- e.g. Complex Type
        }
        protected void BuildConvertEnumerableToProperty()
        {
            var elementType = PropertyInfo.ElementType;

            if (PropertyInfo.IsArray)
            {
                convertEnumerableToProperty = (IEnumerable values) => PropertyInfo.ToArray(values);
            }
            else if (PropertyInfo.IsGenericCollection)
            {
                convertEnumerableToProperty = (IEnumerable values) =>
                    {
                        object result = PropertyInfo.CreateInstance(); //create a new collection object of the property type
                        if (result == null)
                        {
                            throw new InvalidOperationException(
                                String.Format("Failed to create new instance of Generic Collection '{0}' for Property '{1}'." +
                                " Please ensure a parameterless constructor exists."
                                , PropertyInfo.PropertyType.FullName, PropertyInfo.FullName));
                        }
                        PropertyInfo.PopulateCollection(values, result);
                        return result;
                    };
            }
            else if (PropertyInfo.IsEnumerable)
            {
                convertEnumerableToProperty = (IEnumerable values) => values; //Almost certainly results in an invalid cast exception
            }
            else //it's a single value, just return the first one (should really only be one thing but you never know)
            {
                convertEnumerableToProperty = (IEnumerable values) => values.Cast<object>().FirstOrDefault();
            }
        }

    }
}
