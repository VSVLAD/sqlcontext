Imports System.Dynamic

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

        If row.TryGetValue(propName, result) Then
            Return True
        End If

        ' Если передали неизвестное имя, вернём Null
        Return True
    End Function

    ''' <summary>Записываем в свойство</summary>
    Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
        Dim propName = binder.Name

        ' Добавим в словарь, даже если передали неизвестное свойство, всё равно добавим
        row(propName) = value

        Return False
    End Function

    ''' <summary>
    ''' Возвращает скалярное значение или ничего из динамического объекта
    ''' </summary>
    Public Function ScalarValue() As Object
        Return row.Values.FirstOrDefault()
    End Function

    ''' <summary>
    ''' Возвращает скалярное значение преобразуя к заданному типу или ничего из динамического объекта
    ''' </summary>
    Public Function ScalarValue(Of TValueType)() As TValueType
        Return CType(row.Values.FirstOrDefault(), TValueType)
    End Function

End Class