Imports VSProject.SQLContext.Interfaces

Namespace Providers.PostgreSQL

    Public Class PostgresQueryReader
        Implements IQueryReader

        Public Function GetPropertyValue(ColumnValue As Object, Source As Type, Destination As Type) As Object Implements IQueryReader.GetPropertyValue

            ' Если исходное значение было Null, вернём Null
            If ColumnValue Is Nothing Then Return Nothing

            Select Case Destination
                Case GetType(Long)
                    Return CLng(ColumnValue)

                Case GetType(Integer)
                    Return CInt(ColumnValue)

                Case GetType(Short)
                    Return CShort(ColumnValue)

                Case GetType(Byte)
                    Return CByte(ColumnValue)

            End Select

            Return ColumnValue
        End Function

    End Class


End Namespace