Namespace Interfaces

    ''' <summary>Интерфейс предоставляет генератор запросов для операций вставки, обновления и удаления</summary>
    Public Interface IQueryWriter

        Function GetTokenSelect() As String

        Function GetTokenFrom() As String

        Function GetTokenWhere() As String

        Function GetTokenDelete() As String

        Function GetTokenInsert() As String

        Function GetTokenValues() As String

        Function GetTokenUpdate() As String

        Function GetTokenSet() As String

        Function GetTokenNull() As String

        Function GetTokenDot() As String

        Function GetTokenComma() As String

        Function GetTokenSemicolon() As String

        Function GetTokenEqual() As String

        Function GetTokenBracketOpen() As String

        Function GetTokenBracketClose() As String

        ''' <summary>Метод должен вернуть декорированое название столбца для использования в запросах</summary>
        ''' <param name="ColumnName">Название столбца</param>
        Function GetColumnName(ColumnName As String) As String

        ''' <summary>Метод должен вернуть декорированое название таблицы для использования в запросах</summary>
        ''' <param name="TableName">Название таблицы</param>
        Function GetTableName(TableName As String) As String

        ''' <summary>Метод выполняет конвертацию данных .NET в строковое преставление для использования в запросах</summary>
        ''' <param name="Value">Значение свойства пользовательского объекта</param>
        ''' <param name="Source">Тип свойства для исходного значения</param>
        ''' <param name="Destination">Тип столбца в который должно выполняться преобразование</param>
        ''' <returns>Значение для столбца должно быть приведено к строковому виду</returns>
        Function GetColumnValue(Value As Object, Source As Type, Destination As Type) As String

    End Interface

End Namespace