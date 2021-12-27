using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class LightSection : LightKin
  {
    readonly object lockObj = new object();

    readonly RawCache<Tuple<LightSection[], LightKin[]>> subpagesCache;

    readonly RawCache<Dictionary<int, LightUnit>> unitForPaneCache;

    readonly SectionStorage store;
    public LightSection(SectionStorage store, int pageId) :
      base(store.sectionBox, pageId)
    {
      this.store = store;

      this.subpagesCache = new Cache<Tuple<LightSection[], LightKin[]>, long>(
        delegate
        {
          int[] subpageIds = AllChildIds(SectionType.SubsectionLinks);
          List<LightSection> subpages = new List<LightSection>(subpageIds.Length);
          foreach (int subpageId in subpageIds)
          {
            LightSection subpage = store.FindSection(subpageId);
            if (subpage != null)
              subpages.Add(subpage);
          }
          LightSection[] subpageArray = SorterHlp.Sort(subpages, this.Get(SectionType.SortKind));

          int[] unitIds = AllChildIds(SectionType.UnitLinks);
          List<LightKin> units = new List<LightKin>(unitIds.Length);
          foreach (int unitId in unitIds)
          {
            LightKin unit = store.FindUnit(unitId);
            if (unit != null)
              units.Add(unit);
          }

          LightKin[] unitArray = SorterHlp.Sort(units, this.Get(SectionType.UnitSortKind));

          return _.Tuple(subpageArray, unitArray);
        },
        delegate { return 0; }
      );

      this.unitForPaneCache = new Cache<Dictionary<int, LightUnit>, long>(
        delegate
        {
          int[] unitIds = AllChildIds(SectionType.UnitForPaneLinks);
          Dictionary<int, LightUnit> unitForPane = new Dictionary<int, LightUnit>(unitIds.Length);
          foreach (RowLink link in AllChildRows(SectionType.UnitForPaneLinks))
          {
            int unitId = link.Get(LinkType.ChildId);
            int paneIndex = link.Get(LinkType.LinkIndex);
            LightUnit unit = store.FindUnit(unitId);
            if (unit != null)
              unitForPane[paneIndex] = unit;
          }
          return unitForPane;
        },
        delegate { return 0; }
      );
    }

    public bool IsMenu
    {
      get { return GetParentId(SectionType.SubsectionLinks) == null; }
    }

    public LightSection ParentSection
    {
      get
      {
        int? parentId = GetParentId(SectionType.SubsectionLinks);
        return store.FindSection(parentId);
      }
    }

    public LightSection[] Subsections
    {
      get
      {
        lock (lockObj)
          return subpagesCache.Result.Item1;
      }
    }

    public LightUnit UnitForPane(int paneIndex)
    {
      lock (lockObj)
        return DictionaryHlp.GetValueOrDefault(unitForPaneCache.Result, paneIndex);
    }

    public LightUnit[] AllUnits
    {
      get
      {
        lock (lockObj)
          return _.ToArray(unitForPaneCache.Result.Values);
      }
    }

    [Obsolete]
    public LightKin[] Units
    {
      get
      {
        lock (lockObj)
          return subpagesCache.Result.Item2;
      }
    }

    [Obsolete]
    public LightKin Unit
    {
      get
      {
        lock (lockObj)
        {
          LightKin[] units = subpagesCache.Result.Item2;
          if (units.Length == 0)
            return null;
          return units[0];
        }
      }
    }

    public string NameInMenu
    {
      get
      {
        string menuName = this.Get(SectionType.NameInMenu);
        if (!StringHlp.IsEmpty(menuName))
          return menuName;
        return this.Get(SectionType.Title);
      }
    }
  }
}
