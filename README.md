# NAXB
.NET Attribute XML Binding. This is a framework that allows you to bind any XML to .NET Objects by specifying XPaths using Custom Attributes on Properties.

This project aims to solve a problem that has been solved many ways: how to efficiently bind XML to a CLR Object on
a property by property basis without completely coupling the CLR Object to the XML Schema. This problems has been
solved by Java's JAXB and XStream by binding individual properties/fields to XML values by adding XPath annotations
to objects, but no such generic and robust solution has ever been developed for .NET. 

In .NET, the go-to solution is most often XML Serialization. This has the unfortunate side effect of coupling implementations
to the XML structure. NAXB will allow developers to create fully de-coupled solutions that rely solely on relative XPaths
to map data to CLR Objects.

This project is currently in the early development phase -- contact josh.einhorn@gmail.com if you are interested in
contributing!
