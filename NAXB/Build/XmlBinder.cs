using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NAXB.Interfaces;
using System.Reflection;
using NAXB.Exceptions;

namespace NAXB.Build
{
    public virtual class XmlBinder : IXmlModelBinder
    {
        protected readonly IXmlBindingResolver bindingResolver;
        protected readonly IReflector reflector;
        protected readonly IXPathProcessor xPathProcessor;
        public XmlBinder(IXmlBindingResolver bindingResolver, 
            IXPathProcessor xPathProcessor, 
            IReflector reflector)
        {
            if (bindingResolver == null) throw new ArgumentNullException("bindingResolver");
            if (xPathProcessor == null) throw new ArgumentNullException("xPathProcessor");
            if (reflector == null) throw new ArgumentNullException("reflector");
            this.bindingResolver = bindingResolver;
            this.xPathProcessor = xPathProcessor;
            this.reflector = reflector;
        }
        
        public virtual object BindToModel(IXmlData xmlData)
        {
            throw new NotImplementedException();
        }

        public virtual object BindToModel(Type modelType, IXmlData xmlData)
        {
            var binding = bindingResolver.ResolveBinding(modelType);
            return BindToModel(binding, xmlData); 
        }

        public virtual void SetPropertyValue(object model, IXmlData data, IXmlProperty property)
        {
            if (model != null)
            {
                object propValue = null;
                try
                {
                    propValue = property.GetPropertyValue(data, xPathProcessor, this); //All work happens in the Property
                    if (propValue != null) //Don't bother with null values
                        property.Set(model, propValue);
                }
                catch (Exception e)
                {
                    if (e is InvalidCastException || e is TargetException)
                    {
                        throw new PropertyTypeMismatchException(property, propValue, e);
                    }
                    else throw e;
                }
            }
        }

        public virtual T BindToModel<T>(IXmlData xmlData)
        {
            return (T)BindToModel(typeof(T), xmlData);
        }

        public virtual void SetPropertyValue<TModel, TProp>(TModel model, IXmlData data, System.Linq.Expressions.Expression<Func<TModel, TProp>> propertyLambda)
        {
            var propertyName = reflector.GetPropertyInfo(propertyLambda).Name;
            var binding = bindingResolver.ResolveBinding<TModel>();
            var xmlProp = binding.Properties.FirstOrDefault(prop => prop.PropertyInfo.Name == propertyName);
            SetPropertyValue(model, data, xmlProp);
        }

        public virtual object BindToModel(IXmlModelBinding binding, IXmlData xmlData)
        {
            object model = binding.CreateInstance(); //Is it possible to ever use this with Ctor params as opposed to parameterless ctor?
            foreach (var property in binding.Properties)
            {
                SetPropertyValue(model, xmlData, property);
            }
            return model;
        }
    }
}
