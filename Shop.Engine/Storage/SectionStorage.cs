using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class SectionStorage
  {
    public static SectionStorage Load(IDataLayer fabricConnection)
    {
      KinBox sectionBox = new KinBox(fabricConnection, DataCondition.ForTypes(SectionType.Section));
      KinBox unitBox = new KinBox(fabricConnection, DataCondition.ForTypes(UnitType.Unit));

      return new SectionStorage(sectionBox, unitBox);
    }

    public readonly KinBox sectionBox;
    public readonly KinBox unitBox;

    readonly Dictionary<int, LightSection> sectionById = new Dictionary<int, LightSection>();
    public LightSection FindSection(int? sectionId)
    {
      if (sectionId == null)
        return null;
      return DictionaryHlp.GetValueOrDefault(sectionById, sectionId.Value);
    }

    readonly Dictionary<int, LightUnit> unitById = new Dictionary<int, LightUnit>();
    public LightUnit FindUnit(int? unitId)
    {
      if (unitId == null)
        return null;
      return DictionaryHlp.GetValueOrDefault(unitById, unitId.Value);
    }

    public SectionStorage(KinBox sectionBox, KinBox unitBox)
    {
      this.sectionBox = sectionBox;
      this.unitBox = unitBox;

      foreach (int pageId in sectionBox.AllObjectIds)
      {
        LightSection page = new LightSection(this, pageId);
        sectionById[pageId] = page;

        if (page.IsMenu)
        {
          menuByKind[page.Get(SectionType.Title)] = page;
        }

        if (page.Get(SectionType.DesignKind) == "contact")
          this.ContactPage = page;
      }

      foreach (int unitId in unitBox.AllObjectIds)
      {
        LightUnit unit = new LightUnit(this, unitId);
        unitById[unitId] = unit;
      }
    }

    //hack для 2gis
    public readonly LightObject ContactPage = null;

    readonly Dictionary<string, LightSection> menuByKind = new Dictionary<string, LightSection>();
    public LightSection FindMenu(string kind)
    {
      if (StringHlp.IsEmpty(kind))
        return null;
      return DictionaryHlp.GetValueOrDefault(menuByKind, kind);
    }

    public LightSection[] AllMenu
    {
      get { return _.ToArray(menuByKind.Values); }
    }

    public void FillLinks(TranslitLinks links)
    {
      foreach (LightSection menu in AllMenu)
      {
        // hack
        bool isNews = Site.AddFolderForNews && menu.Get(SectionType.Title) == "news";
        foreach (LightSection section in menu.Subsections)
        {
          FillLinks(links, section, isNews);
        }
      }
    }

    static void FillLinks(TranslitLinks links, LightSection section, bool isNews)
    {
      string directory;
      string translit = TranslitLinks.TranslitString(section.Get(SectionType.Title));
      if (isNews)
      {
        if (section.ParentSection.IsMenu)
          translit = "";
        directory = UrlHlp.ToDirectory(Site.Novosti, translit);
      }
      else
      {
        if (Site.DirectPageLinks)
          directory = UrlHlp.ToDirectory("", translit);
        else
          directory = UrlHlp.ToDirectory("page", translit);
      }

      DateTime? modifyTime = section.Get(ObjectType.ActTill);
      foreach (LightUnit unit in section.AllUnits)
      {
        DateTime? unitModifyTime = unit.Get(ObjectType.ActTill);
        if (modifyTime == null || unitModifyTime > modifyTime)
          modifyTime = unitModifyTime;
      }

      foreach (LightUnit unit in section.Units)
      {
        DateTime? unitModifyTime = unit.Get(ObjectType.ActTill);
        if (modifyTime == null || unitModifyTime > modifyTime)
          modifyTime = unitModifyTime;
      }

      links.AddDirectory("page", section.Id, directory, modifyTime);
      foreach (LightSection subsection in section.Subsections)
      {
        FillLinks(links, subsection, isNews);
      }
    }
  }
}
