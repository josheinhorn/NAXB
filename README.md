# NAXB
##Introduction
.NET Attribute XML Binding. This is a framework that allows you to bind any XML to .NET Objects by specifying XPaths using Custom Attributes on Properties.

This project aims to solve a problem that has been solved many ways: how to efficiently bind XML to an in-memory data structure/object on a property by property basis without completely coupling the data structure to the XML Schema. This problems has been solved by Java's MOXy (a JAXB implementation) and XStream by binding individual properties/fields to XML values by adding XPath annotations to objects, but no such generic and robust solution has ever been developed for .NET. 

In .NET, the go-to solution is most often XML Serialization. This has the unfortunate side effect of coupling implementations to the XML structure. NAXB will allow developers to create fully de-coupled solutions that rely solely on relative XPaths to map data to CLR Objects.

To remain competitive with serialization performance, NAXB parses XML and evaluates XPaths using [VTD XML](http://vtd-xml.sourceforge.net/), a highly efficient byte-based XML Processing framework. Additionally, NAXB takes advantage of a number of optimization techniques using the `System.Reflection.Emit` and `System.Linq.Expressions` namespaces instead of relying on repeated run-time reflection to process objects (e.g. create new instances, get custom attributes, set property/field values, etc.).

## Use Cases
The most typical use case of this library is to reduce the amount of time it takes for developers to bind semi-arbitrary XML to CLR objects. More specifically, it allows for using XML data without full control over or even full knowledge of the XML schema. More succinctly, it aids in developing loosely coupled applications.

An example of this may be developing an application that consumes an external web service that outputs data as XML. Your application has no control over the schema of that XML but may wish to bind to it in specific ways. With NAXB, your application can consume the XML with only cursory knowledge of the structure.

Another equally important use of NAXB is the ability to flatten, filter, and/or process source data using nothing but XPath statements. This feature is akin to using XSLT, but NAXB streamlines the process and makes it more familiar to .NET developers. For example, the following property is mapped only to contacts that are brothers of the current person:
````C#
[XPath("contacts/person[@relationship=\"brother\"]")]
public List<Person> Brothers { get; set; }
````
An additional use case is the ability to use XPath functions to perform business logic on the XML data instead of writing many lines of code to do the same thing. For example, the following property tells us the total number of coworker contacts:
````C#
[XPath("count(contacts/person[@relationship=\"coworker\")")]
public int NumberOfCoworkers { get; set; }
````

## Using NAXB
The below is a simple example of a class decorated with NAXB attributes:
````C#
[XmlModelBinding]
[NamespaceDeclaration(Uri = "http://example.com", Prefix = "ex")]
public class Person
{
    [XPath("count(contacts/person)", IsFunction = true)]
    public int NumberOfContacts { get; set; }

    [XPath("count(contacts/person)=count(emails/email)", IsFunction = true)]
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
+	No need to specify how each property should be parsed -- this is inferred based on the Type of the property or field
+	Currently, the NAXB decorated classes must have parameterless constructors
	+	Constructor does not need to be public
+	Properties must have a `set` method
+	Properties/fields do not need to be public
+	Custom property resolvers and formats can be specified
+	White space is trimmed from the ends of attribute/element values

### Supported Property/Field Types
The below are supported "out of the box" types. NAXB can be extended using a Custom Resolver (discussed later).
+	Nested NAXB models
+	Concrete implementations of `ICollection<T>` (`List<T>`, `SortedList<T>`, etc.)
	+	`T` can be any of the other supported non-enumerable Types listed in this section
+	Concrete implementations of `IDictionary<TKey, TValue>`
	+	This requires special "nested" XPaths in order to select a set of Key Value Pairs, see **XPath Attribute** section
	+	Both `TKey` and `TValue` can be any of the other supported non-enumerable Types listed in this section
+	Arrays (e.g. `Person[]`, `Automobile[]`)
	+	Array element Type can be any of the other supported non-enumerable Types listed in this section
+	Nullable types (e.g. `bool?`, `Nullable<int>`)
+	Enums
+	**Simple Types**
	+	All primitive types besides `object` (`string`, `char`, `int`, `bool`, etc.)
	+	`DateTime` and `DateTimeOffset`
	+	Types with a public constructor that take a single primitive type (or Date) parameter (e.g. `Guid`, `HtmlString`, `BigInteger`)
		+	The *first* public constructor found is used. The order that NAXB looks for the single primitive type constructor:
			1.	`string`
			2.	`decimal`
			3.	`double`
			4.	`float`
			5.	`long`
			6.	`ulong`
			7.	`int`
			8.	`uint`
			9.	`short`
			10.	`ushort`
			11.	`byte`
			12.	`sbyte`
			13.	`bool`
			14.	`DateTime` (not primitive but supported)
			15.	`DateTimeOffset` (not primitive but supported)
		+	Values from the XML are parsed into the appropriate type and then passed to the corresponding constructor.
		+	If a different order is desired, use a Custom Binding Resolver.
		+	Note: `char` was specifically excluded due to causing conflicts when retrieving constructors.
+	Types with a public constructor that unambiguously takes a specified number of Simple Type arguments (see above)
	+	This requires multiple nested XPaths (see **XPath Attribute**) which determine the number of constructor arguments to search for.

### XPath Attribute
The XPath attribute is the primary reason for using NAXB. It takes a single string argument representing an XPath expression that may be a node set selection or a function expression. Whether the XPath should be evaluated as a node set or function is determined by the property/field type. Only single text, numeric, and boolean values will be evaluated as functions while other types and collections will be evaluated as node sets.
````C#
//The below is evaluated as a node set since it's a complex type
[XPath("xpath/goes/here")]
protected ComplexType XPathedProperty { get; set; }

//This is evaluated as a function since the type is a single number
[XPath("count(some/stuff)"]
public int FunctionProperty { get; set; }
````
The XPath attribute also allows for something called Nested XPaths. Nested XPaths are for selecting *multiple* values from the same node set. To use this, you specify the node set to iterate on, and then any number of nested XPath expressions that are evaluated on the root node set.

#### Dictionaries with Nested XPaths
````C#
[XPath("contacts/person", "concat(firstName, ' ', lastName)", "emails/email")]
public Dictionary<string, Email> EmailsByName;
````
In the above example, the `contacts/person` XPath will be iterated on and Key Value Pairs of the combined first and last name and associated Email (first) will be extracted from the resulting node set. Note that the `TValue` Type is a complex NAXB decorated type (`Email`), and the same can apply the Key type as well (though not in the above example). Also, it is *required* that an `IDictionary` Property/Field Type have exactly 2 Nested XPaths - the first for the Key and the second for the Value. Currently, Key conflicts will be resolved on a strict first-key-takes-precedence basis. Future versions will allow this to be configurable.

#### Multi-argument Constructor Types with Nested XPaths
````C#
[XPath("contacts/person", "@id", "emails/email[1]/@address", "count(paymentTypes/creditCard)", "DOB")]
public List<Contact> Contacts;
````
````C#
public struct Contact
{
	public Contact(int id, 
		string primaryEmailAddress,
		short numberOfCreditCards,
		DateTime dateOfBirth)
	{
		Id = id;
		PrimaryEmailAddress = primaryEmailAddress;
		NumberOfCreditCards = numberOfCreditCards;
		DateOfBirth = dateOfBirth;
	}
	public int Id { get; private set; }
	public string PrimaryEmailAddress { get; private set; }
	public short NumberOfCreditCards { get; private set; }
	public DateTime DateOfBirth { get; private set; }
}
````
The above example demonstrates that `XPathAttribute` also supports Types with constructors that take multiple primitive type arguments instead of being decorated with NAXB Attributes. It is important to note that there must not be more than one constructor with the same number of arguments such as `public Contact(int id, string name)` and `public Contact(Guid id, string name)`. The number of constructor arguments must also exactly match the number of Nested XPaths (4 in the example above). Remember that the number of Nested XPaths does not include the initial node set XPath.

### XmlElement and XmlAttribute Attributes
For simpler readability and maintainability, you may also specify XML Element or Attribute properties. These are functionally equivalent to using the XPath attribute, though are more semantic.
```C#
[XmlElement("elementName")] //Equivalent to XPath("elementName")
public string Element;

[XmlAttribute("attributeName")] //Equivalent to XPath("@attributeName")
public string Attribute;
````

### XmlModelBinding Attribute
To declare that a class can be bound to XML using NAXB, you must include the `XmlModelBindingAttribute`. Optionally, you can use the `RootXPath` property to specify an XPath that will be appended to the beginning of all the XPaths of the class's properties/fields. This is useful to cut down on repetitive XPaths.
````C#
[XmlModelBinding(RootXPath = "common/xpath/for/properties")]
public class Unicorn
{
    ...
}
````
**WARNING**: If a root document reference is used here (leading slash), a stack overflow exception might occur if a model is recursively nested within itself. For example, the below will cause a stack overflow because it is a circular reference. This can be avoided by either assuring there is no circular reference, or not using a leading slash ("/") on the circularly referenced class.
````C#
[XmlModelBinding(RootXPath = "/person")] //A reference to the root of the document
public class Person
{
	//Circular reference that perpetually calls the root of the document
    [XPath("contacts/person")]
    public Person[] Contacts { get; set; } 
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
+	Date/time format string (see [Custom Date and Time Format Strings guide](https://msdn.microsoft.com/en-us/library/8kb3ddd4%28v=vs.110%29.aspx) for more info)
+	Ignore case setting (for parsing enums)
+	ICustomBindingResolver (see below)

Example:
````C#
[XmlElement("DOB")]
[CustomFormat(DateTimeFormat = "dd-MM-yyyy")]
public DateTime DateOfBirth;
````
The out of the box implementation of this interface is `CustomFormatAttribute`. This implementation allows for setting of the `IFormatProvider` via a Culture Name string and the `System.Globalization.CultureInfo` class. To set a custom binding resolver, the Type can be specified e.g. `CustomBindingResolverType = typeof(MyResolver)`. The Type specified must have a parameterless constructor and must implement `ICustomBindingResolver`.

#### Custom Binding Resolver
In order to completely override the base functionality for resolving a single property/field, you can provide a custom binding resolver by implementing `ICustomBindingResolver`. 

An example for a reason to do this is to read encrypted XML element/attribute values into their actual value in the model instead of performing the decryption at a later point in the application.

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

//There are multiple overloads for CreateXmlData taking various inputs
IXmlData personXmlData = factory.CreateXmlData("path/to/person.xml"); 
Person model = Binder.BindToModel<Person>(personXmlData);
//Do something with the model
...
````
So as you can see, you need to pass in an array of Assemblies. These Assemblies must contain all the NAXB Models that you wish to use (such as `Person` and `Email` in the examples). The Binding Resolver processes all the Types in the Assemblies and assembles the NAXB bindings based on the attributes discussed earlier. Note that this is a very CPU intensive process and is the source of all runtime optimizations, so a **singleton instance of the XML Binder object is high encouraged**.
