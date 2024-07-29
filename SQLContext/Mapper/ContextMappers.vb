Option Explicit On
Option Strict On
Imports System.Linq.Expressions
Imports System.Reflection
Imports VSProject.SQLContext.Attributes

Public Class ContextMappers

    ''' <summary>
    ''' Хранит кеш метаинформации по таблице и столбцам для маппинга
    ''' </summary>
    Private Shared tableInfoCache As New MemoryCache(Of Type, TableInformation)

    ''' <summary>
    ''' Хранит кеш скомпилированных мапперов
    ''' </summary>
    Private Shared compileMapperCache As New MemoryCache(Of Type, [Delegate])

    ''' <summary>Преобразует метаинформацию класса в объект данных о таблице и столбцах</summary>
    Friend Shared Function FromTypeToTableInfo(Of TClass)() As TableInformation
        Return tableInfoCache.Fetch(GetType(TClass),
                Function(type)
                    Dim tableInfo As New TableInformation()

                    'Поиск табличного атрибута у класса
                    For Each xAttrib As Attribute In type.GetCustomAttributes(GetType(TableAttribute), True)
                        tableInfo.TableName = DirectCast(xAttrib, TableAttribute).Name
                    Next

                    'Если нет переопределения имени, то именем таблицы будет название класса
                    If String.IsNullOrEmpty(tableInfo.TableName) Then tableInfo.TableName = type.Name

                    'Добавляем найденные атрибуты. Если атрибут не указан, используется значение атрибута по-умолчанию
                    For Each xClassProperty In type.GetProperties(BindingFlags.Instance Or BindingFlags.Public)

                        Dim columnInfo As New ColumnInformation With {
                                .PropertyName = xClassProperty.Name,
                                .PropertyType = xClassProperty.PropertyType
                            }

                        For Each xAttribute In xClassProperty.GetCustomAttributes(True)
                            If TypeOf xAttribute Is ColumnAttribute Then
                                Dim attrColumn = DirectCast(xAttribute, ColumnAttribute)
                                columnInfo.ColumnName = attrColumn.Name

                            ElseIf TypeOf xAttribute Is PrimaryKeyAttribute Then
                                columnInfo.PrimaryKey = True

                            End If
                        Next

                        'Если название для столбца не было задано через атрибуты, тогда используем как имя свойства
                        If String.IsNullOrEmpty(columnInfo.ColumnName) Then
                            columnInfo.ColumnName = columnInfo.PropertyName
                        End If

                        tableInfo.Columns.Add(columnInfo)
                    Next

                    Return tableInfo
                End Function)
    End Function

    ' Функция для создания делегата маппинга данных из IDataRecord в TClass
    Friend Shared Function FromReaderToExpressionTreeMapper(Of TClass)(reader As IDataRecord) As Func(Of IDataRecord, TClass)
        Dim funcMapper = compileMapperCache.Fetch(GetType(TClass),
                                 Function(typeClass)
                                     Dim tableInfo = FromTypeToTableInfo(Of TClass)()
                                     Dim parameter = Expression.Parameter(GetType(IDataRecord), "reader")

                                     ' Маппинг в зависимости от простого или составного типа
                                     If IsPrimitiveType(typeClass) OrElse IsBaseObjectType(typeClass) Then
                                         Dim returnExp As Expression

                                         ' Для простых типов всегда читаем колонку с индексом 0
                                         Dim methodGetValue = GetType(IDataRecord).GetMethod("GetValue", {GetType(Integer)})
                                         Dim constOrdinalZero = Expression.Constant(0, GetType(Integer))
                                         Dim callGetValue = Expression.Call(parameter,
                                                                            methodGetValue,
                                                                            constOrdinalZero)

                                         ' Если тип поддерживает Nothing, формируем ещё проверку на DBNull
                                         If IsNullableType(typeClass) Then

                                             ' Создаём переменную под чтение скаляра и читаем в пременную типа Object
                                             Dim varValue = Expression.Variable(GetType(Object), "value")
                                             Dim varAssign = Expression.Assign(varValue, callGetValue)

                                             ' Создаём список из последовательности операторов (добавляем присвоение в переменную)
                                             Dim statementList As New List(Of Expression)
                                             statementList.Add(varAssign)

                                             ' Проверяем переменную с значением, если содержит DBNull, тогда возвращает default значение, иначе саму переменную
                                             Dim defaultValue = Expression.Default(typeClass)
                                             Dim callCheckDBNull = Expression.Call(varValue, GetType(Object).GetMethod("Equals", {GetType(Object)}), Expression.Constant(DBNull.Value))

                                             ' Кастуем переменную к типу возвращаемого значения
                                             Dim convertValue = Expression.Convert(varValue, typeClass)

                                             Dim ifConvertedValue = Expression.Condition(
                                                    callCheckDBNull,
                                                    defaultValue,
                                                    convertValue
                                                )

                                             ' Добавляем условие в последовательность и этот же оператор будет return
                                             statementList.Add(ifConvertedValue)

                                             ' Создаём готовый блок из операторов
                                             returnExp = Expression.Block(New ParameterExpression() {varValue}, statementList)
                                         Else
                                             returnExp = Expression.Convert(callGetValue, typeClass)
                                         End If

                                         Dim lambda = Expression.Lambda(Of Func(Of IDataRecord, TClass))(returnExp, parameter)
                                         Return lambda.Compile()

                                     Else
                                         Dim props = typeClass.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                                         Dim bindings As New List(Of MemberBinding)

                                         ' Для составных типов используем мету для маппинга
                                         For Each columnInfo In tableInfo.Columns
                                             Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(columnInfo.PropertyName, StringComparison.InvariantCultureIgnoreCase))

                                             If prop IsNot Nothing Then
                                                 Dim getValueMethod = GetType(IDataRecord).GetMethod("GetValue", {GetType(Integer)})
                                                 Dim columnOrdinal = reader.GetOrdinal(columnInfo.ColumnName)

                                                 ' Если в DataReader не столбца с таким названием, то это пользовательское свойство, пропускаем
                                                 If columnOrdinal = -1 Then Continue For

                                                 Dim columnIndex = Expression.Constant(columnOrdinal, GetType(Integer))
                                                 Dim methodGetValue = Expression.Call(parameter, getValueMethod, columnIndex)

                                                 ' Если тип поддерживает Nothing, формируем ещё проверку на DBNull
                                                 If IsNullableType(columnInfo.PropertyType) OrElse columnInfo.PropertyType Is GetType(String) Then
                                                     Dim defaultValue = Expression.Default(columnInfo.PropertyType)

                                                     Dim callCheckDBNull = Expression.Call(methodGetValue, GetType(Object).GetMethod("Equals", {GetType(Object)}), Expression.Constant(DBNull.Value))
                                                     Dim convertValue = Expression.Condition(
                                                        callCheckDBNull,
                                                        defaultValue,
                                                        Expression.Convert(methodGetValue, columnInfo.PropertyType)
                                                    )

                                                     Dim memberAssignment = Expression.Bind(prop, convertValue)
                                                     bindings.Add(memberAssignment)
                                                 Else
                                                     Dim convertValue = Expression.Convert(methodGetValue, columnInfo.PropertyType)
                                                     Dim memberAssignment = Expression.Bind(prop, convertValue)

                                                     bindings.Add(memberAssignment)
                                                 End If
                                             End If
                                         Next

                                         ' Создаём экземпляр составного типа
                                         Dim memberInit = Expression.MemberInit(Expression.[New](typeClass), bindings)
                                         Dim lambda = Expression.Lambda(Of Func(Of IDataRecord, TClass))(memberInit, parameter)

                                         Return lambda.Compile()
                                     End If
                                 End Function)
        Return CType(funcMapper, Func(Of IDataRecord, TClass))
    End Function

    ''' <summary>Преобразует строку данных в объект пользовательского типа</summary>
    Friend Shared Function FromDictionaryToType(Of T)(row As Dictionary(Of String, Object)) As T
        Dim typeClass = GetType(T)

        If IsPrimitiveType(typeClass) OrElse IsBaseObjectType(typeClass) Then ' Если простой значимый тип или скаляр в Object
            Return CType(row.Values.FirstOrDefault(), T)

        ElseIf typeClass.IsClass Then ' Если тип является классом

            ' Читаем мету из кеша или ищем её, если тип не знаком
            Dim tableInfo = FromTypeToTableInfo(Of T)()

            'Маппим столбцы на свойства
            Dim props = typeClass.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
            Dim retObj As T = Activator.CreateInstance(Of T)

            For Each kvData In row

                Dim ci = tableInfo.Columns.FirstOrDefault(Function(c) c.ColumnName.Equals(kvData.Key, StringComparison.InvariantCultureIgnoreCase))
                If ci Is Nothing Then Continue For

                Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(ci.PropertyName, StringComparison.InvariantCultureIgnoreCase))
                If prop Is Nothing Then Continue For

                ' Записываем в свойство значение
                prop.SetValue(retObj, kvData.Value, Nothing)
            Next

            Return retObj
        End If

        ' В остальных случаях, когда маппинг на тип не поддерживается
        Throw New NotImplementedException(String.Format(Resources.ExceptionMessages.NOT_IMPLEMENTED_WITH_TYPE, typeClass.ToString()))

    End Function

    ''' <summary>Преобразует строку данных в объект DynamicObject</summary>
    Friend Shared Function FromDictionaryToDynamic(row As Dictionary(Of String, Object)) As DynamicRow
        Return New DynamicRow(row)
    End Function

    ''' <summary>Проверяет, является ли переданный объект базовым Object</summary>
    Friend Shared Function IsBaseObjectType(variable As Type) As Boolean
        Return TypeOf variable Is Object AndAlso variable.BaseType Is Nothing
    End Function

    ''' <summary>Проверяет, является ли переданный тип простым значимым</summary>
    Friend Shared Function IsPrimitiveType(variable As Type) As Boolean
        Return variable.IsPrimitive OrElse variable.IsValueType OrElse variable Is GetType(String)
    End Function

    ''' <summary>Метод возвращает значимый тип по свойству, извлекая информацию из Nullable типа</summary>
    Friend Shared Function ValueTypeFrom(variable As Type) As Type
        Return If(IsNullableType(variable), Nullable.GetUnderlyingType(variable), variable)
    End Function

    ''' <summary>Проверяет, является ли тип Nullable</summary>
    Friend Shared Function IsNullableType(variable As Type) As Boolean
        Return Nullable.GetUnderlyingType(variable) IsNot Nothing
    End Function

    ''' <summary>Проверяет, является ли тип Nullable</summary>
    Friend Shared Function IsNullableType(Of T)() As Boolean
        Return Nullable.GetUnderlyingType(GetType(T)) IsNot Nothing
    End Function

    Public Shared Sub ClearFastMapperCache()
        compileMapperCache.Clear()
    End Sub

    Public Shared Sub ClearInternalMapperCache()
        tableInfoCache.Clear()
    End Sub

End Class
