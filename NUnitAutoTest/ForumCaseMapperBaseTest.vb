Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Exceptions
Imports VSProject.SQLContext.Extensions
Imports VSProject.SQLContext
Imports System.Data.SQLite
Imports NUnit.Framework
Imports NUnit.Framework.Legacy

Namespace NUnitAutoTest

    <TestFixture>
    Public Class ForumCaseMapperBaseTest

        Private Shared ConnectionString As String = "Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"

        ' #################               Маппер на словарь 

        <Test>
        Public Sub SelectOneTopicMapperDictionaryNoParams()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))

                    ' Анонимный тип не может быть проброшен в Generic. Поэтому перегрузки (Of T, TAnonymousObject) не реализовано
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = @ID", ContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperInternalIntParamDict()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                SQLContext.UserMappers.Register(Function(reader)
                                                    Return New Topic With {
                                                            .TopicName = reader("topic_name"),
                                                            .Id = reader("id"),
                                                            .ForumId = reader("forum_id"),
                                                            .UserId = reader("user_id")
                                                        }
                                                End Function)

                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = 27").FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

                SQLContext.UserMappers.Unregister(Of Topic)()

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperUserIntParamAnon()
            Try
                SQLContext.UserMappers.Register(Function(reader)
                                                    Return New Topic With {
                                                            .TopicName = reader("topic_name"),
                                                            .Id = reader("id"),
                                                            .ForumId = reader("forum_id"),
                                                            .UserId = reader("user_id")
                                                        }
                                                End Function)

                Using context As New SQLContext(New SQLiteConnection(ConnectionString))

                    ' Анонимный тип не может быть проброшен в Generic. Поэтому перегрузки (Of T, TAnonymousObject) не реализовано
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = @ID", ContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

                SQLContext.UserMappers.Unregister(Of Topic)()

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperUserIntParamDict()
            Try
                SQLContext.UserMappers.Register(Function(reader)
                                                    Return New Topic With {
                                                            .TopicName = reader("topic_name"),
                                                            .Id = reader("id"),
                                                            .ForumId = reader("forum_id"),
                                                            .UserId = reader("user_id")
                                                        }
                                                End Function)

                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim params As New Dictionary(Of String, Object) From {{"ID", "27"}}
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = @ID", params).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

                SQLContext.UserMappers.Unregister(Of Topic)()

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        ' #################               Маппер основанный на Expression Tree

        <Test>
        Public Sub SelectOneTopicMapperFastNoParams()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))

                    ' Анонимный тип не может быть проброшен в Generic. Поэтому перегрузки (Of T, TAnonymousObject) не реализовано
                    Dim row = context.SelectRowsFast(Of Topic)("select * from topic where id = @ID", ContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
                    ClassicAssert.AreEqual("ACCESS2000", row.TopicName)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectOneTopicMapperFastIntParamDict()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
