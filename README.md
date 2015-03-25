# NAXB
##Introduction
.NET Attribute XML Binding. This is a framework that allows you to bind any XML to .NET Objects by specifying XPaths using Custom Attributes on Properties.

This project aims to solve a problem that has been solved many ways: how to efficiently bind XML to an in-memory data structure/object on a property by property basis without completely coupling the data structure to the XML Schema. This problems has been solved by Java's MOXy (a JAXB implementation) and XStream by binding individual properties/fields to XML values by adding XPath annotations to objects, but no such generic and robust solution has ever been developed for .NET. 

In .NET, the go-to solution is most often XML Serialization. This has the unfortunate side effect of coupling implementations to the XML structure. NAXB will allow developers to create fully de-coupled solutions that rely solely on relative XPaths to map data to CLR Objects.

To remain competitive with serialization performance, NAXB parses XML and evaluates XPaths using [VTD XML](http://vtd-xml.sourceforge.net/), a highly efficient byte-based XML Processing framework. Additionally, NAXB takes advantage of a number of optimization techniques using the `System.Reflection.Emit` and `System.Linq.Expressions` namespaces instead of relying on repeated run-time reflection to process objects (e.g. create new instances, get custom attributes, set property/field values, etc.).

## Use Cases
The most typical use case of this library is to reduce the amount of time it takes for developers to bind semi-arbitrary XML to CLR objects. More specifically, it allows for using XML data without full control over or even full knowledge of the XML schema. More succinctly, it aids in developing loosely coupled applications.

An example of this may be developing an application that consumes an external web services that outputs data as XML. Your application has no control over the schema of that XML but may wish to bind to it in specific ways. With NAXB, your application can consume the XML with only cursory knowledge of the structure.

Another equally important use of NAXB is the ability to flatten, filter, and/or process source data using nothing but XPath statements. This feature is akin to using XSLT, but NAXB streamlines the process and makes it more familiar to .NET developers. For example, the following property is mapped only to contacts that are brothers of the current person:
````C#
[XPath("contacts/person[@relationship=\"brother\"]")]
public List<Person> Brothers { get; set; }
````
An additional use case is the ability to use XPath functions to perform business logic on the XML data instead of writing many lines of code to do the same thing. For example, the following property tells us the total number of coworker contacts:
````C#
[XPath("count(contacts/person[@relationship=\"coworkers\")")]
public int NumberOfCoworkers { get; set; }
````

## Using NAXB
The below is a simple example of a class decorated with NAXB attributes:
````C#
[XmlModelBinding]
[NamespaceDeclaration(Uri = "http://example.com", Prefix = "ex")]
public class Person
{
    [XPath("count(contacts/person)")]
    public int NumberOfContacts { get; set; }

    [XPath("count(contacts/person)=count(emails/email)")]
    public bool? ContactsEqualToEmails; //Nullable type

    [XPath("firstName")]
    public string FirstName { get; set; }

    [XPath("lastName")]
    public string LastName { get; set; }

    [XmlElement("middleName")]
    public string MiddleName;

    [XPath("emails/email")]
    public List<Email> Emails { get; set; }

    [XPath("address[@type=\"mailing\"]/line")]
    public List<string> MailingAddressLines { get; set; }

    [XPath("address[@type=\"billing\"]/line")]
    public List<string> BillingAddressLines { get; set; }

    [XPath("contacts/person")]
    public Person[] Contacts { get; set; }

    [XmlAttribute("role")]
    [CustomFormat(IgnoreCase=true)]
    public Role? Role { get; set; } //Enum

    [XPath("contacts/person[@relationship=\"brother\"]")]
    public List<Person> Brothers { get; set; }

    [XmlElement("DOB")]
    [CustomFormat(DateTimeFormat = "dd-MM-yyyy")]
    public DateTime? DateOfBirth; //Nullable type
}
````
A couple of things to note about using NAXB:
+	No need for specify how each property should be parsed -- this is inferred based on the Type of the property or field
+	Currently, the NAXB decorated class and all property/field Types must have parameterless constructors
  +	Constructor does not need to be public
  +	Doesn't apply to value types such as string, int, bool, etc.
+	Properties must have a `set` method
+	Properties/fields do not need to be public
+	Custom property resolvers and formats can be specified

### Supported Property/Field Types
The below are supported "out of the box" types. NAXB can be extended using a Custom Resolver (discussed later).
+	Nested NAXB models
+	Concrete implementations of `ICollection<T>` (e.g. `List<T>`, `SortedList<T>`, etc.)
+	Arrays
+	Nullable types
+	Enums
+	Primitive types
+	DateTime and DateTimeOffset
+	Guid

### XPath Attribute
The XPath attribute is the primary reason for using NAXB. It takes a single string argument representing an XPath expression. The XPath expression may be a node set selection or a function expression.
````C#
[XPath("xpath/goes/here")]
protected string XPathedProperty { get; set; }
````

### XmlElement and XmlAttribute Attributes
For simpler readability and maintainability, you may also specify XML Element or Attribute properties. These are functionally equivalent to using the XPath attribute, though are more semantic.
```C#
[XmlElement("elementName")] //Equivalent to XPath("elementName")
public string Element;

[XmlAttribute("attributeName")] //Equivalent to XPath("@attributeName")
public string Attribute;
````

### XmlModelBinding Attribute
To declare that a class can be bound to XML using NAXB, you must include the `XmlModelBindingAttribute`.
````C#
[XmlModelBinding]
public class TrapperKeeper
{
    ...
}
````
### Namespaces
Namespaces are supported by NAXB and are added to each class using the `NamespaceDeclarationAttribute`. This attribute may be used multiple times on a single class:
````C#
[XmlModelBinding]
[NamespaceDeclaration(Uri = "http://example.com", Prefix = "ex")]
[NamespaceDeclaration(Uri = "http://cars.com", Prefix = "car")]
[NamespaceDeclaration(Uri = "http://trucks.com", Prefix = "tr")]
public class Automobile
{
    ...
}
````
### Custom Formatting and Resolving
There are multiple options for custom formats and even custom property resolving. The interface `ICustomFormatBinding` can be implemented to override default format or resolving settings. This interface provides the application with one or more of the following:
+	System.IFormatProvider
+	Date Time format string (see [Custom Date and Time Format Strings guide](https://msdn.microsoft.com/en-us/library/8kb3ddd4%28v=vs.110%29.aspx) for more info)
+	Ignore case setting (for parsing enums)
+	ICustomBindingResolver (see below)

The out of the box implementation of this interface is `CustomFormatAttribute`. This implementation allows for setting of the `IFormatProvider` via a Culture Name string and the `System.Globalization.CultureInfo` class. To set a custom binding resolver, the Type can be specified e.g. `CustomBindingResolverType = typeof(MyResolver)`. The Type specified must have a parameterless constructor and must implement `ICustomBindingResolver`.

#### Custom Binding Resolver
In order to completely override the base functionality for resolving a single property/field, you can provide a custom binding resolver by implementing `ICustomBindingResolver`. 

An example for a reason to do this is to read XML "Yes"/"No" values into boolean values since the base implementation uses strict boolean parsing so only "true"/"false" would be converted to `true` and `false` out of the box.

### Performing Runtime XML Binding
So after you've properly decorated your class, you need to actually generate your CLR object using XML Data. This is where the XML Factory, XML Binder, and XML Binding Resolvers come in. Below is an example that uses all the base implementations. Note that the below does not use a singleton instance (e.g. through dependency injection), which is **required to reap the performance benefits of NAXB**.

````C#
using NAXB.Build;
using NAXB.Interfaces;
using NAXB.VtdXml;
...

IXPathProcessor processor = new VtdXPathProcessor();
IXmlFactory factory = new VtdXmlFactory();
IReflector reflector = new Reflector();
IXmlBindingResolver resolver = 
	new XmlBindingResolver(reflector, processor, new Assembly[] { this.GetType().Assembly });
//You'll want to have a singleton of the binder instance to take advantage of optimizations
IXmlModelBinder binder = new XmlBinder(resolver, processor, reflector);

//Multiple methods for creating XML Data are supported
IXmlData personXmlData = factory.CreateXmlData("path/to/person.xml"); 
Person model = Binder.BindToModel<Person>(personXmlData);
//Do something with the model
...
````
So as you can see, you need to pass in an array of Assemblies. These Assemblies must contain all the NAXB Models that you wish to use (such as `Person` and `Email` in the examples). The Binding Resolver processes all the Types in the Assemblies and assembles the NAXB bindings based on the attributes discussed earlier. Note that this is a very CPU intensive process and is the source of all runtime optimizations, so a **singleton instance of the XML Binder object is high encouraged**.


























