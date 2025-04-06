Imports System.Dynamic

''' <summary>Класс предоставляет методы для CRUD операций с ORM составляющей</summary>
Public Class SQLContext
    Implements IDisposable

    ''' <summary>
    ''' Соединение для внутреннего использования
    ''' </summary>
    Private dbconnection As IDbConnection

    ''' <summary>
    ''' Ссылка на хранилище пользовательских мапперов
    ''' </summary>
    Public Shared ReadOnly Property UserMappers As UserMappers = UserMappers.Instance

    ''' <summary>
    ''' Ссылка на хранилище пользовательских конвертерор типов столбцов
    ''' </summary>
    Public Shared ReadOnly Property UserConverters As UserConverters = UserConverters.Instance

    ''' <summary>
    ''' Конструктор принимает объект соединения
    ''' </summary>
    Public Sub New(Connection As IDbConnection)
        dbconnection = Connection

        If dbconnection Is Nothing Then
            Throw New ArgumentNullException(NameOf(Connection), Resources.ExceptionMessages.CONNECTION_MUST_BE_CREATED)
        End If

        If dbconnection.State <> ConnectionState.Closed Then
            dbconnection.Close()
        End If

    End Sub

    ''' <summary>Возвращает используемое соединение</summary>
    Public Function OpenConnection() As IDbConnection
        If dbconnection.State = ConnectionState.Closed Then dbconnection.Open()
        Return dbconnection
    End Function

#Region "       Маппинг используя Reflection "

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на простой тип или класс T</summary>
    ''' <typeparam name="TClass">На данный тип может быть спроецирован результат. Типом может быть класс или примитивный тип</typeparam>
    ''' <param name="SqlText">Текст запроса SQL</param>
    ''' <param name="Parameters">Словарь аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRows(Of TClass)(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of TClass)
        Try
            For Each row In SelectRows(SqlText, Parameters)
                Yield SQLContextMappers.FromDictionaryToType(Of TClass)(row)
            Next

        Catch ex As SQLContextException
            Throw

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function

    Public Iterator Function SelectRows(Of TClass)(SqlText As String, Parameters As Object) As IEnumerable(Of TClass)
        For Each row In SelectRows(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Parameters))
            Yield row
        Next
    End Function

    Public Iterator Function SelectRows(Of TClass)(SqlText As String) As IEnumerable(Of TClass)
        For Each row In SelectRows(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Nothing))
            Yield row
        Next
    End Function

#End Region

#Region "       Маппинг с использованием пользовательского маппера"

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на простой тип или класс T</summary>
    ''' <typeparam name="TClass">На данный тип может быть спроецирован результат. Типом может быть класс или примитивный тип</typeparam>
    ''' <param name="SqlText">Текст запроса SQL</param>
    ''' <param name="Parameters">Словарь аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRowsMapper(Of TClass)(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of TClass)
        Try
            ' Проверяем, если для указаного типа пользовательский маппер
            Dim mapper = UserMappers.Instance.GetMapper(Of TClass)()

            If mapper IsNot Nothing Then
                For Each row In SelectRowsMapper(SqlText, Parameters, mapper)
                    Yield row
                Next
            Else
                Throw New SQLContextException(String.Format(Resources.ExceptionMessages.NOT_FOUND_USER_MAPPER, GetType(TClass).ToString()))
            End If

        Catch ex As SQLContextException
            Throw

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function

    ''' <summary>
    ''' Выборка строк с использованием конкретного пользовательского маппера
    ''' </summary>
    Public Iterator Function SelectRowsMapper(Of TClass)(SqlText As String, Parameters As Dictionary(Of String, Object), Mapper As Func(Of IDataRecord, TClass)) As IEnumerable(Of TClass)
        Try
            Dim dbconnection As IDbConnection = OpenConnection()

            Using dbcmd As IDbCommand = SQLContextParameters.FromDictionary(dbconnection, SqlText, Parameters)
                Using dbreader = dbcmd.ExecuteReader()
                    Do While dbreader.Read()
                        Yield Mapper(dbreader)
                    Loop
                End Using
            End Using

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function

    Public Iterator Function SelectRowsMapper(Of TClass)(SqlText As String, Parameters As Object, Mapper As Func(Of IDataRecord, TClass)) As IEnumerable(Of TClass)
        For Each row In SelectRowsMapper(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Parameters), Mapper)
            Yield row
        Next
    End Function

    Public Iterator Function SelectRowsMapper(Of TClass)(SqlText As String, Parameters As Object) As IEnumerable(Of TClass)
        For Each row In SelectRowsMapper(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Parameters))
            Yield row
        Next
    End Function

    ''' <summary>
    ''' Выборка строк с использованием конкретного пользовательского маппера
    ''' </summary>
    Public Iterator Function SelectRowsMapper(Of TClass)(SqlText As String, Mapper As Func(Of IDataRecord, TClass)) As IEnumerable(Of TClass)
        For Each row In SelectRowsMapper(SqlText, SQLContextParameters.ToDictionary(Nothing), Mapper)
            Yield row
        Next
    End Function

    Public Iterator Function SelectRowsMapper(Of TClass)(SqlText As String) As IEnumerable(Of TClass)
        For Each row In SelectRowsMapper(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Nothing))
            Yield row
        Next
    End Function

#End Region

#Region "       Маппинг Expression Tree "

    Public Iterator Function SelectRowsFast(Of TClass)(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of TClass)
        Try
            Dim dbconnection As IDbConnection = OpenConnection()

            Using dbcmd As IDbCommand = SQLContextParameters.FromDictionary(dbconnection, SqlText, Parameters)
                Using dbreader = dbcmd.ExecuteReader()

                    ' Получаем ссылку на скомпилированный маппер
                    Dim mapper = SQLContextMappers.FromReaderToExpressionTreeMapper(Of TClass)(dbreader)

                    ' Цикл по всем записям и маппим на TClass
                    Do While dbreader.Read()
                        Yield mapper(dbreader)
                    Loop
                End Using
            End Using

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function

    Public Iterator Function SelectRowsFast(Of TClass)(SqlText As String, Parameters As Object) As IEnumerable(Of TClass)
        For Each row In SelectRowsFast(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Parameters))
            Yield row
        Next
    End Function

    Public Iterator Function SelectRowsFast(Of TClass)(SqlText As String) As IEnumerable(Of TClass)
        For Each row In SelectRowsFast(Of TClass)(SqlText, SQLContextParameters.ToDictionary(Nothing))
            Yield row
        Next
    End Function

#End Region

#Region "       Маппинг на Dictionary(Of String, Object)"

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк в виде Dictionary(Of String, Object)</summary>
    ''' <param name="SQLText">Текст запроса SQL</param>
    ''' <param name="Parameters">Словарь аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRows(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of Dictionary(Of String, Object))
        Dim dbconnection As IDbConnection = OpenConnection()

        Using dbcmd As IDbCommand = SQLContextParameters.FromDictionary(dbconnection, SqlText, Parameters)
            Using dbreader As IDataReader = dbcmd.ExecuteReader()

                ' Массив с названиями столбцов
                Dim fieldBound = dbreader.FieldCount - 1
                Dim fieldNames(fieldBound) As String
                Dim fieldCached = False

                Do While dbreader.Read()

                    ' Кешируем имена столбцов
                    If Not fieldCached Then
                        fieldCached = True

                        For I = 0 To fieldBound
                            fieldNames(I) = dbreader.GetName(I)
                        Next
                    End If

                    ' Читаем значения в массив
                    Dim fieldValues(fieldBound) As Object
                    dbreader.GetValues(fieldValues)

                    ' Упаковываем массив значений в словарь
                    Dim retDict As New Dictionary(Of String, Object)

                    For I = 0 To fieldBound
                        If fieldValues(I).Equals(DBNull.Value) Then
                            retDict.Add(fieldNames(I), Nothing)
                        Else
                            retDict.Add(fieldNames(I), fieldValues(I))
                        End If
                    Next

                    Yield retDict
                Loop
            End Using
        End Using
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк в виде Dictionary(Of String, Object)</summary>
    Public Iterator Function SelectRows(SqlText As String, Parameters As Object) As IEnumerable(Of Dictionary(Of String, Object))
        For Each row In SelectRows(SqlText, SQLContextParameters.ToDictionary(Parameters))
            Yield row
        Next
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк в виде Dictionary(Of String, Object)</summary>
    Public Iterator Function SelectRows(SqlText As String) As IEnumerable(Of Dictionary(Of String, Object))
        For Each row In SelectRows(SqlText, SQLContextParameters.ToDictionary(Nothing))
            Yield row
        Next
    End Function

#End Region

#Region "       Маппинг на Dynamic "

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на класс DynamicRow, производный от DynamicObject</summary>
    ''' <param name="SqlText">Текст запроса SQL</param>
    Public Iterator Function SelectRowsDynamic(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of Object)
        Try
            For Each row In SelectRows(SqlText, Parameters)
                Yield SQLContextMappers.FromDictionaryToDynamic(row)
            Next

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на класс DynamicRow, производный от DynamicObject</summary>
    ''' <param name="SqlText">Текст запроса SQL</param>
    Public Iterator Function SelectRowsDynamic(SqlText As String, Parameters As Object) As IEnumerable(Of Object)
        For Each row In SelectRowsDynamic(SqlText, SQLContextParameters.ToDictionary(Parameters))
            Yield row
        Next
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на класс DynamicRow, производный от DynamicObject</summary>
    ''' <param name="SqlText">Текст запроса SQL</param>
    Public Iterator Function SelectRowsDynamic(SqlText As String) As IEnumerable(Of Object)
        For Each row In SelectRowsDynamic(SqlText, SQLContextParameters.ToDictionary(Nothing))
            Yield row
        Next
    End Function

#End Region

#Region "       Поддержка IDisposable       "

    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' >> dispose managed state (managed objects).
                If dbconnection IsNot Nothing Then
                    dbconnection.Close()
                    dbconnection.Dispose()
                    dbconnection = Nothing
                End If
            End If

            ' >> free unmanaged resources (unmanaged objects) and override Finalize() below. set large fields to null.
        End If

        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
#End Region

End Class

