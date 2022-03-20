Imports System.Text
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports VSProject.SQLContext.Exceptions

''' <summary>Класс для параметризации запросов SQL</summary>
Public Class PreparedQuery

    Private Shared queries As New Concurrent.ConcurrentDictionary(Of String, PreparedCommand)
    Private Shared reParams As New Regex("((?<![\:|@])[\:|@]\w+_[SsDdIiRr]\b)", RegexOptions.Compiled Or RegexOptions.IgnoreCase Or RegexOptions.Multiline)

    Private Shared lockerClear As New Object

    ''' <summary>Класс для подготовленной команды и списка параметров</summary>
    Private Class PreparedCommand

        ' Текст запроса
        Public Text As String

        ' Хеш запроса
        Public Hash As String

        ' Количество задействованных параметров
        Public ParameterCount As Integer

        ' Ссылка на команду
        Public ItemCommand As IDbCommand

        ' Ссылки на параметры для оптимизации доступа
        Public ItemParam1 As IDbDataParameter
        Public ItemParam2 As IDbDataParameter
        Public ItemParam3 As IDbDataParameter
        Public ItemParam4 As IDbDataParameter
    End Class

    ''' <summary>Очистить кеш запросов</summary>
    Public Shared Sub ClearCache()
        If queries.Count > 0 Then

            SyncLock lockerClear
                If queries.Count > 0 Then

                    ' Вызвать финализацию
                    For Each xCmd In queries.Values()
                        xCmd.ItemCommand.Parameters.Clear()
                        xCmd.ItemCommand.Dispose()

                        xCmd.ItemParam1 = Nothing
                        xCmd.ItemParam2 = Nothing
                        xCmd.ItemParam3 = Nothing
                        xCmd.ItemParam4 = Nothing
                    Next

                    ' Очищаем ссылки
                    queries.Clear()

                End If
            End SyncLock

        End If
    End Sub


    ''' <summary>Метод для параметризованных запросов выполняет их кеширование, готовит параметры и применяет значения</summary>
    ''' <param name="SqlText">Текст обычного запроса или с параметрами</param>
    ''' <param name="Connection">Объект соединения</param>
    ''' <param name="Params">Значения параметров в виде словаря, где ключ - имя параметра с суффиксом</param>
    ''' <returns>Ссылка на команду IDbCommand</returns>
    Public Shared Function Prepare(SqlText As String, Connection As IDbConnection, Params As IDictionary(Of String, Object)) As IDbCommand

        ' Если нет параметров, отдаём простую команду
        If Params Is Nothing OrElse (Params IsNot Nothing AndAlso Params.Count = 0) Then

            Dim retCmd = Connection.CreateCommand()
            retCmd.CommandType = CommandType.Text
            retCmd.CommandText = SqlText
            Return retCmd

        Else

            ' Ищем параметры в кеше или создаём новый
            Dim cachedPrepared As PreparedCommand
            Dim queryHash As String = MD5Hash(String.Concat(Environment.CurrentManagedThreadId, "-D-", SqlText))

            ' Если есть в кеше, то актуализируем коннекшн и отдаём
            If queries.ContainsKey(queryHash) Then
                cachedPrepared = queries(queryHash)
                cachedPrepared.ItemCommand.Connection = Connection

            Else
                ' Создаём подготовленную команду
                cachedPrepared = New PreparedCommand()
                cachedPrepared.Text = SqlText
                cachedPrepared.Hash = queryHash

                cachedPrepared.ItemCommand = Connection.CreateCommand()
                cachedPrepared.ItemCommand.CommandType = CommandType.Text
                cachedPrepared.ItemCommand.CommandText = SqlText

                ' Ищем параметры
                Dim listParamMatches = reParams.Matches(SqlText).Cast(Of Match).Select(Function(m) m.Value).ToList()

                ' Создаём параметры
                For Each k In Params
                    Dim param As IDbDataParameter = cachedPrepared.ItemCommand.CreateParameter()
                    param.ParameterName = k.Key

                    ' Проверим что в SQL запросе присутствует параметр переданный в словаре
                    If Not listParamMatches.Exists(Function(paramName) paramName = param.ParameterName) Then
                        Throw New SQLContextException(String.Format(Resources.ExceptionMessages.PREPARED_QUERY_DICT_PARAM_IS_MISSING_IN_QUERY, param.ParameterName))
                    End If

                    ' Если ещё не заведён в словарь параметр с таким именем
                    If Not cachedPrepared.ItemCommand.Parameters.Contains(param.ParameterName) Then

                        ' Определяем тип параметра по суффиксу
                        Select Case param.ParameterName.Substring(param.ParameterName.Length - 2, 2).ToLowerInvariant()
                            Case "_i"
                                param.DbType = DbType.Int64
                            Case "_s"
                                param.DbType = DbType.String
                            Case "_d"
                                param.DbType = DbType.DateTime
                            Case "_r"
                                param.DbType = DbType.Double
                        End Select

                        cachedPrepared.ItemCommand.Parameters.Add(param)
                        cachedPrepared.ParameterCount += 1
                    End If
                Next

                ' Добавляем параметр в конкурентную коллекцию (кешируем)
                queries.TryAdd(queryHash, cachedPrepared)

            End If

            ' Проверяем, что внешний код не уничтожил команду
            If cachedPrepared.ItemCommand Is Nothing Then
                Throw New SQLContextException(Resources.ExceptionMessages.PREPARED_QUERY_CACHED_COMMAND_IS_NULL)
            End If

            ' Применяем значения параметров к команде
            For Each k In Params
                CType(cachedPrepared.ItemCommand.Parameters.Item(k.Key), IDbDataParameter).Value = k.Value
            Next

            Return cachedPrepared.ItemCommand
        End If
    End Function

    ''' <summary>Метод для параметризованных запросов выполняет их кеширование, готовит параметры и применяет значения</summary>
    ''' <param name="SqlText">Текст обычного запроса или с параметрами</param>
    ''' <param name="Connection">Объект соединения</param>
    ''' <param name="Values">Значения параметров в виде ParamArray массива</param>
    ''' <returns>Ссылка на команду IDbCommand</returns>
    Public Shared Function Prepare(SqlText As String, Connection As IDbConnection, ParamArray Values() As Object) As IDbCommand

        ' Если нет параметров, отдаём простую команду
        If Values.Length = 0 Then

            Dim retCmd = Connection.CreateCommand()
            retCmd.CommandType = CommandType.Text
            retCmd.CommandText = SqlText
            Return retCmd

        Else

            ' Ищем параметры в кеше или создаём новый
            Dim cachedPrepared As PreparedCommand
            Dim queryHash As String = MD5Hash(String.Concat(Environment.CurrentManagedThreadId, "-V-", SqlText))

            ' Если есть в кеше, то актуализируем коннекшн и отдаём
            If queries.ContainsKey(queryHash) Then
                cachedPrepared = queries(queryHash)
                cachedPrepared.ItemCommand.Connection = Connection

            Else
                ' Создаём подготовленную команду
                cachedPrepared = New PreparedCommand()
                cachedPrepared.Text = SqlText
                cachedPrepared.Hash = queryHash

                cachedPrepared.ItemCommand = Connection.CreateCommand()
                cachedPrepared.ItemCommand.CommandType = CommandType.Text
                cachedPrepared.ItemCommand.CommandText = SqlText

                ' Создаём параметры
                For Each m As Match In reParams.Matches(SqlText)
                    Dim param As IDbDataParameter = cachedPrepared.ItemCommand.CreateParameter()
                    param.ParameterName = m.Groups(0).Value

                    ' Если ещё не заведён в словарь параметр с таким именем
                    If Not cachedPrepared.ItemCommand.Parameters.Contains(param.ParameterName) Then

                        ' Определяем тип параметра по суффиксу
                        Select Case param.ParameterName.Substring(param.ParameterName.Length - 2, 2).ToLowerInvariant()
                            Case "_i"
                                param.DbType = DbType.Int64
                            Case "_s"
                                param.DbType = DbType.String
                            Case "_d"
                                param.DbType = DbType.DateTime
                            Case "_r"
                                param.DbType = DbType.Double
                        End Select

                        cachedPrepared.ItemCommand.Parameters.Add(param)
                        cachedPrepared.ParameterCount += 1
                    End If
                Next

                ' Для быстрого доступа вытаскиваем первые 4 параметра
                Select Case cachedPrepared.ParameterCount
                    Case 0
                        Throw New SQLContextException(Resources.ExceptionMessages.PARAMETERS_NAMES_MUST_USE_IF_PASSED_VALUES)
                    Case 1
                        cachedPrepared.ItemParam1 = CType(cachedPrepared.ItemCommand.Parameters.Item(0), IDbDataParameter)
                    Case 2
                        cachedPrepared.ItemParam1 = CType(cachedPrepared.ItemCommand.Parameters.Item(0), IDbDataParameter)
                        cachedPrepared.ItemParam2 = CType(cachedPrepared.ItemCommand.Parameters.Item(1), IDbDataParameter)
                    Case 3
                        cachedPrepared.ItemParam1 = CType(cachedPrepared.ItemCommand.Parameters.Item(0), IDbDataParameter)
                        cachedPrepared.ItemParam2 = CType(cachedPrepared.ItemCommand.Parameters.Item(1), IDbDataParameter)
                        cachedPrepared.ItemParam3 = CType(cachedPrepared.ItemCommand.Parameters.Item(2), IDbDataParameter)
                    Case 4
                        cachedPrepared.ItemParam1 = CType(cachedPrepared.ItemCommand.Parameters.Item(0), IDbDataParameter)
                        cachedPrepared.ItemParam2 = CType(cachedPrepared.ItemCommand.Parameters.Item(1), IDbDataParameter)
                        cachedPrepared.ItemParam3 = CType(cachedPrepared.ItemCommand.Parameters.Item(2), IDbDataParameter)
                        cachedPrepared.ItemParam4 = CType(cachedPrepared.ItemCommand.Parameters.Item(3), IDbDataParameter)
                End Select

                ' Добавляем параметр в конкурентную коллекцию (кешируем)
                queries.TryAdd(queryHash, cachedPrepared)

            End If

            ' Проверяем, что внешний код не уничтожил команду
            If cachedPrepared.ItemCommand Is Nothing Then
                Throw New SQLContextException(Resources.ExceptionMessages.PREPARED_QUERY_CACHED_COMMAND_IS_NULL)
            End If


            Select Case cachedPrepared.ParameterCount
                Case 1
                    cachedPrepared.ItemParam1.Value = Values(0)

                Case 2
                    cachedPrepared.ItemParam1.Value = Values(0)
                    cachedPrepared.ItemParam2.Value = Values(1)

                Case 3
                    cachedPrepared.ItemParam1.Value = Values(0)
                    cachedPrepared.ItemParam2.Value = Values(1)
                    cachedPrepared.ItemParam3.Value = Values(2)

                Case 4
                    cachedPrepared.ItemParam1.Value = Values(0)
                    cachedPrepared.ItemParam2.Value = Values(1)
                    cachedPrepared.ItemParam3.Value = Values(2)
                    cachedPrepared.ItemParam4.Value = Values(3)

                Case Else

                    For ind = 0 To cachedPrepared.ParameterCount - 1
                        CType(cachedPrepared.ItemCommand.Parameters(ind), IDbDataParameter).Value = Values(ind)
                    Next

            End Select

            Return cachedPrepared.ItemCommand
        End If
    End Function


    ''' <summary>Метод вычисляет хеш по алгоритму MD5</summary>
    ''' <param name="Value">Строковые данные для рассчета хеша</param>
    ''' <returns>Строка в кодировке Base64</returns>
    Friend Shared Function MD5Hash(Value As String) As String
        Using md As MD5 = New MD5CryptoServiceProvider()
            Dim digest() As Byte = md.ComputeHash(Encoding.UTF8.GetBytes(Value))
            Dim sb = New StringBuilder()

            For I = 0 To digest.Length - 1
                sb.Append(digest(I).ToString("x2"))
            Next

            Return sb.ToString
        End Using
    End Function

End Class
