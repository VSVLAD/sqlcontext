Namespace Interfaces

    ''' <summary>Интерфейс предоставляет методы для маппинга столбцов таблицы на свойства класса</summary>
    Public Interface IQueryReader

        ''' <summary>Метод должен выполнить конвертацию данных для маппинга в совместимый тип .NET</summary>
        ''' <param name="Value">Значение столбца прочитанного из запроса упакованное в Object</param>
        ''' <param name="Source">Исходный тип столбца с прочитанным значением</param>
        ''' <param name="Destination">Результрущий тип свойства на которое маппятся данные</param>
        ''' <returns>Значение, которое должно быть записано в свойство упакованное в Object</returns>
        Function GetPropertyValue(Value As Object, Source As Type, Destination As Type) As Object

    End Interface

End Namespace