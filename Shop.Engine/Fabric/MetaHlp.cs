using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Commune.Basis;
using Commune.Data;
using Commune.Html;

namespace Shop.Engine
{
  public class MetaHlp
  {
    public static void ReserveDiapasonForMetaProperty(IDataLayer fabricConnection)
    {
      //hack резервируем диапазон для захардкоденных свойств
      {
        object rawMaxId = fabricConnection.GetScalar("",
          "select max_primary_key from light_primary_key where table_name = 'light_object'");
        int? maxId = DatabaseHlp.ConvertToInt(rawMaxId);
        if (maxId == null || maxId < 100000)
        {
          fabricConnection.GetScalar("",
            "update light_primary_key set max_primary_key = 100000 where table_name = 'light_object'");
          Logger.AddMessage("Зарезервирован диапазон для захардкоденных свойств");
        }
      }
    }

    public static string GetCategoryName(ShopStorage store, LightObject property)
    {
      LightObject categoryObject = store.FindPropertyCategory(property.Get(MetaPropertyType.Category));
      return categoryObject?.Get(MetaCategoryType.DisplayName);
    }

    public static string PropertyToDisplay(ShopStorage store, LightObject property)
    {
      StringBuilder builder = new StringBuilder();

      string category = GetCategoryName(store, property);
      if (!StringHlp.IsEmpty(category))
      {
        builder.Append(category);
        builder.Append(" / ");
      }

      bool isPrior = property.Get(MetaPropertyType.IsPrior);

			string name = property.Get(SEOProp.Name);
			string identifier = property.Get(MetaPropertyType.Identifier);

			if (isPrior)
        builder.Append("<strong>");
      builder.Append(StringHlp.IsEmpty(name) ? identifier : name);
      if (isPrior)
        builder.Append("</strong>");
      if (!StringHlp.IsEmpty(name))
        builder.AppendFormat(" ({0})", identifier);

      return builder.ToString();
    }

    public static LightObject[] SortProperties(ShopStorage store, int[] propertyIds)
    {
      LightObject[] properties = ArrayHlp.Convert(propertyIds, delegate(int propertyId)
        {
          return new LightObject(store.fabricPropertyBox, propertyId);
        }
      );

      ArrayHlp.Sort(properties, delegate(LightObject property)
      {
          return new object[] {
            GetCategoryName(store, property),
						MetaPropertyType.DisplayName(property)
            //property.Get(MetaPropertyType.Identifier),
            //property.Get(MetaPropertyType.Marking)
          };
        },
        _.ArrayComparison
      );

      return properties;
    }
  }
}
