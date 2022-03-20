Imports System.Reflection
Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Dynamic
Imports VSProject.SQLContext.Interfaces

''' <summary>Класс предоставляем методы для маппинга строк из базы данных в класс</summary>
Friend Class Mapper

    ''' <summary>Преобразует метаинформацию класса в словарь</summary>
    Friend Shared Function FromClassToTableInfo(Of TClass)() As TableInformation
        Dim typeClass As Type = GetType(TClass)
        Dim tableInfo As TableInformation

        'Проверка в кеше
        If MapperCache.Available(typeClass) Then
            tableInfo = MapperCache.Load(typeClass) 'Сначала читаем из кеша
        Else
            'Если нет в кеше, находим метаданные и сохраняем в кеше
            tableInfo = New TableInformation()

            'Поиск атрибута у класса
            For Each xAttrib As Attribute In typeClass.GetCustomAttributes(GetType(TableAttribute), True)
                tableInfo.TableName = DirectCast(xAttrib, TableAttribute).Name
            Next

            'Если нет переопределения имени, то именем таблицы будет название класса
            If String.IsNullOrEmpty(tableInfo.TableName) Then tableInfo.TableName = typeClass.Name

            'Добавляем найденные атрибуты. Если атрибут не указан, используется значение атрибута по-умолчанию
            For Each xClassProperty In typeClass.GetProperties(BindingFlags.Instance Or BindingFlags.Public)

                Dim columnInfo As New ColumnInformation With {
                    .PropertyName = xClassProperty.Name,
                    .PropertyType = ValueTypeFrom(xClassProperty.PropertyType)
                }

                For Each xAttribute In xClassProperty.GetCustomAttributes(True)
                    If TypeOf xAttribute Is ColumnAttribute Then
                        Dim attrColumn = DirectCast(xAttribute, ColumnAttribute)
                        columnInfo.ColumnName = attrColumn.Name
                        columnInfo.ColumnType = attrColumn.DataType

                    ElseIf TypeOf xAttribute Is PrimaryKeyAttribute Then
                        columnInfo.PrimaryKey = True

                    ElseIf TypeOf xAttribute Is AutoIncrementAttribute Then
                        columnInfo.AutoIncrement = True

                    ElseIf TypeOf xAttribute Is ProgrammableAttribute Then
                        columnInfo.Programmable = True

                    End If
                Next

                'Если название и тип для столбца не были задано через аттрибуты, используем тогда имя и тип свойства
                If String.IsNullOrEmpty(columnInfo.ColumnName) Then columnInfo.ColumnName = columnInfo.PropertyName
                If columnInfo.ColumnType Is Nothing Then columnInfo.ColumnType = columnInfo.PropertyType

                tableInfo.Columns.Add(columnInfo)
            Next

            MapperCache.Store(typeClass, tableInfo)
        End If

        Return tableInfo
    End Function


    ''' <summary>Преобразует строку данных в объект пользовательского типа</summary>
    Friend Shared Function FromDictionaryToType(Of T)(row As Dictionary(Of String, Object), contextOptions As SQLContextOptions) As T
        Dim typeClass = GetType(T)

        ' Если TClass является массивом
        If typeClass.IsArray AndAlso typeClass.GetElementType() Is GetType(Object) Then

            Dim resultArray(row.Count - 1) As Object
            Dim ind As Integer

            For Each val In row.Values
                resultArray(ind) = val
                ind += 1
            Next

            Return CType(resultArray, Object)

            ' Если простой значимый тип
        ElseIf typeClass.IsPrimitive OrElse typeClass.IsValueType OrElse typeClass Is GetType(String) Then
            If IsNullableType(typeClass) Then

                'Dim resultValue As T = Activator.CreateInstance(Of T)
                'typeClass.GetField("value", BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.NonPublic).SetValue(resultValue, row.Values.First())

                'Return resultValue

                Throw New NotImplementedException(Resources.ExceptionMessages.NOT_IMPLEMENTED_WITH_NULLABLE)
            Else
                Return CType(row.Values.First(), T)
            End If

            ' Если TClass является классом
        ElseIf typeClass.IsClass Then

            Dim tableInfo As TableInformation

            'Проверка типа в кеше
            If MapperCache.Available(typeClass) Then
                tableInfo = MapperCache.Load(typeClass) 'Сначала читаем из кеша
            Else
                'Дополняем информацию по метаданным, необходимую для корректного маппинга
                tableInfo = FromClassToTableInfo(Of T)()
                MapperCache.Store(typeClass, tableInfo)
            End If

            'Маппим столбцы на свойства
            Dim props = typeClass.GetProperties(BindingFlags.Instance Or BindingFlags.Public)

            Dim resultObject As T = Activator.CreateInstance(Of T)

            For Each keyRowData In row

                Dim ci = tableInfo.Columns.FirstOrDefault(Function(c) c.ColumnName.Equals(keyRowData.Key, StringComparison.InvariantCultureIgnoreCase))
                If ci Is Nothing Then Continue For

                Dim prop = props.FirstOrDefault(Function(p) p.Name.Equals(ci.PropertyName, StringComparison.InvariantCultureIgnoreCase))
                If prop Is Nothing Then Continue For

                ' Записываем в свойство значение
                prop.SetValue(resultObject, contextOptions.Reader.GetPropertyValue(keyRowData.Value, ci.ColumnType, ci.PropertyType), Nothing)
            Next

            Return resultObject
        End If

        ' В остальных случаях, когда тип T не поддерживается
        Throw New NotImplementedException(String.Format(Resources.ExceptionMessages.NOT_IMPLEMENTED_WITH_TYPE, typeClass.ToString()))

    End Function

    ''' <summary>Преобразует строку данных в объект DynamicObject</summary>
    Friend Shared Function FromDictionaryToDynamic(row As Dictionary(Of String, Object)) As DynamicRowItem
        Return New DynamicRowItem(row)
    End Function

    ''' <summary>Метод читает указанное свойство из класса</summary>
    ''' <typeparam name="T">Тип пользовательского класса</typeparam>
    ''' <param name="ClassObject">Объект пользовательского класса</param>
    ''' <param name="PropertyName">Имя свойства</param>
    ''' <returns>Значение свойства</returns>
    Friend Shared Function GetProperyValue(Of T)(ClassObject As T, PropertyName As String) As Object
        Return GetType(T).GetProperty(PropertyName).GetValue(ClassObject)
    End Function

    ''' <summary>Метод возвращает значимый тип по свойству, извлекая информацию из Nullable типа</summary>
    Friend Shared Function ValueTypeFrom(variable As Type) As Type
        Return If(IsNullableType(variable), Nullable.GetUnderlyingType(variable), variable)
    End Function

    ''' <summary>Метод проверяет, имеет ли свойство тип Nullable</summary>
    Friend Shared Function IsNullableType(variable As Type) As Boolean
        Return variable IsNot Nothing AndAlso variable.IsGenericType AndAlso variable.GetGenericTypeDefinition() = GetType(Nullable(Of ))
    End Function

End Class