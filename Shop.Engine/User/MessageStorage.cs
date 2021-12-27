using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class MessageStorage
  {
    readonly object lockObj = new object();

    public TableLink MessageLink
    {
      get
      {
        return messageLinkCache.Result;
      }
    }

    public TableLink RatingLink
    {
      get
      {
        return ratingLinkCache.Result;
      }
    }

    long messageChangeTick = 0;
    public void UpdateMessages()
    {
      lock (lockObj)
        messageChangeTick++;
    }

    long ratingChangeTick = 0;
    public void UpdateRatings()
    {
      lock (lockObj)
        ratingChangeTick++;
    }

    readonly RawCache<TableLink> messageLinkCache;
    readonly RawCache<TableLink> ratingLinkCache;

    public readonly IDataLayer UserConnection;
    public readonly int ArticleId;

    public MessageStorage(IDataLayer userConnection, int articleId)
    {
      this.UserConnection = userConnection;
      this.ArticleId = articleId;

      this.messageLinkCache = new Cache<TableLink, long>(
        delegate
        {
          lock (lockObj)
          {
            return MessageHlp.LoadMessageLink(UserConnection, ArticleId);
          }
        },
        delegate
        {
          lock (lockObj)
            return messageChangeTick;
        }
      );

      this.ratingLinkCache = new Cache<TableLink, long>(
        delegate
        {
          lock (lockObj)
          {
            return MessageHlp.LoadRatingLink(UserConnection, ArticleId);
          }
        },
        delegate
        {
          lock (lockObj)
            return ratingChangeTick;
        }
      );
    }
  }
}
