using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroBolt.Wui;
using Commune.Html;
using Commune.Data;
using Commune.Basis;
using System.IO;

namespace Shop.Engine
{
  public class UnitEditorHlp
  {
    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    static IStore store
    {
      get { return SiteContext.Default.Store; }
    }

    public static void CheckAndCreatePane(LightSection section, int paneIndex, string designKind)
    {
      if (section.UnitForPane(paneIndex) != null)
        return;

      KinBox box = new KinBox(fabricConnection, "1=0");
      //int? createUnitId = box.CreateUniqueObject(UnitType.Unit,
      //  UnitType.ParentId.CreateXmlIds(section.Id, ""), null);
      //if (createUnitId == null)
      //{
      //  Logger.AddMessage("Не удалось создать Pane: {0}, {1}", section.Id, paneIndex);
      //  return;
      //}

      int createUnitId = box.CreateObject(UnitType.Unit, UnitType.ParentId.CreateXmlIds(section.Id, ""), null);

      LightKin addUnit = new LightKin(box, createUnitId);
      FabricHlp.SetCreateTime(addUnit);

      addUnit.Set(SectionType.DesignKind, designKind);

      addUnit.SetParentId(SectionType.UnitForPaneLinks, paneIndex, section.Id);

      addUnit.Box.Update();

      SiteContext.Default.UpdateStore();
    }

    [Obsolete]
    public static void CheckAndCreateSingleUnit(string menuName, string designKind)
    {
      CheckAndCreateUnit(store.Sections.FindMenu(menuName), designKind, 0);
    }

    [Obsolete]
    public static LightKin CheckAndCreateUnit(LightSection section, string designKind, int unitIndex)
    {
      if (section.Units.Length > unitIndex)
        return section.Units[unitIndex];

      KinBox box = new KinBox(fabricConnection, "1=0");
      int? createUnitId = box.CreateUniqueObject(UnitType.Unit,
        UnitType.ParentId.CreateXmlIds(section.Id, ""), null);
      if (createUnitId == null)
      {
        Logger.AddMessage("Не удалось создать Unit: {0}, {1}", section.Id, unitIndex);
        return null;
      }

      LightKin addUnit = new LightKin(box, createUnitId.Value);
      FabricHlp.SetCreateTime(addUnit);

      addUnit.Set(SectionType.DesignKind, designKind);

      addUnit.SetParentId(SectionType.UnitLinks, unitIndex, section.Id);

      addUnit.Box.Update();

      SiteContext.Default.UpdateStore();

      return store.Sections.FindSection(section.Id).Units[unitIndex];
    }

    public static IHtmlControl GetUnitAdd(EditorSelector selector,
      EditState state, string title, LightKin parent, string fixedDesignKind)
    {
      LightSection parentSection = parent is LightSection ? (LightSection)parent :
        FabricHlp.ParentSectionForUnit(store.Sections, parent);
      string returnUrl = UrlHlp.ReturnUnitUrl(parentSection, parent.Id);
      //string returnUrl = parent.IsMenu ? "/" : UrlHlp.ShopUrl("page", parent.Id);

      string[] allKinds = selector.AllKinds;
      if (!StringHlp.IsEmpty(fixedDesignKind) && allKinds.Contains(fixedDesignKind))
        allKinds = new string[] { fixedDesignKind };

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field(
            new HLabel("Тип элемента").FontBold(),
            new HComboEdit<string>("designKind", "",
              delegate (string kind)
              {
                return selector.GetDisplayName(kind);
              },
              allKinds
            )
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить").Event("add_unit", "addContent",
            delegate (JsonData json)
            {
              string sectionName = json.GetText("name");
              if (StringHlp.IsEmpty(sectionName))
              {
                state.Operation.Message = "Не задан заголовок элемента";
                return;
              }

              string designKind = json.GetText("designKind");

              KinBox box = new KinBox(fabricConnection, "1=0");
              int? createUnitId = box.CreateUniqueObject(UnitType.Unit,
                UnitType.ParentId.CreateXmlIds(parent.Id, ""), null);
              if (createUnitId == null)
              {
                state.Operation.Message = "Элемент с таким заголовком уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              LightKin addUnit = new LightKin(box, createUnitId.Value);
              FabricHlp.SetCreateTime(addUnit);

              addUnit.Set(SectionType.DesignKind, designKind);

              addUnit.AddParentId(UnitType.SubunitLinks, parent.Id);

              addUnit.Box.Update();

              state.CreatingObjectId = addUnit.Id;

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("addContent");
    }

    [Obsolete]
    public static IHtmlControl GetGalleryImagesEdit(EditState state, LightKin unit)
    {
      int? parentId = unit.GetParentId(SectionType.UnitLinks);
      string returnUrl = UrlHlp.ReturnUnitUrl(store.Sections.FindSection(parentId), unit.Id);

      return new HPanel(
        DecorEdit.Title("Редактирование изображений галереи"),
        GetDeletePanel(state, unit),
        new HPanel(
          DecorEdit.FieldArea("Наименование галереи",
            new HTextArea("imageAlt", unit.Get(UnitType.DisplayName)).Height("2em")
          ).MarginLeft(5),
          DecorEdit.FieldInputBlock("Аннотация",
            HtmlHlp.CKEditorCreate("annotation", unit.Get(UnitType.Annotation),
              "200px", true)
          ).MarginLeft(5),
          EditElementHlp.GetDescriptionImagesPanel(state.Option, unit.Id)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_unit", "editContent",
            delegate (JsonData json)
            {
              string imageAlt = json.GetText("imageAlt");
              if (StringHlp.IsEmpty(imageAlt))
              {
                state.Operation.Message = "Не задано наименование галереи";
                return;
              }

              string annotation = json.GetText("annotation");

              LightObject editUnit = DataBox.LoadObject(fabricConnection, UnitType.Unit, unit.Id);

              if (!UnitType.DisplayName.SetWithCheck(editUnit.Box, editUnit.Id, imageAlt))
              {
                state.Operation.Message = "На этой странице уже существует элемент с таким же наименованием";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editUnit.Set(UnitType.Annotation, annotation);

              editUnit.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    //public static IHtmlControl GetDoubleContentEdit(EditState state, LightKin unit)
    //{
    //  return GetMultiContentEdit(state, unit, 2);
    //}

    //public static IHtmlControl GetMultiContentEdit(EditState state, LightKin unit, int contentCount)
    //{
    //  int? parentId = unit.GetParentId(SectionType.UnitLinks);
    //  string returnUrl = UrlHlp.ReturnUnitUrl(store.Sections.FindSection(parentId));

    //  List<IHtmlControl> controls = new List<IHtmlControl>();
    //  controls.Add(
    //    DecorEdit.Field("Заголовок", "header", unit.Get(UnitType.DisplayName))
    //  );

    //  for (int i = 0; i < contentCount; ++i)
    //  {
    //    controls.Add(
    //      DecorEdit.FieldArea(string.Format("Содержимое {0}", i + 1),
    //        new HTextArea(string.Format("content{0}", i), unit.Get(UnitType.Content, i)).Height("4em")
    //      )
    //    );
    //  }

    //  return new HPanel(
    //    DecorEdit.Title("Редактирование элемента страницы"),
    //    GetDeletePanel(state, unit),
    //    new HPanel(
    //      controls.ToArray()
    //    ).Margin(0, 10),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.SaveButton()
    //      .CKEditorOnUpdateAll()
    //      .Event("save_unit", "editContent",
    //        delegate (JsonData json)
    //        {
    //          string header = json.GetText("header");
    //          if (StringHlp.IsEmpty(header))
    //          {
    //            state.Operation.Message = "Не задан заголовок";
    //            return;
    //          }

    //          LightObject editUnit = DataBox.LoadObject(fabricConnection, UnitType.Unit, unit.Id);

    //          if (!UnitType.DisplayName.SetWithCheck(editUnit.Box, editUnit.Id, header))
    //          {
    //            state.Operation.Message = "На этой странице уже существует элемент с таким заголовком";
    //            return;
    //          }

    //          if (HttpContext.Current.IsInRole("nosave"))
    //          {
    //            state.Operation.Message = "Нет прав на сохранение изменений";
    //            return;
    //          }

    //          for (int i = 0; i < contentCount; ++i)
    //          {
    //            string content = json.GetText(string.Format("content{0}", i));
    //            editUnit.Set(UnitType.Content, i, content);
    //          }

    //          editUnit.Box.Update();

    //          SiteContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("editContent");
    //}

    [Obsolete]
    public static IHtmlControl GetTextEdit(EditState state, LightKin unit)
    {
      int? parentId = unit.GetParentId(SectionType.UnitLinks);
      string returnUrl = UrlHlp.ReturnUnitUrl(store.Sections.FindSection(parentId), unit.Id);


      return new HPanel(
        DecorEdit.Title("Редактирование элемента страницы"),
        GetDeletePanel(state, unit),
        new HPanel(
          DecorEdit.Field("Заголовок", "header", unit.Get(UnitType.DisplayName))
            .MarginBottom(20),
          DecorEdit.FieldInputBlock("Текст",
            HtmlHlp.CKEditorCreate("content", unit.Get(UnitType.Content),
              "400px", true)
          ),
          EditElementHlp.GetDescriptionImagesPanel(state.Option, unit.Id)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_unit", "editContent",
            delegate (JsonData json)
            {
              string header = json.GetText("header");
              string content = json.GetText("content");

              LightObject editUnit = DataBox.LoadObject(fabricConnection, UnitType.Unit, unit.Id);

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              if (!UnitType.DisplayName.SetWithCheck(editUnit.Box, editUnit.Id, header))
              {
                state.Operation.Message = "На этой странице уже существует элемент с таким же заголовком";
                return;
              }

              editUnit.Set(UnitType.Content, content);

              editUnit.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetEditor(EditState state, LightKin unit, BaseTunes tunes)
    {
      bool hideTile = !tunes.GetTune("Tile");
      bool hideSortTime = !tunes.GetTune("SortTime");
			bool hideAdaptTitle = !tunes.GetTune("AdaptTitle");
			bool hideAdaptImage = !tunes.GetTune("AdaptImage");
      bool hideImageAlt = !tunes.GetTune("ImageAlt");
      bool hideTag1 = !tunes.GetTune("Tag1");
      bool hideTag2 = !tunes.GetTune("Tag2");
      bool hideSubtitle = !tunes.GetTune("Subtitle");
      bool hideAnnotation = !tunes.GetTune("Annotation");
      bool hideContent = !tunes.GetTune("Content");
      bool hideLink = !tunes.GetTune("Link");
      bool hideGallery = !tunes.GetTune("Gallery");
      bool hideSubunits = !tunes.GetTune("Subunits");
      bool hideSortKind = !tunes.GetTune("SortKind");

			LightSection parentSection = FabricHlp.ParentSectionForUnit(store.Sections, unit);
      string returnUrl = UrlHlp.ReturnUnitUrl(parentSection, unit.Id);
      //Logger.AddMessage("Parent: {0}, {1}, {2}", unit.Id, parentSection != null, returnUrl);

      return new HPanel(
        DecorEdit.Title("Редактирование элемента страницы"),
        GetDeletePanel(state, unit),
        new HPanel(
          DecorEdit.Field("Заголовок элемента", "title", unit.Get(UnitType.DisplayName))
            .MarginLeft(5),
					DecorEdit.Field("Адаптивный заголовок", "adaptTitle", unit.Get(UnitType.AdaptTitle))
						.MarginLeft(5).Hide(hideAdaptTitle),
					new HPanel(
						EditElementHlp.GetImageThumb(unit.Id, tunes).InlineBlock().MarginRight(20)
							.Hide(hideTile),
						EditElementHlp.GetAdaptImage(unit.Id).InlineBlock()
							.Hide(hideAdaptImage)
					).MarginLeft(5),
          DecorEdit.FieldArea("Альтернативный текст для картинки",
            new HTextArea("imageAlt", unit.Get(UnitType.ImageAlt)).Height("2em")
          ).Hide(hideImageAlt).MarginLeft(5),
          DecorEdit.Field(
            new HLabel("Cортировка элементов").FontBold(),
            SorterHlp.SortKindCombo("sortKind", unit.Get(UnitType.SortKind))
          ).MarginLeft(5).Hide(hideSortKind),
          SorterHlp.SortTimeEdit(unit, hideSortTime),
          DecorEdit.Field("Подзаголовок", "subtitle", unit.Get(UnitType.Subtitle))
            .Hide(hideSubtitle).MarginLeft(5),
          DecorEdit.FieldArea("Аннотация",
            new HTextArea("annotation", unit.Get(UnitType.Annotation)).Height("8em")
          ).Hide(hideAnnotation).MarginLeft(5),
          DecorEdit.Field("Адрес ссылки", "link", unit.Get(UnitType.Link))
            .Hide(hideLink).MarginLeft(5),
          DecorEdit.Field("Признак 1", "tag1", unit.Get(UnitType.Tags, 0))
            .Hide(hideTag1).MarginLeft(5),
          DecorEdit.Field("Признак 2", "tag2", unit.Get(UnitType.Tags, 1))
            .Hide(hideTag2).MarginLeft(5),
          new HPanel(
            DecorEdit.FieldInputBlock("Текст",
              HtmlHlp.CKEditorCreate("content", unit.Get(UnitType.Content),
                "400px", true)
            ),
            EditElementHlp.GetDescriptionImagesPanel(state.Option, unit.Id)
          ).Hide(hideContent),
          GalleryEditorHlp.GetGalleryPanel(state, unit.Id, tunes)
            .Hide(hideGallery)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_unit", "editContent",
            delegate (JsonData json)
            {
              string title = json.GetText("title");
              if (StringHlp.IsEmpty(title))
              {
                state.Operation.Message = "Не задан заголовок";
                return;
              }

							string adaptTitle = json.GetText("adaptTitle");
              string imageAlt = json.GetText("imageAlt");
              string sortKind = json.GetText("sortKind");
              string subtitle = json.GetText("subtitle");
              string annotation = json.GetText("annotation");
              string link = json.GetText("link");
              string tag1 = json.GetText("tag1");
              string tag2 = json.GetText("tag2");
              string content = json.GetText("content");

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              LightObject editUnit = DataBox.LoadObject(fabricConnection, UnitType.Unit, unit.Id);

              if (!UnitType.DisplayName.SetWithCheck(editUnit.Box, editUnit.Id, title))
              {
                state.Operation.Message = "Элемент с таким заголовком уже существует";
                return;
              }

							if (!hideAdaptTitle)
								editUnit.Set(UnitType.AdaptTitle, adaptTitle);
              if (!hideImageAlt)
                editUnit.Set(UnitType.ImageAlt, imageAlt);
              if (!hideSortKind)
                editUnit.Set(UnitType.SortKind, sortKind);
              if (!hideSortTime)
                SorterHlp.ParseAndSetSortTime(editUnit, json.GetText("sortTime"));
              if (!hideSubtitle)
                editUnit.Set(UnitType.Subtitle, subtitle);
              if (!hideAnnotation)
                editUnit.Set(UnitType.Annotation, annotation);
              if (!hideLink)
                editUnit.Set(UnitType.Link, link);
              if (!hideTag1)
                editUnit.Set(UnitType.Tags, 0, tag1);
              if (!hideTag2)
                editUnit.Set(UnitType.Tags, 1, tag2);
              if (!hideContent)
                editUnit.Set(UnitType.Content, content);

              editUnit.Box.Update();

              SiteContext.Default.UpdateStore();

              state.Operation.Message = "Изменения успешно сохранены";
              state.Operation.Status = "success";
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    [Obsolete]
    public static IHtmlControl GetImageLinkEdit(EditState state, LightKin unit)
    {
      int? parentId = unit.GetParentId(SectionType.UnitLinks);
      string returnUrl = UrlHlp.ReturnUnitUrl(store.Sections.FindSection(parentId), unit.Id);

      return new HPanel(
        DecorEdit.Title("Редактирование ссылочного элемента страницы"),
        GetDeletePanel(state, unit),
        new HPanel(
          new HPanel(
            EditElementHlp.GetImageThumb(unit.Id)
          ).MarginBottom(20),
          DecorEdit.FieldArea("Альтернативный текст для картинки", 
            new HTextArea("imageAlt", unit.Get(UnitType.DisplayName)).Height("2em")
          ).MarginLeft(5),
          DecorEdit.FieldInputBlock("Аннотация",
            HtmlHlp.CKEditorCreate("annotation", unit.Get(UnitType.Annotation),
              "200px", true)
          ).MarginLeft(5),
          //DecorEdit.FieldArea("Аннотация",
          //  new HTextArea("annotation", unit.Get(UnitType.Annotation)).Height("4em")
          //).MarginLeft(5),
          DecorEdit.Field("Адрес ссылки", "link", unit.Get(UnitType.Link))
            .MarginLeft(5)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_unit", "editContent",
            delegate (JsonData json)
            {
              string imageAlt = json.GetText("imageAlt");
              if (StringHlp.IsEmpty(imageAlt))
              {
                state.Operation.Message = "Не задан альтернативный текст для картинки";
                return;
              }

              string annotation = json.GetText("annotation");
              string link = json.GetText("link");

              LightObject editUnit = DataBox.LoadObject(fabricConnection, UnitType.Unit, unit.Id);

              if (!UnitType.DisplayName.SetWithCheck(editUnit.Box, editUnit.Id, imageAlt))
              {
                state.Operation.Message = "На этой странице уже существует элемент с таким же альтернативным текстом";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editUnit.Set(UnitType.Annotation, annotation);
              editUnit.Set(UnitType.Link, link);

              editUnit.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");

    }

    static IHtmlControl GetDeletePanel(EditState state, LightKin unit)
    {
      return EditElementHlp.GetDeletePanel(state, unit.Id, "", "Удаление элемента страницы",
        delegate
        {
          return true;
        }
      );
    }
  }
}
