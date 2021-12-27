using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine
{
  public class SearchFilter
  {
    readonly Dictionary<int, SearchCondition> conditionByPropertyId = new Dictionary<int, SearchCondition>();

    public SearchCondition[] Conditions
    {
      get { return _.ToArray(conditionByPropertyId.Values); }
    }

    public SearchCondition FindCondition(int propertyId)
    {
      return DictionaryHlp.GetValueOrDefault(conditionByPropertyId, propertyId);
    }

    public void SetCondition(SearchCondition condition)
    {
      conditionByPropertyId[condition.PropertyId] = condition;
    }

    public SearchFilter(params SearchCondition[] conditions)
    {
      foreach (SearchCondition condition in conditions)
      {
        conditionByPropertyId[condition.PropertyId] = condition;
      }
    }
  }

}
