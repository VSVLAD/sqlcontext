Namespace Attributes

    '''<summary>Атрибут применяется к свойству, для указания названия столбца в таблице</summary>
    <AttributeUsage(AttributeTargets.Property)>
    Public Class ColumnAttribute
        Inherits Attribute

        Public Property Name As String = String.Empty

        Protected Sub New()
        End Sub

        Public Sub New(ByVal Name As String)
            Me.Name = Name
        End Sub

    End Class

End Namespace
