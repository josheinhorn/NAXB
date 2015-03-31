using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NAXB.Interfaces;
using System.Globalization;

namespace NAXB.Build
{
    public class Reflector : IReflector
    {
        public virtual Action<object, object> BuildSetter(PropertyInfo propertyInfo)
        {
            //Equivalent to:
            /*delegate (object i, object a)
            {
                ((DeclaringType)i).Property = (PropertyType)a;
            }*/
            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");
            var setterCall = Expression.Call(
                                Expression.Convert(instance, propertyInfo.DeclaringType), propertyInfo.GetSetMethod(true),
                                Expression.Convert(argument, propertyInfo.PropertyType));
            return Expression.Lambda<Action<object, object>>(setterCall, instance, argument).Compile();
        }
        public virtual Func<object, object> BuildGetter(PropertyInfo propertyInfo)
        {
            //Equivalent to:
            /*delegate (object obj)
            {
                return (object)((DeclaringType)obj).Property
            }*/
            ParameterExpression obj = Expression.Parameter(typeof(object), "obj");
            var getterCall = Expression.Convert(
                                Expression.Call(
                                    Expression.Convert(obj, propertyInfo.DeclaringType), propertyInfo.GetGetMethod(true)),
                            typeof(object));
            return Expression.Lambda<Func<object, object>>(getterCall, obj).Compile();
        }
        public virtual MemberInfo GetPropertyOrFieldInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            //Type type = typeof(TSource);
            if (propertyLambda == null) throw new ArgumentNullException("propertyLambda");
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            MemberInfo result = member.Member as PropertyInfo;
            if (result == null)
                result = member.Member as FieldInfo;
            //throw new ArgumentException(string.Format(
            //    "Expression '{0}' refers to a field, not a property.",
            //    propertyLambda.ToString()));

            //Commented this out because it uses reflection
            //if (type != propInfo.ReflectedType &&
            //    !type.IsSubclassOf(propInfo.ReflectedType))
            //    throw new ArgumentException(string.Format(
            //        "Expression '{0}' refers to a property that is not from type {1}.",
            //        propertyLambda.ToString(),
            //        type));

            return result;
        }
        public virtual MemberInfo GetPropertyOrFieldInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return GetPropertyOrFieldInfo<TSource, TProperty>(propertyLambda);
        }

        public virtual bool IsEnumerable(Type type)
        {
            //It's IEnumerable but NOT a string (which is an enumerable of chars)
            return typeof(IEnumerable).IsAssignableFrom(type) && !typeof(string).IsAssignableFrom(type);
        }

        public virtual bool IsArray(Type type, out Type elementType)
        {
            bool result = false;
            if (typeof(Array).IsAssignableFrom(type))
            {
                result = true;
                elementType = type.GetElementType();
            }
            else elementType = null;
            return result;
        }

        public virtual bool IsGenericCollection(Type type, out Type genericType)
        {
            //This actually returns true for an array
            var generics = type.GetGenericArguments();
            genericType = generics.Length > 0 ? generics[0] : null;
            return (genericType != null
                && typeof(ICollection<>).MakeGenericType(genericType).IsAssignableFrom(type));
        }

        public virtual bool IsNullableType(Type type, out Type elementType)
        {
            var generics = type.GetGenericArguments();
            elementType = generics.Length > 0 ? generics[0] : null;
            return (elementType != null
               && typeof(Nullable<>).MakeGenericType(elementType).IsAssignableFrom(type));
        }

        public virtual Func<IEnumerable, Array> BuildToArray(Type elementType)
        {
            //This method doesn't cache the result, assumes the caller will take care of that
            var param = Expression.Parameter(typeof(IEnumerable), "source");
            var cast = Expression.Call(typeof(Enumerable), "Cast", new[] { elementType }, param);
            var toArray = Expression.Call(typeof(Enumerable), "ToArray", new[] { elementType }, cast);
            return Expression.Lambda<Func<IEnumerable, Array>>(toArray, param).Compile();
        }

        public virtual Action<IEnumerable, object> BuildPopulateCollection<TCollection>() where TCollection : ICollection
        {
            return BuildPopulateCollection(typeof(TCollection));
        }

        public virtual Action<IEnumerable, object> BuildPopulateCollection(Type collectionType)
        {
            string methodName = "Add";
            Action<IEnumerable, object> result = null;
            Type genericType;
            if (IsGenericCollection(collectionType, out genericType)) //It has a generic type param and it implements ICollection<T>
            {
                var collectionParam = Expression.Parameter(typeof(object), "collection");
                var itemParam = Expression.Parameter(typeof(object), "item");

                MethodInfo addMethod = collectionType.GetMethod(methodName, new Type[] { genericType }); //The Add method for ICollection<T>

                //Build the Call which casts the inputs to the appropriate type and calls the add method expression
                var addCall = Expression.Call(
                                    Expression.Convert(collectionParam, collectionType), addMethod,
                                    Expression.Convert(itemParam, genericType));

                var addToCollection = Expression.Lambda<Action<object, object>>(addCall, collectionParam, itemParam).Compile();
                //addToCollection equivalent to:
                /*delegate (object collection, object item)
                {
                    ((CollectionType)collection).Add((GenericType)item);   
                }*/

                //Build the final delegate function with loops through the enumerable and adds items to the collection
                result = (IEnumerable enumerable, object collection) =>
                    {
                        foreach (var item in enumerable)
                        {
                            addToCollection(collection, item);
                        }
                    };
            }
            else if (genericType != null)
                throw new ArgumentException("The type (" + collectionType.Name + ") must implement ICollection<" + genericType.Name + ">", "collectionType");
            else throw new ArgumentException("The type (" + collectionType.Name + ") must implement ICollection<>", "collectionType");
            return result;
        }

        public virtual Func<IEnumerable, Array> BuildToArray<TElement>()
        {
            return BuildToArray(typeof(TElement));
        }

        public virtual Func<object> BuildDefaultConstructor(Type type)
        {
            Func<object> result = null;
            //Create dynamic method with correct method signature:
            //type Create_typeName(object[] args) { }
            DynamicMethod dynamicMethod =
                new DynamicMethod("Create_" + type.Name,
                type, new Type[0]);
            // Get the default constructor, public or non public
            ConstructorInfo ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[0], null); //Get parameterless constructor -- there better be one!

            // Generate the intermediate language.       
            ILGenerator ilgen = dynamicMethod.GetILGenerator();
            try
            {
                //Do nothing with the object array right now -- in the future could process them and choose an appropriate constructor
                ilgen.Emit(OpCodes.Newobj, ctor);
                ilgen.Emit(OpCodes.Ret);
                //Create delegate, cast and return
                result = (Func<object>)dynamicMethod
                    .CreateDelegate(typeof(Func<object>));
            }
            catch (Exception e)
            {
                throw new TargetException(
                String.Format("Failed to create a parameterless constructor for Type '{0}'." +
                "See inner exception for more details", type.FullName), e);
            }
            return result;
        }

        public delegate object ConstructorWithParams(params object[] args);

        public virtual Func<object[], object> BuildConstructor(ConstructorInfo ctorInfo)
        {
            //Create dynamic method with correct method signature:
            //type Create_typeName(object[] args) { }
            ParameterInfo[] ctorParams = ctorInfo.GetParameters();
            //if (ctorInfo.ContainsGenericParameters) throw new ArgumentException("Constructor must be of a non-generic type for this method.", "ctorInfo");
            //For generics would the return type of this method be Func<Type[], object[], object>?

            DynamicMethod dynamicMethod =
                new DynamicMethod("Create_" + ctorInfo.DeclaringType.Name,
                typeof(object), new Type[] { typeof(object[]) });

            // Generate the intermediate language.       
            ILGenerator ilgen = dynamicMethod.GetILGenerator();

            //Cast each argument of the input object array to the appropriate type -- the order of objects should match the order set by the Ctor
            //It is also assumed the length of object array args is same length as Ctor args. 
            //Exceptions for the delegate that mean the above weren't satisfied: InvalidCastException, IndexOutOfRangeException
            for (int i = 0; i < ctorParams.Length; i++)
            {
                ilgen.Emit(OpCodes.Ldarg_0); //Push Object array (at position 0 because this is a static method)
                ilgen.Emit(OpCodes.Ldc_I4, i); //Push the index to access
                ilgen.Emit(OpCodes.Ldelem_Ref); //Push the element at the previously loaded index
                CastOrUnbox(ilgen, ctorParams[i].ParameterType); //Cast or Box the object to the appropriate Ctor Parameter Type
            }
            Func<object[], object> result = null;
            try
            {
                ilgen.Emit(OpCodes.Newobj, ctorInfo); //Call the Ctor, all values on the stack are passed to the Ctor
                if (ctorInfo.DeclaringType.IsValueType)
                {
                    //Required to return as an object
                    ilgen.Emit(OpCodes.Box, ctorInfo.DeclaringType);
                }
                else
                {
                    //Required explicit cast to object to use the delegate
                    ilgen.Emit(OpCodes.Castclass, typeof(Object));
                }
                ilgen.Emit(OpCodes.Ret); //Return the new object
                result = (Func<object[], object>)dynamicMethod
                    .CreateDelegate(typeof(Func<object[], object>));
            }
            catch (Exception e)
            {
                throw new TargetException(
                    String.Format("Failed to create the specified constructor for Type '{0}'." +
                    "See inner exception for more details", ctorInfo.DeclaringType.FullName), e);
            }
            //Create delegate, cast and return
            return result;
        }

        public virtual Action<object, object> BuildSetField(FieldInfo field)
        {
            //Define dynamic method that takes 2 objects as arguments
            DynamicMethod dynamicMethod =
                new DynamicMethod("Set_" + field.Name,
                null, new Type[] { typeof(object), typeof(object) });

            // Generate the intermediate language.       
            ILGenerator ilgen = dynamicMethod.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0); //Assumes we are using an instance Field, not static
            CastOrUnbox(ilgen, field.DeclaringType);
            ilgen.Emit(OpCodes.Ldarg_1);
            CastOrUnbox(ilgen, field.FieldType);
            ilgen.Emit(OpCodes.Stfld, field);
            ilgen.Emit(OpCodes.Ret);
            return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
        }

        public virtual Func<object, object> BuildGetField(FieldInfo field)
        {
            //Define dynamic method that takes 2 objects as arguments
            DynamicMethod dynamicMethod =
                new DynamicMethod("Get_" + field.Name,
                typeof(object), new Type[] { typeof(object) });

            // Generate the intermediate language.       
            ILGenerator ilgen = dynamicMethod.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0); //Assumes we are using an instance Field, not static
            CastOrUnbox(ilgen, field.DeclaringType);
            ilgen.Emit(OpCodes.Ldfld, field);
            ilgen.Emit(OpCodes.Ret);
            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }

        public Type GetFieldOrPropertyType(MemberInfo member)
        {
            Type result = null;
            if (member is PropertyInfo)
            {
                result = (member as PropertyInfo).PropertyType;
            }
            else if (member is FieldInfo)
            {
                result = (member as FieldInfo).FieldType;
            }
            return result;
        }

        public Func<string[], object> BuildMultiParser(
            Type elementType,
            ICustomFormatBinding format,
            int argCount,
            out PropertyType[] propertyTypes)
        {
            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            string dateTimeFormat = null;
            bool ignoreCase = false;
            //ConstructorInfo ctor = null;
            if (format != null)
            {
                formatProvider = format.GetFormatProvider() ?? CultureInfo.InvariantCulture;
                dateTimeFormat = format.DateTimeFormat;
                ignoreCase = format.IgnoreCase;
            }
            Func<string[], object> result = null;
            var ctors = elementType.GetConstructors().Where(x => x.GetParameters().Count() == argCount);
            if (ctors.Count() != 1)
            {
                throw new ArgumentException(
                    String.Format("The number of arguments specified ({0}) does not unambiguously match the number of arguments of a single constructor for Type '{1}'."
                    , argCount, elementType.FullName));
            }
            var ctorInfo = ctors.FirstOrDefault();
            
            var parameters = ctorInfo.GetParameters();
            propertyTypes = new PropertyType[parameters.Length];
            var parsers = new Func<string,object>[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parsers[i] = BuildSingleParser(parameters[i].ParameterType,
                    format, out propertyTypes[i]);
            }
            //PropertyType temp;
            //var parsers = ctorInfo.GetParameters()
            //    .Select(x => BuildSingleParser(x.ParameterType, formatProvider, dateTimeFormat, ignoreCase, out temp))
            //    .ToArray();
            var ctor = BuildConstructor(ctorInfo);
            result = (string[] values) =>
                {
                    if (values.Length != argCount)
                    {
                        throw new ArgumentException(
                            String.Format("The number of string values ({0}) does not match the number of constructor arguments ({1}) for Type '{2}'."
                            , values.Length, argCount, elementType.FullName));
                    }
                    object[] args = new object[argCount];
                    for (int i = 0; i < argCount; i++)
                    {
                        args[i] = parsers[i](values[i]);
                    }
                    return ctor(args);
                };
            return result;
        }

        public Func<string, object> BuildSingleParser(
            Type elementType,
             ICustomFormatBinding format,
            out PropertyType propertyType)
        {
            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            string dateTimeFormat = null;
            bool ignoreCase = false;
            //ConstructorInfo ctor = null;
            if (format != null)
            {
                formatProvider = format.GetFormatProvider() ?? CultureInfo.InvariantCulture;
                dateTimeFormat = format.DateTimeFormat;
                ignoreCase = format.IgnoreCase;
            }
            Func<string, object> result = null;
            ConstructorInfo ctor = null;
            propertyType = PropertyType.XmlFragment;
            if (elementType == typeof(string))
            {
                result = (string value) => value; //no format
                propertyType = PropertyType.Text;
            }
            else if (elementType == typeof(char))
            {
                result = (string value) => char.Parse(value); //no format
                propertyType = PropertyType.Text;
            }
            else if (elementType.IsEnum)
            {
                result = (string value) => Enum.Parse(elementType, value, ignoreCase);
                propertyType = PropertyType.Enum;
            }
            else if (elementType == typeof(bool))
            {
                result = (string value) => bool.Parse(value); //no format
                propertyType = PropertyType.Bool;
            }
            else if (elementType == typeof(byte))
            {
                result = (string value) => byte.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(sbyte))
            {
                result = (string value) => sbyte.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(short))
            {
                result = (string value) => short.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(ushort))
            {
                result = (string value) => ushort.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(int))
            {
                result = (string value) => int.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(uint))
            {
                result = (string value) => uint.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(long))
            {
                result = (string value) => long.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(ulong))
            {
                result = (string value) => ulong.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(float))
            {
                result = (string value) => float.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(double))
            {
                result = (string value) => double.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(decimal))
            {
                result = (string value) => decimal.Parse(value, formatProvider);
                propertyType = PropertyType.Number;
            }
            else if (elementType == typeof(DateTime))
            {
                propertyType = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    result = (string value) => DateTime.Parse(value, formatProvider);
                else result = (string value) => DateTime.ParseExact(value, dateTimeFormat, formatProvider);
            }
            else if (elementType == typeof(DateTimeOffset))
            {
                propertyType = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    result = (string value) => DateTimeOffset.Parse(value, formatProvider);
                else result = (string value) => DateTimeOffset.ParseExact(value, dateTimeFormat, formatProvider);
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(string) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { value });
                propertyType = PropertyType.Text;
            }
            //else if ((ctor = elementType.GetConstructor(new Type[] { typeof(Char) })) != null)
            //{
            //    var ctorDel = BuildConstructor(ctor);
            //    result = (string value) => ctorDel(new object[] { char.Parse(value) });
            //    propertyType = PropertyType.Text;
            //}
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(decimal) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { decimal.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(double) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { double.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(float) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { float.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(long) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { long.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(ulong) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { ulong.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(int) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { int.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(uint) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { uint.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(short) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { short.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(ushort) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { ushort.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(byte) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { byte.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(sbyte) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { sbyte.Parse(value, formatProvider) });
                propertyType = PropertyType.Number;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(bool) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                result = (string value) => ctorDel(new object[] { bool.Parse(value) });
                propertyType = PropertyType.Bool;
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(DateTime) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                propertyType = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    result = (string value) => ctorDel(new object[] { DateTime.Parse(value, formatProvider) });
                else result = (string value) => ctorDel(new object[] { DateTime.ParseExact(value, dateTimeFormat, formatProvider) });
            }
            else if ((ctor = elementType.GetConstructor(new Type[] { typeof(DateTimeOffset) })) != null)
            {
                var ctorDel = BuildConstructor(ctor);
                propertyType = PropertyType.DateTime;
                if (String.IsNullOrEmpty(dateTimeFormat))
                    result = (string value) => ctorDel(new object[] { DateTimeOffset.Parse(value, formatProvider) });
                else result = (string value) => ctorDel(new object[] { DateTimeOffset.ParseExact(value, dateTimeFormat, formatProvider) });
            }
            return result;
        }


        public bool IsGenericDictionary(Type type, out Type keyType, out Type valueType)
        {
            keyType = null;
            valueType = null;
            bool result = false;
            var generics = type.GetGenericArguments();
            if (generics.Length == 2 && typeof(IDictionary<,>).MakeGenericType(generics).IsAssignableFrom(type))
            {
                keyType = generics[0];
                valueType = generics[1];
                result = true;
            }
            return result;
        }

        public Action<IEnumerable<KeyValuePair<object, object>>, object> BuildPopulateDictionary(Type dictionaryType)
        {
            Action<IEnumerable<KeyValuePair<object, object>>, object> result = null;
            Type keyType;
            Type valueType;
            if (IsGenericDictionary(dictionaryType, out keyType, out valueType)) //It has a generic type param and it implements IDictionary<T>
            {
                var dictionaryParam = Expression.Parameter(typeof(object), "dictionary");
                var keyParam = Expression.Parameter(typeof(object), "key");
                var valueParam = Expression.Parameter(typeof(object), "item");

                MethodInfo addMethod = dictionaryType.GetMethod("Add", new Type[] { keyType, valueType }); //The Add method for ICollection<T>

                //Build the Call which casts the inputs to the appropriate type and calls the add method expression
                var addCall = Expression.Call(
                                    Expression.Convert(dictionaryParam, dictionaryType), addMethod,
                                    Expression.Convert(keyParam, keyType),
                                    Expression.Convert(valueParam, valueType));

                var addToDictionary = Expression.Lambda<Action<object, object, object>>(addCall, dictionaryParam, keyParam, valueParam).Compile();
                //addToCollection equivalent to:
                /*delegate (object dictionary, object item)
                {
                    ((CollectionType)dictionary).Add((GenericType)item);   
                }*/

                var containsKeyMethod = dictionaryType.GetMethod("ContainsKey");
                var containsKeyCall = Expression.Call(
                    Expression.Convert(dictionaryParam, dictionaryType), containsKeyMethod,
                    Expression.Convert(keyParam, keyType));

                var containsKey = Expression.Lambda<Func<object, object, bool>>(containsKeyCall, dictionaryParam, keyParam).Compile();

                //Build the final delegate function with loops through the enumerable and adds items to the dictionary
                result = (IEnumerable<KeyValuePair<object, object>> kvps, object dictionary) =>
                {
                    foreach (var kvp in kvps)
                    {
                        if (!containsKey(dictionary, kvp.Key)) //Make the behavior configurable here
                            addToDictionary(dictionary, kvp.Key, kvp.Value);
                    }
                };
            }
            else throw new ArgumentException("The type (" + dictionaryType.Name + ") must implement IDictionary<,>", "dictionaryType");
            
            return result;

        }
        protected void CastOrUnbox(ILGenerator ilgen, Type type)
        {
            if (type.IsValueType)
            {
                ilgen.Emit(OpCodes.Unbox_Any, type);
            }
            else
                ilgen.Emit(OpCodes.Castclass, type);
        }

        //var kvpType = typeof(KeyValuePair<object,object>);
        //        var getKey = kvpType.GetProperty("Key").GetGetMethod();
        //        var getValue = kvpType.GetProperty("Value").GetGetMethod();
        //        var sourceType = typeof(IEnumerable<KeyValuePair<object, object>>);
        //        var selectKeyType = typeof(Func<,>).MakeGenericType(new Type[] { sourceType, keyType});
        //        var selectValueType = typeof(Func<,>).MakeGenericType(new Type[] { sourceType, valueType});

        //        //var toDictionaryMethod = typeof(Enumerable).GetMethod("ToDictionary", new Type[] { 
        //            //typeof(IEnumerable<>), typeof(Func<,>), typeof(Func<,>) });
        //        var enumerableType = typeof(Enumerable);
        //        var toDictionaryMethod =
        //        (
        //            from m in enumerableType.GetMethods(BindingFlags.Static | BindingFlags.Public)
        //            where m.Name == "ToDictionary"
        //            let p = m.GetParameters()
        //            where p.Length == 3
        //                && p[0].ParameterType.IsGenericType
        //                && p[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        //                && p[1].ParameterType.IsGenericType
        //                && p[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
        //                && p[2].ParameterType.IsGenericType
        //                && p[2].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
        //            select m
        //        ).SingleOrDefault();

        //        var parameterTypes = new Type[] { typeof(object), selectKeyType, selectValueType };
        //        var dynamicToDict = new DynamicMethod("toDictionary", typeof(object), new Type[] { typeof(object), selectKeyType, selectValueType });

        //        var ilGen = dynamicToDict.GetILGenerator();

        //        ilGen.Emit(OpCodes.Ldarg_0); //source
        //        ilGen.Emit(OpCodes.Ldarg_1); //select Key
        //        ilGen.Emit(OpCodes.Ldarg_2); //select Value
        //        ilGen.Emit(OpCodes.Call, toDictionaryMethod);
        //        ilGen.Emit(OpCodes.Ret);

                

        //        var source = Expression.Parameter(sourceType, "source");
        //        var lambdaParam = Expression.Parameter(kvpType, "kvp");

        //        //(KeyValuePair<object,object> kvp) => (KeyType)kvp.Key
        //        var selectKey = Expression.Convert(Expression.Call(lambdaParam, getKey), keyType);
        //        //(KeyValuePair<object,object> kvp) => (ValueType)kvp.Value
        //        var selectValue = Expression.Convert(Expression.Call(lambdaParam, getValue), valueType);
        //        //source.ToDictionary<KvpType, KeyType, ValueType>(selectKey, selectValue);
        //        var toDict = Expression.Call(typeof(Enumerable), "ToDictionary", new Type[1], null); //Can't use this because there are 2 methods with same # of generics
        //        //var toDict = Expression.Call(toDictionaryMethod, source, selectKey, selectValue);
        //        result = Expression.Lambda<Func<IEnumerable<KeyValuePair<object, object>>, object>>(toDict, source).Compile();
    }
}
