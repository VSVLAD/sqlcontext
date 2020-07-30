Imports System.Globalization
Imports VSProject.MicroORM.Interfaces

Namespace Providers.SQLite

    Public Class SQLiteQueryReader
        Implements IQueryReader

        Public Function GetPropertyValue(Value As Object, Source As Type, Destination As Type) As Object Implements IQueryReader.GetPropertyValue

            ' Если исходное значение было Null, вернём Null
            If Value Is Nothing Then Return Nothing

            ' Если тип столбца и тип свойства совпали, передаём данные без изменения
            If Destination Is Source Then
                Return Value
            Else

                ' Для даты SQLite в виде строки. Дата в формате ISO 8606
                If Source Is GetType(String) AndAlso Destination Is GetType(Date) Then
                    Return Date.ParseExact(CStr(Value), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                End If

                ' Для даты SQLite в виде целых чисел. Дата в формате Unix epoch
                If Source Is GetType(Long) AndAlso Destination Is GetType(Date) Then
                    Return New Date(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CDbl(Value))
                End If

                ' Для даты SQLite в виде дробных чисел. Дата в формате Julian Date
                If Source Is GetType(Double) AndAlso Destination Is GetType(Date) Then
                    Dim unixTime = (CDbl(Value) - 2440587.5) * 86400
                    Dim epoch = New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                    Return epoch.AddSeconds(unixTime)
                End If

                ' Для булевых значений SQLite в виде целых чисел. 1 - True, 0 - False
                If (Source Is GetType(Integer) OrElse Source Is GetType(Long)) AndAlso Destination Is GetType(Boolean) Then
                    Return CInt(Value) = 1
                End If

                ' Для остальных типов, просто возвращаем без изменения
                Return Value

            End If
        End Function

    End Class


End Namespace