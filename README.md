# What is it?
**SQL Context** it is microORM for easy mapping your sql queries to VB and C# classes. The library has some special features that provide the flexibility to configure the mapping:

* Attributes (PrimaryKey, AutoIncrement, Programmable, Table, Column)
	* Table, Column - allow you to change the names of classes and properties, and the projection on the other table and column names
* Mapping to static .NET types, such as classes and structures
* Mapping to dynamic objects
* Caching types for optimizing speed work
* The architecture is based on an interface _IDbConnection_, that redesign library to connect any ADO.NET data provider

# How I can use ?

* Download small library from NuGet package [https://www.nuget.org/packages/VSProject.SQLContext/](https://www.nuget.org/packages/VSProject.SQLContext/)
* Add reference in your project and import namespace _VSProject.MicroORM_
* Everything is ready for work

# Sample Code

Define your class for table row

<code>
	
	<Table("table_test")>
	Public Class ClassTest

	    <PrimaryKey>
	    Public Property ID As Long

	    <Column("field_name")>
	    Public Property Name As String

	    <Column("field_address")>
	    Public Property Address As String

	    <Programmable>
	    Public Property Phone As PhoneObject

	    <Programmable>
	    Public Property Skype As PhoneObject

	End Class
</code>

Connect to database and get each row in object class

<code>
	
	Using context As New SQLContext(New SQLiteConnection("Data Source=C:\test.db"))

	    For Each row In context.SelectRows(Of ClassTest)("select id, name from table_test")
		Console.WriteLine($"Your record data is {row.ID}, {row.Name}, {row.Address}")	
	    Next

	End Using
</code>
