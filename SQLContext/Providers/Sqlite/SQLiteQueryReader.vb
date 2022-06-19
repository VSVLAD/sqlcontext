Imports System.Globalization
Imports VSProject.SQLContext.Interfaces

Namespace Providers.SQLite

    Public Class SQLiteQueryReader
        Implements IQueryReader

        Private ReadOnly typeDate As Type = GetType(Date)
        Private ReadOnly typeLong As Type = GetType(Long)
        Private ReadOnly typeString As Type = GetType(String)
        Private ReadOnly typeInteger As Type = GetType(Integer)
        Private ReadOnly typeDouble As Type = GetType(Double)
        Private ReadOnly typeBoolean As Type = GetType(Boolean)

        Private Shared dateFormats As String() = {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.f",
            "yyyy-MM-dd HH:mm:ss.ff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss.ffff",
            "yyyy-MM-dd HH:mm:ss.fffff",
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd HH:mm:ss.fffffff"
        }

        Public Function GetPropertyValue(Value As Object, Source As Type, Destination As Type) As Object Implements IQueryReader.GetPropertyValue

            ' Если исходное значение было Null, вернём Null
            If Value Is Nothing Then Return Nothing

            ' Если тип столбца и тип свойства совпали, передаём данные без изменения
            If Destination Is Source Then
                Return Value
            Else

                ' Если даты SQLite получены в нативном типе, конвертация не требуется
                If Value.GetType() Is typeDate AndAlso Destination Is typeDate Then
                    Return Value
                End If

                ' Если булево SQLite получено в нативном типе, конвертация не требуется
                If Value.GetType() Is typeBoolean And Destination Is typeBoolean Then
                    Return Value
                End If

                ' Для даты SQLite в виде строки. Дата в формате ISO 8606
                If Source Is typeString AndAlso Destination Is typeDate Then
                    Return Date.ParseExact(CStr(Value), dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None)
                End If

                ' Для даты SQLite в виде целых чисел. Дата в формате Unix epoch
                If Source Is typeLong AndAlso Destination Is typeDate Then
                    Return New Date(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CDbl(Value))
                End If

                ' Для даты SQLite в виде дробных чисел. Дата в формате Julian Date
                If Source Is typeDouble AndAlso Destination Is typeDate Then
                    Dim unixTime = (CDbl(Value) - 2440587.5) * 86400
                    Dim epoch = New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                    Return epoch.AddSeconds(unixTime)
                End If

                ' Для булевых значений SQLite в виде целых чисел. 1 - True, 0 - False
                If (Source Is typeInteger OrElse Source Is typeLong) AndAlso Destination Is typeBoolean Then
                    Return CInt(Value) = 1
                End If

                ' Для остальных типов, просто возвращаем без изменения
                Return Value

            End If
        End Function

    End Class


End Namespace