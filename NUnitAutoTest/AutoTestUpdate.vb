Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Exceptions
Imports VSProject.SQLContext.Extensions
Imports VSProject.SQLContext
Imports NUnit.Framework
Imports System.Data.SQLite

Namespace NUnitAutoTest

    <TestFixture>
    Public Class AutoTestUpdate

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
        Public Sub UpdateRowsTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows(Of AddressInfo)("select * from address where id = 1").First

                    row.City = "Сочи"
                    context.UpdateRow(row)

                    row = context.SelectRows(Of AddressInfo)("select * from address where id = 1").First
                    Assert.AreEqual("Сочи", row.City)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub UpdateRowsNotFoundPrimaryKeyTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows(Of AddressInfoWithoutPrimaryKey)("select * from address where id = 1").First
                    context.UpdateRow(row)
                End Using

            Catch ex As PrimaryKeyNotFoundException
                Assert.Pass("Удачно. Получили исключение PrimaryKeyNotFoundException")

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

        <Test, Order(2)>
        Public Sub UpdateRowsNowDateTimeTest()
            Try
                Using context As New SQLContext(New SQLiteConnection(ConnectionString))
                    Dim row = context.SelectRows(Of AddressInfo)("select * from address where id = 1").First

                    Dim saveDate = Now.AddDays(1)
                    saveDate = New Date(saveDate.Year, saveDate.Month, saveDate.Day, saveDate.Hour, saveDate.Minute, saveDate.Second, saveDate.Kind)

                    row.DateOne = saveDate
                    row.DateTwo = saveDate
                    row.DateThree = saveDate

                    context.UpdateRow(row)

                    row = context.SelectRows(Of AddressInfo)("select * from address where id = 1").First

                    Assert.AreEqual(saveDate, row.DateOne)
                    Assert.AreEqual(saveDate, row.DateTwo)
                    Assert.AreEqual(saveDate, row.DateThree)
                End Using

            Catch ex As Exception
                Assert.Fail(ex.Message)

            End Try
        End Sub

    End Class

End Namespace