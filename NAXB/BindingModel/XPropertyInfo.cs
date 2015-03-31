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
        protected Action<IEnumerable<KeyValuePair<object, object>>, object> populateDictionary;
        public XPropertyInfo(MemberInfo property, IReflector reflector)
        {
            Name = property.Name;
            DeclaringType = property.DeclaringType;
            FullName = DeclaringType.FullName + "." + Name;
            PropertyType = reflector.GetFieldOrPropertyType(property); //Null if this is a Method
            IsEnumerable = false;
            IsArray = false;
            IsGenericCollection = false;
            Type keyType;
            Type temp;
            if (IsArray = reflector.IsArray(PropertyType, out temp))
            {
                IsEnumerable = true;
                ElementType = temp;
                toArray = reflector.BuildToArray(ElementType);

            }
            else if (IsDictionary = reflector.IsGenericDictionary(PropertyType, out keyType, out temp))
            {
                IsEnumerable = true;
                ElementType = temp;
                KeyType = keyType;
                populateDictionary = reflector.BuildPopulateDictionary(PropertyType);
                //build a populateDictionary
            }
            else if (IsGenericCollection = reflector.IsGenericCollection(PropertyType, out temp))
            {
                IsEnumerable = true;
                ElementType = temp;
                populateCollection = reflector.BuildPopulateCollection(PropertyType);
            }
            else if (IsNullableType = reflector.IsNullableType(PropertyType, out temp))
            {
                ElementType = temp;
            }
            else
            {
                IsEnumerable = reflector.IsEnumerable(PropertyType);
                ElementType = PropertyType; //The individual Element is the same as the Property (even if it is non-generic IEnumerable -- Highly complex List implementations will break)
            }
            //Note: Don't need to check that the Element of Array or Collection is a nullable type -- this is not allowed by the compiler
            IsEnum = ElementType.IsEnum;

            try
            {
                //Just need default ctor for generic collections?
                defaultConstructor = reflector.BuildDefaultConstructor(PropertyType);
            }
            catch
            {
                //No parameterless ctor
                //Could be custom resolved (e.g. MvcHtmlString)
            }
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

        public bool IsNullableType
        {
            get;
            protected set;
        }

        public virtual object CreateInstance()
        {
            return defaultConstructor == null ? null : defaultConstructor();
        }


        public string FullName
        {
            get;
            protected set;
        }


        public bool IsDictionary
        {
            get;
            protected set;
        }


        public Type KeyType
        {
            get;
            protected set;
        }

        public void PopulateDictionary(IEnumerable<KeyValuePair<object, object>> kvps, object dictionary)
        {
            populateDictionary(kvps, dictionary);
        }
    }
}
