Option Explicit On
Option Strict On

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
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

    ' Функция для создания делегата маппинга данных из IDataRecord в TClass
    Friend Shared Function FromReaderToExpressionTreeMapper(Of TClass)(reader As IDataRecord) As Func(Of IDataRecord, TClass)
        Dim funcMapper = compileMapperCache.Fetch(GetType(TClass),
                                 Function(typeClass)

                                     Dim tableInfo = FromClassToTableInfo(Of TClass)()
                                     Dim props = typeClass.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
                                     Dim parameter = Expression.Parameter(GetType(IDataRecord), "reader")

                                     Dim bindings As New List(Of MemberBinding)

                                     For Each columnInfo In tableInfo.Columns
                                         Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(columnInfo.PropertyName, StringComparison.InvariantCultureIgnoreCase))

                                         If prop IsNot Nothing Then
                                             Dim getValueMethod = GetType(IDataRecord).GetMethod("GetValue", {GetType(Integer)})
                                             Dim columnIndex = Expression.Constant(reader.GetOrdinal(columnInfo.ColumnName), GetType(Integer))
                                             Dim getValueCall = Expression.Call(parameter, getValueMethod, columnIndex)

                                             ' Если тип поддерживает Nothing, формируем ещё проверку на DBNull
                                             If IsNullableType(columnInfo.PropertyType) Then
                                                 Dim defaultValue = Expression.Default(columnInfo.PropertyType)

                                                 Dim checkDBNull = Expression.Call(getValueCall, GetType(Object).GetMethod("Equals", {GetType(Object)}), Expression.Constant(DBNull.Value))
                                                 Dim convertValue = Expression.Condition(
                                                        checkDBNull,
                                                        defaultValue,
                                                        Expression.Convert(getValueCall, columnInfo.PropertyType)
                                                    )

                                                 Dim memberAssignment = Expression.Bind(prop, convertValue)

                                                 bindings.Add(memberAssignment)
                                             Else
                                                 Dim convertValue = Expression.Convert(getValueCall, columnInfo.PropertyType)
                                                 Dim memberAssignment = Expression.Bind(prop, convertValue)

                                                 bindings.Add(memberAssignment)
                                             End If
                                         End If
                                     Next

                                     Dim memberInit = Expression.MemberInit(Expression.[New](typeClass), bindings)
                                     Dim lambda = Expression.Lambda(Of Func(Of IDataRecord, TClass))(memberInit, parameter)
                                     Dim func = lambda.Compile()

                                     Return func
                                 End Function)
        Return CType(funcMapper, Func(Of IDataRecord, TClass))
    End Function

    ''' <summary>Преобразует метаинформацию класса в объект данных о таблице и столбцах</summary>
    Friend Shared Function FromClassToTableInfo(Of TClass)() As TableInformation
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


    ''' <summary>Преобразует строку данных в объект пользовательского типа</summary>
    Protected Friend Shared Function FromDictionaryToType(Of T)(row As Dictionary(Of String, Object)) As T
        Dim typeClass = GetType(T)

        ' Если T является массивом
        If typeClass.IsArray AndAlso typeClass.GetElementType() Is GetType(Object) Then

            Dim resultArray(row.Count - 1) As Object
            Dim ind As Integer

            For Each item In row.Values
                resultArray(ind) = item
                ind += 1
            Next

            ' TODO CHECK!
            Return CType(CType(resultArray, Object), T)

            ' Если простой значимый тип
        ElseIf typeClass.IsPrimitive OrElse typeClass.IsValueType OrElse typeClass Is GetType(String) Then
            If IsNullableType(typeClass) Then
                Throw New NotImplementedException(Resources.ExceptionMessages.NOT_IMPLEMENTED_WITH_NULLABLE)
            Else
                Return CType(row.Values.FirstOrDefault(), T)
            End If

            ' Если TClass является классом
        ElseIf typeClass.IsClass Then

            ' Читаем мету из кеша или обновляем его
            Dim tableInfo = FromClassToTableInfo(Of T)()

            'Маппим столбцы на свойства
            Dim props = typeClass.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
            Dim resultObject As T = Activator.CreateInstance(Of T)

            For Each kvData In row

                Dim ci = tableInfo.Columns.FirstOrDefault(Function(c) c.ColumnName.Equals(kvData.Key, StringComparison.InvariantCultureIgnoreCase))
                If ci Is Nothing Then Continue For

                Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(ci.PropertyName, StringComparison.InvariantCultureIgnoreCase))
                If prop Is Nothing Then Continue For

                ' Записываем в свойство значение
                prop.SetValue(resultObject, kvData.Value, Nothing)
            Next

            Return resultObject
        End If

        ' В остальных случаях, когда тип T не поддерживается
        Throw New NotImplementedException(String.Format(Resources.ExceptionMessages.NOT_IMPLEMENTED_WITH_TYPE, typeClass.ToString()))

    End Function

    ''' <summary>Преобразует строку данных в объект DynamicObject</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Friend Shared Function FromDictionaryToDynamic(row As Dictionary(Of String, Object)) As DynamicRow
        Return New DynamicRow(row)
    End Function

    ''' <summary>Метод читает указанное свойство из класса</summary>
    ''' <typeparam name="T">Тип пользовательского класса</typeparam>
    ''' <param name="ClassObject">Объект пользовательского класса</param>
    ''' <param name="PropertyName">Имя свойства</param>
    ''' <returns>Значение свойства</returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Friend Shared Function GetProperyValue(Of T)(ClassObject As T, PropertyName As String) As Object
        Return GetType(T).GetProperty(PropertyName).GetValue(ClassObject)
    End Function

    ''' <summary>Метод возвращает значимый тип по свойству, извлекая информацию из Nullable типа</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Friend Shared Function ValueTypeFrom(variable As Type) As Type
        Return If(IsNullableType(variable), Nullable.GetUnderlyingType(variable), variable)
    End Function

    ''' <summary>Метод проверяет, имеет ли свойство тип Nullable</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Friend Shared Function IsNullableType(variable As Type) As Boolean
        Return Nullable.GetUnderlyingType(variable) IsNot Nothing
    End Function

End Class
