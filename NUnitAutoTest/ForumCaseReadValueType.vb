Imports VSProject.SQLContext
Imports System.Data.SQLite
Imports NUnit.Framework
Imports NUnit.Framework.Legacy
Imports System.Data

Namespace NUnitAutoTest

    <TestFixture>
    Public Class ForumCaseReadValueType

        Public Function InitConnection() As IDbConnection
            Dim connection = SQLiteFactory.Instance.CreateConnection()
            connection.ConnectionString = "Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"
            Return connection
        End Function

        <Test>
        Public Sub SelectValueTypeMapperInternal()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of Long)("select count(*) from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeNullableMapperInternal()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of Long?)("select count(*) from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeNullableGetNullMapperInternal()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of Long?)("select null").FirstOrDefault()
                    ClassicAssert.AreEqual(Nothing, row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeStringMapperInternal()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of String)("select topic_name from topic where id = 11").FirstOrDefault()
                    ClassicAssert.AreEqual("CurDir", row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeObjectMapperInternal()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRows(Of Object)("select topic_name from topic where id = 11").FirstOrDefault()
                    ClassicAssert.AreEqual("CurDir", row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongMapperUser()
            Try
                SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                          Dim value = reader.GetInt64(0)
                                                          Return value
                                                      End Function)

                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsMapper(Of Long)("select id from topic where id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(22, row)
                End Using

                SQLContext.UserMappers.UnregisterMapper(Of Long)()

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongMapperFast()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of Long)("select count(*) from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongNullableMapperFast()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of Long?)("select count(*) from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongNullableGetNullMapperFast()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of Long?)("select null").FirstOrDefault()
                    ClassicAssert.AreEqual(Nothing, row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeStringMapperFast()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of String)("select topic_name from topic where id = 11").FirstOrDefault()
                    ClassicAssert.AreEqual("CurDir", row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeObjectMapperFast()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsFast(Of Object)("select topic_name from topic where id = 11").FirstOrDefault()
                    ClassicAssert.AreEqual("CurDir", row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongMapperDynamic()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsDynamic("select count(*) as value from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, row.value)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongScalarValueMapperDynamic()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsDynamic("select count(*) + 1 - 1 from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, CType(row, DynamicRow).ScalarValue(Of Long))
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongGetNullMapperDynamic()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsDynamic("select null as value from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(Nothing, row?.value)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeLongScalarValueGetNullMapperDynamic()
            Try
                Using context As New SQLContext(InitConnection())
                    Dim row = context.SelectRowsDynamic("select null as value from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(Nothing, CType(row, DynamicRow).ScalarValue(Of Long?))
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace
