using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine
{
  public abstract class SearchCondition
  {
    public readonly int PropertyId;

    public SearchCondition(int propertyId)
    {
      this.PropertyId = propertyId;
    }
  }

  public class MultiEnumSearchCondition : SearchCondition
  {
    public readonly string[] Values;
    public MultiEnumSearchCondition(int propertyId, params string[] values) :
      base(propertyId)
    {
      this.Values = values;
    }
  }

  public class EnumSearchCondition : SearchCondition
  {
    public readonly string Value;

    public EnumSearchCondition(int propertyId, string value) :
      base(propertyId)
    {
      this.Value = value;
    }
  }

  public class NumericalSearchCondition : SearchCondition
  {
    public readonly decimal? Min;
    public readonly decimal? Max;

    public NumericalSearchCondition(int propertyId, decimal? min, decimal? max) :
      base(propertyId)
    {
      this.Min = min;
      this.Max = max;
    }
  }
}
