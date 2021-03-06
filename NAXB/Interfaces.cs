﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq.Expressions;

namespace NAXB.Interfaces
{
    //Consider the benefits of generics here. It simplifies the code closer to the client but complicates
    //the inner workings a lot because can't have generic collections of objects with different generics
    //PRO: More intuitive, don't need to pass lots of Type parameters
    //CON: Creates a lot of complexity in the implementation

    public interface IXmlModelBinding
    {
        IList<IXmlProperty> Properties { get; }
        object CreateInstance(params object[] ctorArgs); //Do we need this method??
        object CreateInstance();
        //Requirement of user would be to enter the params in the right order and the right # of params??
        //The framework will have to make a decision on which Ctor to create, or all can be created and chosen based on the params? Lot of work, start
        //with parameterless constructor support for now

        //It is impossible to get a basic Ctor here unless we constrain client to only use object with parameterless
        //constructors. Therefore we have an unknown number of parameters -- need to Emit, but which Ctor to emit if multiple?
        Type ModelType { get; }
        string Name { get; } 
        IXmlModelDescription Description { get; }
        INamespace[] Namespaces { get; }
        //Don't appear to need this, right?
        //IXPathProcessor XPathProcessor { get; } //To save compiled XPaths, etc.
    }

    public interface INamespaceBinding
    {
        string Uri { get; }
        string Prefix { get; }
    }
    /// <summary>
    /// A Binder for a single Type
    /// </summary>
    public interface IXmlModelBinder //Name?
    {
        object BindToModel(IXmlData xmlData); //infer the Type based on the model data -- advanced, perhaps Version 2.0, requires pre-loading types
        object BindToModel(IXmlModelBinding binding, IXmlData xmlData); //Mostly for internal use
        object BindToModel(Type modelType, IXmlData xmlData);
        T BindToModel<T>(IXmlData xmlData);
        void SetPropertyValue(object model, IXmlData data, IXmlProperty property);
        void SetPropertyValue<TModel, TProp>(TModel model, IXmlData data, Expression<Func<TModel, TProp>> propertyLambda); //The property's associated binding should already be set in this object
    }

    public interface IXmlBindingResolver
    {
        //Should hold a record of all the bindings for lookup
        //void LoadBindings(params Assembly[] assemblies);
        IXmlModelBinding ResolveBinding<T>();
        IXmlModelBinding ResolveBinding(Type type); //If you don't have the generic type you can use this
    }
    public interface IPropertyInfo //TODO: Merge this entire interface into IXmlProperty
    {
        string Name { get; }
        string FullName { get; }
        object CreateInstance(); //for collections
        bool IsArray { get; }
        bool IsGenericCollection { get; }
        bool IsEnumerable { get; }
        bool IsEnum { get; } //Enumerated type
        bool IsDictionary { get; }
        Type PropertyType { get; }
        Type KeyType { get; } //For Dictionary
        Type ElementType { get; } //T for ICollection<T>, T[], IDictionary<TKey, T> or same as PropertyType. IEnumerable<T> not supported -- would have to rely entirely on Linq to build property
        Type DeclaringType { get; }
        /// <summary>
        /// Gets a delegate that takes an IEnumerable&lt;T&gt; and populates an ICollection&lt;T&gt; where T is ElementType
        /// </summary>
        /// <remarks>This is necessary because the actual generic is unknown.</remarks>
        void PopulateCollection(IEnumerable enumerable, object collection); //it's actually To PropertyType : ICollection<T> but we don't know PropertyType or T
        /// <summary>
        /// Converts IEnumerable&lt;T&gt; to T[] where T is current ElementType
        /// </summary>
        /// <remarks>This is necessary because the actual generic is unknown.</remarks>
        Array ToArray(IEnumerable enumerable);
        void PopulateDictionary(IEnumerable<KeyValuePair<object, object>> kvps, object dictionary);
    }
    public interface IBindableCollection //Future use: can use this if Array or ICollection<T> isn't sufficient
    {
        Type ElementType { get; }
        void AddToCollection(object item);
    }
    public enum PropertyType //Where to use this...
    {
        Text, //string (maybe also StringBuilder?)
        Number, //short, int, long, float, double, byte, sbyte, decimal, ushort, uint, ulong
        Bool, //bool
        DateTime, //DateTime, DateTimeOffset
        Enum, //enum
        Complex, //Nested type -- this is where the magic happens
        XmlFragment //Raw XML data
    }

    public interface IXmlProperty //It is possible to use generics but it breaks down when getting nested types because property type can't be assgned as generic
    {
        INAXBPropertyBinding Binding { get; }
        IPropertyInfo PropertyInfo { get; }
        /// <summary>
        /// Gets the value of this property given an object that contains this property.
        /// </summary>
        /// <param name="obj">Object that contains this property.</param>
        /// <returns>The value of this property for the supplied object.</returns>
        /// <remarks>Underlying implementation should use a delegate created from the Type.</remarks>
        object Get(object obj);
        /// <summary>
        /// Sets the value of this property given an object that contains this property.
        /// </summary>
        /// <param name="obj">Object that contains this property.</param>
        /// <param name="value">Value to set property to.</param>
        /// <remarks>Underlying implementation should use a delegate created from the Type.</remarks>
        void Set(object obj, object value);

        object GetPropertyValue(IXmlData data, IXPathProcessor xPathProcessor, IXmlModelBinder binder); //underlying implementation should use a delegate that is created by Initialize method

        void Initialize(IXmlBindingResolver resolver, IXPathProcessor xPathProcessor, INamespace[] namespaces); //Needs to be called to initialize complex binding and compiled XPath

        //IXmlModelBinding ComplexBinding { get; } //returns null if it either hasn't been set yet, or the property type is not complex

        ICustomFormatBinding CustomFormatBinding { get; }

        PropertyType Type { get; }
    }

    /// <summary>
    /// A Custom Formatting for an XML Property
    /// </summary>
    public interface ICustomFormatBinding
    {
        //TODO: Find a good way to get DateTimeFormat/IgnoreCase from the XML itself
        //For custom formats and/or custom binding resolving
        /// <summary>
        /// Custom Date Time Format
        /// </summary>
        string DateTimeFormat { get; }
        /// <summary>
        /// Ignore case when parsing values in to Enums
        /// </summary>
        bool IgnoreCase { get; }
        /// <summary>
        /// Get a Format Provider for parsing values
        /// </summary>
        /// <returns>Format Provider</returns>
        IFormatProvider GetFormatProvider(); //Base implementation can hide delegate default constructor of Type, should return null if none exists
        /// <summary>
        /// Get a Custom Resolver for processing XML
        /// </summary>
        /// <returns>Custom Resolver</returns>
        ICustomBindingResolver GetCustomResolver(); //Base implementation can hide delegate default constructor of Type, should return null if none exists
        /// <summary>
        /// Initialize this Custom Formatting for use
        /// </summary>
        /// <param name="reflector">Reflector to help initialization</param>
        /// <param name="propertyType">If a custom resolver is initialized, should return the expected type for the property.</param>
        /// <returns>True if a Custom Binding Resolver was initialized, otherwise false.</returns>
        bool InitializeResolver(IReflector reflector, out PropertyType propertyType); //Initialize ctor of resolver or other stuff -- alternative to ctor since this is an attribute
    }

    /// <summary>
    /// Custom Binding Resolver used to resolve XML Data into the final property/field value
    /// </summary>
    public interface ICustomBindingResolver 
    {
        //Allows overriding the standard GetPropertyValue to convert values in non-standard way
        //Example: convert "yes"/"no" to bool
        /// <summary>
        /// Resolve the XML Data into the final property value
        /// </summary>
        /// <param name="data">XML Data</param>
        /// <param name="binder">Model Binder</param>
        /// <returns>Property value</returns>
        object GetPropertyValue(IEnumerable<IXmlData[]> data, IXmlModelBinder binder); //Should IXmlProperty be passed?
        bool TryGetPropertyValue(IEnumerable<IXmlData[]> data, IXmlModelBinder binder, out object propertyValue);  //Should IXmlProperty be passed?
    }

    #region Reflection
    public interface IReflector
    {
        /// <summary>
        /// Builds the default parameterless constructor for the specified type.
        /// </summary>
        /// <param name="type">Type to build constructor for.</param>
        /// <returns>A delegate that creates a new instance of the specified type.</returns>
        Func<object> BuildDefaultConstructor(Type type);
        /// <summary>
        /// Builds a constructor based on the specified constructor info. The constructor may have parameters.
        /// </summary>
        /// <param name="ctorInfo">Constructor Info</param>
        /// <returns>A delegate that creates a new instance of the specified type taking arguments of the Types specified in the Constructor Info.</returns>
        /// <remarks>When using the delegate, the order and number of arguments supplied in the object array must exactly match the 
        /// Parameters of the supplied Constructor Info.</remarks>
        Func<object[], object> BuildConstructor(ConstructorInfo ctorInfo);
        /// <summary>
        /// Builds a Setter delegate for the given Property
        /// </summary>
        /// <param name="propertyInfo">Property - must have a Set method</param>
        /// <returns>Setter delegate action</returns>
        Action<object, object> BuildSetter(PropertyInfo propertyInfo);
        /// <summary>
        /// Builds a Getter delegate for a given Prpoerty
        /// </summary>
        /// <param name="propertyInfo">Property</param>
        /// <returns>Getter delegate function</returns>
        Func<object, object> BuildGetter(PropertyInfo propertyInfo);
        /// <summary>
        /// Gets the PropertyInfo for a Lambda Expression
        /// </summary>
        /// <typeparam name="TSource">The source Type</typeparam>
        /// <typeparam name="TProperty">The property Type</typeparam>
        /// <param name="propertyLambda">Lambda Expression representing a Property of the source Type</param>
        /// <returns>Property Info</returns>
        MemberInfo GetPropertyOrFieldInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda);
        /// <summary>
        /// Gets the PropertyInfo for a Lambda Expression
        /// </summary>
        /// <param name="source">Source object - for type inferrence of the Lambda Expression</param>
        /// <typeparam name="TSource">The source Type</typeparam>
        /// <typeparam name="TProperty">The property Type</typeparam>
        /// <param name="propertyLambda">Lambda Expression representing a Property of the source Type</param>
        /// <returns>Property Info</returns>
        MemberInfo GetPropertyOrFieldInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda);
        /// <summary>
        /// Builds a delegate that will populate and existing collection of the specified type with values from an Enumerable. 
        /// The input Type must implement ICollection&lt;&gt;
        /// </summary>
        /// <typeparam name="TCollection">Type of collection. Must implement ICollection&lt;&gt;</param>
        /// <returns>Delegate function that takes two parameters: the collection and the item to add to it.</returns>
        Action<IEnumerable, object> BuildPopulateCollection<TCollection>() where TCollection : ICollection;
        /// <summary>
        /// Builds a delegate that will populate and existing collection of the specified type with values from an Enumerable. 
        /// The input Type must implement ICollection&lt;&gt;
        /// </summary>
        /// <param name="collectionType">Type of collection. Must implement ICollection&lt;&gt;</param>
        /// <returns>Delegate function that takes two parameters: the collection and the item to add to it.</returns>
        Action<IEnumerable, object> BuildPopulateCollection(Type collectionType);
        /// <summary>
        /// Determines if a type is a generic collection (ICollection&lt;T&gt;)
        /// </summary>
        /// <param name="type">Type to inspect</param>
        /// <param name="genericType">Returns the Generic type T for ICollection&lt;T&gt;, otherwise it is null</param>
        /// <returns>True if this is a generic collection</returns>
        bool IsGenericCollection(Type type, out Type genericType);
        bool IsNullableType(Type type, out Type elementType);
        /// <summary>
        /// Determines if a type is an Array
        /// </summary>
        /// <param name="type">Array to inspect</param>
        /// <param name="elementType">Returns a single element's Type if this is an array, otherwise null.</param>
        /// <returns>True if this is an Array.</returns>
        bool IsArray(Type type, out Type elementType);
        /// <summary>
        /// Determines if a type is an enumerable (IEnumerable)
        /// </summary>
        /// <param name="type">Type to inspect</param>
        /// <returns>True is this is an enumerable</returns>
        bool IsEnumerable(Type type);
        /// <summary>
        /// Builds a re-usable function for converting an IEnumerable to an Array of the specified Type.
        /// </summary>
        /// <remarks>Useful when the generic type of IEnumerable is unknown at compile time.</remarks>
        /// <param name="elementType">Single element type of the array</param>
        /// <returns>Function for converting an IEnumerable to an Array</returns>
        Func<IEnumerable, Array> BuildToArray(Type elementType);
        /// <summary>
        /// Builds a re-usable function for converting an IEnumerable to an Array of the specified Type.
        /// </summary>
        /// <remarks>Useful when the generic type of IEnumerable is unknown at compile time.</remarks>
        /// <typeparam name="TElement">Single element type of the array</param>
        /// <returns>Function for converting an IEnumerable to an Array of TElement</returns>
        Func<IEnumerable, Array> BuildToArray<TElement>();

        Action<object, object> BuildSetField(FieldInfo field);

        Func<object, object> BuildGetField(FieldInfo field);

        Type GetFieldOrPropertyType(MemberInfo member);

        Func<string, object> BuildSingleParser(
            Type elementType,
            ICustomFormatBinding format,
            out PropertyType propertyType);

        Func<string[], object> BuildMultiParser(
            Type elementType,
            ICustomFormatBinding format,
            int argCount,
            out PropertyType[] propertyTypes);

        bool IsGenericDictionary(Type type, out Type keyType, out Type valueType);
        Action<IEnumerable<KeyValuePair<object, object>>, object> BuildPopulateDictionary(Type dictionaryType);
    }
    //public interface IMethodSignature
    //{
    //    Type[] ParameterTypes { get; }
    //    Type ReturnType { get; }
    //    Type[] GenericTypeArguments { get; } //This shouldn't be a type array!
    //}

    //public interface IConstructor : IMethodSignature
    //{
    //    Func<object[], object> ConstructInstance { get; }
    //    ConstructorInfo ConstructorInfo { get; }
    //}
    //public interface IDependencyResolver //Don't think I need this...
    //{
    //    T CreateInstance<T>(params object[] ctorArgs);
    //    object CreateInstance(Type type, params object[] ctorArgs);
    //}
    #endregion

    public interface INAXBPropertyBinding  //Can't use generic because this will be used as Custom Attribute
    {
        //Should compiled XPath be stored here? JSE: No, better off in the Property itself, not a binding which contains user input
        string RootXPath { get; }
        string[] XPaths { get; }
        //bool IsFunction { get; }
        //Are the below 2 necessary?
        //bool IsAttribute { get; }
        //bool IsElement { get; }
    }

    public interface IXmlModelDescription
    {
        string RootXPath { get; }

    }
    public interface INamespace
    {
        string Prefix { get; }
        string Uri { get; }
    }

    public interface IXmlQualifiedName
    {
        string QualifiedName { get; }
        INamespace Namespace { get; }
    }

    public interface IXPathProcessor
    { 
        IEnumerable<IXmlData> ProcessSingleXPath(IXmlData data, IXPath xpath);

        IEnumerable<IXmlData[]> ProcessXPath(IXmlData data, IXPath root, IXPath[] xpath);
        IXPath CompileXPath(string xpath, INamespace[] namespaces, PropertyType propertyType, bool isFunction); 
    }

    public interface IXPath //really can just be a string right?
    {
        string XPathAsString { get; }
        object UnderlyingObject { get; } //e.g. AutoPilot, XPathExpression, etc.
        INamespace[] Namespaces { get; }
        XPathType Type { get; }
        bool IsFunction { get; set; }
        bool IsMultiValue { get; }
    }
    public enum XPathType
    {
        Text,
        Boolean,
        Numeric
    }

    public interface IXmlData
    {
        object BaseData { get; } //e.g. a VTDNav object
        //Should the call to Stream open a new stream if the current one is disposed?
        //When object is disposed, close the Stream, if it is requested again, re-open?
        Stream XmlAsStream { get; } //Perhaps change to StreamReader or something more appropriate? This one is problematic because it needs to be disposed of at some point
        byte[] XmlAsByteArray { get; } //Byte Array representation of XML Data
        string XmlAsString { get; } //String representation
        string Value { get; } //Inner Value
        Encoding Encoding { get; } //Encoding for byte array
    }

    public interface IXmlFactory
    {
        IXmlData CreateXmlData(string fileName, INamespace[] namespaces = null);
        IXmlData CreateXmlData(string xml, Encoding encoding, INamespace[] namespaces = null);
        IXmlData CreateXmlData(byte[] byteArray, Encoding encoding, INamespace[] namespaces = null);
        IXmlData CreateXmlData(Stream xmlStream, INamespace[] namespaces = null);
        //IXPathProcessor CreateXPathProcessor(IEnumerable<IXmlProperty> properties, IEnumerable<INamespace> namespaces); //Doesn't seem necessary -- implementers can create a new processor anyway they see fit
    }

}
