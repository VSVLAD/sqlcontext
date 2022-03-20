Imports VSProject.SQLContext.Interfaces
Imports VSProject.SQLContext.Providers

''' <summary>Настройки для поведения класса SQLContext</summary>
Public Class SQLContextOptions

    ''' <summary>Сериализатор запросов. Реализация для SQLite по-умолчанию</summary>
    Public Property Writer As IQueryWriter = New SQLite.SQLiteQueryWriter

    ''' <summary>Маппер типов по-умолчанию. Реализация для SQLite по-умолчанию</summary>
    Public Property Reader As IQueryReader = New SQLite.SQLiteQueryReader

End Class
