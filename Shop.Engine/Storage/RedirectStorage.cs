using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class RedirectStorage
  {
    public static RedirectStorage Load(IDataLayer fabricConnection)
    {
      ObjectBox redirectBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(RedirectType.Redirect) +
        " order by xml_ids asc"
      );

      return new RedirectStorage(redirectBox);
    }

    readonly Dictionary<string, LightObject> redirectByUrl = new Dictionary<string, LightObject>();
    readonly Dictionary<int, LightObject> redirectById = new Dictionary<int, LightObject>();
    public readonly LightObject[] All;

    public LightObject Find(string url)
    {
      if (StringHlp.IsEmpty(url))
        return null;

      return DictionaryHlp.GetValueOrDefault(redirectByUrl, url.ToLower());
    }

    public LightObject Find(int? deadLinkId)
    {
      if (deadLinkId == null)
        return null;

      return DictionaryHlp.GetValueOrDefault(redirectById, deadLinkId.Value);
    }

    public readonly ObjectBox redirectBox;

    public RedirectStorage(ObjectBox redirectBox)
    {
      this.redirectBox = redirectBox;

      foreach (int redirectId in redirectBox.AllObjectIds)
      {
        LightObject redirect = new LightObject(redirectBox, redirectId);

        redirectById[redirectId] = redirect;

        string deadUrl = redirect.Get(RedirectType.From);
        if (StringHlp.IsEmpty(deadUrl))
          continue;
        redirectByUrl[deadUrl.ToLower()] = redirect;
      }

      this.All = _.ToArray(redirectByUrl.Values);
    }
  }
}
