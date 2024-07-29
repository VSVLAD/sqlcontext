Option Strict On

Imports System.Data.Common
Imports System.Data.SQLite
Imports Dapper
Imports VSProject.IteratorAsyncHelper
Imports VSProject.SQLContext

Public Class TestCanary

    Public Shared Sub Main()
        MainAsync().GetAwaiter().GetResult()
    End Sub

    Public Shared Async Function MainAsync() As Task
        Dim probeSum As Double = 0.0

        'For I = 1 To 20
        '    Dim sw As New Stopwatch()
        '    sw.Start()

        '    Using dbconnection As DbConnection = New SQLiteConnection("Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db")
        '        Await dbconnection.OpenAsync()

        '        Using dbcmd As DbCommand = dbconnection.CreateCommand()
        '            dbcmd.CommandText = "select id, topic_name from topic where forum_id = @fid"

        '            Dim par = dbcmd.CreateParameter()
        '            par.ParameterName = "@fid"
        '            par.DbType = DbType.Int64
        '            par.Value = 1

        '            dbcmd.Parameters.Add(par)

        '            Using dbreader = Await dbcmd.ExecuteReaderAsync()
        '                ' Массив с названиями столбцов
        '                Dim fieldNames() As String = {}
        '                Dim fieldCached As Boolean = False

        '                Dim fieldBound = dbreader.FieldCount - 1
        '                ReDim fieldNames(fieldBound)

        '                Dim iter = BuildIteratorAsync(
        '                    Async Function() As Task(Of Dictionary(Of String, Object))

        '                        Do While Await dbreader.ReadAsync()

        '                            ' Кешируем имена столбцов
        '                            If Not fieldCached Then
        '                                fieldCached = True

        '                                For K = 0 To fieldBound
        '                                    fieldNames(K) = dbreader.GetName(K)
        '                                    If fieldNames(K) Is Nothing Then Stop
        '                                Next
        '                            End If

        '                            Await Task.Delay(100)

        '                            ' Читаем значения в массив
        '                            Dim fieldValues(fieldBound) As Object
        '                            dbreader.GetValues(fieldValues)

        '                            ' Упаковываем массив значений в словарь
        '                            Dim retDict As New Dictionary(Of String, Object)
        '                            For K = 0 To fieldBound
        '                                If fieldValues(K).Equals(DBNull.Value) Then
        '                                    retDict.Add(fieldNames(K), Nothing)
        '                                Else
        '                                    retDict.Add(fieldNames(K), fieldValues(K))
        '                                End If
        '                            Next

        '                            Return retDict
        '                        Loop

        '                        Return Nothing
        '                    End Function)

        '                Await iter.ForEachAsync(
        '                    Sub(item)
        '                        Console.WriteLine(item("topic_name"))
        '                    End Sub)

        '            End Using
        '        End Using
        '    End Using

        '    sw.Stop()

        '    probeSum += sw.ElapsedMilliseconds
        '    Console.WriteLine($"probe #{I}, time {sw.ElapsedMilliseconds}")

        'Next

        'Console.WriteLine($"probe time avg {probeSum / 20}")
        'Console.WriteLine("SQLContext ManualWrite")
        'Console.ReadLine()
        'Return

        For I = 1 To 20
            Dim sw As New Stopwatch()

            Using conn As New VSProject.SQLContext.SQLContext(New SQLiteConnection("Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"))
                Dim resultList As New List(Of TopicInfo)

                sw.Start()

                ' Возвращает словарь
                resultList.Clear()
                For Each row In conn.SelectRows("select id, topic_name from topic where forum_id = @fid",
                                                       New Dictionary(Of String, Object) From {{"fid", 1}})
                    resultList.Add(New TopicInfo With {
                        .ID = CLng(row("id")),
                        .Name = CStr(row("topic_name"))
                    })
                Next

                ' Стандартный маппер
                resultList.Clear()
                For Each row In conn.SelectRows(Of TopicInfo)("select id, topic_name from topic where forum_id = @fid",
                                                       New Dictionary(Of String, Object) From {{"fid", 1}})
                    resultList.Add(row)
                Next

                ' С пользовательским маппером
                VSProject.SQLContext.SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                                               Return New TopicInfo With {
                                                                                    .ID = reader.GetInt64(0),
                                                                                    .Name = reader.GetString(1)
                                                                                }
                                                                           End Function)

                resultList.Clear()
                For Each row In conn.SelectRowsMapper(Of TopicInfo)("select id, topic_name from topic where forum_id = @fid",
                                                                    New Dictionary(Of String, Object) From {{"fid", 1}})
                    resultList.Add(row)
                Next

                ' Возвращает скомпилированный маппер из выражений
                resultList.Clear()
                For Each row In conn.SelectRowsFast(Of TopicInfo)("select id, topic_name, cast(case when random() > 0.5 then null else 1 end as integer) as r
                                                                   from topic where forum_id = @fid",
                                     New Dictionary(Of String, Object) From {{"fid", 1}})
                    resultList.Add(row)
                Next

                sw.Stop()

                probeSum += sw.ElapsedMilliseconds
                Console.WriteLine($"probe #{I}, time {sw.ElapsedMilliseconds} with {resultList.Count}")

            End Using
        Next

        Console.WriteLine($"probe time avg {probeSum / 20}")
        Console.WriteLine("SQLContext VS PROJECT")
        Console.ReadLine()
        Return

        For I = 1 To 20
            Dim sw As New Stopwatch()
            sw.Start()

            Using conn As New SQLiteConnection("Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db")
                Dim resultList As New List(Of TopicInfo)

                conn.Open()
                Dim context As New SqlContext.SqlContext(conn, "select id, topic_name from topic where forum_id = @fid")
                SqlContext.SqlContext.AddMapper(Function(reader)
                                                    Return New TopicInfo With {
                                                            .ID = CLng(reader("id")),
                                                            .Name = CStr(reader("topic_name"))
                                                        }
                                                End Function)

                For Each topic In context.Parameter("@fid", 1).Many(Of TopicInfo)

                    resultList.Add(New TopicInfo With {
                        .ID = topic.ID,
                        .Name = topic.Name
                    })

                    'Console.WriteLine($" topic_id = {topic.ID}, name = {topic.Name}")
                Next

                'Dim cmd = GetCommand(conn)
                'Dim reader = cmd.ExecuteReader

                'Do While reader.Read()
                '    'Dim values(reader.FieldCount - 1) As Object
                '    'reader.GetValues(values)

                '    Dim topic_id = reader.GetInt64(0)
                '    Dim topic_name = reader.GetString(1)

                '    resultList.Add(New TopicInfo With {
                '        .ID = topic_id,
                '        .Name = topic_name
                '    })
                'Loop

                sw.Stop()

                probeSum += sw.ElapsedMilliseconds
                Console.WriteLine($"probe #{I}, time {sw.ElapsedMilliseconds} with {resultList.Count}")

                'cmd.Dispose()
                'conn.Close()
            End Using
        Next

        Console.WriteLine($"probe time avg {probeSum / 20}")
        Console.WriteLine("DAPPER")

        probeSum = 0

        For I = 1 To 20
            Dim sw As New Stopwatch()

            Using conn As New SQLiteConnection("Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db")
                Dim resultList As New List(Of TopicInfo)

                sw.Start()


                For Each row In conn.Query(Of TopicInfo)("select id as ID, topic_name as Name from topic where forum_id = @fid", New With {.fid = 1})
                    resultList.Add(New TopicInfo With {
                        .ID = row.ID,
                        .Name = row.Name
                    })
                Next

                sw.Stop()

                probeSum += sw.ElapsedMilliseconds
                Console.WriteLine($"probe #{I}, time {sw.ElapsedMilliseconds} with {resultList.Count}")

            End Using
        Next

        Console.WriteLine($"probe time avg {probeSum / 20}")

        Console.WriteLine("Готово")
        Console.ReadKey()

    End Function

End Class


Public Class TopicInfo

    <Attributes.Column("id")>
    Public Property ID As Long

    <Attributes.Column("topic_name")>
    Public Property Name As String

    <Attributes.Column("r")>
    Public Property RandValue As Long?


End Class


Namespace SqlContext

    Public Class SqlContext
        Public Property Command As DbCommand
        Public Property CommandText As String
        Public Property Connection As DbConnection
        Public Property Transaction As DbTransaction
        Protected Property NeedClose As Boolean = False

        Public Sub New(ByVal conn As DbConnection, ByVal commandText As String)
            Connection = conn
            Command = conn.CreateCommand()
            Me.CommandText = commandText
            Command.CommandText = commandText
        End Sub

        Public Sub New(ByVal trans As DbTransaction, ByVal commandText As String)
            Me.New(trans.Connection, commandText)
            Transaction = trans
        End Sub

        Public Function Parameter(Of T)(ByVal name As String, ByVal value As T) As SqlContext
            Return Parameter(name, GetDbType(GetType(T)), value)
        End Function

        Public Function Parameter(ByVal name As String, ByVal dbType As DbType, ByVal value As Object) As SqlContext
            If Not name.StartsWith("@") Then name = "@" & name

            For Each param As DbParameter In Command.Parameters
                If param.ParameterName <> name Then Continue For
                param.DbType = dbType
                param.Value = value
                Return Me
            Next

            Dim dbParameter = Command.CreateParameter()
            Command.Parameters.Add(dbParameter)
            dbParameter.ParameterName = name
            dbParameter.DbType = dbType
            dbParameter.Value = value
            Return Me
        End Function

        Public Function Parameters(ParamArray params As Object()) As SqlContext
            Dim parameterNames = New List(Of String)()

            For i = 0 To CommandText.Length - 1
                If CommandText(i) <> "@"c Then Continue For
                Dim sb = New Text.StringBuilder()

                For j = i + 1 To CommandText.Length - 1
                    Dim c = CommandText(j)

                    If c = "_"c OrElse c = "-"c OrElse Char.IsLetterOrDigit(c) Then
                        sb.Append(c)
                        i += 1
                        Continue For
                    End If

                    Exit For
                Next

                Dim name = sb.ToString()

                If Not parameterNames.Contains(name) Then
                    parameterNames.Add(name)
                End If
            Next

            For Each parameter As DbParameter In Command.Parameters
                Dim name = parameter.ParameterName.TrimStart("@"c)
                parameterNames.Remove(name)
            Next

            Dim count = Math.Min(params.Length, parameterNames.Count)

            For i = 0 To count - 1
                Dim name = parameterNames(i)
                If Not name.StartsWith("@") Then name = "@" & name
                Dim value = Parameters(i)
                Dim dbParameter = Command.CreateParameter()
                Command.Parameters.Add(dbParameter)
                dbParameter.ParameterName = name
                dbParameter.DbType = GetDbType(If(value Is Nothing, GetType(String), value.GetType()))
                dbParameter.Value = value
            Next

            Return Me
        End Function

        Public Sub NonQuery()
            Try
                TryOpen()
                Command.ExecuteNonQuery()
            Finally
                TryClose()
            End Try
        End Sub

        Public Iterator Function Many(Of T)(selector As Func(Of DbDataReader, T)) As IEnumerable(Of T)
            Try
                TryOpen()
                Dim ls = New List(Of T)()
                If selector IsNot Nothing Then
                    Dim reader = Command.ExecuteReader()

                    Do While reader.Read()
                        Dim item = selector(reader)
                        If item IsNot Nothing Then
                            Yield item
                        End If
                    Loop
                End If

            Finally
                TryClose()
            End Try
        End Function

        Public Iterator Function Many(Of T)(Optional callback As Action(Of T) = Nothing) As IEnumerable(Of T)
            Dim selector = GetMapper(Of T)()

            For Each row In Many(selector)
                If callback IsNot Nothing Then callback(row)
                Yield row
            Next

        End Function

        Public Function [Single](Of T)(selector As Func(Of DbDataReader, T)) As T
            Try
                TryOpen()
                If selector Is Nothing Then Return Nothing
                Dim reader = Command.ExecuteReader()

                If reader.Read() Then
                    Dim ret = selector(reader)
                    Return ret
                End If

                Return Nothing
            Finally
                TryClose()
            End Try
        End Function

        Public Function [Single](Of T)(Optional callback As Action(Of T) = Nothing) As T
            Dim selector = GetMapper(Of T)()
            Dim ret = [Single](selector)
            callback?.Invoke(ret)
            Return ret
        End Function

        Public Function SingleValue(Of T)(Optional col As String = Nothing) As T
            Try
                TryOpen()
                Dim reader = Command.ExecuteReader()

                If reader.Read() Then

                    If String.IsNullOrEmpty(col) Then
                        Dim ret = reader(0)
                        Return CType(ret, T)
                    Else
                        Dim ret = reader(col)
                        Return CType(ret, T)
                    End If
                End If

                Return Nothing
            Finally
                TryClose()
            End Try
        End Function

        Protected Sub TryOpen()
            If Connection.State = ConnectionState.Closed Then
                Connection.Open()
                NeedClose = True
            End If
        End Sub

        Protected Sub TryClose()
            If NeedClose Then
                Connection.Close()
                NeedClose = False
            End If
        End Sub

        Private Function GetDbType(type As Type) As DbType
            If typeMap Is Nothing Then
                typeMap = New Dictionary(Of Type, DbType)()
                typeMap(GetType(String)) = DbType.String
                typeMap(GetType(Byte)) = DbType.Byte
                typeMap(GetType(Short)) = DbType.Int16
                typeMap(GetType(Integer)) = DbType.Int32
                typeMap(GetType(Long)) = DbType.Int64
                typeMap(GetType(Boolean)) = DbType.Boolean
                typeMap(GetType(Date)) = DbType.DateTime2
                typeMap(GetType(DateTimeOffset)) = DbType.DateTimeOffset
                typeMap(GetType(Decimal)) = DbType.Decimal
                typeMap(GetType(Double)) = DbType.Double
                typeMap(GetType(Single)) = DbType.Single
                typeMap(GetType(Decimal)) = DbType.Decimal
                typeMap(GetType(TimeSpan)) = DbType.Time
            End If

            Return typeMap(type)
        End Function

        Private Shared typeMap As Dictionary(Of Type, DbType)
        Private Shared ReadOnly mappers As New Dictionary(Of Type, [Delegate])

        Public Shared Sub AddMapper(Of T)(Mapper As Func(Of IDataReader, T))
            Dim type = GetType(T)

            If mappers.ContainsKey(type) Then
                mappers(type) = Mapper
            Else
                mappers.Add(type, Mapper)
            End If
        End Sub

        Public Shared Function Map(Of T)(Reader As IDataReader) As T
            Dim mapper = GetMapper(Of T)()
            If mapper Is Nothing Then Return Nothing
            Return mapper(Reader)
        End Function

        Public Shared Function GetMapper(Of T)() As Func(Of IDataReader, T)
            Dim type = GetType(T)
            If Not mappers.ContainsKey(type) Then Return Nothing
            Return TryCast(mappers(type), Func(Of IDataReader, T))
        End Function

    End Class


End Namespace
