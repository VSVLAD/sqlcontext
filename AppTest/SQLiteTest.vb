Imports System.Data.SQLite
Imports VSProject.SQLContext
Imports VSProject.SQLContext.Attributes
Imports VSProject.SQLContext.Extensions

Module SQLiteTest

    'Sub Main2()

    '    Using context = New SQLContext(New SQLiteConnection("Data Source=C:\test.db"))
    '        context.ExecNonQuery("drop table if exists test")
    '        context.ExecNonQuery("create table if not exists test(id integer, name text, ondate datetime)")
    '        context.ExecNonQuery("pragma synchronous = OFF")
    '        context.ExecNonQuery("pragma journal_mode = OFF")

    '        For Each num In Enumerable.Range(1, 10)
    '            context.InsertRow(New ClassTest With {.ID = num, .Name = "First " & num})
    '        Next


    '        ' Читаем 1 столбец, маппим на класс
    '        Console.WriteLine("Тест. Выборка 1 столбца и маппим на класс")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows(Of ClassTest)("select id, name, ondate from test")
    '            Console.WriteLine($"Ваша запись {row.Name}")
    '        Next

    '        ' Читаем 1 столбец, маппим на тип массив
    '        Console.WriteLine("Тест. Выборка 2 столбцов и маппим на массив")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows(Of Object())("select id, name from test")
    '            Console.WriteLine($"Ваша запись {row(0)} - {row(1)}")
    '        Next

    '        ' Читаем 1 столбец, маппим на простой тип
    '        Console.WriteLine("Тест. Выборка 1 столбца и маппим на простой значимый тип")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows(Of Integer)("select id, name from test")
    '            Console.WriteLine($"Ваша запись {row}")
    '        Next

    '        Console.WriteLine("Тест. Выборка записей и передаём параметры в массиве")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows(Of ClassTest)("select * from test where name like :name_s and id >= :id_i", "First %", 5)
    '            Console.WriteLine($"Ваша запись {row.ID} {row.Name} {row.OnDateValue}")
    '        Next

    '        Console.WriteLine("Тест. Выборка записей и передаём параметры в словаре, возвращаем словарь данных")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows("select * from test where name like :name_s and id >= :id_i", New Dictionary(Of String, Object) From {
    '                                                                                                                            {":id_i", 5},
    '                                                                                                                            {":name_s", "First %"}
    '                                                                                                                        })
    '            Console.WriteLine($"Ваша запись {row("id")} {row("name")} {row("ondate")}")
    '        Next

    '        Console.WriteLine("Тест. Выборка записей и передаём параметры в словаре, возвращаем класс")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows(Of ClassTest)("select * from test where name like :name_s and id >= :id_i", New Dictionary(Of String, Object) From {
    '                                                                                                                                {":id_i", 5},
    '                                                                                                                                {":name_s", "First %"}
    '                                                                                                                            })
    '            Console.WriteLine($"Ваша запись {row.ID} {row.Name} {row.OnDateValue}")
    '        Next


    '        ' Пишем
    '        Console.WriteLine("Тест. Вставка строк")
    '        Console.ReadKey()

    '        For Each num In Enumerable.Range(1, 2000)
    '            context.InsertRow(New ClassTest With {
    '                                  .ID = num,
    '                                  .Name = "Hello" & num,
    '                                  .OnDateValue = Now().ToUniversalTime
    '                      })

    '            Console.WriteLine($"Вставка записи {num}")
    '        Next

    '        ' Читаем
    '        Console.WriteLine("Тест. Выборка несколько столбцов с сложным типом")
    '        Console.ReadKey()

    '        For Each row In context.SelectRows(Of ClassTest)("select id, name from test where id > @id_i", 500)
    '            Console.WriteLine($"Ваша запись {row.ID}, {row.Name}")
    '        Next

    '        ' Удаляем
    '        Console.WriteLine("Тест. Удаление записей")
    '        Console.ReadKey()

    '        For Each num In Enumerable.Range(100, 1500)
    '            context.DeleteRow(New ClassTest With {
    '                                  .ID = num
    '                                })

    '            Console.WriteLine($"Удаление записи {num}")
    '        Next

    '        Console.WriteLine("Готово")
    '        context.Dispose()

    '        Console.ReadKey()
    '    End Using
    'End Sub


End Module