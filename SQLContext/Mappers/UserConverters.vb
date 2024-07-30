Option Strict On
Option Explicit On

Imports System.Collections.Concurrent

Public Class UserConverters

    Private Shared locker As New Object
    Private Shared self As UserConverters

    Private userConvertersCache As New ConcurrentDictionary(Of String, Func(Of Object, Object))

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Ссылка на объект управления пользовательскими конверерами
    ''' </summary>
    Friend Shared ReadOnly Property Instance As UserConverters
        Get
            If self Is Nothing Then
                SyncLock locker
                    If self Is Nothing Then
                        self = New UserConverters()
                    End If
                End SyncLock
            End If

            Return self
        End Get
    End Property

    Public Sub RegisterConverter(Name As String, Converter As Func(Of Object, Object))
        userConvertersCache.AddOrUpdate(Name, Converter, Function(key, oldValue) Converter)
    End Sub

    Public Sub UnregisterConverter(Name As String)
        Dim value As Func(Of Object, Object) = Nothing
        userConvertersCache.TryRemove(Name, value)
    End Sub

    Public Function GetConverter(Name As String) As Func(Of Object, Object)
        Dim value As Func(Of Object, Object) = Nothing

        If userConvertersCache.TryGetValue(Name, value) Then
            Return TryCast(value, Func(Of Object, Object))
        Else
            Return Nothing
        End If
    End Function

    Public Sub ClearAll()
        userConvertersCache.Clear()
    End Sub

End Class
