Namespace Interfaces

    Public Interface ICache(Of TKey, TValue)

        Function Fetch(Key As TKey, Producer As Func(Of TKey, TValue)) As TValue

        Function FetchValues() As IEnumerable(Of TValue)

        Function FetchKeys() As IEnumerable(Of TKey)

        Function Available(Key As TKey) As Boolean

        Sub Clear()

        Sub Remove(Key As TKey)

        Function Count() As Integer

    End Interface

End Namespace