Imports System.Runtime.CompilerServices

Namespace Extensions

    ''' <summary>Общие методы для выполнения простых запросов</summary>
    Public Module SQLContextBaseExtensions

#Region "       Перегрузки в которых текушщий объект это IDbConnection и параметры словарь "

        <Extension>
        Public Function ExecReader(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As IDataReader
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextParameters.FromDictionary(Connection, SqlText, Parameters)
            Dim sqlReader = sqlCmd.ExecuteReader()

            sqlCmd.Dispose()
            Return sqlReader
        End Function

        <Extension>
        Public Function ExecNonQuery(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As Integer
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextParameters.FromDictionary(Connection, SqlText, Parameters)
            Dim sqlResult = sqlCmd.ExecuteNonQuery()

            sqlCmd.Dispose()
            Return sqlResult
        End Function

        <Extension>
        Public Function ExecScalar(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As Object
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextParameters.FromDictionary(Connection, SqlText, Parameters)
            Dim sqlResult = sqlCmd.ExecuteScalar()

            sqlCmd.Dispose()
            Return sqlResult
        End Function

        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As IDbConnection, SqlText As String, Parameters As Dictionary(Of String, Object)) As TResult
            If Connection.State = ConnectionState.Closed Then Connection.Open()

            Dim sqlCmd = SQLContextParameters.FromDictionary(Connection, SqlText, Parameters)
            Dim sqlResult = sqlCmd.ExecuteScalar()

            sqlCmd.Dispose()
            Return CType(sqlResult, TResult)
        End Function
#End Region

#Region "       Перегрузки в которых текушщий объект это IDbConnection и параметры анонимный объект "
        <Extension>
        Public Function ExecReader(Connection As IDbConnection, SqlText As String, Parameters As Object) As IDataReader
            Return ExecReader(Connection, SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecNonQuery(Connection As IDbConnection, SqlText As String, Parameters As Object) As Integer
            Return ExecNonQuery(Connection, SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecScalar(Connection As IDbConnection, SqlText As String, Parameters As Object) As Object
            Return ExecScalar(Connection, SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As IDbConnection, SqlText As String, Parameters As Object) As TResult
            Return ExecScalar(Of TResult)(Connection, SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function
#End Region

#Region "       Перегрузки в которых текушщий объект это SQLContext и параметры словарь "
        <Extension>
        Public Function ExecReader(Connection As SQLContext, SqlText As String, Parameters As Dictionary(Of String, Object)) As IDataReader
            Return ExecReader(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecNonQuery(Connection As SQLContext, SqlText As String, Parameters As Dictionary(Of String, Object)) As Integer
            Return ExecNonQuery(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecScalar(Connection As SQLContext, SqlText As String, Parameters As Dictionary(Of String, Object)) As Object
            Return ExecScalar(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As SQLContext, SqlText As String, Parameters As Dictionary(Of String, Object)) As TResult
            Return ExecScalar(Of TResult)(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function
#End Region

#Region "       Перегрузки в которых текушщий объект это SQLContext и параметры анонимный объект "
        <Extension>
        Public Function ExecReader(Connection As SQLContext, SqlText As String, Parameters As Object) As IDataReader
            Return ExecReader(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecNonQuery(Connection As SQLContext, SqlText As String, Parameters As Object) As Integer
            Return ExecNonQuery(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecScalar(Connection As SQLContext, SqlText As String, Parameters As Object) As Object
            Return ExecScalar(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function

        <Extension>
        Public Function ExecScalar(Of TResult)(Connection As SQLContext, SqlText As String, Parameters As Object) As TResult
            Return ExecScalar(Of TResult)(Connection.OpenConnection(), SqlText, SQLContextParameters.ToDictionary(Parameters))
        End Function
#End Region

    End Module

End Namespace
