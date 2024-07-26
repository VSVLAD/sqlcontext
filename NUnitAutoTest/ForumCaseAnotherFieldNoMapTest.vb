Imports VSProject.SQLContext
Imports System.Data.SQLite
Imports NUnit.Framework
Imports NUnit.Framework.Legacy
Imports System.Data

Namespace NUnitAutoTest

    <TestFixture>
    Public Class ForumCaseAnotherFieldNoMapTest

        Public Function InitConnection() As IDbConnection
            Dim connection = SQLiteFactory.Instance.CreateConnection()
            connection.ConnectionString = "Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"
            Return connection
        End Function

        <Test>
        Public Sub SelectCustomFieldMapperInternal()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of Topic)("select * from topic where id = 29").FirstOrDefault()
                    ClassicAssert.AreEqual("X", row.CustomString)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectCustomFieldMapperUser()
            Try
                SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                          Return New Topic With {
                                                                .TopicName = If(reader("topic_name").Equals(DBNull.Value), Nothing, reader("topic_name")),
                                                                .Id = If(reader("id").Equals(DBNull.Value), Nothing, reader("id")),
                                                                .ForumId = If(reader("forum_id").Equals(DBNull.Value), Nothing, reader("forum_id")),
                                                                .UserId = If(reader("user_id").Equals(DBNull.Value), Nothing, reader("user_id"))
                                                            }
                                                      End Function)

                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsMapper(Of Topic)("select * from topic where id = 29").FirstOrDefault()
                    ClassicAssert.AreEqual("X", row.CustomString)
                End Using

                SQLContext.UserMappers.UnregisterMapper(Of Topic)()

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectCustomFieldMapperFast()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of Topic)("select * from topic where id = 29").FirstOrDefault()
                    ClassicAssert.AreEqual("X", row.CustomString)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

    End Class

End Namespace
