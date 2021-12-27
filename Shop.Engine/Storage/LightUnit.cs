using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class LightUnit : LightKin
  {
    readonly object lockObj = new object();

    readonly RawCache<LightUnit[]> subunitsCache;

    readonly SectionStorage store;
    public LightUnit(SectionStorage store, int unitId) :
      base(store.unitBox, unitId)
    {
      this.store = store;

      this.subunitsCache = new Cache<LightUnit[], long>(
        delegate
        {
          int[] subunitIds = AllChildIds(UnitType.SubunitLinks);

          List<LightUnit> subunits = new List<LightUnit>(subunitIds.Length);
          foreach (int subunitId in subunitIds)
          {
            LightUnit subunit = store.FindUnit(subunitId);
            if (subunit != null)
              subunits.Add(subunit);
          }

          LightUnit[] unitArray = SorterHlp.Sort(subunits, this.Get(UnitType.SortKind));

          //List<LightKin> subunits = new List<LightKin>(subunitIds.Length);
          //foreach (int subunitId in subunitIds)
          //{
          //  LightKin subunit = store.FindUnit(subunitId);
          //  if (subunit != null)
          //    subunits.Add(subunit);
          //}

          //LightKin[] unitArray = SorterHlp.Sort(subunits, this.Get(UnitType.SortKind));

          return unitArray;
        },
        delegate { return 0; }
      );
    }

    public LightUnit[] Subunits
    {
      get
      {
        lock (lockObj)
          return subunitsCache.Result;
      }
    }
  }
}
