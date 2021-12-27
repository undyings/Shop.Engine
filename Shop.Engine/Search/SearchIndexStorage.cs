using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class SearchIndexStorage
  {
    public readonly int KindId;

    readonly Dictionary<int, ISearchIndex> searchIndexByPropertyId = new Dictionary<int, ISearchIndex>();
    public readonly ISearchIndex[] AllSearchIndices;

    public readonly ISearchIndex[] PriorSearchIndices;
    public readonly ISearchIndex[] MinorSearchIndices;

    public int[] FindObjectIds(SearchCondition[] conditions)
    {
      Dictionary<int, int> entryCountByObjectId = new Dictionary<int, int>();

      int dueEntryCount = -1;
      foreach (SearchCondition condition in conditions)
      {
        dueEntryCount++;

        ISearchIndex index = DictionaryHlp.GetValueOrDefault(searchIndexByPropertyId, condition.PropertyId);

        if (index == null)
          return new int[0];

        int[] objectIds = index.FindObjectIds(condition);
        if (objectIds.Length == 0)
          return new int[0];

        foreach (int objectId in objectIds)
        {
          int entryCount;
          entryCountByObjectId.TryGetValue(objectId, out entryCount);
          if (entryCount == dueEntryCount)
            entryCountByObjectId[objectId] = dueEntryCount + 1;
        }        
      }

      List<int> foundObjectIds = new List<int>();
      foreach (int objectId in entryCountByObjectId.Keys)
      {
        if (entryCountByObjectId[objectId] == conditions.Length)
          foundObjectIds.Add(objectId);
      }
      return foundObjectIds.ToArray();
    }

    public SearchIndexStorage(int kindId, IEnumerable<ISearchIndex> indices)
    {
      this.KindId = kindId;
      this.AllSearchIndices = _.ToArray(indices);

      List<ISearchIndex> findedIndices = _.FindAll(AllSearchIndices, delegate (ISearchIndex index)
        { return index.Property.Get(MetaPropertyType.IsPrior); });
      this.PriorSearchIndices = findedIndices.ToArray();

      this.MinorSearchIndices = _.FindAll(AllSearchIndices, delegate(ISearchIndex index)
        { return !index.Property.Get(MetaPropertyType.IsPrior); }).ToArray();

      foreach (ISearchIndex index in indices)
      {
        searchIndexByPropertyId[index.Property.Id] = index;
      }
    }
  }

	public class SearchTune
	{
		public readonly string DisplayName;
		public readonly string PropertyKind;
		public readonly string IndexType;

		public SearchTune(string displayName, string propertyKind, string indexType)
		{
			this.DisplayName = displayName;
			this.PropertyKind = propertyKind;
			this.IndexType = indexType;
		}
	}

	public class SearchTunes
	{
		readonly Dictionary<string, SearchTune> indexTypeByPropertyKind = new Dictionary<string, SearchTune>();

		public SearchTune[] All
		{
			get
			{
				return _.ToArray(indexTypeByPropertyKind.Values);
			}
		}

		public SearchTunes(params SearchTune[] tunes)
		{
			foreach (SearchTune tune in tunes)
			{
				indexTypeByPropertyKind[tune.PropertyKind] = tune;
			}
		}

		public SearchTune FindIndexType(string propertyKind)
		{
			return DictionaryHlp.GetValueOrDefault(indexTypeByPropertyKind, propertyKind);
		}
	}
}
