using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class UserStorage
  {
    readonly object lockObj = new object();

    public LightObject FindUser(int userId)
    {
      Dictionary<int, LightObject> userById = storageCache.Result.Item1;

      return DictionaryHlp.GetValueOrDefault(userById, userId);
    }

    public LightObject[] All
    {
      get { return _.ToArray(storageCache.Result.Item1.Values); }
    }

    public LightObject FindUser(string xmlLogin)
    {
      if (StringHlp.IsEmpty(xmlLogin))
        return null;

      Dictionary<string, LightObject> userByXmlLogin = storageCache.Result.Item2;

      return DictionaryHlp.GetValueOrDefault(userByXmlLogin, xmlLogin);
    }

    long changeTick = 0;
    public void Update()
    {
      lock (lockObj)
        changeTick++;
    }

    readonly RawCache<Tuple<Dictionary<int, LightObject>, Dictionary<string, LightObject>>> storageCache;

    readonly IDataLayer userConnection;

    public UserStorage(IDataLayer userConnection)
    {
      this.userConnection = userConnection;

      this.storageCache = new Cache<Tuple<Dictionary<int, LightObject>, Dictionary<string, LightObject>>, long>(
        delegate
        {
          lock (lockObj)
          {
            ObjectBox userBox = new ObjectBox(userConnection, DataCondition.ForTypes(UserType.User));
            int[] allObjectIds = userBox.AllObjectIds;

            Dictionary<int, LightObject> userById = new Dictionary<int, LightObject>(allObjectIds.Length);
            Dictionary<string, LightObject> userByXmlLogin = new Dictionary<string, LightObject>(allObjectIds.Length);

            foreach (int userId in allObjectIds)
            {
              LightObject user = new LightObject(userBox, userId);
              userById[userId] = user;

              string xmlLogin = user.Get(ObjectType.XmlObjectIds);
              if (!StringHlp.IsEmpty(xmlLogin))
                userByXmlLogin[xmlLogin] = user;
            }

            return _.Tuple(userById, userByXmlLogin);
          }
        },
        delegate
        {
          lock (lockObj)
            return changeTick;
        }
      );
    }
  }
}
