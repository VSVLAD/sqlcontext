Option Strict On

Imports System.Data.Common
Imports System.Data.SQLite
Imports Dapper
Imports VSProject.SQLContext

Public Class TestCanary

    Public Shared Sub Main()
        MainAsync().GetAwaiter().GetResult()
    End Sub

    Public Shared Async Function MainAsync() As Task
        Dim probeSum As Double = 0.0


        Dim anon = New With {.Prop1 = 1, .Prop2 = 2}
        Dim boxed As Object = anon

        Dim res = SQLContextParameters.ToDictionary(boxed)

        For I = 1 To 20
            Dim sw As New Stopwatch()
            sw.Start()

            Dim retList As New List(Of TopicInfo)

            Using dbconnection = New SQLContext(New SQLiteConnection("Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"))

                For Each row In dbconnection.SelectRowsFast(Of TopicInfo)("select id, topic_name from topic where forum_id = @fid", New With {.fid = 1})
                    retList.Add(row)
                Next

            End Using

            sw.Stop()

            probeSum += sw.ElapsedMilliseconds
            Console.WriteLine($"probe #{I}, time {sw.ElapsedMilliseconds}, count(*) = {retList.Count}")
        Next

        Console.WriteLine($"probe time avg {probeSum / 20}")
        Console.WriteLine("SQLContext FAST")
        Console.ReadLine()
        Return


        'For I = 1 To 20
        '    Dim sw As New Stopwatch()
        '    sw.Start()

        '    Dim retList As New List(Of TopicInfo)

        '    Using dbconnection As DbConnection = New SQLiteConnection("Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db")
        '        dbconnection.Open()

        '        Using dbcmd As DbCommand = dbconnection.CreateCommand()
        '            dbcmd.CommandText = "select id, topic_name from topic where forum_id = @fid"

        '            Dim par = dbcmd.CreateParameter()
        '            par.ParameterName = "@fid"
        '            par.DbType = DbType.Int64
        '            par.Value = 1

        '            dbcmd.Parameters.Add(par)

        '            Using dbreader = dbcmd.ExecuteReader()
        '                Do While dbreader.Read()

        '                    retList.Add(New TopicInfo() With {
        '                            .ID = CLng(dbreader("id")),
        '                            .Name = dbreader.GetString(1)
        '                        })

        '                Loop

        '            End Using
        '        End Using
        '    End Using

        '    sw.Stop()

        '    probeSum += sw.ElapsedMilliseconds
        '    Console.WriteLine($"probe #{I}, time {sw.ElapsedMilliseconds}, count(*) = {retList.Count}")
        'Next

        'Console.WriteLine($"probe time avg {probeSum / 20}")
        'Console.WriteLine("Manual write")
        'Console.ReadLine()
        'Return


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

        'sw.Stop()

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

