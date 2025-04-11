Imports VSProject.SQLContext
Imports System.Data.SQLite
Imports NUnit.Framework
Imports NUnit.Framework.Legacy
Imports System.Data

Namespace NUnitAutoTest

    <TestFixture>
    Public Class ForumCaseConverterTest

        Public Shared Function InitConnection() As IDbConnection
            Dim connection = SQLiteFactory.Instance.CreateConnection()
            connection.ConnectionString = "Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"
            Return connection
        End Function

        ' #################               Маппер на словарь 

        <Test>
        Public Sub SelectManyTopicsGetDateSQLiteWithConverterMapperInternal()
            Try

                SQLContext.UserConverters.RegisterConverter("sqlite_text_date",
                                                            Function(columnValue)
                                                                If columnValue IsNot Nothing Then
                                                                    Return Date.ParseExact(columnValue, "yyyy-MM-dd HH:mm:ss.FFF", Globalization.CultureInfo.CurrentCulture)
                                                                Else
                                                                    Return Nothing
                                                                End If
                                                            End Function)

                Using context As New SQLContext(InitConnection())
                    Dim rowCount As Integer = 0

                    For Each row In context.SelectRows(Of ForumTopic)(
                             " select t.id, t.topic_name,
                                      t.forum_id, f.forum_name,
                                      (select min(date_created) from message where topic_id = t.id) as topic_created,
                                      (select count(id) from message where topic_id = t.id)         as message_count,
                                      (select max(date_created) from message where topic_id = t.id) as message_last_date
                                 from topic t
                           inner join forum f on f.id = t.forum_id
                                where t.forum_id = @ForumId
                             order by message_last_date desc
                                limit @Limit
                               offset @Offset ", New With {.ForumId = 1, .Limit = 100, .Offset = 1000})

                        ' Дата старше текущего дня
                        ClassicAssert.Greater(Date.Now, row.MessageLastDate)

                        ' Считаем количество топиков
                        rowCount += 1
                    Next

                    ClassicAssert.AreEqual(100, rowCount)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectManyTopicsGetDateSQLiteWithConverterMapperFast()
            ' TODO: Пока не реализовано. Нужно учесть в Expression Tree генерацию вызова конвертера, если найден будет атрибут
            ClassicAssert.Zero(0)

            'Try

            '    SQLContext.UserConverters.RegisterConverter("sqlite_text_date",
            '                                                Function(columnValue)
            '                                                    If columnValue IsNot Nothing Then
            '                                                        Return Date.ParseExact(columnValue, "yyyy-MM-dd HH:mm:ss.FFF", Globalization.CultureInfo.CurrentCulture)
            '                                                    Else
            '                                                        Return Nothing
            '                                                    End If
            '                                                End Function)

            '    Using context As New SQLContext(InitConnection())
            '        Dim rowCount As Integer = 0

            '        For Each row In context.SelectRowsFast(Of ForumTopic)(
            '                 " select t.id, t.topic_name,
            '                          t.forum_id, f.forum_name,
            '                          (select min(date_created) from message where topic_id = t.id) as topic_created,
            '                          (select count(id) from message where topic_id = t.id)         as message_count,
            '                          (select max(date_created) from message where topic_id = t.id) as message_last_date
            '                     from topic t
            '               inner join forum f on f.id = t.forum_id
            '                    where t.forum_id = @ForumId
            '                 order by message_last_date desc
            '                    limit @Limit
            '                   offset @Offset ", New With {.ForumId = 1, .Limit = 100, .Offset = 1000})

            '            ' Дата старше текущего дня
            '            ClassicAssert.Greater(Date.Now, row.MessageLastDate)

            '            ' Считаем количество топиков
            '            rowCount += 1
            '        Next

            '        ClassicAssert.AreEqual(100, rowCount)
            '    End Using

            'Catch ex As Exception
            '    Assert.Fail(ex.Message)

            'End Try
        End Sub


        <Test>
        Public Sub SelectManyTopicsGetDateAsDateSQLiteMapperUser()
            Try
                Using context As New SQLContext(InitConnection())

                    Dim rowCount As Integer = 0
                    For Each row In context.SelectRowsMapper(
                             " select t.id, t.topic_name,
                                      t.forum_id, f.forum_name,
                                      (select min(date_created) from message where topic_id = t.id) as topic_created,
                                      (select count(id) from message where topic_id = t.id)         as message_count,
                                      (select max(date_created) from message where topic_id = t.id) as message_last_date
                                 from topic t
                           inner join forum f on f.id = t.forum_id
                                where t.forum_id = @ForumId
                             order by message_last_date desc
                                limit @Limit
                               offset @Offset ", New With {.ForumId = 1, .Limit = 100, .Offset = 1000},
                                                 Function(reader)
                                                     Return New ForumTopic With {
                                                        .ForumId = reader("forum_id"),
                                                        .TopicId = reader("id"),
                                                        .TopicName = reader("topic_name"),
                                                        .MessageLastDate = reader.GetDateTime(6)
                                                     }
                                                 End Function)

                        ' Дата старше текущего дня
                        ClassicAssert.Greater(Date.Now, row.MessageLastDate)

                        ' Считаем количество топиков
                        rowCount += 1
                    Next

                    ClassicAssert.AreEqual(100, rowCount)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace
