Imports System.Data
Imports System.Runtime.CompilerServices

Namespace Extensions

    ''' <summary>Общие методы для выполнения простых запросов</summary>
    Public Module SQLContextExtensions

        ''' <summary>Метод выполняет SQL запрос и возвращает объект IDataReader</summary>
        ''' <param name="Context">Экземпляр класса в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecReader(Context As SQLContext, SqlText As String, ParamArray Values() As Object) As IDataReader
            Return ExecReader(Context.OpenConnection, SqlText, Values)
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает количество обработанных записей</summary>
        ''' <param name="Context">Экземпляр класса в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecNonQuery(Context As SQLContext, SqlText As String, ParamArray Values() As Object) As Integer
            Return ExecNonQuery(Context.OpenConnection, SqlText, Values)
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа Object</summary>
        ''' <param name="Context">Экземпляр класса в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecScalar(Context As SQLContext, SqlText As String, ParamArray Values() As Object) As Object
            Return ExecScalar(Context.OpenConnection, SqlText, Values)
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа TResult</summary>
        ''' <param name="Context">Экземпляр класса в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecScalar(Of TResult)(Context As SQLContext, SqlText As String, ParamArray Values() As Object) As TResult
            Return ExecScalar(Of TResult)(Context.OpenConnection, SqlText, Values)
        End Function


        ''' <summary>Метод выполняет SQL запрос и возвращает объект IDataReader</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecReader(Connection As IDbConnection, SqlText As String, ParamArray Values() As Object) As IDataReader
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextOptions.PreparedCommand.Prepare(SqlText, Connection, Values)
            Dim sqlReader = sqlCmd.ExecuteReader()

            If Values.Length = 0 Then sqlCmd.Dispose()
            Return sqlReader
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает количество обработанных записей</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecNonQuery(Connection As IDbConnection, SqlText As String, ParamArray Values() As Object) As Integer
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextOptions.PreparedCommand.Prepare(SqlText, Connection, Values)
            Dim sqlResult = sqlCmd.ExecuteNonQuery()

            If Values.Length = 0 Then sqlCmd.Dispose()
            Return sqlResult
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа Object</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecScalar(Connection As IDbConnection, SqlText As String, ParamArray Values() As Object) As Object
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextOptions.PreparedCommand.Prepare(SqlText, Connection, Values)
            Dim sqlResult = sqlCmd.ExecuteScalar()

            If Values.Length = 0 Then sqlCmd.Dispose()
            Return sqlResult
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа TResult</summary>
        ''' <typeparam name="TResult">Тип результата скалярного значения</typeparam>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As IDbConnection, SqlText As String, ParamArray Values() As Object) As TResult
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextOptions.PreparedCommand.Prepare(SqlText, Connection, Values)
            Dim sqlResult = sqlCmd.ExecuteScalar()

            If Values.Length = 0 Then sqlCmd.Dispose()
            Return CType(sqlResult, TResult)
        End Function

    End Module

End Namespace
