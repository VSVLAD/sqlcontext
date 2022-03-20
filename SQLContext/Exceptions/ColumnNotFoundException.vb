Namespace Exceptions

    ''' <summary>Исключение генерируется при использовании только программируемых полей или единственного поля типа автоинкремент с значением 0. Невозможно выполнять вставку и обновление таких таблиц</summary>
    Public Class ColumnNotFoundException
        Inherits SQLContextException
    End Class

End Namespace
