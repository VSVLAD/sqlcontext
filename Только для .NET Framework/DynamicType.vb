Imports System.Reflection
Imports System.Text

Namespace Dynamic

    Public Class DynamicType

        ' Кеш раннее скомпилированных типов
        Private Shared types As New Dictionary(Of Type, DynamicType)

        ' Представление текущего типа
        Private Compiler As New ScriptCompiler
        Private ClassType As Type
        Private ClassName As String
        Private MethodName As String

        ' Закрываем конструктор
        Private Sub New()
        End Sub

        ''' <summary>Метод выполняет чтение из DataReader и возвращает экземпляр пользовательского типа с значениями</summary>
        ''' <param name="Reader">Объект из которого будет выполнено чтение значений</param>
        ''' <returns>Экземпляр пользовательского класса</returns>
        Public Function YieldFromDataReader(Reader As IDataReader) As Object
            Return Compiler.RunStatic(ClassName, MethodName, Reader, ClassType)
        End Function

        ''' <summary>Метод генерирует и компилирует код на VB.NET для чтения данных из DataReader и маппинга на пользовательский тип</summary>
        ''' <param name="ClassType">Пользовательский тип (класс) в свойства которого будут заданы значения при чтении из DataReader</param>
        ''' <returns>Объект DynamicType готовый для исполнения</returns>
        Public Shared Function Compile(ClassType As Type) As DynamicType

            ' Если тип раннее компилировался, возвращаем его
            If types.ContainsKey(ClassType) Then

                Return types(ClassType)
            Else
                ' Собираем метод чтения
                Dim dti As New DynamicType

                dti.ClassType = ClassType
                dti.ClassName = $"row_{ClassType.DeclaringType.Name & PreparedQuery.MD5Hash(CStr(ClassType.GetHashCode))}"
                dti.MethodName = $"read_{ClassType.DeclaringType.Name & PreparedQuery.MD5Hash(CStr(ClassType.GetHashCode))}"

                Dim sbCode As New StringBuilder
                sbCode.AppendLine("Imports System.Data")
                sbCode.AppendLine("Imports System.Reflection")
                sbCode.AppendLine("Imports System")
                sbCode.AppendLine($"Public Class {dti.ClassName}")
                sbCode.AppendLine($"Public Function {dti.MethodName}(reader As IDataReader, ClassType As Type) As Object")
                sbCode.AppendLine("Dim result = Activator.CreateInstance(classType)")

                sbCode.AppendLine(" Логику дорисовать тут ")

                '' Читаем поле по индексу
                '_instance.T1 = sqlRead.GetInt32(0)

                '' Читаем поле с проверкой на Null
                '_instance.T1 = If(sqlRead.IsDBNull(0), Nothing, sqlRead.GetInt32(0))


                sbCode.AppendLine("End Function")
                sbCode.AppendLine("End Class")

                ' Компилируем
                dti.Compiler.CompileCode(sbCode.ToString)

                ' Кешируем
                types.Add(ClassType, dti)

                ' Возвращаем
                Return dti
            End If
        End Function


    End Class

End Namespace
