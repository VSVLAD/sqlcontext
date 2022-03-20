Namespace Exceptions

    ''' <summary>Базовое исключение создаётся, если не найдено подходящего типа исключения</summary>
    Public Class SQLContextException
        Inherits Exception

        Public Sub New(Message As String)
            MyBase.New(Message)
        End Sub

        Public Sub New(Message As String, InnerException As Exception)
            MyBase.New(Message, InnerException)
        End Sub

        Public Sub New()
            MyBase.New()
        End Sub

    End Class

End Namespace
