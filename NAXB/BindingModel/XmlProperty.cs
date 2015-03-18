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
        protected readonly Action<object, object> set;
        protected readonly Func<object, object> get;

        public XmlProperty(PropertyInfo property, IReflector reflector)
        {
            PropertyInfo = new XPropertyInfo(property, reflector);
            set = reflector.BuildSetter(property);
            get = reflector.BuildGetter(property);
            //Get the first Property Binding attribute
            Binding = PropertyInfo.PropertyType.GetCustomAttributes(typeof(INAXBPropertyBinding), true).Cast<INAXBPropertyBinding>().FirstOrDefault();
        }

        public INAXBPropertyBinding Binding
        {
            get;
            private set;
        }

        public IPropertyInfo PropertyInfo 
        {
            get; private set;
        }

        public object Get(object obj)
        {
            return get(obj);
        }
        public void Set(object obj, object value)
        {
            set(obj, value);
        }
      

        public void Initialize(IXmlBindingResolver resolver, IXPathProcessor xPathProcessor, INamespace[] namespaces) //Must be called before ComplexBinding property can be used
        {
            ComplexBinding = resolver.ResolveBinding(PropertyInfo.ElementType);
            compiledXPath = xPathProcessor.CompileXPath(this.Binding.XPath, namespaces); //We should pass in the Root Element Name from the Model here!! just a simple concat? modelDescription.RootElementName + this.Binding.XPath ??
            //Build up the delegates that are used to get property value
            BuildParseXmlValue();
            BuildConvertEnumerableToProperty();
            BuildConvertXmlListToEnumerable();
        }

        public IXmlModelBinding ComplexBinding
        {
            get; private set;
        }


        public object GetPropertyValue(IXmlData data, IXPathProcessor xPathProcessor, IXmlModelBinder binder)
        {
            object result = null;
            if (xPathProcessor != null && data != null && binder != null)
            {
                var xml = xPathProcessor.ProcessXPath(data, compiledXPath);
                result = convertEnumerableToProperty(convertXmlListToEnumerable(xml, binder));
            }
            return result;
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
            var elementType = PropertyInfo.ElementType;
            if (elementType == typeof(string))
                parseXmlValue = (string value) => value;
            else if (PropertyInfo.IsEnum)
                parseXmlValue = (string value) => Enum.Parse(elementType, value);
            else if (elementType == typeof(bool))
                parseXmlValue = (string value) => bool.Parse(value);
            else if (elementType == typeof(byte))
                parseXmlValue = (string value) => byte.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(sbyte))
                parseXmlValue = (string value) => sbyte.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(short))
                parseXmlValue = (string value) => short.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(ushort))
                parseXmlValue = (string value) => ushort.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(int))
                parseXmlValue = (string value) => int.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(uint))
                parseXmlValue = (string value) => uint.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(long))
                parseXmlValue = (string value) => long.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(ulong))
                parseXmlValue = (string value) => ulong.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(float))
                parseXmlValue = (string value) => float.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(double))
                parseXmlValue = (string value) => double.Parse(value, CultureInfo.InvariantCulture);
            else if (elementType == typeof(DateTime))
                parseXmlValue = (string value) => DateTime.Parse(value); //Get Format from XmlData or from Attribute? e.g. IDateFormatProvider --> GetDateFormat(IXmlData xml);
            else if (elementType == typeof(DateTimeOffset))
                parseXmlValue = (string value) => DateTimeOffset.Parse(value);//Format?
            else if (elementType == typeof(Guid))
                parseXmlValue = (string value) => new Guid(value);

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
                        object result = ComplexBinding.CreateInstance();
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
