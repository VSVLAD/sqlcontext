﻿Imports System.Text
Imports VSProject.MicroORM.Exceptions
Imports VSProject.MicroORM.Extensions
Imports VSProject.MicroORM.Interfaces

''' <summary>Класс предоставляет методы для CRUD операций с ORM составляющей</summary>
Public Class SQLContext
    Implements IDisposable

    ''' <summary>Соединение для внутреннего использования</summary>
    Private sqlConn As IDbConnection


    ''' <summary>Конструктор принимает объект соединения</summary>
    Public Sub New(Connection As IDbConnection)
        sqlConn = Connection

        If sqlConn Is Nothing Then
            Throw New ArgumentNullException("Connection", "Объект соединения должен быть создан")
        End If

        If sqlConn.State <> ConnectionState.Closed Then
            sqlConn.Close()
        End If

        sqlConn.Open()
    End Sub

    ''' <summary>Возвращает используемое соединение</summary>
    Friend Function OpenConnection() As IDbConnection
        If sqlConn.State <> ConnectionState.Open Then sqlConn.Open()
        Return sqlConn
    End Function


    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на простой тип или класс T</summary>
    ''' <typeparam name="T">На данный тип может быть спроецирован результат. Типом может быть класс или примитивный тип</typeparam>
    ''' <param name="SqlText">Текст запроса SQL</param>
    ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRows(Of T)(ByVal SqlText As String, ParamArray Values() As Object) As IEnumerable(Of T)
        Try
            Dim tpClass As Type = GetType(T)

            For Each row In SelectRows(SqlText, Values)
                Yield Mapper.FromDictionaryToClass(Of T)(row)
            Next

        Catch ex As Exception
            Throw New SQLContextException(ex.Message)

        End Try
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк спроецированных на класс DynamicRow, производный от DynamicObject</summary>
    ''' <param name="SqlText">Текст запроса SQL</param>
    ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRowsDynamic(ByVal SqlText As String, ParamArray Values() As Object) As IEnumerable(Of Object)
        Try
            For Each row In SelectRows(SqlText, Values)
                Yield Mapper.FromDictionaryToDynamic(row)
            Next

        Catch ex As Exception
            Throw New SQLContextException(ex.Message)

        End Try
    End Function

    ''' <summary>Выборка данных по SQL запросу, возвращает коллекцию строк в виде Dictionary(Of String, Object)</summary>
    ''' <param name="SQLText">Текст запроса SQL</param>
    ''' <param name="Values">Массив необязательных аргументов для параметризованного запроса</param>
    Public Iterator Function SelectRows(ByVal SqlText As String, ParamArray Values() As Object) As IEnumerable(Of Dictionary(Of String, Object))

        ' Создаём команду с запросом
        Dim sqlCn As IDbConnection = OpenConnection()
        Dim sqlCmd As IDbCommand = SQLContextOptions.PreparedCommand.Prepare(SqlText, sqlCn, Values)

        Using sqlRead As IDataReader = sqlCmd.ExecuteReader()

            ' Массив с названиями столбцов
            Dim fieldNames As String()
            Dim fieldBound As Integer
            Dim fieldCached As Boolean

            ' Читаем данные
            fieldBound = sqlRead.FieldCount - 1
            ReDim fieldNames(fieldBound)

            While sqlRead.Read()

                ' Кешируем имена столбцов
                If Not fieldCached Then
                    fieldCached = True

                    For I = 0 To fieldBound
                        fieldNames(I) = sqlRead.GetName(I)
                    Next
                End If

                ' Читаем значения в массив
                Dim fieldValues(fieldBound) As Object
                sqlRead.GetValues(fieldValues)

                ' Упаковываем массив значений в словарь
                Dim dict As New Dictionary(Of String, Object)

                For I = 0 To fieldBound
                    If sqlRead.IsDBNull(I) Then
                        dict.Add(fieldNames(I), Nothing)
                    Else
                        dict.Add(fieldNames(I), fieldValues(I))
                    End If
                Next

                Yield dict

            End While
        End Using

        ' Если запрос не из кеша, тогда очищаем команду
        If Values.Length = 0 Then sqlCmd.Dispose()

    End Function


    ''' <summary>Вставка строки на основе данных класса. Свойства с атрибутом AutoIncrement и нулевым значением, а также Programmable не задействованы в операциях вставки</summary>
    Public Function InsertRow(Of TClass)(Data As TClass) As Integer
        Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)
        Dim forEachFlag As Boolean

        Dim writer As IQueryWriter = SQLContextOptions.Writer
        Dim sbBuilder As New StringBuilder
        Dim sbNames As New StringBuilder
        Dim sbValues As New StringBuilder

        For Each xCI In tableInfo.Columns

            ' Пропуск поле типа Программное, то пропускаем
            If xCI.Programmable Then Continue For

            ' Пропуск поле типа Автоинкремент и значение равно 0, то пропускаем
            Dim xPropertyValue = Mapper.GetProperyValue(Data, xCI.PropertyName)
            If xCI.AutoIncrement AndAlso CLng(xPropertyValue) = 0L Then Continue For

            ' Собираем запрос
            sbNames.Append(writer.GetColumnName(xCI.ColumnName))
            sbNames.Append(writer.GetTokenComma)

            If xPropertyValue Is Nothing Then
                sbValues.Append(writer.GetTokenNull)
                sbValues.Append(writer.GetTokenComma)
            Else
                sbValues.Append(writer.GetColumnValue(xPropertyValue, xCI.ColumnType, xCI.PropertyType))
                sbValues.Append(writer.GetTokenComma)
            End If

            forEachFlag = True
        Next

        ' Проверяем флаг, сформировали ли мы поля в запросе
        If Not forEachFlag Then Throw New ColumnNotFoundException
        sbNames.Remove(sbNames.Length - 1, 1)
        sbValues.Remove(sbValues.Length - 1, 1)

        sbBuilder.Append(writer.GetTokenInsert)
        sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
        sbBuilder.Append(writer.GetTokenBracketOpen)
        sbBuilder.Append(sbNames.ToString)
        sbBuilder.Append(writer.GetTokenBracketClose)
        sbBuilder.Append(writer.GetTokenValues)
        sbBuilder.Append(writer.GetTokenBracketOpen)
        sbBuilder.Append(sbValues.ToString)
        sbBuilder.Append(writer.GetTokenBracketClose)

        Return ExecNonQuery(sbBuilder.ToString)
    End Function


    ''' <summary>Обновление данных на основе данных класса. По свойству с атрибутом PrimaryKey происходит выборка обновляемой строки</summary>
    Public Function UpdateRow(Of TClass)(Data As TClass) As Integer
        Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)
        Dim forEachFlag As Boolean

        Dim sbBuilder As New StringBuilder
        Dim writer As IQueryWriter = SQLContextOptions.Writer

        ' Поиск имени столбца с первичным ключом
        Dim pkeyCount = tableInfo.Columns.Where(Function(ci) ci.PrimaryKey = True).Count()

        ' Проверка на корректное заполнение аттрибутов. Первичный ключ из несколько столбцов не поддерживается
        If pkeyCount = 0 Then Throw New PrimaryKeyNotFoundException
        If pkeyCount > 1 Then Throw New PrimaryKeyTooManyException

        Dim pkeyCI = tableInfo.Columns.First(Function(ci) ci.PrimaryKey = True)

        ' Проверяем значение свойства, если пустота, выходим
        Dim pkeyPropertyValue = Mapper.GetProperyValue(Data, pkeyCI.PropertyName)
        If pkeyPropertyValue Is Nothing Then Throw New PrimaryKeyNullException

        ' Собираем запрос
        sbBuilder.Append(writer.GetTokenUpdate)
        sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
        sbBuilder.Append(writer.GetTokenSet)

        For Each xCI In tableInfo.Columns

            ' Пропуск поле типа Программное, то пропускаем
            If xCI.Programmable Then Continue For

            ' Пропуск поле типа Автоинкремент и значение равно 0, то пропускаем
            Dim xPropertyValue = Mapper.GetProperyValue(Data, xCI.PropertyName)
            If xCI.AutoIncrement AndAlso CLng(xPropertyValue) = 0L Then Continue For

            sbBuilder.Append(writer.GetColumnName(xCI.ColumnName))
            sbBuilder.Append(writer.GetTokenEqual)

            If xPropertyValue IsNot Nothing Then
                sbBuilder.Append(writer.GetColumnValue(xPropertyValue, xCI.ColumnType, xCI.PropertyType))
            Else
                sbBuilder.Append(writer.GetTokenNull)
            End If

            sbBuilder.Append(writer.GetTokenComma)

            forEachFlag = True
        Next

        ' Проверяем флаг, сформировали ли мы поля в запросе
        If Not forEachFlag Then Throw New ColumnNotFoundException
        sbBuilder.Remove(sbBuilder.Length - 1, 1)

        ' Дописываем условие
        sbBuilder.Append(writer.GetTokenWhere)
        sbBuilder.Append(writer.GetColumnName(pkeyCI.ColumnName))
        sbBuilder.Append(writer.GetTokenEqual)
        sbBuilder.Append(writer.GetColumnValue(pkeyPropertyValue, pkeyCI.ColumnType, pkeyCI.PropertyType))

        Return ExecNonQuery(sbBuilder.ToString)
    End Function


    ''' <summary>Удаление строки из таблицы. Выборка удаляемой строки выполняется по свойству с атрибутом PrimaryKey</summary>
    Public Function DeleteRow(Of TClass)(Data As TClass) As Integer
        Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)

        Dim sbBuilder As New StringBuilder
        Dim writer As IQueryWriter = SQLContextOptions.Writer

        ' Поиск имени столбца с первичным ключом
        Dim pkeyCount = tableInfo.Columns.Where(Function(ci) ci.PrimaryKey = True).Count()

        ' Проверка на корректное заполнение аттрибутов. Первичный ключ из несколько столбцов не поддерживается
        If pkeyCount = 0 Then Throw New PrimaryKeyNotFoundException
        If pkeyCount > 1 Then Throw New PrimaryKeyTooManyException

        Dim pkeyCI = tableInfo.Columns.First(Function(ci) ci.PrimaryKey = True)

        ' Проверяем значение свойства, если пустота, выходим
        Dim pkeyPropertyValue = Mapper.GetProperyValue(Data, pkeyCI.PropertyName)
        If pkeyPropertyValue Is Nothing Then Throw New PrimaryKeyNullException

        ' Собираем запрос
        sbBuilder.Append(writer.GetTokenDelete())
        sbBuilder.Append(writer.GetTokenFrom())
        sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
        sbBuilder.Append(writer.GetTokenWhere)
        sbBuilder.Append(writer.GetColumnName(pkeyCI.ColumnName))
        sbBuilder.Append(writer.GetTokenEqual)
        sbBuilder.Append(writer.GetColumnValue(pkeyPropertyValue, pkeyCI.ColumnType, pkeyCI.PropertyType))

        Return ExecNonQuery(sbBuilder.ToString)
    End Function


#Region "       Поддержка IDisposable       "

    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' >> dispose managed state (managed objects).
                If sqlConn IsNot Nothing Then
                    sqlConn.Close()
                    sqlConn.Dispose()
                    sqlConn = Nothing
                End If

                SQLContextOptions.PreparedCommand.ClearCache()
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
