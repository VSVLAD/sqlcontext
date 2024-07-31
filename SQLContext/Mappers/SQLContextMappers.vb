Option Explicit On
Option Strict On

Imports System.Linq.Expressions
Imports System.Reflection
Imports VSProject.SQLContext.Attributes

Public Class SQLContextMappers

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

                            ElseIf TypeOf xAttribute Is ConverterAttribute Then
                                Dim attrColumn = DirectCast(xAttribute, ConverterAttribute)
                                columnInfo.UserConverter = attrColumn.Name

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
                                 Function(typeUser)
                                     Dim tableInfo = FromTypeToTableInfo(Of TClass)()
                                     Dim parameter = Expression.Parameter(GetType(IDataRecord), "reader")
                                     Dim bodyBlock As Expression

                                     Dim statementList As New List(Of Expression)          ' Создаём список для последовательности операторов (добавляем присвоение в переменные)
                                     Dim variableList As New List(Of ParameterExpression)  ' Создаём список переменных для блока
                                     Dim bindingsList As New List(Of MemberBinding)        ' Для инициализации свойств пользовательского типа

                                     ' Маппинг в зависимости от простого или составного типа
                                     If IsValueType(typeUser) OrElse IsBaseObjectType(typeUser) Then

                                         ' Для простых типов всегда читаем колонку с индексом 0
                                         Dim callGetValue = Expression.Call(parameter,
                                                                            GetType(IDataRecord).GetMethod("GetValue", {GetType(Integer)}),
                                                                            Expression.Constant(0, GetType(Integer)))

                                         ' Если тип поддерживает Nothing, формируем ещё проверку на DBNull
                                         If IsNullableType(typeUser) Then

                                             ' Создаём переменную под чтение скаляра и читаем в пременную типа Object
                                             Dim varValue = Expression.Variable(GetType(Object), "value")
                                             variableList.Add(varValue)

                                             Dim varAssign = Expression.Assign(varValue, callGetValue)
                                             statementList.Add(varAssign)

                                             ' Проверяем переменную с значением, если содержит DBNull, тогда возвращает default значение, иначе саму сконвертированную переменную в целевой тип
                                             Dim varCheckConvert = Expression.Condition(
                                                                        Expression.Call(varValue,
                                                                                        GetType(Object).GetMethod("Equals", {GetType(Object)}),
                                                                                        Expression.Constant(DBNull.Value)),
                                                                            Expression.Default(typeUser),
                                                                            Expression.Convert(varValue, typeUser)
                                                                    )

                                             ' Добавляем условие в последовательность и этот же оператор будет return
                                             statementList.Add(varCheckConvert)

                                             ' Создаём блок из одного оператора с присваиванием
                                             bodyBlock = Expression.Block(variableList, statementList)
                                         Else
                                             bodyBlock = Expression.Convert(callGetValue, typeUser)
                                         End If

                                     Else
                                         ' Читаем все свойства класса
                                         Dim props = typeUser.GetProperties(BindingFlags.Instance Or BindingFlags.Public)

                                         ' Для составных типов используем мету для маппинга
                                         For Each ci In tableInfo.Columns
                                             Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(ci.PropertyName, StringComparison.InvariantCultureIgnoreCase))

                                             If prop IsNot Nothing Then

                                                 ' Если в DataReader не столбца с таким названием, то это пользовательское свойство, пропускаем
                                                 Dim columnOrdinal = reader.GetOrdinal(ci.ColumnName)
                                                 If columnOrdinal = -1 Then Continue For

                                                 ' Создаём переменную 
                                                 Dim varValue = Expression.Variable(GetType(Object), $"value_{ci.PropertyName}")
                                                 variableList.Add(varValue)

                                                 ' И операцию присваивания
                                                 Dim varAssign = Expression.Assign(varValue,
                                                                                   Expression.Call(parameter,
                                                                                                   GetType(IDataRecord).GetMethod("GetValue", {GetType(Integer)}),
                                                                                                   Expression.Constant(columnOrdinal, GetType(Integer))))
                                                 statementList.Add(varAssign)

                                                 ' Если тип поддерживает Nothing, формируем ещё проверку на DBNull
                                                 If IsNullableType(ci.PropertyType) Then
                                                     Dim memberAssignment = Expression.Bind(
                                                                    prop,
                                                                    Expression.Condition(
                                                                        Expression.Call(varValue,
                                                                                        GetType(Object).GetMethod("Equals", {GetType(Object)}),
                                                                                        Expression.Constant(DBNull.Value)),
                                                                            Expression.Default(ci.PropertyType),
                                                                            Expression.Convert(varValue, ci.PropertyType)
                                                                    ))

                                                     bindingsList.Add(memberAssignment)
                                                 Else
                                                     Dim memberAssignment = Expression.Bind(
                                                                                prop,
                                                                                Expression.Convert(varValue, ci.PropertyType)
                                                                            )

                                                     bindingsList.Add(memberAssignment)
                                                 End If
                                             End If
                                         Next

                                         ' Операция для создания экземпляр пользовательского класса
                                         Dim returnInit = Expression.MemberInit(Expression.[New](typeUser), bindingsList)
                                         statementList.Add(returnInit)

                                         ' Создаём блок кода с присваиванием переменным и возвратом пользовательского типа
                                         bodyBlock = Expression.Block(variableList, statementList)
                                     End If

                                     ' Создаём лямбду и компилируем
                                     Dim lambda = Expression.Lambda(Of Func(Of IDataRecord, TClass))(bodyBlock, parameter)
                                     Return lambda.Compile()

                                 End Function)
        Return CType(funcMapper, Func(Of IDataRecord, TClass))
    End Function

    ''' <summary>Преобразует строку данных в объект пользовательского типа</summary>
    Friend Shared Function FromDictionaryToType(Of T)(row As Dictionary(Of String, Object)) As T
        Dim typeUser = GetType(T)

        If IsValueType(typeUser) OrElse IsBaseObjectType(typeUser) Then ' Если простой тип или скаляр как Object
            Return CType(row.Values.FirstOrDefault(), T)

        ElseIf typeUser.IsClass Then ' Если тип является классом

            ' Читаем мету из кеша или ищем её, если тип не знаком
            Dim tableInfo = FromTypeToTableInfo(Of T)()

            'Маппим столбцы на свойства
            Dim props = typeUser.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
            Dim retObj As T = Activator.CreateInstance(Of T)

            For Each kvData In row

                Dim ci = tableInfo.Columns.FirstOrDefault(Function(c) c.ColumnName.Equals(kvData.Key, StringComparison.InvariantCultureIgnoreCase))
                If ci Is Nothing Then Continue For

                Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(ci.PropertyName, StringComparison.InvariantCultureIgnoreCase))
                If prop Is Nothing Then Continue For

                ' Если есть пользовательский преобразователь, то выполняем его
                If Not String.IsNullOrEmpty(ci.UserConverter) Then
                    Dim converter = UserConverters.Instance.GetConverter(ci.UserConverter)
                    prop.SetValue(retObj, converter(kvData.Value), Nothing)
                Else
                    ' Записываем в свойство значение
                    prop.SetValue(retObj, kvData.Value, Nothing)
                End If
            Next

            Return retObj
        End If

        ' В остальных случаях, когда маппинг на тип не поддерживается
        Throw New NotImplementedException(String.Format(Resources.ExceptionMessages.NOT_IMPLEMENTED_WITH_TYPE, typeUser.ToString()))

    End Function

    ''' <summary>Преобразует строку данных в объект DynamicObject</summary>
    Friend Shared Function FromDictionaryToDynamic(row As Dictionary(Of String, Object)) As DynamicRow
        Return New DynamicRow(row)
    End Function

    ''' <summary>Проверяет, является ли переданный объект базовым Object</summary>
    Friend Shared Function IsBaseObjectType(variable As Type) As Boolean
        Return TypeOf variable Is Object AndAlso
                      variable.BaseType Is Nothing
    End Function

    ''' <summary>
    ''' Проверяет, является ли переданный тип простым значимым (не класс и не структура)
    ''' 1. Хотя DateTime и Int64 являются структурами, считаем их простыми типами
    ''' 2. Enum также считаем простым типом
    ''' 3. String также считаем простым типом
    ''' </summary>
    Friend Shared Function IsValueType(variable As Type) As Boolean
        Return variable.IsPrimitive OrElse
               variable.IsEnum OrElse
               variable Is GetType(String) OrElse
               (variable.IsValueType AndAlso variable.Namespace IsNot Nothing AndAlso variable.Namespace.StartsWith("System"))
    End Function

    ''' <summary>
    ''' Проверка, что тип является сложной пользовательской структурой
    ''' </summary>
    Friend Shared Function IsStructure(variable As Type) As Boolean
        Return Not variable.IsEnum AndAlso
                   variable.IsValueType AndAlso
               Not variable.IsPrimitive AndAlso
                   variable.Namespace IsNot Nothing AndAlso
               Not variable.Namespace.StartsWith("System")
    End Function

    ''' <summary>Метод возвращает значимый тип по свойству, извлекая информацию из Nullable типа</summary>
    Friend Shared Function ValueTypeFrom(variable As Type) As Type
        Return If(IsNullableType(variable), Nullable.GetUnderlyingType(variable), variable)
    End Function

    ''' <summary>Проверяет, является ли тип Nullable</summary>
    Friend Shared Function IsNullableType(variable As Type) As Boolean
        Return Nullable.GetUnderlyingType(variable) IsNot Nothing OrElse variable Is GetType(String)
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
