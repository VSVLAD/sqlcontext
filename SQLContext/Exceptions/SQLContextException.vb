Namespace Exceptions

    ''' <summary>Базовое исключение создаётся, если не найдено подходящего типа исключения</summary>
    Public Class SQLContextException
        Inherits Exception

        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub

        Public Sub New()
            MyBase.New()
        End Sub

    End Class

End Namespace
