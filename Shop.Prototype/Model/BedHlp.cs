using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Basis;
using Commune.Data;
using Shop.Engine;

namespace Shop.Prototype
{
  public class BedHlp
  {
    public static int CalcBedPrice(LightObject fabric, LightObject cloth, LightObject width)
    {
      int price = fabric.Get(FabricType.Price);
      if (cloth != null)
      {
        int categoryId = cloth.Get(FeatureValueType.WithFeatures, 0);
        price = fabric.Get(FabricType.FeatureMarkups, categoryId);
      }

      if (width != null)
        price += fabric.Get(FabricType.FeatureMarkups, width.Id);

      return price;
    }

    public static string GetProductName(ShopStorage shop, int productId, VirtualRowLink option)
    {
      Product product = shop.FindProduct(productId);
      if (product == null)
        return "";

      MetaKind kind = shop.FindFabricKind(product.Get(FabricType.Kind));
      if (kind == null || kind.Get(MetaKindType.DesignKind) != "bed")
        return product.ProductName;

      LightFeature clothFeature = shop.FindFeature("Ткань");
      LightFeature widthFeature = shop.FindFeature("Ширина кровати");
      LightFeature lengthFeature = shop.FindFeature("Длина кровати");

      LightObject cloth = clothFeature.FindValue(option.Get(OptionType.SelectClothId));
      LightObject width = widthFeature.FindValue(option.Get(OptionType.SelectWidthId));
      LightObject length = lengthFeature.FindValue(option.Get(OptionType.SelectLengthId));

      return string.Format("{0}, {1}x{2}, {3}", 
        product.ProductName, width?.Get(FeatureValueType.DisplayName),
        length?.Get(FeatureValueType.DisplayName), cloth?.Get(FeatureValueType.DisplayName)
      );
    }

    public static Tuple<int, string>[] GetComboItems(string allDisplay, LightObject[] featureValues)
    {
      return ArrayHlp.Merge(new Tuple<int, string>[] { _.Tuple(0, allDisplay) },
        ArrayHlp.Convert(featureValues, delegate (LightObject value)
          {
            return _.Tuple(value.Id, value.Get(FeatureValueType.DisplayName));
          }
        )
      );
    }

    public static Dictionary<int, List<LightObject>> FindClothValues(ShopStorage shop, Product product,
      int filterColorId, int filterMaterialId)
    {
      int[] filterColorIds = filterColorId < 1 ? null : new int[] { filterColorId };
      int[] filterMaterialIds = filterMaterialId < 1 ? null : new int[] { filterMaterialId };

      LightFeature clothFeature = shop.FindFeature("Ткань");

      LightFeature categoryFeature = shop.FindFeature("Категория ткани");
      LightFeature materialFeature = shop.FindFeature("Материал ткани");
      LightFeature colorFeature = shop.FindFeature("Цвет");

      List<LightObject> findClothValues = new List<LightObject>();
      foreach (LightObject clothValue in clothFeature.FeatureValues)
      {
        int colorId = clothValue.Get(FeatureValueType.WithFeatures, 2);
        int materialId = clothValue.Get(FeatureValueType.WithFeatures, 1);

        if (filterColorIds != null && !filterColorIds.Contains(colorId))
          continue;

        if (filterMaterialIds != null && !filterMaterialIds.Contains(materialId))
          continue;

        findClothValues.Add(clothValue);
      }

      List<LightObject> sortClothValues = _.SortBy(findClothValues, delegate (LightObject clothValue)
        {
          int categoryId = clothValue.Get(FeatureValueType.WithFeatures, 0);
          LightObject category = categoryFeature.FindValue(categoryId);

          return new object[] { category?.Get(FeatureValueType.DisplayName),
            clothValue.Get(FeatureValueType.DisplayName)
          };
        },
        _.ArrayComparison
      );

      return _.GroupBy(sortClothValues, delegate (LightObject clothValue)
        {
          return clothValue.Get(FeatureValueType.WithFeatures, 0);
        }
      );
    }
  }
}