Option Explicit On
Option Strict On

Imports System.Collections.Concurrent
Imports VSProject.SQLContext.Interfaces

Friend Class MemoryCache(Of TKey, TValue)
    Implements ICache(Of TKey, TValue)

    Private items As New ConcurrentDictionary(Of TKey, TValue)
    Private locker As New Object

    ''' <summary>
    ''' Читает данные из кеша по ключу. Если данных нет, то выполняет делегат продюсера данных
    ''' </summary>
    Public Function Fetch(Key As TKey, Producer As Func(Of TKey, TValue)) As TValue Implements ICache(Of TKey, TValue).Fetch
        If Not items.ContainsKey(Key) Then
            SyncLock locker
                If Not items.ContainsKey(Key) Then
                    items.TryAdd(Key, Producer(Key))
                End If
            End SyncLock
        End If

        Return items(Key)
    End Function

    Public Function FetchValues() As IEnumerable(Of TValue) Implements ICache(Of TKey, TValue).FetchValues
        Return items.Values
    End Function

    Public Function FetchKeys() As IEnumerable(Of TKey) Implements ICache(Of TKey, TValue).FetchKeys
        Return items.Keys
    End Function

    Public Function Available(Key As TKey) As Boolean Implements ICache(Of TKey, TValue).Available
        Return items.ContainsKey(Key)
    End Function

    Public Sub Clear() Implements ICache(Of TKey, TValue).Clear
        items.Clear()
    End Sub

    Public Function Count() As Integer Implements ICache(Of TKey, TValue).Count
        Return items.Count
    End Function

    Public Sub Remove(Key As TKey) Implements ICache(Of TKey, TValue).Remove
        Dim removedValue As TValue = Nothing
        items.TryRemove(Key, removedValue)
    End Sub

End Class