using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NAXB.Interfaces;

namespace NAXB.BindingModel
{
    public class XPropertyInfo : IPropertyInfo
    {
        protected Action<System.Collections.IEnumerable, object> populateCollection;
        protected Func<System.Collections.IEnumerable, Array> toArray;
        protected readonly Func<object> defaultConstructor;
        public XPropertyInfo(PropertyInfo property, IReflector reflector)
        {
            Name = property.Name;
            PropertyType = property.PropertyType;
            IsEnumerable = false;
            IsArray = false;
            IsGenericCollection = false;
            Type temp;
            if (IsArray = reflector.IsArray(PropertyType, out temp))
            {
                ElementType = temp;
                toArray = reflector.BuildToArray(ElementType);
            }
            else if (IsGenericCollection = reflector.IsGenericCollection(PropertyType, out temp))
            {
                ElementType = temp;
                populateCollection = reflector.BuildPopulateCollection(ElementType);
            }
            else
            {
                IsEnumerable = reflector.IsEnumerable(PropertyType);
                ElementType = PropertyType; //The individual Element is the same as the Property (even if it is IEnumerable! Highly complex List implementations will break)
            }
            IsEnum = ElementType.IsEnum;
            DeclaringType = property.DeclaringType;
            defaultConstructor = reflector.BuildDefaultConstructor(PropertyType);
        }
        public string Name
        {
            get;
            protected set;
        }

        public bool IsArray
        {
            get;
            protected set;
        }

        public bool IsGenericCollection
        {
            get;
            protected set;
        }

        public bool IsEnumerable
        {
            get;
            protected set;
        }

        public Type PropertyType
        {
            get;
            protected set;
        }

        public Type ElementType
        {
            get;
            protected set;
        }

        public Type DeclaringType
        {
            get;
            protected set;
        }

        public virtual void PopulateCollection(IEnumerable enumerable, object collection)
        {
            populateCollection(enumerable, collection);
        }
        
        public Array ToArray(IEnumerable enumerable)
        {
            return toArray(enumerable);
        }


        public bool IsEnum
        {
            get;
            protected set;
        }


        public virtual object CreateInstance()
        {
            return defaultConstructor();
        }
    }
}
