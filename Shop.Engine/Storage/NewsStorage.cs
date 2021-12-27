using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class NewsStorage
  {
    public static NewsStorage Load(IDataLayer fabricConnection, int actualNewsCount)
    {
      ObjectBox actualNewsBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(NewsType.News) +
        " order by act_from desc " + SQLiteHlp.LimitCondition(0, actualNewsCount)
      );

      ObjectHeadBox headNewsBox = new ObjectHeadBox(fabricConnection,
        DataCondition.ForTypes(NewsType.News)
      );

      return new NewsStorage(actualNewsBox, headNewsBox);
    }

    public readonly ObjectBox actualNewsBox;
    public readonly ObjectHeadBox headNewsBox;

    public NewsStorage(ObjectBox actualNewsBox, ObjectHeadBox headNewsBox)
    {
      this.actualNewsBox = actualNewsBox;
      this.headNewsBox = headNewsBox;

      this.Actual = ArrayHlp.Convert(actualNewsBox.AllObjectIds, delegate (int newsId)
      {
        return new LightObject(actualNewsBox, newsId);
      });
    }

    public readonly LightObject[] Actual;

    public void FillLinks(TranslitLinks links)
    {
      foreach (int newsId in headNewsBox.AllObjectIds)
      {
        links.AddLink(Site.Novosti, newsId, NewsType.Title.Get(headNewsBox, newsId),
          headNewsBox.ObjectById.Any(ObjectType.ActTill, newsId)
        );
      }
    }
  }
}
