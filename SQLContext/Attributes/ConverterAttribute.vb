Namespace Attributes

    '''<summary>Атрибут применяется к свойству, для конвертации типов столбцов.Например, дата в SQLite может хранится как Text, но для маппера требуется Date, а не как String</summary>
    <AttributeUsage(AttributeTargets.Property)>
    Public Class ConverterAttribute
        Inherits Attribute

        Public Property Name As String

        Protected Sub New()
        End Sub

        Public Sub New(Name As String)
            Me.Name = Name
        End Sub

    End Class

End Namespace