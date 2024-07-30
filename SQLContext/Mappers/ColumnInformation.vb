''' <summary>Класс содержит метаданные для каждого столбца таблицы</summary>
Friend Class ColumnInformation

    Public PrimaryKey As Boolean

    Public AutoIncrement As Boolean

    Public UserConverter As String

    Public ColumnName As String

    Public PropertyName As String

    Public PropertyType As Type

    Public Overrides Function ToString() As String
        Return $"ColumnName: {ColumnName}, PropertyName: {PropertyName}, PropertyType: {PropertyType.Name}"
    End Function

End Class