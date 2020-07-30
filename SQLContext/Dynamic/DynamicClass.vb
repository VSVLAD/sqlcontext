Imports System.Dynamic

Namespace Dynamic

    Public Class DynamicRow
        Inherits DynamicObject

        Private row As Dictionary(Of String, Object)

        ''' <summary>Конструктор инициализируется строкой с данными</summary>
        Public Sub New(Value As Dictionary(Of String, Object))
            row = Value
        End Sub

        ''' <summary>Читаем из свойства</summary>
        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            Dim propName = binder.Name

            For Each key In row.Keys
                If key.ToLowerInvariant = propName.ToLowerInvariant Then
                    result = row(key)
                    Return True
                End If
            Next

            ' Если передали неизвестное имя, вернём ничего
            Return True
        End Function

        ''' <summary>Записываем в свойство</summary>
        Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
            Dim propName = binder.Name

            For Each key In row.Keys
                If key.ToLowerInvariant = propName.ToLowerInvariant Then
                    row(key) = value
                    Return True
                End If
            Next

            ' Если передали неизвестное имя, добавим в словарь
            row(propName) = value

            Return False
        End Function

    End Class

End Namespace
