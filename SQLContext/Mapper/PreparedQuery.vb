Imports System.Text
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports VSProject.MicroORM.Exceptions

''' <summary>Класс для параметризации запросов SQL</summary>
Public Class PreparedQuery

    Private reParams As New Regex("((?<![\:|@])[\:|@]\w{1,})", RegexOptions.Compiled Or RegexOptions.IgnoreCase Or RegexOptions.Multiline)
    Private queries As New Concurrent.ConcurrentDictionary(Of String, PreparedCommand)

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
    Public Sub ClearCache()

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

    End Sub

    ''' <summary>Метод для параметризованных запросов выполняет их кеширование, готовит параметры и применяет значения</summary>
    ''' <param name="SqlText">Текст обычного запроса или с параметрами</param>
    ''' <param name="Connection">Объект соединения</param>
    ''' <param name="Values">Значения параметров в виде ParamArray массива</param>
    ''' <returns>Ссылка на команду IDbCommand</returns>
    Public Function Prepare(SqlText As String, Connection As IDbConnection, ParamArray Values() As Object) As IDbCommand

        ' Если нет параметров, отдаём простую команду
        If Values.Length = 0 Then

            Dim retCmd = Connection.CreateCommand()
            retCmd.CommandType = CommandType.Text
            retCmd.CommandText = SqlText
            Return retCmd

        Else

            ' Ищем параметры в кеше или создаём новый
            Dim cachedPrepared As PreparedCommand
            Dim queryHash As String = MD5Hash(String.Concat(Environment.CurrentManagedThreadId, "=", SqlText))

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

                    ' Определяем тип параметра по суффиксу
                    Select Case param.ParameterName.Substring(param.ParameterName.Length - 1, 1).ToUpperInvariant()
                        Case "I"
                            param.DbType = DbType.Int64
                        Case "S"
                            param.DbType = DbType.String
                        Case "D"
                            param.DbType = DbType.DateTime
                        Case "R"
                            param.DbType = DbType.Double
                        Case Else
                            Throw New SQLContextException(Resources.ExceptionMessages.PARAMETERIZED_QUERY_MUST_USE_SUFFIX)
                    End Select

                    cachedPrepared.ItemCommand.Parameters.Add(param)
                    cachedPrepared.ParameterCount += 1
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
                Do While Not queries.TryAdd(queryHash, cachedPrepared)
                Loop
            End If

            ' Применяем значения параметров к команде
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
    Private Shared Function MD5Hash(Value As String) As String
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
