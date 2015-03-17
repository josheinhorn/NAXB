using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.Build
{
    public class XmlBinder : IXmlModelBinder
    {
        protected readonly IXmlBindingResolver bindingResolver;
        //protected readonly IDependencyResolver dependencyResolver;
        protected readonly IReflector reflector;
        protected readonly IXPathProcessor xPathProcessor;
        //protected readonly IXmlFactory factory;
        public XmlBinder(IXmlBindingResolver bindingResolver, 
            IXPathProcessor xPathProcessor, 
            //IDependencyResolver dependencyResolver,
            IReflector reflector
            //, IXmlFactory factory
            )
        {
            if (bindingResolver == null) throw new ArgumentNullException("bindingResolver");
            if (xPathProcessor == null) throw new ArgumentNullException("xPathProcessor");
            //if (dependencyResolver == null) throw new ArgumentNullException("dependencyResolver");
            if (reflector == null) throw new ArgumentNullException("reflector");
            //if (factory == null) throw new ArgumentNullException("factory"); 
            this.bindingResolver = bindingResolver;
            this.xPathProcessor = xPathProcessor;
            //this.dependencyResolver = dependencyResolver;
            this.reflector = reflector;
            //this.factory = factory;
        }
        
        public object BindToModel(IXmlData xmlData)
        {
            throw new NotImplementedException();
        }

        public object BindToModel(Type modelType, IXmlData xmlData)
        {
            var binding = bindingResolver.ResolveBinding(modelType);
            return BindToModel(binding, xmlData); 
        }

        public void SetPropertyValue(object model, IXmlData data, IXmlProperty property)
        {
            if (model != null)
            {
                object propValue = property.GetPropertyValue(data, xPathProcessor, this); //All work happens in the Property
                if (propValue != null)
                    property.Set(model, propValue);
            }
        }

        public T BindToModel<T>(IXmlData xmlData)
        {
            return (T)BindToModel(typeof(T), xmlData);
        }

        public void SetPropertyValue<TModel, TProp>(TModel model, IXmlData data, System.Linq.Expressions.Expression<Func<TModel, TProp>> propertyLambda)
        {
            var propertyName = reflector.GetPropertyInfo(propertyLambda).Name;
            var binding = bindingResolver.ResolveBinding<TModel>();
            var xmlProp = binding.Properties.FirstOrDefault(prop => prop.PropertyInfo.Name == propertyName);
            SetPropertyValue(model, data, xmlProp);
        }

        public object BindToModel(IXmlModelBinding binding, IXmlData xmlData)
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
