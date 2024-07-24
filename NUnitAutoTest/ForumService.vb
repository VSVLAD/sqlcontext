'Imports VSProject.SQLContext.Extensions
'Imports VSProject.SQLContext
'Imports System.Data.SQLite

'Public Class ForumService


'    Public Sub New(ConnectionString As String)
'        Me.dataSourceString = ConnectionString
'    End Sub

'    ''' <summary>
'    ''' Получить список форумов и количество топиков в них
'    ''' </summary>
'    Public Iterator Function GetForumsTopicCount() As IEnumerable(Of ForumTopicCount)
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            For Each item In cntx.SelectRows(Of ForumTopicCount)(
'                         " select t.forum_id, (select forum_name from forum where id = t.forum_id) as forum_name, count(*) as topic_count
'                                 from topic t
'                                group by t.forum_id ")
'                Yield item
'            Next
'        End Using
'    End Function

'    ''' <summary>
'    ''' Получить количество сообщений в топике
'    ''' </summary>
'    ''' <param name="TopicId">ID топика</param>
'    Public Function GetMessageCount(TopicId As Long) As Integer
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            Return CInt(cntx.ExecScalar(" select count(id)
'                                                from message m
'                                               where m.topic_id = @topic_id_i ", New With {.topic_id_i = TopicId}))
'        End Using
'    End Function



'    ''' <summary>
'    ''' Найти сообщения через полнотекстовый поиск
'    ''' </summary>
'    Public Function FindMessages(query As FindMessageQuery, Optional Limit As Integer = -1, Optional Offset As Integer = -1) As List(Of FindMessageResult)

'        ' Валидация параметров
'        ForumSearch.ValidateThrowMatchParameter(query)

'        ' Условия сортировки
'        Dim messageOrderClause = ForumSearch.BuildMessageOrderByParameter(query)
'        Dim topicOrderClause = ForumSearch.BuildTopicOrderByParameter(query)

'        ' Фильтрация по форумам
'        Dim forumWhereClause = ForumSearch.BuildWhereForumParameter(query)

'        ' Запросы
'        Dim queryMessage = $"   select f.id as forum_id, f.forum_name,
'                                           t.id as topic_id, t.topic_name, m.user_name,
'                                           m.id as message_id, m.date_created,
'                                           highlight(MessageFts, 1, '[search]', '[/search]') as searched_text
'                                    from Forum f
'                                    inner join Topic t on t.forum_id = f.id
'                                    inner join Message m on m.topic_id = t.id
'                                    inner join MessageFts mf on m.id = mf.id
'                                    where mf.MessageFts match @text_search_s
'                                    {forumWhereClause}
'                                    {messageOrderClause}
'                                    limit @limit_i
'                                    offset @offset_i"

'        Dim queryTopic = $"     select f.id as forum_id, f.forum_name,
'                                           t.id as topic_id, t.topic_name, t.user_name,
'                                           cast(null as integer) message_id, cast(null as date) date_created,
'                                           highlight(TopicFts, 1, '[search]', '[/search]') as searched_text
'                                    from Forum f
'                                    inner join Topic t on t.forum_id = f.id
'                                    inner join TopicFts tf on tf.rowid = t.id
'                                    where tf.TopicFts match @text_search_s
'                                    {forumWhereClause}
'                                    {topicOrderClause}
'                                    limit @limit_i
'                                    offset @offset_i"

'        ' Формируем параметры, если ищем сообщения
'        Dim paramsMessageList As New List(Of Object)
'        Dim paramsTopicList As New List(Of Object)

'        paramsMessageList.Add(ForumSearch.BuildMessageMatchParameter(query))
'        paramsTopicList.Add(ForumSearch.BuildTopicMatchParameter(query))

'        ' Добавить лимиты строк
'        paramsMessageList.Add(Limit)
'        paramsMessageList.Add(Offset)

'        paramsTopicList.Add(Limit)
'        paramsTopicList.Add(Offset)

'        Dim messages As New List(Of FindMessageResult)
'        Dim topics As New List(Of FindMessageResult)

'        ' Выполняем поиск
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))

'            If query.FromMessage OrElse query.FromUser Then
'                messages = cntx.SelectRows(Of FindMessageResult)(queryMessage, paramsMessageList.ToArray()).ToList()
'            End If

'            If query.FromTopic OrElse query.FromUser Then
'                topics = cntx.SelectRows(Of FindMessageResult)(queryTopic, paramsTopicList.ToArray()).ToList()
'            End If

'        End Using

'        Return messages.Union(topics).ToList()
'    End Function

'    ''' <summary>
'    ''' Получить список сообщений топика
'    ''' </summary>
'    ''' <param name="TopicId">ID Топика</param>
'    Public Iterator Function GetMessages(TopicId As Long, Optional Limit As Integer = -1, Optional Offset As Integer = -1) As IEnumerable(Of TopicMessageModel)
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            For Each item In cntx.SelectRows(Of TopicMessageModel)(
'                         " select m.id,
'                                      m.topic_id, t.topic_name,
'                                      m.date_created, m.message_text, m.user_id, m.user_name
'                                 from message m
'                           inner join topic t on t.id = m.topic_id
'                                where m.topic_id = @topic_id_i
'                                order by date_created asc
'                                limit @limit_i
'                               offset @offset_i ", TopicId, Limit, Offset)
'                Yield item
'            Next
'        End Using
'    End Function

'    ''' <summary>
'    ''' Получить список топиков по из выбранного форума
'    ''' </summary>
'    ''' <param name="ForumId">ID форума</param>
'    ''' <returns></returns>
'    Public Iterator Function GetTopics(ForumId As Long, Optional Limit As Integer = -1, Optional Offset As Integer = -1) As IEnumerable(Of ForumTopicModel)
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            For Each item In cntx.SelectRows(Of ForumTopicModel)(
'                         " select t.id, t.topic_name,
'                                      t.forum_id, f.forum_name,
'                                      (select min(date_created) from message where topic_id = t.id) as topic_created,
'                                      (select count(id) from message where topic_id = t.id)         as message_count,
'                                      (select max(date_created) from message where topic_id = t.id) as message_last_date
'                                 from topic t
'                           inner join forum f on f.id = t.forum_id
'                                where t.forum_id = @forum_id_i
'                             order by message_last_date desc
'                                limit @limit_i
'                               offset @offset_i ", ForumId, Limit, Offset)
'                Yield item
'            Next
'        End Using
'    End Function

'    ''' <summary>
'    ''' Получить список топиков из всех форумов
'    ''' </summary>
'    ''' <returns></returns>
'    Public Iterator Function GetTopics(Optional Limit As Integer = -1, Optional Offset As Integer = -1) As IEnumerable(Of ForumTopic)
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            For Each item In cntx.SelectRows(Of ForumTopic)(
'                         " select t.id as topic_id,
'                                      t.topic_name,
'                                      t.forum_id,
'                                      f.forum_name,
'                                      (select max(date_created) from message where topic_id = t.id) as message_last_date
'                                 from topic t
'                           inner join forum f on f.id = t.forum_id
'                                limit @limit_i
'                               offset @offset_i ", Limit, Offset)
'                Yield item
'            Next
'        End Using
'    End Function

'    ''' <summary>
'    ''' Получить количество топиков в форуме
'    ''' </summary>
'    ''' <param name="ForumId">ID форума</param>
'    ''' <returns></returns>
'    Public Function GetTopicCount(ForumId As Long) As Integer
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            Return CInt(cntx.ExecScalar(" select count(id)
'                                                from topic t
'                                               where t.forum_id = @forum_id_i ", ForumId))
'        End Using
'    End Function

'    ''' <summary>
'    ''' Получить количество всего топиков
'    ''' </summary>
'    ''' <returns></returns>
'    Public Function GetTopicCount() As Integer
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            Return CInt(cntx.ExecScalar(" select count(id) from topic t "))
'        End Using
'    End Function

'    ''' <summary>
'    ''' Получить список форумов
'    ''' </summary>
'    Public Iterator Function GetForums() As IEnumerable(Of Forum)
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            For Each item In cntx.SelectRows(Of Forum)("select * from forum")
'                Yield item
'            Next
'        End Using
'    End Function

'    ''' <summary>
'    ''' Найти топик по ID
'    ''' </summary>
'    Public Function GetTopicByID(TopicId As Long) As Topic
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            Return cntx.SelectRows(Of Topic)("select * from topic where id = @topic_id_i", TopicId).FirstOrDefault()
'        End Using
'    End Function

'    ''' <summary>
'    ''' Найти форум по ID
'    ''' </summary>
'    Public Function GetForumByID(ForumId As Long) As Forum
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            Return cntx.SelectRows(Of Forum)("select * from forum where id = @forum_id_i", ForumId).FirstOrDefault()
'        End Using
'    End Function

'    ''' <summary>
'    ''' Найти форумы по имени группы
'    ''' </summary>
'    Public Iterator Function GetForumsByGroupName(GroupName As String) As IEnumerable(Of Forum)
'        Using cntx As New SQLContext(New SQLiteConnection(dataSourceString))
'            For Each item In cntx.SelectRows(Of Forum)("select * from forum where group_name = @group_name_s", GroupName)
'                Yield item
'            Next
'        End Using
'    End Function

'End Class

'''' <summary>
'''' Варианты сортировки в поиске
'''' </summary>
'Public Enum FindMessageOrderBy
'    byDefault
'    byRank
'    byDate
'    byTopic
'    byUser
'End Enum

'''' <summary>
'''' Исключение, при неверно заданном поиске
'''' </summary>
'Public Class FindMessageWrongParameterException
'    Inherits Exception

'    Public Sub New(Message As String)
'        MyBase.New(Message)
'    End Sub

'    Public Sub New(Message As String, InnerException As Exception)
'        MyBase.New(Message, InnerException)
'    End Sub

'    Public Sub New()
'        MyBase.New()
'    End Sub
'End Class

'End Namespace