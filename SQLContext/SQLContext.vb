Imports VSProject.SQLContext.Exceptions


''' <summary>Класс предоставляет методы для CRUD операций с ORM составляющей</summary>
Public Class SQLContext
    Implements IDisposable

    ''' <summary>
    ''' Соединение для внутреннего использования
    ''' </summary>
    Private dbconnection As IDbConnection

    ''' <summary>
    ''' Настройки по-умолчанию для инстанса класса
    ''' </summary>
    Public ReadOnly Property Configuration As New SQLContextConfiguration

    ''' <summary>
    ''' Конструктор принимает объект соединения
    ''' </summary>
    ''' <param name="Connection">Инициализированный объект соединения</param>
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
    Friend Function OpenConnection() As IDbConnection
        If dbconnection.State = ConnectionState.Closed Then dbconnection.Open()
        Return dbconnection
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на простой тип или класс T</summary>
    ''' <typeparam name="T">На данный тип может быть спроецирован результат. Типом может быть класс или примитивный тип</typeparam>
    ''' <param name="SqlText">Текст запроса SQL</param>
    ''' <param name="Parameters">Словарь аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRows(Of T)(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of T)
        Try
            ' Проверяем, если для указаного типа пользовательский маппер
            Dim userMapper = UserMappers.Instance.GetMapper(Of T)()

            If userMapper IsNot Nothing Then
                ' Вызываем версию метода с указанием конкретного маппера полученого из хранилища мапперов
                For Each row In SelectRows(SqlText, Parameters, userMapper)
                    Yield row
                Next
            Else
                ' Иначе выбираем предустановленный "классический" маппер использующий словарь
                For Each row In SelectRows(SqlText, Parameters)
                    Yield ContextMappers.FromDictionaryToType(Of T)(row)
                Next
            End If

        Catch ex As SQLContextException
            Throw ex

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function


    ''' <summary>
    ''' Выборка строк с использованием конкретного пользовательского маппера
    ''' </summary>
    Public Iterator Function SelectRows(Of TClass)(SqlText As String, Parameters As Dictionary(Of String, Object), Mapper As Func(Of IDataRecord, TClass)) As IEnumerable(Of TClass)
        Try
            Dim dbconnection As IDbConnection = OpenConnection()

            Using dbcmd As IDbCommand = ContextParameters.FromParametersToCommand(dbconnection, SqlText, Parameters)
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

    Public Iterator Function SelectRowsFast(Of TClass)(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of TClass)
        Try
            Dim dbconnection As IDbConnection = OpenConnection()

            Using dbcmd As IDbCommand = ContextParameters.FromParametersToCommand(dbconnection, SqlText, Parameters)
                Using dbreader = dbcmd.ExecuteReader()

                    ' Получаем ссылку на скомпилированный маппер
                    Dim mapper = ContextMappers.FromReaderToExpressionTreeMapper(Of TClass)(dbreader)

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

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк в виде Dictionary(Of String, Object)</summary>
    ''' <param name="SQLText">Текст запроса SQL</param>
    ''' <param name="Parameters">Словарь аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRows(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of Dictionary(Of String, Object))
        Dim dbconnection As IDbConnection = OpenConnection()

        Using dbcmd As IDbCommand = ContextParameters.FromParametersToCommand(dbconnection, SqlText, Parameters)
            Using dbreader = dbcmd.ExecuteReader()

                ' Массив с названиями столбцов
                Dim fieldNames() As String
                Dim fieldCached As Boolean

                Dim fieldBound = dbreader.FieldCount - 1
                ReDim fieldNames(fieldBound)

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
                        If dbreader.IsDBNull(I) Then
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
        For Each row In SelectRows(SqlText, ContextParameters.FromParametersToDictionary(Parameters))
            Yield row
        Next
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на класс DynamicRow, производный от DynamicObject</summary>
    ''' <param name="SqlText">Текст запроса SQL</param>
    Public Iterator Function SelectRowsDynamic(SqlText As String, Parameters As Dictionary(Of String, Object)) As IEnumerable(Of Object)
        Try
            For Each row In SelectRows(SqlText, Parameters)
                Yield ContextMappers.FromDictionaryToDynamic(row)
            Next

        Catch ex As Exception
            Throw New SQLContextException(ex.Message, ex)

        End Try
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на класс DynamicRow, производный от DynamicObject</summary>
    ''' <param name="SqlText">Текст запроса SQL</param>
    Public Iterator Function SelectRowsDynamic(SqlText As String, Parameters As Object) As IEnumerable(Of Object)
        For Each row In SelectRowsDynamic(SqlText, ContextParameters.FromParametersToDictionary(Parameters))
            Yield row
        Next
    End Function

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

