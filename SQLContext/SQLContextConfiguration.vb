Imports VSProject.SQLContext.Interfaces

''' <summary>Настройки для поведения класса SQLContext</summary>
Public Class SQLContextConfiguration

    ''' <summary>
    ''' Ссылка на хранилище пользовательских мапперов
    ''' </summary>
    Public Shared ReadOnly Property UserMappers As UserMappers = UserMappers.Instance


    Friend Shared Function FNVHash(Value As String) As ULong
        Dim prime = &H811C9DC5UI
        Dim hash As ULong

        For Each c In Value
            hash = (hash * prime) And UInteger.MaxValue
            hash = hash Xor AscW(c)
        Next

        Return hash
    End Function

End Class
