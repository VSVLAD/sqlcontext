﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Этот код создан программой.
'     Исполняемая версия:4.0.30319.42000
'
'     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
'     повторной генерации кода.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace Resources
    
    'Этот класс создан автоматически классом StronglyTypedResourceBuilder
    'с помощью такого средства, как ResGen или Visual Studio.
    'Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    'с параметром /str или перестройте свой проект VS.
    '''<summary>
    '''  Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Class ExceptionMessages
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("VSProject.MicroORM.ExceptionMessages", GetType(ExceptionMessages).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Перезаписывает свойство CurrentUICulture текущего потока для всех
        '''  обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Not found rule serialization for value type {0} from {1} to {2}.
        '''</summary>
        Friend Shared ReadOnly Property COLUMN_NOT_SERIALIZABLE() As String
            Get
                Return ResourceManager.GetString("COLUMN_NOT_SERIALIZABLE", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Connection object must be created before using in SQLContext.
        '''</summary>
        Friend Shared ReadOnly Property CONNECTION_MUST_BE_CREATED() As String
            Get
                Return ResourceManager.GetString("CONNECTION_MUST_BE_CREATED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Not yet implemented with arrays.
        '''</summary>
        Friend Shared ReadOnly Property NOT_IMPLEMENTED_WITH_ARRAY() As String
            Get
                Return ResourceManager.GetString("NOT_IMPLEMENTED_WITH_ARRAY", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Not yet implemented with Nullable types.
        '''</summary>
        Friend Shared ReadOnly Property NOT_IMPLEMENTED_WITH_NULLABLE() As String
            Get
                Return ResourceManager.GetString("NOT_IMPLEMENTED_WITH_NULLABLE", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на For parameterized queries, you must use the suffix in the parameter name!
        '''
        '''Suffixes: S - string, R - fractional number, I - integer, D - datetime. Knowing the suffix, the class correctly declares the parameter type.
        '''Examples: @USER_NAME_S, :USER_ID_I, :DATE_OF_CHANGE_D&quot;.
        '''</summary>
        Friend Shared ReadOnly Property PARAMETERIZED_QUERY_MUST_USE_SUFFIX() As String
            Get
                Return ResourceManager.GetString("PARAMETERIZED_QUERY_MUST_USE_SUFFIX", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Values passed for parameterized query, but parameter names not recognized!
        '''Named parameters must use the prefixes and suffixes. @param_name_s or :param_name_s are supported.
        '''
        '''Suffixes: S - string, R - fractional number, I - integer, D - datetime. Knowing the suffix, the class correctly declares the parameter type.
        '''Examples: @USER_NAME_S, :USER_ID_I, :DATE_OF_CHANGE_D&quot;.
        '''</summary>
        Friend Shared ReadOnly Property PARAMETERS_NAMES_MUST_USE_IF_PASSED_VALUES() As String
            Get
                Return ResourceManager.GetString("PARAMETERS_NAMES_MUST_USE_IF_PASSED_VALUES", resourceCulture)
            End Get
        End Property
    End Class
End Namespace