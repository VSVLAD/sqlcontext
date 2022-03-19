Imports System.Data.SQLite
Imports VSProject.SQLContext
Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Extensions

Module SQLiteTest

    Sub Main()

        Using context = New SQLContext(New SQLiteConnection("Data Source=C:\test.db"))
            context.ExecNonQuery("drop table if exists test")
            context.ExecNonQuery("create table if not exists test(id integer, name text, ondate datetime)")
            context.ExecNonQuery("pragma synchronous = OFF")
            context.ExecNonQuery("pragma journal_mode = OFF")

            For Each num In Enumerable.Range(1, 10)
                context.InsertRow(New ClassTest With {.ID = num, .Name = "First " & num})
            Next


            ' Читаем 1 столбец, маппим на простой тип
            Console.WriteLine("Тест. Выборка 1 столбца с простым типом")
            Console.ReadKey()

            For Each row In context.SelectRows(Of ClassTest)("select id, name, ondate from test")
                Console.WriteLine($"Ваша запись {row.Name}")
            Next


            ' Пишем
            Console.WriteLine("Тест. Вставка строк")
            Console.ReadKey()

            For Each num In Enumerable.Range(1, 2000)
                context.InsertRow(New ClassTest With {
                                      .ID = num,
                                      .Name = "Hello" & num,
                                      .OnDateValue = Now().ToUniversalTime
                          })

                Console.WriteLine($"Вставка записи {num}")
            Next

            ' Читаем
            Console.WriteLine("Тест. Выборка несколько столбцов с сложным типом")
            Console.ReadKey()

            For Each row In context.SelectRows(Of ClassTest)("select id, name from test where id > @id_i", 500)
                Console.WriteLine($"Ваша запись {row.ID}, {row.Name}")
            Next

            ' Удаляем
            Console.WriteLine("Тест. Удаление записей")
            Console.ReadKey()

            For Each num In Enumerable.Range(100, 1500)
                context.DeleteRow(New ClassTest With {
                                      .ID = num
                                    })

                Console.WriteLine($"Удаление записи {num}")
            Next

            Console.WriteLine("Готово")
            context.Dispose()

            Console.ReadKey()
        End Using
    End Sub



    <Table("test")>
    Public Class ClassTest

        <PrimaryKey>
        Public Property ID As Long

        Public Property Name As String

        <Column("ondate", GetType(String))>
        Public Property OnDateValue As Date?

        <Programmable>
        Public Property Address As String

        <Programmable>
        Public Property Phone As Integer?

        <Programmable>
        Public Property Skype As Integer?

    End Class


End Module