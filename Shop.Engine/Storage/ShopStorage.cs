using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class ShopStorage
  {
    public static ShopStorage Load(IDataLayer fabricConnection)
    {
      ParentBox fabricKindBox = new ParentBox(fabricConnection,
        DataCondition.ForTypes(MetaKindType.FabricKind)
      );

      ObjectBox fabricPropertyBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(MetaPropertyType.Property)
      );

      ObjectBox propertyCategoryBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(MetaCategoryType.PropertyCategory)
      );

      KinBox groupBox = new KinBox(fabricConnection,
        DataCondition.ForTypes(GroupType.Group));
      KinBox fabricBox = new KinBox(fabricConnection,
        DataCondition.ForTypes(FabricType.Fabric));
      ObjectBox varietyBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(VarietyType.Variety));

      ParentBox featureBox = new ParentBox(fabricConnection, DataCondition.ForTypes(FeatureType.Feature));
      ObjectBox featureValueBox = new ObjectBox(fabricConnection, DataCondition.ForTypes(FeatureValueType.FeatureValue));

      return new ShopStorage(fabricKindBox, fabricPropertyBox, propertyCategoryBox,
        groupBox, fabricBox, varietyBox, featureBox, featureValueBox);
    }

    public readonly ParentBox fabricKindBox;
    public readonly ObjectBox fabricPropertyBox;
    public readonly ObjectBox propertyCategoryBox;

    public readonly KinBox groupBox;
    public readonly KinBox fabricBox;
    public readonly ObjectBox varietyBox;

    public readonly ParentBox featureBox;
    public readonly ObjectBox featureValueBox;

    public ShopStorage(
      ParentBox fabricKindBox, ObjectBox fabricPropertyBox, ObjectBox propertyCategoryBox,
      KinBox groupBox, KinBox fabricBox, ObjectBox varietyBox,
      ParentBox qualityBox, ObjectBox qualityValueBox)
    {
      this.fabricKindBox = fabricKindBox;
      this.fabricPropertyBox = fabricPropertyBox;
      this.propertyCategoryBox = propertyCategoryBox;
      this.groupBox = groupBox;
      this.fabricBox = fabricBox;
      this.varietyBox = varietyBox;
      this.featureBox = qualityBox;
      this.featureValueBox = qualityValueBox;

      foreach (int kindId in fabricKindBox.AllObjectIds)
        fabricKindById[kindId] = new MetaKind(fabricKindBox, fabricPropertyBox, kindId);
      this.AllFabricKindIds = ArrayHlp.Merge((int?)null,
        ArrayHlp.Convert(fabricKindBox.AllObjectIds, delegate (int id) { return (int?)id; })
      );

      foreach (int categoryId in propertyCategoryBox.AllObjectIds)
        propertyCategoryById[categoryId] = new LightObject(propertyCategoryBox, categoryId);
      this.AllPropertyCategoryIds = ArrayHlp.Merge((int?)null,
        ArrayHlp.Convert(propertyCategoryBox.AllObjectIds, delegate (int id) { return (int?)id; })
      );

      foreach (int fabricId in fabricBox.AllObjectIds)
      {
        fabricById[fabricId] = new LightKin(fabricBox, fabricId);
      }

      foreach (int varietyId in varietyBox.AllObjectIds)
      {
        varietyById[varietyId] = new LightObject(varietyBox, varietyId);
      }

      foreach (int featureId in featureBox.AllObjectIds)
      {
        LightFeature feature = new LightFeature(featureBox, featureValueBox, featureId);
        featureByCode[feature.Get(FeatureType.Code) ?? ""] = feature;
      }

      foreach (LightKin fabric in fabricById.Values)
      {
        LightObject[] varieties = FabricHlp.GetVarieties(this, fabric);
        if (varieties.Length == 0)
        {
          productById[fabric.Id] = new Product(fabricBox, fabric.Id, null);
          continue;
        }

        foreach (LightObject variety in varieties)
        {
          productById[variety.Id] = new Product(fabricBox, fabric.Id, variety);
        }
      }

      DateTime productsModifyTime = FabricHlp.RefTime;
      foreach (Product product in productById.Values)
      {
        if (product.ModifyTime > productsModifyTime)
          productsModifyTime = product.ModifyTime;
      }
      this.ProductsModifyTime = productsModifyTime;

      foreach (int groupId in groupBox.AllObjectIds)
      {
        groupById[groupId] = new LightGroup(this, groupId);
      }

      List<LightGroup> rootGroups = new List<LightGroup>();
      foreach (LightGroup group in groupById.Values)
      {
        if (group.GetParentId(GroupType.SubgroupTypeLink) == null)
          rootGroups.Add(group);
      }
      this.RootGroups = rootGroups.ToArray();
      ArrayHlp.Sort(RootGroups, delegate (LightGroup group)
      {
        return group.Get(SEOProp.SortingPrefix) + GroupType.DisplayName(group);
      });
    }

    static void FillLinks(TranslitLinks links, LightGroup parentGroup, LightGroup group)
    {
      links.AddShopLinks(parentGroup, group);

      foreach (LightGroup subgroup in group.Subgroups)
        FillLinks(links, group, subgroup);
    }

    public void FillLinks(TranslitLinks links)
    {
      foreach (LightGroup group in RootGroups)
      {
        FillLinks(links, null, group);
      }

      //foreach (Product product in productById.Values)
      //{
      //  links.AddLink("product", product.ProductId, product.ProductName, product.ModifyTime);
      //}

      links.AddLink("catalog", null);

      //foreach (LightKin group in groupById.Values)
      //{
      //  links.AddLink("group", group.Id, group.Get(GroupType.DisplayName),
      //    MathHlp.Max(group.Get(ObjectType.ActTill) ?? FabricHlp.RefTime, ProductsModifyTime)
      //  );
      //}
    }

    public readonly DateTime ProductsModifyTime;

    public LightGroup FindGroup(int? groupId)
    {
      if (groupId == null)
        return null;

      return DictionaryHlp.GetValueOrDefault(groupById, groupId.Value);
    }

    public LightKin FindFabric(int? fabricId)
    {
      if (fabricId == null)
        return null;

      return DictionaryHlp.GetValueOrDefault(fabricById, fabricId.Value);
    }

    public LightObject FindVariety(int varietyId)
    {
      return DictionaryHlp.GetValueOrDefault(varietyById, varietyId);
    }

    public Product FindProduct(int? productId)
    {
      if (productId == null)
        return null;
      return DictionaryHlp.GetValueOrDefault(productById, productId.Value);
    }

    public IEnumerable<Product> AllProducts
    {
      get
      {
        return productById.Values;
      }
    }

    readonly Dictionary<int, MetaKind> fabricKindById = new Dictionary<int, MetaKind>();
    public MetaKind[] AllFabricKinds
    {
      get { return _.ToArray(fabricKindById.Values); }
    }
    public readonly int?[] AllFabricKindIds;
    public MetaKind FindFabricKind(int? fabricKindId)
    {
      if (fabricKindId == null)
        return null;

      return DictionaryHlp.GetValueOrDefault(fabricKindById, fabricKindId.Value);
    }

		public MetaProperty FindProperty(string identificator)
		{
			string xmlIds = MetaPropertyType.Identifier.CreateXmlIds(identificator);

			RowLink propertyRow = fabricPropertyBox.ObjectByXmlIds.AnyRow(xmlIds);
			int? propertyId = propertyRow?.Get(ObjectType.ObjectId);
			if (propertyId == null)
				return null;

			return new MetaProperty(fabricPropertyBox, propertyId.Value);
		}

    readonly Dictionary<int, LightObject> propertyCategoryById = new Dictionary<int, LightObject>();
    public readonly int?[] AllPropertyCategoryIds;
    public LightObject FindPropertyCategory(int? categoryId)
    {
      if (categoryId == null)
        return null;

      return DictionaryHlp.GetValueOrDefault(propertyCategoryById, categoryId.Value);
    }

    readonly Dictionary<int, LightGroup> groupById = new Dictionary<int, LightGroup>();
    readonly Dictionary<int, LightKin> fabricById = new Dictionary<int, LightKin>();
    readonly Dictionary<int, LightObject> varietyById = new Dictionary<int, LightObject>();
    public readonly LightGroup[] RootGroups;

    readonly Dictionary<int, Product> productById = new Dictionary<int, Product>();

    readonly Dictionary<string, LightFeature> featureByCode = new Dictionary<string, LightFeature>();
    public LightFeature FindFeature(string code)
    {
      if (StringHlp.IsEmpty(code))
        return null;

      return DictionaryHlp.GetValueOrDefault(featureByCode, code);
    }
  }
}
