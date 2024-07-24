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
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = 27").FirstOrDefault()
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
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = @ID", ContextParameters.FromObject(New With {.ID = 27})).FirstOrDefault()
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
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = @ID", params).FirstOrDefault()
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




'    <TestFixture>
'    Public Class AutoTestSelect

'        Public Shared ConnectionString As String = "Data Source=C:\test.db"

'        <Test, Order(1)>
'        Public Sub InitCreateTable()
'            Try
'                IO.File.Delete("C:\test.db")

'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    context.ExecNonQuery("create table address(
'                                id integer primary key autoincrement,
'                                city text not null,
'                                street text not null,
'                                home integer null,
'                                square float not null,
'                                commercial boolean not null,
'                                dateOne text null,
'                                dateTwo integer null,
'                                dateThree real null
'                            )")

'                    context.ExecNonQuery("insert into address(id, city, street, home, square, commercial)
'                                                values(1, 'Краснодар', 'Тургенева', 118, 31.2, 0)")

'                    context.ExecNonQuery("insert into address(id, city, street, home, square, commercial)
'                                                values(2, 'Краснодар', 'Заводская', null, 150, 1)")
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'        <Test, Order(2)>
'        Public Sub SelectRowsCountAsClassTest()
'            Try
'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    Dim row = context.SelectRows(Of Models)("select * from address where id = 1").FirstOrDefault()
'                    Assert.AreEqual("Краснодар", row.City)
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'        <Test, Order(2)>
'        Public Sub SelectRowsCountAsDictionaryTest()
'            Try
'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    Dim row = context.SelectRows("select * from address where id = 1").FirstOrDefault()
'                    Assert.AreEqual("Краснодар", row("city"))
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'        <Test, Order(2)>
'        Public Sub SelectRowsCountAsDynamicTest()
'            Try
'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    Dim row = context.SelectRowsDynamic("select * from address where id = 1").FirstOrDefault()
'                    Assert.AreEqual("Краснодар", row.City)
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'        <Test, Order(2)>
'        Public Sub SelectRowsIntegerNullTest()
'            Try
'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    Dim addr = context.SelectRows(Of Models)("select home from address where street = 'Заводская'").FirstOrDefault()
'                    Assert.IsNull(addr.Home)
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'        <Test, Order(2)>
'        Public Sub SelectRowsBooleanTrueTest()
'            Try
'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    Dim addr = context.SelectRows(Of Models)("select * from address where street = 'Заводская'").FirstOrDefault()
'                    Assert.IsTrue(addr.IsCommercial)
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'        <Test, Order(2)>
'        Public Sub SelectRowsBooleanFalseTest()
'            Try
'                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
'                    Dim addr = context.SelectRows(Of Models)("select * from address where street = 'Тургенева'").FirstOrDefault()
'                    Assert.IsFalse(addr.IsCommercial)
'                End Using

'            Catch ex As Exception
'                Assert.Fail(ex.Message)

'            End Try
'        End Sub

'    End Class

'End Namespace