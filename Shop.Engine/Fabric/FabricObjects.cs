using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class LightGroup : LightKin
  {
    readonly object lockObj = new object();

    readonly RawCache<LightGroup[]> groupsCache;
    readonly RawCache<Product[]> productsCache;

    readonly ShopStorage store;
    public LightGroup(ShopStorage store, int groupId) :
      base(store.groupBox, groupId)
    {
      this.store = store;

      this.groupsCache = new Cache<LightGroup[], long>(
        delegate
        {
          int[] subgroupIds = AllChildIds(GroupType.SubgroupTypeLink);
          List<LightGroup> subgroups = new List<LightGroup>(subgroupIds.Length);
          foreach (int subgroupId in subgroupIds)
          {
            LightGroup subgroup = store.FindGroup(subgroupId);
            if (subgroup != null)
              subgroups.Add(subgroup);
          }
          LightGroup[] subgroupArray = subgroups.ToArray();
          ArrayHlp.Sort(subgroupArray, delegate (LightGroup subgroup)
          {
            return subgroup.Get(SEOProp.SortingPrefix) + GroupType.DisplayName(subgroup);
          });
          return subgroupArray;
        },
        delegate { return 0; }
      );

      this.productsCache = new Cache<Product[], long>(
        delegate
        {
          List<Product> products = new List<Product>();
          foreach (int fabricId in AllChildIds(GroupType.FabricTypeLink))
          {
            LightKin fabric = store.FindFabric(fabricId);
            if (fabric == null)
              continue;

            LightObject[] varieties = FabricHlp.GetVarieties(store, fabric);
            if (varieties.Length == 0)
            {
              Product fabricProduct = store.FindProduct(fabric.Id);
              if (fabricProduct != null)
                products.Add(fabricProduct);
              continue;
            }

            foreach (LightObject variety in varieties)
            {
              Product varietyProduct = store.FindProduct(variety.Id);
              if (varietyProduct != null)
                products.Add(varietyProduct);
            }
          }

          Product[] productArray = products.ToArray();
          ArrayHlp.Sort(productArray, delegate(Product product)
          {
            return product.Get(SEOProp.SortingPrefix) + product.ProductName;
          });

          return productArray;
        },
        delegate { return 0; }
      );
    }

    public LightGroup ParentGroup
    {
      get
      {
        int? parentId = GetParentId(GroupType.SubgroupTypeLink);
        return store.FindGroup(parentId);
      }
    }

    public LightGroup[] Subgroups
    {
      get
      {
        lock (lockObj)
          return groupsCache.Result;
      }
    }

    public Product[] Products
    {
      get
      {
        lock (lockObj)
          return productsCache.Result;
      }
    }
  }

  public class MetaKind : LightParent
  {
    public static string ToDisplayName(MetaKind kind)
    {
      if (kind == null)
        return "Обычный";
			return MetaKindType.DisplayName(kind); // kind.Get(MetaKindType.Identifier);
    }

    readonly ObjectBox propertyBox;

    public readonly MetaProperty[] Properties;

    public MetaKind(ParentBox kindBox, ObjectBox propertyBox, int kindId) :
      base(kindBox, kindId)
    {
      this.propertyBox = propertyBox;

      List<MetaProperty> propertyList = new List<MetaProperty>();
      foreach (int propertyId in AllChildIds(MetaKindType.PropertyLinks))
      {
        if (propertyBox.ObjectById.Exist(propertyId))
          propertyList.Add(new MetaProperty(propertyBox, propertyId));
      }
      this.Properties = propertyList.ToArray();
    }
  }

  public class MetaProperty : LightObject
  {
    //public static string ValueToDisplay(LightObject fabric, MetaProperty property)
    //{
    //  string value = fabric.Get(property.Blank);
    //  if (StringHlp.IsEmpty(value))
    //    return "";

    //  string measureUnit = property.Get(MetaPropertyType.MeasureUnit);
    //  if (StringHlp.IsEmpty(measureUnit))
    //    return value;

    //  return string.Format("{0} {1}", value, measureUnit);
    //}

    public readonly RowPropertyBlank<string> Blank;

    public MetaProperty(ObjectBox propertyBox, int propertyId) :
      base(propertyBox, propertyId)
    {
      this.Blank = DataBox.Create(propertyId, DataBox.StringValue);
    }
  }

  public class LightFeature : LightParent
  {
    public readonly LightObject[] FeatureValues;

    readonly Dictionary<int, LightObject> featureValueById = new Dictionary<int, LightObject>();
    public LightObject FindValue(int featureValueId)
    {
      return DictionaryHlp.GetValueOrDefault(featureValueById, featureValueId);
    }

    public LightFeature(ParentBox featureBox, ObjectBox featureValueBox, int featureId) :
      base(featureBox, featureId)
    {
      List<LightObject> valueList = new List<LightObject>();
      foreach (int valueId in this.AllChildIds(FeatureType.FeatureValueLinks))
      {
        if (!featureValueBox.ObjectById.Exist(valueId))
          continue;

        LightObject featureValue = new LightObject(featureValueBox, valueId);
        valueList.Add(featureValue);
      }

      LightObject[] valueArray = valueList.ToArray();
      ArrayHlp.Sort(valueArray, delegate (LightObject value)
        { return value.Get(FeatureValueType.DisplayName); }
      );

      this.FeatureValues = valueArray;

      foreach (LightObject featureValue in FeatureValues)
        featureValueById[featureValue.Id] = featureValue;
    }
  }
}
