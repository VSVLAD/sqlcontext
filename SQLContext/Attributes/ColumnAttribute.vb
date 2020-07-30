Namespace Attributes

    '''<summary>Атрибут применяется к свойству, для указания названия столбца в таблице</summary>
    <AttributeUsage(AttributeTargets.Property)>
    Public Class ColumnAttribute
        Inherits Attribute

        Public Property Name As String = String.Empty
        Public Property DataType As Type

        Protected Sub New()
        End Sub

        Public Sub New(ByVal Name As String)
            Me.Name = Name
        End Sub

        Public Sub New(ByVal Name As String, DataType As Type)
            Me.Name = Name
            Me.DataType = DataType
        End Sub

        Public Sub New(ByVal DataType As Type)
            Me.DataType = DataType
        End Sub

    End Class

End Namespace
