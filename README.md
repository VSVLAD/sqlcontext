# What is it?
**SQL Context** it is microORM for easy mapping your sql queries to VB and C# classes. The library has some special features that provide the flexibility to configure the mapping:

* Custom attributes (PrimaryKey, Table, Column). Table, Column - allows you to map property names to different column names
* Mapping to .NET types, such as classes, structures and value types
* Mapping to dynamic objects
* Caching mapper functions for optimizing speed work
* The architecture is based on an interface _IDbConnection_, that redesign library to connect any ADO.NET data provider

# How I can use ?

* Download small library from NuGet package [https://www.nuget.org/packages/VSProject.SQLContext/](https://www.nuget.org/packages/VSProject.SQLContext/)
* Add reference in your project and import namespaces from _VSProject.SQLContext.*_
* Everything is ready for work

# Example code

Define your class for table row

```
	<Table("table_test")>
	Public Class ClassTest

	    <PrimaryKey>
	    Public Property ID As Long

	    <Column("field_name")>
	    Public Property Name As String

	    <Column("field_address")>
	    Public Property Address As String

		' This is custom user property
	    Public Property Phone As PhoneObject

		' This is custom user property
	    Public Property Skype As PhoneObject

	End Class
```

Connect to database and get each row in object class

```
	Using context As New SQLContext(New SQLiteConnection("Data Source=C:\test.db"))

	    ' You can read row as object class
	    
	    For Each row In context.SelectRows(Of ClassTest)("select id, field_name, field_address from table_test")
		Console.WriteLine($"Your record data is {row.ID}, {row.Name}, {row.Address}")	
	    Next
	    
	    ' Or you can read as Dictionary(Of String, Object)
	    
	    For Each row In context.SelectRows("select id, field_name, field_address from table_test")
		Console.WriteLine($"Your record data is {row("id")}, {row("field_name")}, {row("field_address")}")	
	    Next

	    ' Or you can read as dynamic object
	    For Each row In context.SelectRowsDynamic("select id, field_name, field_address from table_test")
		Console.WriteLine($"Your record data is {row.id}, {row.field_name}, {row.field_address}")	
	    Next
	    
	    ' Or do you have full control?
	    Dim reader = context.ExecuteReader("select id, field_name, field_address from table_test")
	    
	End Using
```
