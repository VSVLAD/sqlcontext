
'Вот код на VB.NET, который генерирует Expression Tree для указанного вами кода


'Imports System.Linq.Expressions

'Module Module1
'    Sub Main()
'        Dim readerParam = Expression.Parameter(GetType(IDataReader), "reader")

'        Dim var1 = Expression.Variable(GetType(Long), "var1")
'        Dim var2 = Expression.Variable(GetType(String), "var2")
'        Dim var3 = Expression.Variable(GetType(Date), "var3")

'        Dim block = Expression.Block(
'            New ParameterExpression() {var1, var2, var3},
'            Expression.Assign(var1, Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(0))),
'            Expression.Assign(var2, Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(1))),
'            Expression.Assign(var3, Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(2)))
'        )

'        Dim lambda = Expression.Lambda(Of Action(Of IDataReader))(block, readerParam)

'        Console.WriteLine(lambda.ToString())
'    End Sub
'End Module
'Этот код создает Expression Tree, соответствующий вашему примеру, и выводит его строковое представление.



'А измени код так, чтобы можно было вынести Expression.Assign в некий массив и его можно было дополнять



'Конечно, вот модифицированный код, который позволяет хранить Expression.Assign в массиве и легко его дополнять


'Imports System.Linq.Expressions

'Module Module1
'    Sub Main()
'        Dim readerParam = Expression.Parameter(GetType(IDataReader), "reader")

'        Dim variables As New List(Of ParameterExpression) From {
'            Expression.Variable(GetType(Long), "var1"),
'            Expression.Variable(GetType(String), "var2"),
'            Expression.Variable(GetType(Date), "var3")
'        }

'        Dim assignments As New List(Of Expression) From {
'            Expression.Assign(variables(0), Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(0))),
'            Expression.Assign(variables(1), Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(1))),
'            Expression.Assign(variables(2), Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(2)))
'        }

'        ' Пример добавления нового присваивания
'        variables.Add(Expression.Variable(GetType(Integer), "var4"))
'        assignments.Add(Expression.Assign(variables(3), Expression.Call(readerParam, "GetOrdinal", Nothing, Expression.Constant(3))))

'        Dim block = Expression.Block(variables, assignments)

'        Dim lambda = Expression.Lambda(Of Action(Of IDataReader))(block, readerParam)

'        Console.WriteLine(lambda.ToString())
'    End Sub
'End Module