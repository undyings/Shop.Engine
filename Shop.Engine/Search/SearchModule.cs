using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class SearchModule
  {
    public static SearchModule Create(IDataLayer fabricConnection, SearchTunes tunes, MetaKind[] kinds)
    {
      List<SearchIndexStorage> storages = new List<SearchIndexStorage>();

      foreach (MetaKind kind in kinds)
      {
        List<ISearchIndex> indices = new List<ISearchIndex>();

        //hack подмешиваем цену в фильтр
        MetaProperty priceProperty = SearchHlp.CreatePriceProperty(fabricConnection);
        indices.Add(new NumericalSearchIndex(priceProperty));

        foreach (MetaProperty property in kind.Properties)
        {
          string propertyKind = property.Get(MetaPropertyType.Kind);
					SearchTune tune = tunes.FindIndexType(propertyKind);

					string type = tune?.IndexType;

          if (type == "enum")
            indices.Add(new EnumSearchIndex(property));
          else if (type == "numerical")
            indices.Add(new NumericalSearchIndex(property));
        }

        //indices.Add(new PriceSearchIndex());

        SearchIndexStorage storage = new SearchIndexStorage(kind.Id, indices);
        storages.Add(storage);
      }

      return new SearchModule(storages);
    }

    public void Fill(IEnumerable<Product> products)
    {
      foreach (Product product in products)
      {
        int? kindId = product.Get(FabricType.Kind);
        if (kindId == null)
          continue;

        SearchIndexStorage storage = FindIndexStorage(kindId.Value);
        if (storage == null)
          continue;

        foreach (ISearchIndex index in storage.AllSearchIndices)
        {
          index.Add(product);
        }
      }
    }

    readonly Dictionary<int, SearchIndexStorage> indexStorageByKindId = new Dictionary<int, SearchIndexStorage>();

    public SearchModule(IEnumerable<SearchIndexStorage> storages)
    {
      foreach (SearchIndexStorage storage in storages)
        indexStorageByKindId[storage.KindId] = storage;
    }

    public SearchIndexStorage FindIndexStorage(int? kindId)
    {
      if (kindId == null)
        return null;
      return DictionaryHlp.GetValueOrDefault(indexStorageByKindId, kindId.Value);
    }
  }
}
