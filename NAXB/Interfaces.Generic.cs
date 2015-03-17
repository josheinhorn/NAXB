using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq.Expressions;

namespace XBind.Interfaces.Generic
{
    //Consider the benefits of generics here. It simplifies the code closer to the client but complicates
    //the inner workings a lot because can't have generic collections of objects with different generics
    //PRO: More intuitive, don't need to pass lots of Type parameters
    //CON: Creates a lot of complexity in the implementation

    //TODO: Where should we actually store nested bindings? If an XML Binder is used for a single type, 
    //should we just re-build the binding whenever it's found in a nested property? Or allow some way
    //for a Binder of one type to access a Binder of another type. That can only happen with some sort
    //of Dependency Injection if we want IoC instead of Static methods. We could start off with a global
    //Static container and later re-factor to allow for IoC. Problem is the client needs to access everything
    //through that Static container.

    //Could try something like have a BindingResolver that is passed a Type and either creates a new Binding
    //for nested types or retrieves an existing one. How to deal with a circular reference though? The resolver
    //could only be called whenever the Binding is required, and couldn't be cached in the Binding itself

    public interface IXmlModelBinding<T>
    {
        IList<IXmlProperty<T>> Properties { get; }
        object CreateInstance(params object[] ctorArgs); //Do we need both this method and a delegate? Could just have a delegate behind the scenes
        //Requirement of user would be to enter the params in the right order and the right # of params??
        //The framework will have to make a decision on which Ctor to create, or all can be created
        Func<object[], object> CreateInstance { get; } //e.g. CreateInstance("hello","world");
        //Use Reflection.Emit to build a proper constructor -- will take in an array of objects and
        //figure out which ones to use, the order, and pass them to the Ctor of a concrete implementation

        //It is impossible to get a basic Ctor here unless we constrain client to only use object with parameterless
        //constructors. Therefore we have an unknown number of parameters -- need to Emit, but which Ctor to emit if multiple?

        string Name { get; } //Sorta redundant -- taken right from typeof(T).Name
        //Type Type { get; }
        IList<IXmlModelAttribute> Attributes { get; }
    }
    /// <summary>
    /// A Binder for a single Type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IXmlBinder<T> //Name -- Factory, Marshaler, Serializer? Not actualy a serializer
    {
        //void SetBinding(IXmlModelBinding<T> modelBinding); //Use Ctor injection instead of a set method -- better for Dependency Injection
        T BindToModel(IXmlData xmlData); //Call this Deserialize? It's not truly deserialization, it's just populating an object based on bindings
        void SetPropertyValue(T model, IXmlData data, IXmlProperty<T> property);
        void SetPropertyValue<T, TProp>(T model, IXmlData data, Expression<Func<T, TProp>> propertyLambda); //The property's associated binding should already be set in this object
        //Should the binding even be publicly exposed? could be a Protected property of an abstract class instead
        //IXmlModelBinding<T> XmlModelBinding { get; } //Should this be a property or a method? Will we ever need more parameters?
    }

    public interface IXmlBindingResolver
    {
        //This is an alternative to new XmlBinding<T>(). This gives us more flexibility than normal object instantiation (but you still need new XmlBindingFactory() somewhere, can be done with DI)
        IXmlModelBinding<T> ResolveBinding<T>(); //If you don't have the generic type you can always use .MakeGenericMethod
    }
    public interface IPropertyInfo<TModel>
    {
        string Name { get; }
        bool IsArray { get; }
        bool IsGenericCollection { get; }
        bool IsEnumerable { get; }
        Type PropertyType { get; }
        Type ElementType { get; } //T for ICollection<T> or T[], or same as PropertyType. IEnumerable<T> not supported -- would have to rely entirely on Linq to build property
        Type DeclaringType { get; } //Should be typeof(TModel)
        Action<IEnumerable, ICollection> ToCollection { get; } //it's actually ICollection<T> but we don't know T
        Action<IEnumerable, Array> ToArray { get; }
    }
    public enum ElementType
    {
        Text, //string (maybe also StringBuilder?)
        Number, //short, int, long, double, byte, sbyte, decimal, ushort, uint, ulong
        Bool, //bool
        DateTime, //DateTime, DateTimeOffset
        Enum, //enum
        Complex, //Nested type -- this is where the magic happens
        XmlFragment //Raw XML data
        
    }
 
    public interface IXmlProperty<TModel> //Is it actually possible to assign this generic? Should be -- one Binder per Type
    {
        IList<IXmlPropertyBinding> Bindings { get; }
        IPropertyInfo<TModel> PropertyInfo { get; }
        /// <summary>
        /// Setter delegate
        /// </summary>
        Action<TModel, object> Set { get; }
        /// <summary>
        /// Getter delegate
        /// </summary>
        Func<TModel, object> Get { get; }
        //IXmlModelBinding<TProp> ComplexTypeBinding { get; } //how the heck to get TProp? Can't have TProp in the generic of this class due to collection constraints
        /// <summary>
        /// Delegate for retrieving the Binding for a nested Complex Type. The actual Binding itself is not stored as part of this object.
        /// </summary>
        Func<IXmlBindingResolver, object> GetComplexBinding { get; } //object is really IXmlModelBinding for the nested type
    }

    public interface IXmlPropertyBinding //Can't use generic because this will be used as Custom Attribute
    {
        string ElementName { get; } //can refer to an Attribute or a XML Node
        INamespace Namespace { get; }
        //Should the binding be responsible for getting property values or should it all happen
        //in the builder? If it happens in the builder, the framework isn't easily extensible
        IEnumerable<IXmlData> GetPropertyValues<TModel>(IXmlData xmlData, IPropertyInfo<TModel> propertyInfo, IXPathEngine engine); //Do we also need the Property passed?
    }

    public interface IXPathPropertyBinding : IXmlPropertyBinding //Can't use generic because this will be used as Custom Attribute
    {
        string XPath { get; }

    }

    public interface IXmlModelAttribute
    {
        string RootElementName { get; }
        INamespace[] Namespaces { get; }
    }
    public interface INamespace
    {
        string Prefix { get; }
        string Fullname { get; }
    }

    public interface IXmlQualifiedName
    {
        string QualifiedName { get; }
        INamespace Namespace { get; }
    }

    public interface IXPathEngine
    { 
        //Needs more research
    }

    public interface IXPathable
    {
        //Needs more research
    }

    public interface IXPath //really can just be a string right?
    {
        //Needs more research
    }

    public interface IXmlData : IDisposable
    {
        object BaseData { get; }
        Stream XmlStream { get; } //Perhaps change to StreamReader or something more appropriate

        //Imitate other XML frameworks? E.g. Nodes, Namespaces, etc.
        //Needs more research
    }


}
