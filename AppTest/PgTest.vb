Imports Npgsql
Imports VSProject.SQLContext
Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Extensions

Module PgTest

    Public Sub Main()

        'Using context As New SQLContext(New NpgsqlConnection("Server=localhost;Port=5432;Database=metric_db;User Id=postgres;"))
        '    context.Options.Writer = New PostgreSQL.PostgresQueryWriter
        '    context.Options.Reader = New PostgreSQL.PostgresQueryReader

        '    context.ExecNonQuery("drop table if exists test")
        '    context.ExecNonQuery("create table test(id integer, name char(10), ondate timestamp)")

        '    For Each num In Enumerable.Range(1, 10)
        '        context.InsertRow(New ClassTest With {.ID = num, .Name = "First " & num})
        '    Next

        '    ' Читаем 1 столбец, маппим на простой тип
        '    Console.WriteLine("Тест. Выборка 1 столбца с простым типом.")
        '    Console.ReadKey()

        '    For Each row In context.SelectRows(Of ClassTest)("select id, name, ondate from test")
        '        Console.WriteLine($"Ваша запись {row.Name}")
        '    Next


        '    ' Пишем
        '    Console.WriteLine("Тест. Вставка строк")
        '    Console.ReadKey()

        '    For Each num In Enumerable.Range(1, 2000)
        '        context.InsertRow(New ClassTest With {
        '                              .ID = num,
        '                              .Name = "Hello" & num,
        '                              .OnDateValue = Now().ToUniversalTime
        '                  })

        '        Console.WriteLine($"Вставка записи {num}")
        '    Next

        '    ' Читаем
        '    Console.WriteLine("Тест. Выборка несколько столбцов с сложным типом")
        '    Console.ReadKey()

        '    For Each row In context.SelectRows(Of ClassTest)("select id, name from test where id > @id_i", 500)
        '        Console.WriteLine($"Ваша запись {row.ID}, {row.Name}")
        '    Next

        '    ' Удаляем
        '    Console.WriteLine("Тест. Удаление записей")
        '    Console.ReadKey()

        '    For Each num In Enumerable.Range(100, 1500)
        '        context.DeleteRow(New ClassTest With {
        '                              .ID = num
        '                            })

        '        Console.WriteLine($"Удаление записи {num}")
        '    Next

        '    Console.WriteLine("Готово")
        'End Using

        'Console.ReadKey()
    End Sub

End Module