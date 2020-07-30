Namespace Attributes

    ''' <summary>Атрибут применяется для классов для указания названия таблицы</summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct)>
    Public Class TableAttribute
        Inherits Attribute

        Public Property Name As String = String.Empty

        Protected Sub New()
        End Sub

        Public Sub New(Name As String)
            Me.Name = Name
        End Sub
    End Class

End Namespace
