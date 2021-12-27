using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using System.IO;
using Commune.Html;
using NitroBolt.Wui;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class SectionHlp
  {
    public static LightSection FindSection(IStore store, string menuKind, string designKind)
    {
      LightSection menu = store.Sections.FindMenu(menuKind);
      if (menu == null)
        return null;

      return FindSection(menu, designKind);
    }

    public static LightSection FindSection(LightSection menu, string designKind)
    {
      return _.Find(menu.Subsections, designKind, delegate (LightSection section)
        { return section.Get(SectionType.DesignKind); }
      );
    }

    public static LightSection[] GetSectionBranch(SectionStorage sectionStorage, int sectionId)
    {
      List<LightSection> branch = new List<LightSection>();
      FillSectionBranch(branch, sectionStorage, sectionId);
      return branch.ToArray();
    }

    static void FillSectionBranch(List<LightSection> branch, SectionStorage sectionStorage, int sectionId)
    {
      LightSection section = sectionStorage.FindSection(sectionId);
      if (section == null)
        return;

      branch.Insert(0, section);

      LightSection parent = section.ParentSection;
      if (parent != null)
        FillSectionBranch(branch, sectionStorage, parent.Id);
    }

    public static LightSection[] FindGroupsForMoveSection(SectionStorage sectionStorage, 
      string groupDesignKind, LightSection moveSection)
    {
      LightSection[] branch = GetSectionBranch(sectionStorage, moveSection.Id);

      List<LightSection> groupsForMove = new List<LightSection>();
      if (branch.Length > 1)
      {
        groupsForMove.Add(branch[0]);
        FillGroupsForMove(groupsForMove, groupDesignKind, moveSection.Id, branch[0]);
      }

      return groupsForMove.ToArray();
    }

    static void FillGroupsForMove(List<LightSection> groupsForMove, 
      string groupDesignKind, int moveSectionId, LightSection parentSection)
    {
      foreach (LightSection section in parentSection.Subsections)
      {
        if (section.Id == moveSectionId)
          continue;

        if (section.Get(SectionType.DesignKind) != groupDesignKind)
          continue;

        groupsForMove.Add(section);
        FillGroupsForMove(groupsForMove, groupDesignKind, moveSectionId, section);
      }
    }
  }
}
