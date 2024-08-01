Option Explicit On
Option Strict On

Public Class SQLContextParameters

    ''' <summary>
    ''' Стандартные типы .NET замапленные на типы SQL
    ''' </summary>
    Private Shared typeNetDbTypes As New Dictionary(Of Type, DbType) From {
            {GetType(String), DbType.String},
            {GetType(Byte), DbType.Byte},
            {GetType(Short), DbType.Int16},
            {GetType(Integer), DbType.Int32},
            {GetType(Long), DbType.Int64},
            {GetType(Boolean), DbType.Boolean},
            {GetType(Date), DbType.DateTime2},
            {GetType(DateTimeOffset), DbType.DateTimeOffset},
            {GetType(Double), DbType.Double},
            {GetType(Single), DbType.Single},
            {GetType(Decimal), DbType.Decimal},
            {GetType(TimeSpan), DbType.Time}
        }

    ''' <summary>
    ''' Сформировать подготовленную команду по словарю, где ключ - имя параметра, а значение - .NET тип упакованный в Object
    ''' </summary>
    Public Shared Function FromDictionary(Connection As IDbConnection, SqlText As String, Paramerers As Dictionary(Of String, Object)) As IDbCommand

        ' Создаём объект для подготовленной команды
        Dim prepCommand = Connection.CreateCommand()
        prepCommand.CommandText = SqlText

        If Paramerers IsNot Nothing Then
            For Each kv In Paramerers

                ' Создаём параметр с типом как значение свойства
                Dim param = prepCommand.CreateParameter()
                param.ParameterName = $"@{kv.Key}"
                param.DbType = GetDbType(If(kv.Value, "").GetType())
                param.Value = If(kv.Value, DBNull.Value)

                ' Добавляем параметр к команде
                prepCommand.Parameters.Add(param)
            Next
        End If

        Return prepCommand
    End Function

    ''' <summary>
    ''' Сформировать словарь с параметрами по свойствам из анонимного объекта
    ''' </summary>
    Public Shared Function ToDictionary(Paramerers As Object) As Dictionary(Of String, Object)
        Dim retDict As New Dictionary(Of String, Object)

        If Paramerers IsNot Nothing Then
            Dim type = Paramerers.GetType()
            Dim props = type.GetProperties()

            For Each prop In props
                retDict.Add(prop.Name, prop.GetValue(Paramerers))
            Next
        End If

        Return retDict
    End Function

    Private Shared Function GetDbType(type As Type) As DbType
        Dim result As DbType

        If typeNetDbTypes.TryGetValue(type, result) Then
            Return result
        End If

        Throw New SQLContextException(Resources.ExceptionMessages.NO_CONVERT_NETTYPE_TO_DBTYPE)
    End Function

End Class
