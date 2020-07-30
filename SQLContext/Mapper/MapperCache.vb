''' <summary>Класс кеширует на время работы приложения типы для маппинга</summary>
Friend Class MapperCache

    Private Shared types As New Dictionary(Of Type, TableInformation)

    ''' <summary>Сохранить в кеше. Если уже есть запись с таким ключом, то она заменяется новой</summary>
    ''' <param name="ClassType">Тип в качестве ключа</param>
    ''' <param name="TableInfo">Метаданные в виде TableInformation</param>
    Public Shared Sub Store(ByVal ClassType As Type, ByVal TableInfo As TableInformation)
        If types.ContainsKey(ClassType) Then types.Remove(ClassType)
        types.Add(ClassType, TableInfo)
    End Sub

    ''' <summary>Загрузка метаданных из кеша</summary>
    ''' <param name="ClassType">Тип в качестве ключа</param>
    ''' <returns>Возвращает TableInformation или Nothing</returns>
    Public Shared Function Load(ByVal ClassType As Type) As TableInformation
        If types.ContainsKey(ClassType) Then
            Return types(ClassType)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>Проверка, есть ли в кеше метаданных</summary>
    ''' <param name="ClassType">Тип в качестве ключа</param>
    ''' <returns>Возвращает истину, если есть метаданные или ложь в противном случае</returns>
    Public Shared Function Available(ByVal ClassType As Type) As Boolean
        Return types.ContainsKey(ClassType)
    End Function

    ''' <summary>Очистка кеша</summary>
    Public Shared Sub Clear()
        types.Clear()
    End Sub

End Class