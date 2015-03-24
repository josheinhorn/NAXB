using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NAXB.Interfaces;

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

        public virtual Func<object[], object> BuildConstructor(ConstructorInfo ctorInfo)
        {
            //Create dynamic method with correct method signature:
            //type Create_typeName(object[] args) { }
            ParameterInfo[] ctorParams = ctorInfo.GetParameters();
            //if (ctorInfo.ContainsGenericParameters) throw new ArgumentException("Constructor must be of a non-generic type for this method.", "ctorInfo");
            //For generics would the return type of this method be Func<Type[], object[], object>?

            DynamicMethod dynamicMethod =
                new DynamicMethod("Create_" + ctorInfo.Name,
                ctorInfo.DeclaringType, new Type[] { typeof(object[]) });

            // Generate the intermediate language.       
            ILGenerator ilgen = dynamicMethod.GetILGenerator();

            //Cast each argument of the input object array to the appropriate type -- the order of objects should match the order set by the Ctor
            //It is also assumed the length of object array args is same length as Ctor args. 
            //Exceptions for the delegate that mean the above weren't satisfied: InvalidCastException, IndexOutOfRangeException
            for (int i = 0; i < ctorParams.Length; i++)
            {
                ilgen.Emit(OpCodes.Ldarg_0); //Push Object array
                ilgen.Emit(OpCodes.Ldc_I4, i); //Push the index to access
                ilgen.Emit(OpCodes.Ldelem_Ref); //Push the element at the previously loaded index
                CastOrUnbox(ilgen, ctorParams[i].ParameterType); //Cast or Box the object to the appropriate Ctor Parameter Type
            }
            Func<object[], object> result = null;
            try
            {
                ilgen.Emit(OpCodes.Newobj, ctorInfo); //Call the Ctor, all values on the stack are passed to the Ctor
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

        protected void CastOrUnbox(ILGenerator ilgen, Type type)
        {
            if (type.IsValueType)
                ilgen.Emit(OpCodes.Unbox_Any, type);
            else
                ilgen.Emit(OpCodes.Castclass, type);
        }
    }
}
