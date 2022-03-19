Imports VSProject.SQLContext.Attributes

<Table("address")>
Public Class AddressInfo

    <Column("id"), PrimaryKey, AutoIncrement>
    Public Property AddressId As Long

    Public Property City As String

    Public Property Street As String

    Public Property Home As Long?

    Public Property Square As Double

    <Column("commercial")>
    Public Property IsCommercial As Boolean

    <Column(GetType(String))>
    Public Property DateOne As Date? = Now()

    <Column(GetType(Long))>
    Public Property DateTwo As Date? = Now()

    <Column(GetType(Double))>
    Public Property DateThree As Date? = Now()

    <Programmable>
    Public Property DummyData As Integer

End Class


Public Class AddressInfoWithoutPrimaryKey

    Public Property AddressId As Long

    Public Property City As String

End Class

Public Class AddressInfoWithoutDataColumn

    <Programmable>
    Public Property Test1 As Long

    <Programmable>
    Public Property Test2 As Long

End Class