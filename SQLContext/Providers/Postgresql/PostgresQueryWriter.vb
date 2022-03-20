Imports System.Globalization
Imports VSProject.SQLContext.Exceptions
Imports VSProject.SQLContext.Interfaces

Namespace Providers.PostgreSQL

    Public Class PostgresQueryWriter
        Implements IQueryWriter

        Private Const TOKEN_SELECT As String = " select "
        Private Const TOKEN_FROM As String = " from "
        Private Const TOKEN_WHERE As String = " where "
        Private Const TOKEN_DELETE As String = " delete "
        Private Const TOKEN_INSERT As String = " insert into "
        Private Const TOKEN_UPDATE As String = " update "
        Private Const TOKEN_SET As String = " set "
        Private Const TOKEN_VALUES As String = " values "
        Private Const TOKEN_NULL As String = " null "
        Private Const TOKEN_EQUAL As String = " = "
        Private Const TOKEN_BRACKET_OPEN As String = " ( "
        Private Const TOKEN_BRACKET_CLOSE As String = " ) "
        Private Const TOKEN_DOT As String = "."
        Private Const TOKEN_COMMA As String = ","
        Private Const TOKEN_SEMICOLON As String = ";"

        Public Function GetTokenSemicolon() As String Implements IQueryWriter.GetTokenSemicolon
            Return TOKEN_SEMICOLON
        End Function

        Public Function GetTokenDot() As String Implements IQueryWriter.GetTokenDot
            Return TOKEN_DOT
        End Function

        Public Function GetTokenComma() As String Implements IQueryWriter.GetTokenComma
            Return TOKEN_COMMA
        End Function

        Public Function getTokenSelect() As String Implements IQueryWriter.GetTokenSelect
            Return TOKEN_SELECT
        End Function

        Public Function getTokenFrom() As String Implements IQueryWriter.GetTokenFrom
            Return TOKEN_FROM
        End Function

        Public Function GetTokenWhere() As String Implements IQueryWriter.GetTokenWhere
            Return TOKEN_WHERE
        End Function

        Public Function getTokenDelete() As String Implements IQueryWriter.GetTokenDelete
            Return TOKEN_DELETE
        End Function

        Public Function getTokenInsert() As String Implements IQueryWriter.GetTokenInsert
            Return TOKEN_INSERT
        End Function

        Public Function getTokenUpdate() As String Implements IQueryWriter.GetTokenUpdate
            Return TOKEN_UPDATE
        End Function

        Public Function getTokenSet() As String Implements IQueryWriter.GetTokenSet
            Return TOKEN_SET
        End Function

        Public Function getTokenValues() As String Implements IQueryWriter.GetTokenValues
            Return TOKEN_VALUES
        End Function

        Public Function getTokenNull() As String Implements IQueryWriter.GetTokenNull
            Return TOKEN_NULL
        End Function

        Public Function GetTokenEqual() As String Implements IQueryWriter.GetTokenEqual
            Return TOKEN_EQUAL
        End Function

        Public Function GetTokenBracketOpen() As String Implements IQueryWriter.GetTokenBracketOpen
            Return TOKEN_BRACKET_OPEN
        End Function

        Public Function GetTokenBracketClose() As String Implements IQueryWriter.GetTokenBracketClose
            Return TOKEN_BRACKET_CLOSE
        End Function

        Public Function GetTableName(TableName As String) As String Implements IQueryWriter.GetTableName
            Return $"""{TableName}"""
        End Function

        Public Function GetColumnName(ColumnName As String) As String Implements IQueryWriter.GetColumnName
            Return $"""{ColumnName}"""
        End Function

        Public Function GetColumnValue(Value As Object, Source As Type, Destination As Type) As String Implements IQueryWriter.GetColumnValue

            ' Для Nothing значений вернём null
            If Value Is Nothing Then Return getTokenNull()

            Select Case Source
                Case GetType(Date)
                    Return $"'{CDate(Value).ToString("yyyy-MM-dd HH:mm:ss")}'"

                Case GetType(String)
                    Return $"'{CStr(Value)}'"

                Case GetType(Single), GetType(Double), GetType(Decimal)
                    Return CDec(Value).ToString(CultureInfo.InvariantCulture)

                Case GetType(Byte), GetType(Short), GetType(Integer), GetType(Long), GetType(SByte), GetType(UInteger), GetType(UShort), GetType(ULong)
                    Return Value.ToString()

                Case GetType(Boolean)
                    Return If(CBool(Value), "TRUE", "FALSE")

                Case Else
                    Throw New ColumnNotSerializableException

            End Select
        End Function

    End Class

End Namespace