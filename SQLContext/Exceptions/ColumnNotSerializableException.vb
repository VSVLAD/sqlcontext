Namespace Exceptions

    ''' <summary>Исключение вызывается, когда свойство имеет ссылочный тип, который невозможно сконвертировать в строковое представление</summary>
    Public Class ColumnNotSerializableException
        Inherits SQLContextException

        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub

        Public Sub New()
            MyBase.New()
        End Sub

    End Class

End Namespace
