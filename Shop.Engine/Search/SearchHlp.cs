using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;
using Commune.Html;
using NitroBolt.Wui;

namespace Shop.Engine
{
  public class SearchHlp
  {
    public static string[] AllSortKinds = new string[] { "", "cheap", "pricey" };
    public static string SortKindToDisplay(string kind)
    {
      switch (kind)
      {
        case "cheap":
          return "Сначала дешевые";
        case "pricey":
          return "Сначала дорогие";
        default:
          return "По наименованию";        
      }
    }

    public static List<Product> FindProducts(SearchModule search, ShopStorage shop,
      int kindId, SearchFilter filter)
    {
      SearchIndexStorage storage = search.FindIndexStorage(kindId);
      if (storage == null)
        return new List<Product>();

      int[] productIds = storage.FindObjectIds(filter.Conditions);
      List<Product> findedProducts = new List<Product>(productIds.Length);
      foreach (int productId in productIds)
      {
        Product product = shop.FindProduct(productId);
        if (product != null)
          findedProducts.Add(product);
      }
      return findedProducts;
    }

    public static SearchFilter GetFilterFromJson2(SearchIndexStorage storage, JsonData json)
    {
      List<SearchCondition> conditions = new List<SearchCondition>();
      foreach (ISearchIndex index in storage.AllSearchIndices)
      {
        int propertyId = index.Property.Id;

        if (index is EnumSearchIndex)
        {
          EnumSearchIndex enumIndex = (EnumSearchIndex)index;
          List<string> selectedValues = new List<string>();
          for (int i = 0; i < enumIndex.SortedEnumVariants.Length; ++i)
          {
            bool selected = json.GetText(string.Format("property_{0}_{1}", propertyId, i))?.ToLower() == "true";
            if (selected)
              selectedValues.Add(enumIndex.SortedEnumVariants[i]);
          }

          if (selectedValues.Count == 0)
            continue;

          conditions.Add(new MultiEnumSearchCondition(propertyId, selectedValues.ToArray()));
        }
        else if (index is NumericalSearchIndex)
        {
          NumericalSearchIndex numericalIndex = (NumericalSearchIndex)index;

          string value = json.GetText(string.Format("property_{0}", propertyId));
          if (StringHlp.IsEmpty(value))
            continue;

          string[] minmax = value.Split(';');
          decimal? min = ConvertHlp.ToDecimal(minmax[0]);
          decimal? max = ConvertHlp.ToDecimal(minmax[1]);

          if (min == numericalIndex.Min && max == numericalIndex.Max)
            continue;

          conditions.Add(new NumericalSearchCondition(propertyId, min, max));
        }
      }

      return new SearchFilter(conditions.ToArray());
    }

    public static SearchFilter GetFilterFromJson(SearchIndexStorage storage, JsonData json)
    {
      List<SearchCondition> conditions = new List<SearchCondition>();
      foreach (ISearchIndex index in storage.AllSearchIndices)
      {
        int propertyId = index.Property.Id;

        if (index is EnumSearchIndex)
        {
          string value = json.GetText(string.Format("property_{0}", propertyId));
          if (StringHlp.IsEmpty(value))
            continue;

          conditions.Add(new EnumSearchCondition(propertyId, value));
        }
        else if (index is NumericalSearchIndex)
        {
          NumericalSearchIndex numericalIndex = (NumericalSearchIndex)index;

          string value = json.GetText(string.Format("property_{0}", propertyId));
          if (StringHlp.IsEmpty(value))
            continue;

          string[] minmax = value.Split(';');
          decimal? min = ConvertHlp.ToDecimal(minmax[0]);
          decimal? max = ConvertHlp.ToDecimal(minmax[1]);

          if (min == numericalIndex.Min && max == numericalIndex.Max)
            continue;

          //decimal? min = ConvertHlp.ToDecimal(
          //  json.GetText(string.Format("property_{0}_min", propertyId))
          //);
          //decimal? max = ConvertHlp.ToDecimal(
          //  json.GetText(string.Format("property_{0}_max", propertyId))
          //);
          //if (min == null && max == null)
          //  continue;

          conditions.Add(new NumericalSearchCondition(propertyId, min, max));
        }
      }

      return new SearchFilter(conditions.ToArray());
    }

    public static void FilterToLandingPage(SearchIndexStorage storage,
      SearchFilter filter, LightObject landingPage)
    {
      foreach (ISearchIndex index in storage.AllSearchIndices)
      {
        RowPropertyBlank<string> blank = index.Property.Blank;
        SearchCondition condition = filter.FindCondition(blank.Kind);
        if (condition == null)
        {
          landingPage.RemoveProperty(blank, 1);
          landingPage.RemoveProperty(blank, 0);
          continue;
        }

        if (condition is EnumSearchCondition)
        {
          string value = ((EnumSearchCondition)condition).Value;
          landingPage.Set(blank, value);
        }
        else if (condition is NumericalSearchCondition)
        {
          NumericalSearchCondition numerical = (NumericalSearchCondition)condition;
          landingPage.Set(blank, 0, DatabaseHlp.DecimalToString(numerical.Min));
          landingPage.Set(blank, 1, DatabaseHlp.DecimalToString(numerical.Max));
        }
      }
    }

    public static SearchFilter FilterFromLandingPage(SearchIndexStorage storage, LightObject landingPage)
    {
      if (storage == null)
        return null;

      List<SearchCondition> conditions = new List<SearchCondition>();
      foreach (ISearchIndex index in storage.AllSearchIndices)
      {
        string type = index.Property.Get(MetaPropertyType.Kind);
        if (type == "enum")
        {
          string value = landingPage.Get(index.Property.Blank);
          if (!StringHlp.IsEmpty(value))
            conditions.Add(new EnumSearchCondition(index.Property.Id, value));
        }
        else if (type == "numerical")
        {
          decimal? min = ConvertHlp.ToDecimal(landingPage.Get(index.Property.Blank, 0));
          decimal? max = ConvertHlp.ToDecimal(landingPage.Get(index.Property.Blank, 1));
          if (min != null || max != null)
            conditions.Add(new NumericalSearchCondition(index.Property.Id, min, max));
        }
      }

      if (conditions.Count == 0)
        return null;

      return new SearchFilter(conditions.ToArray());
    }

    public static int[] FindObjectIds(int propertyId, List<Tuple<int, decimal>> sortedObjects,
      NumericalSearchCondition condition)
    {
      if (condition == null)
      {
        Logger.AddMessage("EnumSearchIndex: Неверный тип условия {0}", propertyId);
        return new int[0];
      }

      int minIndex;
      int count;
      _.GetDiapason(sortedObjects,
        delegate (Tuple<int, decimal> item) { return item.Item2; },
        condition.Min, condition.Max,
        out minIndex, out count
      );

      return ArrayHlp.Convert(_.GetRange(sortedObjects, minIndex, count),
        delegate (Tuple<int, decimal> item) { return item.Item1; });
    }

    public static MetaProperty CreatePriceProperty(IDataLayer dbConnection)
    {
      ObjectBox box = new ObjectBox(dbConnection, "1=0", true);
      RowLink propertyRow = box.CreateObjectRow(MetaPropertyType.Property,
        MetaPropertyType.Identifier.CreateXmlIds("Цена"), null);
      int propertyId = FabricType.Price.Kind;
      propertyRow.Set(ObjectType.ObjectId, propertyId);
      box.ObjectById.TableLink.AddRow(propertyRow);

      MetaProperty property = new MetaProperty(box, propertyId);
      property.Set(MetaPropertyType.IsPrior, true);
      property.Set(MetaPropertyType.MeasureUnit, "руб");
      property.Set(MetaPropertyType.Kind, "numerical");

      return property;
    }

  }
}
