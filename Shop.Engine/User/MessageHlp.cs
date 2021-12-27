using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class MessageHlp
  {
    public static void DeleteMessage(IDataLayer messageConnection, int messageId)
    {
      messageConnection.GetScalar("", "Delete From message Where id = @messageId",
        new DbParameter("messageId", messageId)
      );
    }

    public static void DeleteTopicMessages(IDataLayer messageConnection, int topicId)
    {
      messageConnection.GetScalar("", "Delete From message Where article_id = @topicId",
        new DbParameter("topicId", topicId)
      );
    }

    public static string GetRatingCaption(TableLink ratingLink, int messageId,
      out int plusCount, out int minusCount, out string tooltip)
    {
      plusCount = ratingLink.FindRows(
        RatingType.RatingsByMessageIdAndValue, messageId, 1).Length;
      minusCount = ratingLink.FindRows(
        RatingType.RatingsByMessageIdAndValue, messageId, -1).Length;

      tooltip = "";
      if (plusCount != 0 || minusCount != 0)
        tooltip = string.Format("+{0} | -{1}", plusCount, minusCount);

      return minusCount > plusCount ? (plusCount - minusCount).ToString() :
        string.Format("+{0}", plusCount - minusCount);
    }

    public static string MessageTimeToString(DateTime? messageUtcTime)
    {
      if (messageUtcTime == null)
        return "Время неизвестно";

      DateTime localTime = messageUtcTime.Value.ToLocalTime();
      DateTime currentTime = DateTime.Now;

      if (currentTime - localTime < TimeSpan.FromMinutes(1))
        return "Только что";

      if (currentTime - localTime < TimeSpan.FromHours(1))
      {
        int minuteCount = (int)((currentTime - localTime).TotalMinutes);
        return string.Format("{0} {1} назад", minuteCount, 
          StringHlp.GetMeasureUnitWithEnding(minuteCount, "минут", "а", "ы", "")
        );
      }

      if (currentTime.Year == localTime.Year && currentTime.Month == localTime.Month)
      {
        if (currentTime.Day == localTime.Day)
          return string.Format("Сегодня, {0}", localTime.ToString("HH:mm"));

        if (currentTime.Day - 1 == localTime.Day)
          return string.Format("Вчера, {0}", localTime.ToString("HH:mm"));
      }

      if (currentTime.Year == localTime.Year)
        return localTime.ToString("dd MMMM, HH:mm");

      return localTime.ToString("dd MMMM yyyy, HH:mm");
    }

    public static TableLink LoadMessageLink(IDataLayer messageConnection, int topicId)
    {
      return LoadMessageLink(messageConnection, topicId, "order by create_time asc");
    }

    public static TableLink LoadMessageLink(IDataLayer messageConnection, int topicId, string sort)
    {
      return LoadMessageLink(messageConnection,
        string.Format("article_id = @articleId {0}", sort),
        new DbParameter("articleId", topicId)
      );
    }

    public static TableLink LoadMessageLink(IDataLayer messageConnection, 
      string conditionWithoutWhere, params DbParameter[] conditionParameters)
    {
      return TableLink.Load(messageConnection,
        new FieldBlank[]
        {
          MessageType.Id, MessageType.ArticleId, MessageType.UserId,
          MessageType.WhomId, MessageType.Content,
          MessageType.CreateTime, MessageType.ModifyTime,
          MessageType.PlusCounter, MessageType.MinusCounter, MessageType.Xml
        },
        new IndexBlank[]
        {
          MessageType.MessageById,
          MessageType.MessagesByUserId
        }, "",
        "Select id, article_id, user_id, whom_id, content, create_time, modify_time, plus_counter, minus_counter, xml From message",
        conditionWithoutWhere,
        conditionParameters
      );
    }

    public static TableLink LoadRatingLink(IDataLayer messageConnection, int topicId)
    {
      return TableLink.Load(messageConnection,
        new FieldBlank[]
        {
          RatingType.Id, RatingType.ArticleId, RatingType.MessageId,
          RatingType.UserId, RatingType.Value, RatingType.CreateTime
        },
        new IndexBlank[]
        {
          RatingType.RatingById,
          RatingType.RatingsByMessageId,
          RatingType.RatingsByMessageIdAndValue,
          RatingType.RatingByMessageIdAndUserId
        }, "",
        "Select id, article_id, message_id, user_id, value, create_time From rating",
        "article_id = @articleId order by create_time desc",
        new DbParameter("articleId", topicId)
      );
    }

    public static void InsertMessage(IDataLayer messageConnection,
      int topicId, int userId, int? whomId, string content)
    {
      DateTime createTime = DateTime.UtcNow;

      messageConnection.GetScalar("",
        "INSERT INTO message (article_id, user_id, whom_id, content, create_time, modify_time) VALUES (@articleId, @userId, @whomId, @content, @createTime, @modifyTime);",
        new DbParameter("articleId", topicId), new DbParameter("userId", userId),
        new DbParameter("whomId", whomId), new DbParameter("content", content),
        new DbParameter("createTime", createTime), new DbParameter("modifyTime", createTime)
      );
    }

    public static void InsertRating(IDataLayer messageConnection,
      int topicId, int messageId, int userId, int value)
    {
      messageConnection.GetScalar("",
        "INSERT INTO rating (article_id, message_id, user_id, value, create_time) VALUES (@articleId, @messageId, @userId, @value, @createTime);",
        new DbParameter("articleId", topicId), new DbParameter("messageId", messageId),
        new DbParameter("userId", userId), new DbParameter("value", value),
        new DbParameter("createTime", DateTime.UtcNow)
      );
    }

    public static void CheckAndCreateMessageTables(IDataLayer messageConnection)
    {
      if (!SQLiteDatabaseHlp.TableExist(messageConnection, "message"))
      {
        CreateTableForMessages(messageConnection);
        Logger.AddMessage("Создана таблица сообщений");
      }

      if (!SQLiteDatabaseHlp.TableExist(messageConnection, "rating"))
      {
        CreateTableForRatings(messageConnection);
        Logger.AddMessage("Создана таблица оценок");
      }
    }

    public static void CreateTableForMessages(IDataLayer messageConnection)
    {
      messageConnection.GetScalar("",
        @"CREATE TABLE message (
            id    integer PRIMARY KEY AUTOINCREMENT,
            article_id    integer NOT NULL,
            user_id       integer NOT NULL,
            whom_id       integer,
            content       text,
            create_time   datetime NOT NULL,
            modify_time   datetime NOT NULL,
            plus_counter  integer NOT NULL DEFAULT 0,
            minus_counter integer NOT NULL DEFAULT 0,
            xml           text
          );

          CREATE INDEX message_by_article_create_time
            ON message
            (article_id, create_time);

          CREATE INDEX message_by_user_create_time
            ON message
            (user_id, create_time);

          CREATE INDEX message_by_article_plus_counter
            ON message
            (article_id, plus_counter);
         "
      );
    }

    public static void CreateTableForRatings(IDataLayer messageConnection)
    {
      messageConnection.GetScalar("",
        @"CREATE TABLE rating (
            id    integer PRIMARY KEY AUTOINCREMENT,
            article_id   integer NOT NULL,
            message_id   integer NOT NULL,
            user_id   integer NOT NULL,
            value     integer NOT NULL,
            create_time  datetime NOT NULL
          );

          CREATE INDEX rating_by_article_create_time
            ON rating
            (article_id, create_time);

          CREATE INDEX rating_by_article_message_create_time
            ON rating
            (article_id, message_id, create_time);

          CREATE UNIQUE INDEX rating_by_article_message_user_id
            ON rating
            (article_id, message_id, user_id);

          CREATE INDEX rating_by_user_create_time
            ON rating
            (user_id, create_time);
         "
      );
    }
  }
}
