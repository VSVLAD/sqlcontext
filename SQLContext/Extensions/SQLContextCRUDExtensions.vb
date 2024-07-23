'Imports System.Data
'Imports System.Runtime.CompilerServices
'Imports System.Text
'Imports VSProject.SQLContext.Exceptions
'Imports VSProject.SQLContext.Interfaces

'Namespace Extensions

'    ''' <summary>Общие методы для выполнения простых запросов</summary>
'    Public Module SQLContextCRUDExtensions

'        ''' <summary>Вставка строки на основе данных класса. Свойства с атрибутом AutoIncrement и 0 значением не задействованы в операциях вставки</summary>
'        Public Function InsertRow(Of TClass)(DataObject As TClass) As Integer
'            Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)
'            Dim forEachFlag As Boolean

'            Dim writer As IQueryWriter = Me.Options.Writer
'            Dim sbBuilder As New StringBuilder
'            Dim sbNames As New StringBuilder
'            Dim sbValues As New StringBuilder

'            For Each xCI In tableInfo.Columns

'                ' Пропускаем столбцы заданные как программные
'                If xCI.Programmable Then Continue For

'                ' Пропуск поле типа Автоинкремент и значение равно 0, то пропускаем
'                Dim xPropertyValue = Mapper.GetProperyValue(DataObject, xCI.PropertyName)
'                If xCI.AutoIncrement AndAlso CLng(xPropertyValue) = 0L Then Continue For

'                ' Собираем запрос
'                sbNames.Append(writer.GetColumnName(xCI.ColumnName))
'                sbNames.Append(writer.GetTokenComma)

'                If xPropertyValue Is Nothing Then
'                    sbValues.Append(writer.GetTokenNull)
'                    sbValues.Append(writer.GetTokenComma)
'                Else
'                    sbValues.Append(writer.GetColumnValue(xPropertyValue, xCI.PropertyType, xCI.ColumnType))
'                    sbValues.Append(writer.GetTokenComma)
'                End If

'                forEachFlag = True
'            Next

'            ' Проверяем флаг, сформировали ли мы поля в запросе
'            If Not forEachFlag Then Throw New ColumnNotFoundException
'            sbNames.Remove(sbNames.Length - 1, 1)
'            sbValues.Remove(sbValues.Length - 1, 1)

'            sbBuilder.Append(writer.GetTokenInsert)
'            sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
'            sbBuilder.Append(writer.GetTokenBracketOpen)
'            sbBuilder.Append(sbNames.ToString)
'            sbBuilder.Append(writer.GetTokenBracketClose)
'            sbBuilder.Append(writer.GetTokenValues)
'            sbBuilder.Append(writer.GetTokenBracketOpen)
'            sbBuilder.Append(sbValues.ToString)
'            sbBuilder.Append(writer.GetTokenBracketClose)

'            Dim query = sbBuilder.ToString
'            Return ExecNonQuery(query)
'        End Function


'        ''' <summary>Обновление данных на основе данных класса. По свойству с атрибутом PrimaryKey происходит выборка обновляемой строки</summary>
'        Public Function UpdateRow(Of TClass)(DataObject As TClass) As Integer
'            Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)
'            Dim forEachFlag As Boolean

'            Dim sbBuilder As New StringBuilder
'            Dim writer As IQueryWriter = Me.Options.Writer

'            ' Поиск имени столбца с первичным ключом
'            Dim pkeyCount = tableInfo.Columns.Where(Function(ci) ci.PrimaryKey = True).Count()

'            ' Проверка на корректное заполнение аттрибутов. Первичный ключ из несколько столбцов не поддерживается
'            If pkeyCount = 0 Then Throw New PrimaryKeyNotFoundException
'            If pkeyCount > 1 Then Throw New PrimaryKeyTooManyException

'            Dim pkeyCI = tableInfo.Columns.First(Function(ci) ci.PrimaryKey = True)

'            ' Проверяем значение свойства, если пустота, выходим
'            Dim pkeyPropertyValue = Mapper.GetProperyValue(DataObject, pkeyCI.PropertyName)
'            If pkeyPropertyValue Is Nothing Then Throw New PrimaryKeyNullException

'            ' Собираем запрос
'            sbBuilder.Append(writer.GetTokenUpdate)
'            sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
'            sbBuilder.Append(writer.GetTokenSet)

'            For Each xCI In tableInfo.Columns

'                ' Пропускаем столбцы заданные как программные
'                If xCI.Programmable Then Continue For

'                ' Пропуск поле типа Автоинкремент и значение равно 0, то пропускаем
'                Dim xPropertyValue = Mapper.GetProperyValue(DataObject, xCI.PropertyName)
'                If xCI.AutoIncrement AndAlso CLng(xPropertyValue) = 0L Then Continue For

'                sbBuilder.Append(writer.GetColumnName(xCI.ColumnName))
'                sbBuilder.Append(writer.GetTokenEqual)

'                If xPropertyValue IsNot Nothing Then
'                    sbBuilder.Append(writer.GetColumnValue(xPropertyValue, xCI.PropertyType, xCI.ColumnType))
'                Else
'                    sbBuilder.Append(writer.GetTokenNull)
'                End If

'                sbBuilder.Append(writer.GetTokenComma)

'                forEachFlag = True
'            Next

'            ' Проверяем флаг, сформировали ли мы поля в запросе
'            If Not forEachFlag Then Throw New ColumnNotFoundException
'            sbBuilder.Remove(sbBuilder.Length - 1, 1)

'            ' Дописываем условие
'            sbBuilder.Append(writer.GetTokenWhere)
'            sbBuilder.Append(writer.GetColumnName(pkeyCI.ColumnName))
'            sbBuilder.Append(writer.GetTokenEqual)
'            sbBuilder.Append(writer.GetColumnValue(pkeyPropertyValue, pkeyCI.PropertyType, pkeyCI.ColumnType))

'            Dim query = sbBuilder.ToString
'            Return ExecNonQuery(query)
'        End Function


'        ''' <summary>Удаление строки из таблицы. Выборка удаляемой строки выполняется по свойству с атрибутом PrimaryKey</summary>
'        Public Function DeleteRow(Of TClass)(DataObject As TClass) As Integer
'            Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)

'            Dim sbBuilder As New StringBuilder
'            Dim writer As IQueryWriter = Me.Options.Writer

'            ' Поиск имени столбца с первичным ключом
'            Dim pkeyCount = tableInfo.Columns.Where(Function(ci) ci.PrimaryKey = True).Count()

'            ' Проверка на корректное заполнение аттрибутов. Первичный ключ из несколько столбцов не поддерживается
'            If pkeyCount = 0 Then Throw New PrimaryKeyNotFoundException
'            If pkeyCount > 1 Then Throw New PrimaryKeyTooManyException

'            Dim pkeyCI = tableInfo.Columns.First(Function(ci) ci.PrimaryKey = True)

'            ' Проверяем значение свойства, если пустота, выходим
'            Dim pkeyPropertyValue = Mapper.GetProperyValue(DataObject, pkeyCI.PropertyName)
'            If pkeyPropertyValue Is Nothing Then Throw New PrimaryKeyNullException

'            ' Собираем запрос
'            sbBuilder.Append(writer.GetTokenDelete())
'            sbBuilder.Append(writer.GetTokenFrom())
'            sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
'            sbBuilder.Append(writer.GetTokenWhere)
'            sbBuilder.Append(writer.GetColumnName(pkeyCI.ColumnName))
'            sbBuilder.Append(writer.GetTokenEqual)
'            sbBuilder.Append(writer.GetColumnValue(pkeyPropertyValue, pkeyCI.PropertyType, pkeyCI.ColumnType))

'            Dim query = sbBuilder.ToString
'            Return ExecNonQuery(query)
'        End Function


'    End Module

'End Namespace'Imports System.Data
'Imports System.Runtime.CompilerServices
'Imports System.Text
'Imports VSProject.SQLContext.Exceptions
'Imports VSProject.SQLContext.Interfaces

'Namespace Extensions

'    ''' <summary>Общие методы для выполнения простых запросов</summary>
'    Public Module SQLContextCRUDExtensions

'        ''' <summary>Вставка строки на основе данных класса. Свойства с атрибутом AutoIncrement и 0 значением не задействованы в операциях вставки</summary>
'        Public Function InsertRow(Of TClass)(DataObject As TClass) As Integer
'            Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)
'            Dim forEachFlag As Boolean

'            Dim writer As IQueryWriter = Me.Options.Writer
'            Dim sbBuilder As New StringBuilder
'            Dim sbNames As New StringBuilder
'            Dim sbValues As New StringBuilder

'            For Each xCI In tableInfo.Columns

'                ' Пропускаем столбцы заданные как программные
'                If xCI.Programmable Then Continue For

'                ' Пропуск поле типа Автоинкремент и значение равно 0, то пропускаем
'                Dim xPropertyValue = Mapper.GetProperyValue(DataObject, xCI.PropertyName)
'                If xCI.AutoIncrement AndAlso CLng(xPropertyValue) = 0L Then Continue For

'                ' Собираем запрос
'                sbNames.Append(writer.GetColumnName(xCI.ColumnName))
'                sbNames.Append(writer.GetTokenComma)

'                If xPropertyValue Is Nothing Then
'                    sbValues.Append(writer.GetTokenNull)
'                    sbValues.Append(writer.GetTokenComma)
'                Else
'                    sbValues.Append(writer.GetColumnValue(xPropertyValue, xCI.PropertyType, xCI.ColumnType))
'                    sbValues.Append(writer.GetTokenComma)
'                End If

'                forEachFlag = True
'            Next

'            ' Проверяем флаг, сформировали ли мы поля в запросе
'            If Not forEachFlag Then Throw New ColumnNotFoundException
'            sbNames.Remove(sbNames.Length - 1, 1)
'            sbValues.Remove(sbValues.Length - 1, 1)

'            sbBuilder.Append(writer.GetTokenInsert)
'            sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
'            sbBuilder.Append(writer.GetTokenBracketOpen)
'            sbBuilder.Append(sbNames.ToString)
'            sbBuilder.Append(writer.GetTokenBracketClose)
'            sbBuilder.Append(writer.GetTokenValues)
'            sbBuilder.Append(writer.GetTokenBracketOpen)
'            sbBuilder.Append(sbValues.ToString)
'            sbBuilder.Append(writer.GetTokenBracketClose)

'            Dim query = sbBuilder.ToString
'            Return ExecNonQuery(query)
'        End Function


'        ''' <summary>Обновление данных на основе данных класса. По свойству с атрибутом PrimaryKey происходит выборка обновляемой строки</summary>
'        Public Function UpdateRow(Of TClass)(DataObject As TClass) As Integer
'            Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)
'            Dim forEachFlag As Boolean

'            Dim sbBuilder As New StringBuilder
'            Dim writer As IQueryWriter = Me.Options.Writer

'            ' Поиск имени столбца с первичным ключом
'            Dim pkeyCount = tableInfo.Columns.Where(Function(ci) ci.PrimaryKey = True).Count()

'            ' Проверка на корректное заполнение аттрибутов. Первичный ключ из несколько столбцов не поддерживается
'            If pkeyCount = 0 Then Throw New PrimaryKeyNotFoundException
'            If pkeyCount > 1 Then Throw New PrimaryKeyTooManyException

'            Dim pkeyCI = tableInfo.Columns.First(Function(ci) ci.PrimaryKey = True)

'            ' Проверяем значение свойства, если пустота, выходим
'            Dim pkeyPropertyValue = Mapper.GetProperyValue(DataObject, pkeyCI.PropertyName)
'            If pkeyPropertyValue Is Nothing Then Throw New PrimaryKeyNullException

'            ' Собираем запрос
'            sbBuilder.Append(writer.GetTokenUpdate)
'            sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
'            sbBuilder.Append(writer.GetTokenSet)

'            For Each xCI In tableInfo.Columns

'                ' Пропускаем столбцы заданные как программные
'                If xCI.Programmable Then Continue For

'                ' Пропуск поле типа Автоинкремент и значение равно 0, то пропускаем
'                Dim xPropertyValue = Mapper.GetProperyValue(DataObject, xCI.PropertyName)
'                If xCI.AutoIncrement AndAlso CLng(xPropertyValue) = 0L Then Continue For

'                sbBuilder.Append(writer.GetColumnName(xCI.ColumnName))
'                sbBuilder.Append(writer.GetTokenEqual)

'                If xPropertyValue IsNot Nothing Then
'                    sbBuilder.Append(writer.GetColumnValue(xPropertyValue, xCI.PropertyType, xCI.ColumnType))
'                Else
'                    sbBuilder.Append(writer.GetTokenNull)
'                End If

'                sbBuilder.Append(writer.GetTokenComma)

'                forEachFlag = True
'            Next

'            ' Проверяем флаг, сформировали ли мы поля в запросе
'            If Not forEachFlag Then Throw New ColumnNotFoundException
'            sbBuilder.Remove(sbBuilder.Length - 1, 1)

'            ' Дописываем условие
'            sbBuilder.Append(writer.GetTokenWhere)
'            sbBuilder.Append(writer.GetColumnName(pkeyCI.ColumnName))
'            sbBuilder.Append(writer.GetTokenEqual)
'            sbBuilder.Append(writer.GetColumnValue(pkeyPropertyValue, pkeyCI.PropertyType, pkeyCI.ColumnType))

'            Dim query = sbBuilder.ToString
'            Return ExecNonQuery(query)
'        End Function


'        ''' <summary>Удаление строки из таблицы. Выборка удаляемой строки выполняется по свойству с атрибутом PrimaryKey</summary>
'        Public Function DeleteRow(Of TClass)(DataObject As TClass) As Integer
'            Dim tableInfo = Mapper.FromClassToTableInfo(Of TClass)

'            Dim sbBuilder As New StringBuilder
'            Dim writer As IQueryWriter = Me.Options.Writer

'            ' Поиск имени столбца с первичным ключом
'            Dim pkeyCount = tableInfo.Columns.Where(Function(ci) ci.PrimaryKey = True).Count()

'            ' Проверка на корректное заполнение аттрибутов. Первичный ключ из несколько столбцов не поддерживается
'            If pkeyCount = 0 Then Throw New PrimaryKeyNotFoundException
'            If pkeyCount > 1 Then Throw New PrimaryKeyTooManyException

'            Dim pkeyCI = tableInfo.Columns.First(Function(ci) ci.PrimaryKey = True)

'            ' Проверяем значение свойства, если пустота, выходим
'            Dim pkeyPropertyValue = Mapper.GetProperyValue(DataObject, pkeyCI.PropertyName)
'            If pkeyPropertyValue Is Nothing Then Throw New PrimaryKeyNullException

'            ' Собираем запрос
'            sbBuilder.Append(writer.GetTokenDelete())
'            sbBuilder.Append(writer.GetTokenFrom())
'            sbBuilder.Append(writer.GetTableName(tableInfo.TableName))
'            sbBuilder.Append(writer.GetTokenWhere)
'            sbBuilder.Append(writer.GetColumnName(pkeyCI.ColumnName))
'            sbBuilder.Append(writer.GetTokenEqual)
'            sbBuilder.Append(writer.GetColumnValue(pkeyPropertyValue, pkeyCI.PropertyType, pkeyCI.ColumnType))

'            Dim query = sbBuilder.ToString
'            Return ExecNonQuery(query)
'        End Function


'    End Module

'End Namespace
