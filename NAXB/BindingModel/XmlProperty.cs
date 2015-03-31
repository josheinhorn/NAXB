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
        protected IXPath compiledRootXPath;
        protected IXPath[] compiledXPaths;
        protected Func<IEnumerable<IXmlData[]>, IXmlModelBinder, IEnumerable> convertXmlListToEnumerable = null;
        protected Func<string[], object> parseXmlValues = null;
        protected Func<IEnumerable, object> convertEnumerableToProperty = null;
        protected Func<IEnumerable<IXmlData[]>, IXmlModelBinder, object> convertXmlToProperty = null;
        protected readonly Action<object, object> set;
        protected readonly Func<object, object> get;
        protected bool isInitialized = false;
        protected IReflector reflector;
        protected string rootXPath = null;
        protected string[] childXPaths = null;
        protected bool rootIsFunction = false;
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
        PropertyType[] nestedPropertyTypes;
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

            childXPaths = Binding.XPaths; //initial setup
            BuildParseXmlValue(); //Build the parse XML delegate and set the Property Type
            //Evaluate as function if it is a single text, number, or boolean value -- could easily break for custom binding resolvers though

            bool hasChildXPaths = Binding.XPaths != null && Binding.XPaths.Length > 0;

            if (hasChildXPaths)// && rootXPath != null)
            {
                //Node set and there is a root XPath, combine XPaths
                this.rootXPath = CombineXPaths(rootXPath, Binding.RootXPath);
                childXPaths = Binding.XPaths;
                rootIsFunction = false;
            }
            else // if (!hasChildXPaths)
            {
                if (!PropertyInfo.IsEnumerable
                    && (Type == PropertyType.Text || Type == PropertyType.Number || Type == PropertyType.Bool))
                {
                    //Single text/number/bool with no child XPaths
                    if (!String.IsNullOrEmpty(rootXPath))
                    {
                        //There is a root XPath, use it as the root, make the child the old root
                        this.rootXPath = rootXPath;
                        this.childXPaths = new string[] { Binding.RootXPath };
                        BuildParseXmlValue(); //rebuild the parser -- TODO: fix this logic, super inefficient
                        rootIsFunction = false;
                    }
                    else
                    {
                        //No root, use existing Binding's root
                        this.rootXPath = Binding.RootXPath;
                        rootIsFunction = true;
                    }
                }
                else
                {
                    //No child XPaths, Not text/number/bool or is multivalue
                    this.rootXPath = CombineXPaths(rootXPath, Binding.RootXPath);
                    rootIsFunction = false;
                }
            }
            //else //Has child XPaths but root XPath is null
            //{
            //    this.rootXPath = Binding.RootXPath;
            //    this.childXPaths = Binding.XPaths;
            //    rootIsFunction = false;
            //}

            //TODO: Fix this, it is WRONG if the XPath is just a selection for a single text node
            //if (!rootIsFunction && rootXPath != null)
            //{
            //    //Node set and there is a root XPath, combine XPaths
            //    this.rootXPath = CombineXPaths(rootXPath, Binding.RootXPath);
            //}
            //else
            //{
            //    //It's either a function or there's no root XPath
            //    this.rootXPath = Binding.RootXPath;
            //}

            if (PropertyInfo.IsDictionary && (childXPaths == null || childXPaths.Length != 2))
            {
                throw new ArgumentOutOfRangeException("XPaths",
                    String.Format("Property '{0}' is a Dictionary but the wrong number of XPaths are specified. Required 2."
                    , PropertyInfo.FullName));
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
            KeyComplexBinding = PropertyInfo.KeyType == null ? null : resolver.ResolveBinding(PropertyInfo.KeyType);
            //Build up the delegates that are used to get property value
            BuildConvertEnumerableToProperty();
            BuildConvertXmlListToEnumerable();
            BuildConvertXmlToProperty();
            try
            {
                compiledRootXPath = xPathProcessor.CompileXPath(rootXPath, namespaces, Type, rootIsFunction);
                if (childXPaths != null)
                {
                    compiledXPaths = new IXPath[childXPaths.Length];
                    for (int i = 0; i < childXPaths.Length; i++)
                    {
                        var xpath = childXPaths[i];
                        var propType = nestedPropertyTypes[i];
                        var isFunction = propType == PropertyType.Text || propType == PropertyType.Number || propType == PropertyType.Bool;
                        compiledXPaths[i] = xPathProcessor.CompileXPath(xpath, namespaces, propType, isFunction);
                    }
                }
            }
            catch (Exception)
            {
                throw new XPathCompilationException(this);
            }
            isInitialized = true;
        }

        protected IXmlModelBinding ComplexBinding
        {
            get;
            set;
        }

        protected IXmlModelBinding KeyComplexBinding
        {
            get;
            set;
        }


        public virtual object GetPropertyValue(IXmlData data, IXPathProcessor xPathProcessor, IXmlModelBinder binder)
        {
            if (!isInitialized) throw new InvalidOperationException("GetPropertyValue cannot be called before the Initialize method is called on this object.");
            object result = null;
            if (xPathProcessor != null && data != null && binder != null)
            {
                IEnumerable<IXmlData[]> xml;
                try
                {
                    xml = xPathProcessor.ProcessXPath(data, compiledRootXPath, compiledXPaths);
                }
                catch (Exception e)
                {
                    throw new XPathEvaluationException(this, compiledRootXPath, e);
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
                convertXmlToProperty = (IEnumerable<IXmlData[]> data, IXmlModelBinder binder) =>
                    {
                        object result;
                        //use the Try version here?
                        resolver.TryGetPropertyValue(data, binder, out result);
                        return result;
                    };
            }
            else
            {
                convertXmlToProperty = (IEnumerable<IXmlData[]> data, IXmlModelBinder binder) =>
                    convertEnumerableToProperty(convertXmlListToEnumerable(data, binder));
            }
        }

        private Func<IXmlData, IXmlModelBinder, object> GetSingleValueDelegate(IXmlModelBinding complexBinding, Type type)
        {
            Func<IXmlData, IXmlModelBinder, object> result = null;
            if (complexBinding != null)
            {
                result = (IXmlData data, IXmlModelBinder binder) =>
                {
                    return data != null ? binder.BindToModel(complexBinding, data) : null;
                };
            }
            else
            {
                PropertyType temp;
                var parseKey = reflector.BuildSingleParser(type, CustomFormatBinding, out temp);
                result = (IXmlData data, IXmlModelBinder binder) =>
                {
                    return data != null ? parseKey(data.Value) : null;
                };
            }
            return result;
        }

        protected void BuildConvertXmlListToEnumerable()
        {
            if (PropertyInfo.IsDictionary)
            {
                Func<IXmlData, IXmlModelBinder, object> getKey = GetSingleValueDelegate(KeyComplexBinding, PropertyInfo.KeyType);
                Func<IXmlData, IXmlModelBinder, object> getValue = GetSingleValueDelegate(ComplexBinding, PropertyInfo.ElementType);
                convertXmlListToEnumerable = (IEnumerable<IXmlData[]> data, IXmlModelBinder binder) =>
                    {
                        return data.Select(xml =>
                            {
                                var key = getKey(xml[0], binder);
                                var value = getValue(xml[1], binder);
                                
                                if (key != null && value != null)
                                {
                                    return new KeyValuePair<object, object>(key, value);
                                }
                                else return default(KeyValuePair<object, object>);
                            }).Where(x => !x.Equals(default(KeyValuePair<object, object>)));
                    };
            }
            else if (ComplexBinding != null) //it's a complex type
            {
                convertXmlListToEnumerable = (IEnumerable<IXmlData[]> data, IXmlModelBinder binder) =>
                    data.Select(xml => binder.BindToModel(ComplexBinding, xml[0]))
                    .Where(value => value != null); //filter nulls or not? Seems strange to return null values
                Type = PropertyType.Complex;
            }
            else //simple type or Dictionary of simple types
            {
                //convertXmlListToEnumerable = (IEnumerable<IXmlData[]> data, IXmlModelBinder binder) =>
                //    data.Select(xml =>
                //    {
                //        try { return parseXmlValues(xml.Select(x => x.Value).ToArray()); }
                //        catch (Exception) { return null; } //Just return null if the value couldn't be parsed
                //    })
                //        .Where(value => value != null); //filter out any values that couldn't be parsed

                //The below is (almost) functionally equivalent and allows us to actually Debug paseXmlValues
                //if ever needed
                convertXmlListToEnumerable = (IEnumerable<IXmlData[]> data, IXmlModelBinder binder) =>
               {
                   var result = new List<object>();
                   foreach (var xml in data)
                   {
                       try
                       {
                           result.Add(parseXmlValues(xml.Select(x => x.Value).ToArray()));
                       }
                       catch (Exception) { }
                   }
                   return result;
               };

            }
        }

        protected void BuildParseXmlValue()
        {
            //TODO: Make this method more transparent -- it is modifying property type and also building the Parsing delegate
            var elementType = PropertyInfo.ElementType;
            PropertyType temp;
            if (childXPaths == null || childXPaths.Length < 2)
            {
                var parseXmlValue = reflector.BuildSingleParser(elementType, CustomFormatBinding, out temp);
                parseXmlValues = (string[] values) => values.Length > 0 ? parseXmlValue(values[0]) : null;
                if (temp != PropertyType.XmlFragment) Type = temp;
                if (childXPaths != null && childXPaths.Length == 1) //If it's a single child XPath, treat it as if it is the root
                {
                    nestedPropertyTypes = new PropertyType[] { temp };
                }
            }
            else if (PropertyInfo.IsDictionary)
            {
                nestedPropertyTypes = new PropertyType[2];
                var parseKey = reflector.BuildSingleParser(
                    PropertyInfo.KeyType, CustomFormatBinding, out nestedPropertyTypes[0]);
                var parseValue = reflector.BuildSingleParser(
                    elementType, CustomFormatBinding, out nestedPropertyTypes[1]);
                parseXmlValues = (string[] values) =>
                    {
                        KeyValuePair<object, object> result = default(KeyValuePair<object, object>);
                        if (values.Length > 1)
                        {
                            result = new KeyValuePair<object, object>(
                                parseKey(values[0]), parseValue(values[1]));
                        }
                        return result;
                    };
            }
            else
            {
                parseXmlValues = reflector.BuildMultiParser(elementType, CustomFormatBinding,
                    childXPaths.Length, out nestedPropertyTypes);
            }
            //Exception if user violated the rules
            if (nestedPropertyTypes != null && (childXPaths == null || childXPaths.Length != nestedPropertyTypes.Length))
            {
                throw new ArgumentOutOfRangeException("XPaths",
                    String.Format("The number of XPaths specified ({0}) does not match the required number of arguments ({1}) for Property '{2}'."
                    , childXPaths == null ? 0 : childXPaths.Length, nestedPropertyTypes.Length, PropertyInfo.FullName));
            }

        }
        protected void BuildConvertEnumerableToProperty()
        {
            var elementType = PropertyInfo.ElementType;

            if (PropertyInfo.IsArray)
            {
                convertEnumerableToProperty = (IEnumerable values) => PropertyInfo.ToArray(values);
            }
            else if (PropertyInfo.IsDictionary)
            {
                convertEnumerableToProperty = (IEnumerable values) =>
                {
                    object result = PropertyInfo.CreateInstance(); //create a new dictionary object of the property type
                    if (result == null)
                    {
                        throw new InvalidOperationException(
                            String.Format("Failed to create new instance of Generic Collection '{0}' for Property '{1}'." +
                            " Please ensure a parameterless constructor exists."
                            , PropertyInfo.PropertyType.FullName, PropertyInfo.FullName));
                    }
                    PropertyInfo.PopulateDictionary(values.Cast<KeyValuePair<object, object>>(), result);
                    return result;
                };
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
