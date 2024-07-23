Option Strict On
Option Explicit On

Imports System.Collections.Concurrent

Public Class UserMappers

    Private Shared locker As New Object
    Private Shared self As UserMappers

    ' Зарегистрированные пользовательские маперы
    Private Shared userMapperCache As New ConcurrentDictionary(Of Type, [Delegate])

    ' Приватный конструктор
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Ссылка на объект управления пользовательскими мапперами
    ''' </summary>
    Public Shared ReadOnly Property Instance As UserMappers
        Get
            If self Is Nothing Then
                SyncLock locker
                    If self Is Nothing Then
                        self = New UserMappers()
                    End If
                End SyncLock
            End If

            Return self
        End Get
    End Property

    Public Sub Register(Of T)(Mapper As Func(Of IDataRecord, T))
        Dim type = GetType(T)
        userMapperCache.AddOrUpdate(type, Mapper, Function(key, oldValue) Mapper)
    End Sub

    Public Sub Unregister(Of T)()
        Dim type = GetType(T)
        Dim value As [Delegate] = Nothing
        userMapperCache.TryRemove(type, value)
    End Sub

    Public Function GetMapper(Of T)() As Func(Of IDataRecord, T)
        Dim type = GetType(T)
        Dim value As [Delegate] = Nothing

        If userMapperCache.TryGetValue(type, value) Then
            Return TryCast(value, Func(Of IDataRecord, T))
        Else
            Return Nothing
        End If
    End Function

    Public Sub RemoveAll()
        userMapperCache.Clear()
    End Sub

End Class
