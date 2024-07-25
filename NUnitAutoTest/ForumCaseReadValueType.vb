Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Exceptions
Imports VSProject.SQLContext.Extensions
Imports VSProject.SQLContext
Imports System.Data.SQLite
Imports NUnit.Framework
Imports NUnit.Framework.Legacy

Namespace NUnitAutoTest

    <TestFixture>
    Public Class ForumCaseReadValueType

        Private Shared ConnectionString As String = "Data Source=C:\inetpub\wwwroot\murcode\app_data\SqlRu.db"

        <Test>
        Public Sub SelectValueTypeMapperInternal()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows(Of Object)("select topic_name from topic where id = 11").FirstOrDefault()
                    ClassicAssert.AreEqual("CurDir", row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeMapperUser()
            Try
                SQLContext.UserMappers.RegisterMapper(Function(reader)
                                                    Dim value = reader.GetInt64(0)
                                                    Return value
                                                End Function)

                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
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
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRowsFast(Of Object)("select topic_name from topic where id = 11").FirstOrDefault()
                    ClassicAssert.AreEqual("CurDir", row)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)
            End Try
        End Sub

        <Test>
        Public Sub SelectValueTypeMapperDynamic()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRowsDynamic("select count(*) as value from topic where forum_id = 22").FirstOrDefault()
                    ClassicAssert.AreEqual(17052, row.value)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace
