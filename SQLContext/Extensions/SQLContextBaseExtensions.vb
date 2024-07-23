Imports System.Data
Imports System.Runtime.CompilerServices

Namespace Extensions

    ''' <summary>Общие методы для выполнения простых запросов</summary>
    Public Module SQLContextBaseExtensions

        ''' <summary>Метод выполняет SQL запрос и возвращает объект IDataReader</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecReader(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As IDataReader
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = ContextParameters.FromParametersToCommand(Connection, SqlText, Parameters)
            Dim sqlReader = sqlCmd.ExecuteReader()

            sqlCmd.Dispose()
            Return sqlReader
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает количество обработанных записей</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecNonQuery(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As Integer
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = ContextParameters.FromParametersToCommand(Connection, SqlText, Parameters)
            Dim sqlResult = sqlCmd.ExecuteNonQuery()

            sqlCmd.Dispose()
            Return sqlResult
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа Object</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecScalar(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As Object
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = ContextParameters.FromParametersToCommand(Connection, SqlText, Parameters)
            Dim sqlResult = sqlCmd.ExecuteScalar()

            sqlCmd.Dispose()
            Return sqlResult
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа TResult</summary>
        ''' <typeparam name="TResult">Тип результата скалярного значения</typeparam>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As TResult
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = ContextParameters.FromParametersToCommand(Connection, SqlText, Parameters)
            Dim sqlResult = sqlCmd.ExecuteScalar()

            sqlCmd.Dispose()
            Return CType(sqlResult, TResult)
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает объект IDataReader</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecReader(Connection As IDbConnection, SqlText As String, Parameters As Object) As IDataReader
            Return ExecReader(Connection, SqlText, ContextParameters.FromParametersToDictionary(Parameters))
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает количество обработанных записей</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecNonQuery(Connection As IDbConnection, SqlText As String, Parameters As Object) As Integer
            Return ExecNonQuery(Connection, SqlText, ContextParameters.FromParametersToDictionary(Parameters))
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа Object</summary>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecScalar(Connection As IDbConnection, SqlText As String, Parameters As Object) As Object
            Return ExecScalar(Connection, SqlText, ContextParameters.FromParametersToDictionary(Parameters))
        End Function

        ''' <summary>Метод выполняет SQL запрос и возвращает скалярное значение типа TResult</summary>
        ''' <typeparam name="TResult">Тип результата скалярного значения</typeparam>
        ''' <param name="Connection">Объект соединения в контексте которого будет выполнен запрос</param>
        ''' <param name="SqlText">Текст запроса SQL</param>
        ''' <param name="Parameters">Объект содержащий свойства которые будут подготовленными параметрами</param>
        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As IDbConnection, SqlText As String, Parameters As Object) As TResult
            Return ExecScalar(Of TResult)(Connection, SqlText, ContextParameters.FromParametersToDictionary(Parameters))
        End Function

    End Module

End Namespace
