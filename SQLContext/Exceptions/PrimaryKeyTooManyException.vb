Namespace Exceptions

    ''' <summary>Исключение при использовании атрибута первичного ключа у разных свойства одного класса</summary>
    Public Class PrimaryKeyTooManyException
        Inherits SQLContextException
    End Class

End Namespace
