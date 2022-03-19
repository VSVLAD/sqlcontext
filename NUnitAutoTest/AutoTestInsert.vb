Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Exceptions
Imports VSProject.SQLContext.Extensions
Imports VSProject.SQLContext
Imports NUnit.Framework
Imports System.Data.SQLite

Namespace NUnitAutoTest

    <TestFixture>
    Public Class AutoTestInsert

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
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub InsertRowsAllFieldsTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    context.InsertRow(New AddressInfo With {.AddressId = 99,
                                                            .City = "Краснодар",
                                                            .Street = "Заводская",
                                                            .Home = "19",
                                                            .Square = 120,
                                                            .IsCommercial = True})
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub InsertRowsAutoIncrementFieldTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    context.InsertRow(New AddressInfo With {.City = "Тимашевск",
                                                            .Street = "Индустриальный",
                                                            .Home = "13",
                                                            .IsCommercial = False})
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub InsertRowsNullFieldTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    context.InsertRow(New AddressInfo With {.City = "Краснодар",
                                                            .Street = "Красная",
                                                            .Home = Nothing})
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub InsertRowsColumnNotFoundTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows(Of AddressInfoWithoutDataColumn)("select * from address where id = 1").First
                    context.InsertRow(row)
                End Using

            Catch ex As ColumnNotFoundException
                Assert.Pass("Удачно. Получили исключение ColumnNotFoundException")

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace