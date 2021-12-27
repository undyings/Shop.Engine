using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public interface ISearchIndex
  {
    MetaProperty Property { get; }
    void Add(Product product);
    int[] FindObjectIds(SearchCondition condition);
  }

  public class EnumSearchIndex : ISearchIndex
  {
    readonly object lockObj = new object();

    readonly Dictionary<string, List<int>> objectIdsByPropertyValue = new Dictionary<string, List<int>>();

    public void Add(Product product)
    {
      string value = product.Get(property.Blank);
      if (StringHlp.IsEmpty(value))
        return;

      List<int> objectIds;
      if (!objectIdsByPropertyValue.TryGetValue(value, out objectIds))
      {
        objectIds = new List<int>();
        objectIdsByPropertyValue[value] = objectIds;
      }

      objectIds.Add(product.Id);
    }

    readonly RawCache<string[]> sortedEnumVariantsCache;

    public string[] SortedEnumVariants
    {
      get { return sortedEnumVariantsCache.Result; }
    }

    public int[] FindObjectIds(SearchCondition condition)
    {
      if (condition is EnumSearchCondition)
      {
        EnumSearchCondition enumCondition = (EnumSearchCondition)condition;
        return FindObjectIds(enumCondition.Value);
      }

      if (condition is MultiEnumSearchCondition)
      {
        MultiEnumSearchCondition multiCondition = (MultiEnumSearchCondition)condition;
        return FindObjectIds(multiCondition.Values);
      }

      Logger.AddMessage("EnumSearchIndex: Неверный тип условия {0}, {1}", property.Id, condition);
      return new int[0];
    }

    public int[] FindObjectIds(string propertyValue)
    {
      List<int> objectIds = DictionaryHlp.GetValueOrDefault(objectIdsByPropertyValue, propertyValue);
      if (objectIds == null)
        return new int[0];
      return objectIds.ToArray();
    }

    public int[] FindObjectIds(string[] propertyValues)
    {
      Dictionary<int, bool> byObjectId = new Dictionary<int, bool>();
      foreach (string value in propertyValues)
      {
        int[] objectIds = FindObjectIds(value);
        foreach (int objectId in objectIds)
          byObjectId[objectId] = true;
      }

      return _.ToArray(byObjectId.Keys);
    }

    readonly MetaProperty property;
    public MetaProperty Property
    {
      get { return property; }
    }

    public EnumSearchIndex(MetaProperty property)
    {
      this.property = property;

      this.sortedEnumVariantsCache = new Cache<string[], int>(delegate
      {
        lock (lockObj)
        {
          string[] enumVariants = _.ToArray(objectIdsByPropertyValue.Keys);
          ArrayHlp.Sort(enumVariants, delegate (string s) { return s; });
					return enumVariants;
          //return ArrayHlp.Merge("", enumVariants);
        }
      },
        delegate
        {
          lock (lockObj)
            return 0;
        }
      );
    }
  }

  public class NumericalSearchIndex : ISearchIndex
  {
    readonly List<Tuple<int, decimal>> sortedObjects = new List<Tuple<int, decimal>>();
    decimal accuracy = 1;
    public decimal Accuracy
    {
      get { return accuracy; }
    }

    public void Add(Product product)
    {
      decimal? value = ConvertHlp.ToDecimal(product.Get(property.Blank));
      if (value == null)
        return;

      _.InsertInSortedList(sortedObjects, _.Tuple(product.Id, (decimal)value),
        delegate (Tuple<int, decimal> item) { return item.Item2; }
      );

      if (value.Value != decimal.Round(value.Value))
        accuracy = (decimal)0.1;
    }

    public decimal? Min
    {
      get
      {
        if (sortedObjects.Count == 0)
          return null;
        return sortedObjects[0].Item2;
      }
    }

    public decimal? Max
    {
      get
      {
        if (sortedObjects.Count == 0)
          return null;
        return sortedObjects[sortedObjects.Count - 1].Item2;
      }
    }

    public int[] FindObjectIds(SearchCondition condition)
    {
      return SearchHlp.FindObjectIds(property.Id, sortedObjects, condition as NumericalSearchCondition);
    }

    readonly MetaProperty property;
    public MetaProperty Property
    {
      get { return property; }
    }

    public NumericalSearchIndex(MetaProperty property)
    {
      this.property = property;
    }
  }
}
