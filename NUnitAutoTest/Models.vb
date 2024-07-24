Imports VSProject.SQLContext.Attributes

<Serializable>
Public Class Forum

    <PrimaryKey>
    <Column("id")>
    Public Property Id As Long

    <Column("forum_name")>
    Public Property ForumName As String

    <Column("group_name")>
    Public Property GroupName As String

End Class

<Serializable>
Public Class ForumTopic

    <Column("topic_id")>
    Public Property TopicId As Long

    <Column("topic_name")>
    Public Property TopicName As String

    <Column("forum_id")>
    Public Property ForumId As Long

    <Column("forum_name")>
    Public Property ForumName As String

    <Column("message_last_date")>
    Public Property MessageLastDate As Date


End Class

<Serializable>
Public Class ForumTopicCount

    <PrimaryKey>
    <Column("forum_id")>
    Public Property ForumId As Long

    <Column("forum_name")>
    Public Property ForumName As String

    <Column("topic_count")>
    Public Property TopicCount As Long

End Class

<Serializable>
Public Class ForumTopicModel

    <Column("id")>
    Public Property Id As Long

    <Column("topic_name")>
    Public Property TopicName As String

    <Column("forum_id")>
    Public Property ForumId As Long

    <Column("forum_name")>
    Public Property ForumName As String

    <Column("topic_created")>
    Public Property TopicCreated As Date

    <Column("message_count")>
    Public Property MessageCount As Long

    <Column("message_last_date")>
    Public Property MessageLastDate As Date

End Class

<Serializable>
Public Class Message

    <PrimaryKey>
    <Column("id")>
    Public Property Id As Long

    <Column("topic_id")>
    Public Property TopicId As Long

    <Column("user_id")>
    Public Property UserId As Long

    <Column("date_created")>
    Public Property DateCreated As Date

    <Column("message_text")>
    Public Property MessageText As String

End Class

<Serializable>
Public Class Topic

    <PrimaryKey>
    <Column("id")>
    Public Property Id As Long

    <Column("forum_id")>
    Public Property ForumId As Long

    <Column("user_id")>
    Public Property UserId As Long

    <Column("topic_name")>
    Public Property TopicName As String

End Class

<Serializable>
Public Class TopicMessageModel

    <PrimaryKey>
    <Column("id")>
    Public Property Id As Long

    <Column("topic_id")>
    Public Property TopicId As Long

    <Column("topic_name")>
    Public Property TopicName As String

    <Column("user_id")>
    Public Property UserId As Long

    <Column("user_name")>
    Public Property UserName As String

    <Column("date_created")>
    Public Property DateCreated As Date

    <Column("message_text")>
    Public Property MessageText As String

End Class

<Serializable>
Public Class User

    <PrimaryKey>
    <Column("id")>
    Public Property Id As Long

    <Column("user_name")>
    Public Property UserName As String

End Class
