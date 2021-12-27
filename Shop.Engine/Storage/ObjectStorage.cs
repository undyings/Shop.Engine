using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class ObjectStorage
  {
    public readonly ObjectBox box;

    readonly Dictionary<int, LightObject> objectById = new Dictionary<int, LightObject>();

    public LightObject Find(int? objectId)
    {
      if (objectId == null)
        return null;

      return DictionaryHlp.GetValueOrDefault(objectById, objectId.Value);
    }

    public readonly LightObject[] All;

    public ObjectStorage(ObjectBox box) :
      this(box, delegate(LightObject obj)
      {
        return obj.Get(SEOProp.SortingPrefix) ?? "" + obj.Get(SEOProp.Identifier) ?? ""; ;
      })
    {
    }

    public ObjectStorage(ObjectBox box, Getter<string, LightObject> sorter)
    {
      this.box = box;

      foreach (int objectId in box.AllObjectIds)
      {
        objectById[objectId] = new LightObject(box, objectId);
      }

      this.All = _.SortBy(objectById.Values, sorter).ToArray();
    }
  }
}
