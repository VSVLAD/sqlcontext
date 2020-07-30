Option Explicit On
Option Strict On

''' <summary>Класс содержит информацию о таблице, её столбцах и атрибутах</summary>
Friend Class TableInformation

    ''' <summary>Название таблицы</summary>
    Public TableName As String

    ''' <summary>Список столбцов</summary>
    Public Columns As New List(Of ColumnInformation)

End Class
