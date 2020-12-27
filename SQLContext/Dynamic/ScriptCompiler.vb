Option Explicit On
Option Strict On

Imports System.CodeDom.Compiler
Imports System.Reflection

''' <summary>Перечисление поддерживаемых языков</summary>
Friend Enum ScriptLanguage
    VBNET = 0
    CSharp = 1
End Enum

Friend Class ScriptCompiler

    Private m_CErrors As CompilerErrorCollection
    Private m_CResults As CompilerResults
    Private m_TextCode As String

    ''' <summary>Конструктор</summary>
    Public Sub New()
        m_CErrors = New CompilerErrorCollection
    End Sub

    ''' <summary>Список всех ошибок во время компиляции кода</summary>
    Public ReadOnly Property Errors() As CompilerErrorCollection
        Get
            Return m_CErrors
        End Get
    End Property

    ''' <summary>Код скрипта на VB.NET или C#</summary>
    Public ReadOnly Property TextCode As String
        Get
            Return m_TextCode
        End Get
    End Property

    ''' <summary>Скомпилированная сборка</summary>
    Public ReadOnly Property CompiledAssembly As Assembly
        Get
            Return m_CResults.CompiledAssembly
        End Get
    End Property

    ''' <summary>Выполняет раннее скомпилированный код</summary>
    Public Function RunStatic(ClassName As String, MethodName As String, ParamArray Param() As Object) As Object
        Dim objType As Type = m_CResults.CompiledAssembly.GetType(ClassName)
        Dim objMethod As MethodInfo = objType.GetMethod(MethodName, BindingFlags.IgnoreCase Or BindingFlags.Static Or BindingFlags.Public)

        Try
            If Param.Length = 0 Then
                Return objMethod.Invoke(Nothing, Nothing)
            Else
                Return objMethod.Invoke(Nothing, Param)
            End If

        Catch ex As Exception
            Throw New Exception($"ScriptCompiler.RunStatic() : Исполняемый код вызвал исключение: {ex.Message}")

        End Try
    End Function

    ''' <summary>Выполняет раннее скомпилированный код</summary>
    Public Function RunInstance(ClassName As String, MethodName As String, ParamArray Param() As Object) As Object
        Dim objType As Type = m_CResults.CompiledAssembly.GetType(ClassName)

        Dim objClass = Activator.CreateInstance(objType)
        Dim objMethod As MethodInfo = objType.GetMethod(MethodName, BindingFlags.IgnoreCase Or BindingFlags.Instance Or BindingFlags.Public)

        Try
            If Param.Length = 0 Then
                Return objMethod.Invoke(objClass, Nothing)
            Else
                Return objMethod.Invoke(objClass, Param)
            End If

        Catch ex As Exception
            Throw New Exception($"ScriptCompiler.RunInstance() : Исполняемый код вызвал исключение: {ex.Message}")

        End Try
    End Function

    ''' <summary>Выполняет компиляцию предварительную компиляцию кода. После его можно исполнить</summary>
    Public Function CompileCode(ByVal Text As String,
                                Optional ByVal Language As ScriptLanguage = ScriptLanguage.VBNET,
                                Optional ByVal ReferencedAssemblies() As String = Nothing) As ScriptCompiler

        Dim cParams As CompilerParameters = New CompilerParameters()
        Dim provider As CodeDomProvider

        'Выбор провайдера языка
        If Language = ScriptLanguage.CSharp Then
            provider = CodeDomProvider.CreateProvider("CSharp")
        ElseIf Language = ScriptLanguage.VBNET Then
            provider = CodeDomProvider.CreateProvider("VisualBasic")
        Else
            Throw New Exception("ScriptLanguage.CompileCode() : Передан язык неподдерживаемый классом")
        End If

        'Настраиваем параметры компиляции
        cParams.ReferencedAssemblies.Add("system.dll")
        cParams.ReferencedAssemblies.Add("system.data.dll")
        cParams.ReferencedAssemblies.Add("system.core.dll")
        cParams.ReferencedAssemblies.Add("system.windows.forms.dll")
        cParams.ReferencedAssemblies.Add("system.drawing.dll")
        cParams.ReferencedAssemblies.Add("system.xml.dll")
        cParams.ReferencedAssemblies.Add("system.xml.linq.dll")

        'добавляем ещё кастомные библиотеки
        If ReferencedAssemblies IsNot Nothing Then
            For Each xItem In ReferencedAssemblies
                cParams.ReferencedAssemblies.Add(xItem)
            Next
        End If

        cParams.CompilerOptions = "/t:library"
        cParams.GenerateInMemory = True
        cParams.IncludeDebugInformation = True
        cParams.TreatWarningsAsErrors = False

        'Компиляция
        m_TextCode = Text
        m_CResults = provider.CompileAssemblyFromSource(cParams, m_TextCode)
        m_CErrors = m_CResults.Errors

        'Проверка на ошибки
        If m_CErrors.Count > 0 Then
            Dim strAllErrors As String = String.Empty

            For Each xErr As CompilerError In m_CErrors
                If xErr.IsWarning Then
                    Continue For
                Else
                    strAllErrors &= $"{xErr.ErrorNumber},{xErr.ErrorText}  {{{xErr.Line}, {xErr.Column}}}" & vbCrLf & vbCrLf
                End If
            Next

            If Not String.IsNullOrEmpty(strAllErrors) Then
                Throw New Exception($"ScriptCompiler.CompileCode() : Имеются ошибки компиляции кода. Выполнение кода невозможно. {vbCrLf}{vbCrLf} {strAllErrors}")
            End If
        End If

        Return Me
    End Function

End Class