Imports VSProject.MicroORM.Interfaces
Imports VSProject.MicroORM.Providers

''' <summary>Настройки для поведения класса SQLContext</summary>
Public Class SQLContextOptions

    ''' <summary>Подготовка и кеширование параметризованных запросов</summary>
    Public Shared Property PreparedCommand As New PreparedQuery

    ''' <summary>Сериализатор запросов. Реализация для SQLite по-умолчанию</summary>
    Public Shared Property Writer As IQueryWriter = New SQLite.SQLiteQueryWriter

    ''' <summary>Маппер типов по-умолчанию. Реализация для SQLite по-умолчанию</summary>
    Public Shared Property Reader As IQueryReader = New SQLite.SQLiteQueryReader

End Class
