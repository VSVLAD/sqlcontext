Imports VSProject.SQLContext
Imports System.Data.SQLite
Imports NUnit.Framework
Imports NUnit.Framework.Legacy
Imports System.Data

Namespace NUnitAutoTest

    <TestFixture>
    Public Class ForumCaseMapperBaseTest

        Public Function InitConnection() As IDbConnection
            Dim connection = SQLiteFactory.Instance.CreateConnection()
            connection.ConnectionString = "Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"
            Return connection
        End Function

        ' #################               Маппер на словарь 

        <Test>
        Public Sub SelectOneTopicMapperDictionaryNoParams()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows("select * from topic where id = 27").FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row("topic_name"))
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperDictionaryIntParamAnon()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows("select * from topic where id = @ID", New With {.ID = 27}).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row("topic_name"))
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperDictionaryIntParamDict()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim params As New Dictionary(Of String, Object) From {{"ID", "27"}}
                    Dim row = context.SelectRows("select * from topic where id = @ID", params).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row("topic_name"))
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        ' #################               Маппер стандарт 

        <Test>
        Public Sub SelectOneTopicMapperInternalNoParams()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = 27").FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperInternalIntParamAnon()
            Try
                Using context As New SQLContext(InitConnection())

                    ' Анонимный тип не может быть проброшен в Generic. Поэтому перегрузки (Of T, TAnonymousObject) не реализовано
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = @ID", SQLContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperInternalIntParamDict()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim params As New Dictionary(Of String, Object) From {{"ID", "27"}}
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = @ID", params).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        ' #################               Маппер пользовательский 

        <Test>
        Public Sub SelectOneTopicMapperUserNoParams()
            Try
                SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                          Return New Topic With {
.TopicName = reader("topic_name"),
.Id = reader("id"),
.ForumId = reader("forum_id"),
.UserId = reader("user_id")
}
                                                      End Function)

                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = 27").FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

                SQLContext.UserMappers.UnregisterMapper(Of Topic)()

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperUserIntParamAnon()
            Try
                SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                          Return New Topic With {
                                                            .TopicName = reader("topic_name"),
                                                            .Id = reader("id"),
                                                            .ForumId = reader("forum_id"),
.UserId = reader("user_id")
}
                                                      End Function)
                Using context As New SQLContext(InitConnection())

                    ' Анонимный тип не может быть проброшен в Generic. Поэтому перегрузки (Of T, TAnonymousObject) не реализовано
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = @ID", SQLContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

                SQLContext.UserMappers.UnregisterMapper(Of Topic)()

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperUserIntParamDict()
            Try
                SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                          Return New Topic With {
                                                            .TopicName = reader("topic_name"),
                                                            .Id = reader("id"),
                                                            .ForumId = reader("forum_id"),
                                                            .UserId = reader("user_id")
                                                        }
                                                      End Function)

                Using context As New SQLContext(InitConnection())
                    Dim params As New Dictionary(Of String, Object) From {{"ID", "27"}}
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = @ID", params).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using
                SQLContext.UserMappers.UnregisterMapper(Of Topic)()
            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        ' #################               Маппер основанный на Expression Tree

        <Test>
        Public Sub SelectOneTopicMapperFastNoParams()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of Topic)("select * from topic where id = 27").FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperFastIntParamAnon()
            Try
                Using context As New SQLContext(InitConnection())

                    ' Анонимный тип не может быть проброшен в Generic. Поэтому перегрузки (Of T, TAnonymousObject) не реализовано
                    Dim row = context.SelectRowsFast(Of Topic)("select * from topic where id = @ID", SQLContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperFastIntParamDict()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim params As New Dictionary(Of String, Object) From {{"ID", "27"}}
                    Dim row = context.SelectRowsFast(Of Topic)("select * from topic where id = @ID", params).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        ' #################               Маппер основанный на Dynamic

        <Test>
        Public Sub SelectOneTopicMapperDynamicNoParams()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsDynamic("select * from topic where id = 27").FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.topic_name)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperDynamicIntParamAnon()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsDynamic("select * from topic where id = @ID", New With {.ID = 27}).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.topic_name)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperDynamicIntParamDict()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim params As New Dictionary(Of String, Object) From {{"ID", "27"}}
                    Dim row = context.SelectRowsDynamic("select * from topic where id = @ID", params).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.topic_name)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace
