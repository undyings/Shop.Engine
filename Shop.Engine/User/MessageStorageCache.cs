using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class MessageStorageCache
  {
    readonly object lockObj = new object();

    readonly Dictionary<int, MessageStorage> messageStorageByArticleId = new Dictionary<int, MessageStorage>();

    public MessageStorage ForArticle(int articleId)
    {
      lock (lockObj)
      {
        MessageStorage storage;
        if (!messageStorageByArticleId.TryGetValue(articleId, out storage))
        {
          storage = new MessageStorage(userConnection, articleId);
          messageStorageByArticleId[articleId] = storage;
        }
        return storage;
      }
    }

    readonly IDataLayer userConnection;
    public MessageStorageCache(IDataLayer userConnection)
    {
      this.userConnection = userConnection;
    }
  }
}
