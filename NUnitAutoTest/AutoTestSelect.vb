Imports VSProject.MicroORM.Attributes
Imports VSProject.MicroORM.Exceptions
Imports VSProject.MicroORM.Extensions
Imports VSProject.MicroORM
Imports NUnit.Framework
Imports System.Data.SQLite

Namespace NUnitAutoTest

    <TestFixture>
    Public Class AutoTestSelect

        Public Shared ConnectionString As String = "Data Source=C:\test.db"

        <Test, Order(1)>
        Public Sub InitCreateTable()
            Try
                IO.File.Delete("C:\test.db")

                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    context.ExecNonQuery("create table address(
                                id integer primary key autoincrement,
                                city text not null,
                                street text not null,
                                home integer null,
                                square float not null,
                                commercial boolean not null,
                                dateOne text null,
                                dateTwo integer null,
                                dateThree real null
                            )")

                    context.ExecNonQuery("insert into address(id, city, street, home, square, commercial)
                                                values(1, 'Краснодар', 'Тургенева', 118, 31.2, 0)")

                    context.ExecNonQuery("insert into address(id, city, street, home, square, commercial)
                                                values(2, 'Краснодар', 'Заводская', null, 150, 1)")
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub SelectRowsCountAsClassTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows(Of AddressInfo)("select * from address where id = 1").First
                    Assert.AreEqual("Краснодар", row.City)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub SelectRowsCountAsDictionaryTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows("select * from address where id = 1").First
                    Assert.AreEqual("Краснодар", row("city"))
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub SelectRowsCountAsDynamicTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRowsDynamic("select * from address where id = 1").First
                    Assert.AreEqual("Краснодар", row.City)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub SelectRowsIntegerNullTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim addr = context.SelectRows(Of AddressInfo)("select home from address where street = 'Заводская'").FirstOrDefault
                    Assert.IsNull(addr.Home)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub SelectRowsBooleanTrueTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim addr = context.SelectRows(Of AddressInfo)("select * from address where street = 'Заводская'").FirstOrDefault
                    Assert.IsTrue(addr.IsCommercial)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub SelectRowsBooleanFalseTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim addr = context.SelectRows(Of AddressInfo)("select * from address where street = 'Тургенева'").FirstOrDefault
                    Assert.IsFalse(addr.IsCommercial)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace