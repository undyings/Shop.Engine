using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroBolt.Wui;
using Commune.Html;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class SectionEditorHlp
  {
    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetSectionAdd(EditorSelector selector,
      EditState state, string title, LightSection parent, string fixedDesignKind)
    {
      string returnUrl = parent.IsMenu ? "/" : UrlHlp.ShopUrl("page", parent.Id);

      string[] allKinds = selector.AllKinds;
      if (!StringHlp.IsEmpty(fixedDesignKind) && allKinds.Contains(fixedDesignKind))
        allKinds = new string[] { fixedDesignKind };

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field(
            new HLabel("Тип раздела").FontBold(),
            new HComboEdit<string>("designKind", "",
              delegate (string kind)
              {
                return selector.GetDisplayName(kind);
              },
              allKinds
            )
          ),
          DecorEdit.Field("Заголовок раздела", "name", "")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить").Event("add_section", "addContent",
            delegate (JsonData json)
            {
              string sectionName = json.GetText("name");
              if (StringHlp.IsEmpty(sectionName))
              {
                state.Operation.Message = "Не задан заголовок раздела";
                return;
              }

              string designKind = json.GetText("designKind");

              KinBox box = new KinBox(fabricConnection, "1=0");
              int? createSectionId = box.CreateUniqueObject(SectionType.Section,
                SectionType.Title.CreateXmlIds(sectionName), null);
              if (createSectionId == null)
              {
                state.Operation.Message = "Раздел с таким заголовком уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              LightKin addSection = new LightKin(box, createSectionId.Value);
              FabricHlp.SetCreateTime(addSection);

              addSection.Set(SectionType.DesignKind, designKind);

              addSection.AddParentId(SectionType.SubsectionLinks, parent.Id);

              addSection.Box.Update();

              state.CreatingObjectId = addSection.Id;

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("addContent");
    }

    //static IHtmlControl GetDeletePanel(EditState state, LightKin section)
    //{
    //  string deleteId = string.Format("delete_{0}", section.Id);
    //  return new HPanel(
    //    std.Button("Удалить раздел").Event("delete_section", "",
    //      delegate (JsonData json)
    //      {
    //        state.PopupDialog = deleteId;
    //      }
    //    ),
    //    EditElementHlp.GetPopupDialog(state, "Удаление раздела", 
    //      "Вы уверены, что хотите удалить раздел?", "Удалить",
    //      delegate
    //      {
    //        if (HttpContext.Current.IsInRole("nosave"))
    //        {
    //          state.Operation.Message = "Нет прав на сохранение изменений";
    //          return;
    //        }

    //        if (section is LightSection && ((LightSection)section).Subsections.Length > 0)
    //        {
    //          state.Operation.Message = "Раздел имеющий подразделы не может быть удален";
    //          return;
    //        }

    //        SQLiteDatabaseHlp.DeleteParentObject(fabricConnection, section.Id);
    //        Logger.AddMessage("Раздел '{0}' успешно удален", section.Id);

    //        state.Operation.Complete("Раздел успешно удален", "/");

    //        SiteContext.Default.UpdateStore();
    //      }
    //    ).Hide(state.PopupDialog != deleteId)
    //  ).Align(false).Padding(5, 10);
    //}

    public static IHtmlControl GetEditor(EditState state, LightKin section, BaseTunes tunes)
    {
      bool hideTile = !tunes.GetTune("Tile");
      bool hideSortKind = !tunes.GetTune("SortKind");
      bool hideUnitSortKind = !tunes.GetTune("UnitSortKind");
      bool hideSortTime = !tunes.GetTune("SortTime");
      bool hideTag1 = !tunes.GetTune("Tag1");
      bool hideTag2 = !tunes.GetTune("Tag2");
      bool hideNameInMenu = !tunes.GetTune("NameInMenu");
			bool hideHideInMenu = !tunes.GetTune("HideInMenu");
      bool hideSubtitle = !tunes.GetTune("Subtitle");
      bool hideLink = !tunes.GetTune("Link");
      bool hideRawAnnotation = !tunes.GetTune("Annotation") || tunes.GetTune("IsHtmlAnnotation");
      bool hideHtmlAnnotation = !tunes.GetTune("Annotation") || !tunes.GetTune("IsHtmlAnnotation");
      bool hideContent = !tunes.GetTune("Content");
      //bool hideWidget = !tunes.GetTune("Widget");
      //bool hideUnderContent = !tunes.GetTune("UnderContent");

      string returnUrl = UrlHlp.ShopUrl("page", section.Id);

      return new HPanel(
        DecorEdit.Title("Редактирование страницы"),
        EditElementHlp.GetDeletePanel(state, section.Id, "раздел", "Удаление раздела",
          delegate
          {
            if (section is LightSection && ((LightSection)section).Subsections.Length > 0)
            {
              state.Operation.Message = "Раздел имеющий подразделы не может быть удален";
              return false;
            }

            return true;
          }
        ),
        new HPanel(
          DecorEdit.Field("Заголовок страницы", "title", section.Get(SectionType.Title))
            .MarginLeft(5),
          EditElementHlp.GetImageThumb(section.Id, tunes).Hide(hideTile).MarginLeft(5),
          DecorEdit.Field(
            new HLabel("Cортировка подразделов").FontBold(),
            SorterHlp.SortKindCombo("sortKind", section.Get(SectionType.SortKind))
          ).MarginLeft(5).Hide(hideSortKind),
          DecorEdit.Field(
            new HLabel("Сортировка элементов").FontBold(),
            SorterHlp.SortKindCombo("unitSortKind", section.Get(SectionType.UnitSortKind))
          ).MarginLeft(5).Hide(hideUnitSortKind),
         SorterHlp.SortTimeEdit(section, hideSortTime),
          //DecorEdit.Field("Дата для сортировки", "sortDate",
          //  section.Get(SectionType.SortDate)?.ToLocalTime().ToShortDateString()
          //).MarginLeft(5).Hide(hideSortDate),
          DecorEdit.Field("Признак 1", "tag1", section.Get(SectionType.Tags, 0)).MarginLeft(5).Hide(hideTag1),
          DecorEdit.Field("Признак 2", "tag2", section.Get(SectionType.Tags, 1)).MarginLeft(5).Hide(hideTag2),
					new HPanel(
						new HInputCheck("hideInMenu", section.Get(SectionType.HideInMenu),
							new HAfter().Content("Скрывать в меню").MarginLeft(18).VAlign(1)
						).NoWrap()
					).Margin(5).Hide(hideHideInMenu),
					DecorEdit.Field("Заголовок в меню", "nameInMenu", section.Get(SectionType.NameInMenu))
            .MarginLeft(5).Hide(hideNameInMenu),
          DecorEdit.Field("Подзаголовок", "subtitle", section.Get(SectionType.Subtitle))
            .MarginLeft(5).Hide(hideSubtitle),
          DecorEdit.Field("Адрес ссылки", "link", section.Get(SectionType.Link))
            .Hide(hideLink).MarginLeft(5),
          DecorEdit.FieldInputBlock("Аннотация",
            new HTextArea("annotation", section.Get(SectionType.Annotation)).Height("4em").Width("100%")
          ).Hide(hideRawAnnotation),
          DecorEdit.FieldInputBlock("Аннотация",
            HtmlHlp.CKEditorCreate("annotation", section.Get(SectionType.Annotation),
              "100px", true)
          ).Hide(hideHtmlAnnotation),
          new HPanel(
            DecorEdit.FieldInputBlock("Текст",
              HtmlHlp.CKEditorCreate("content", section.Get(SectionType.Content),
                "400px", true)
            ),
            EditElementHlp.GetDescriptionImagesPanel(state.Option, section.Id)
          ).Hide(hideContent)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_section", "editContent",
            delegate (JsonData json)
            {
              string title = json.GetText("title");
              if (StringHlp.IsEmpty(title))
              {
                state.Operation.Message = "Не задан заголовок";
                return;
              }

              string sortKind = json.GetText("sortKind");
              string unitSortKind = json.GetText("unitSortKind");
              string tag1 = json.GetText("tag1");
              string tag2 = json.GetText("tag2");
							bool hideInMenu = json.GetBool("hideInMenu");
              string nameInMenu = json.GetText("nameInMenu");
              string subtitle = json.GetText("subtitle");
              string annotation = json.GetText("annotation");
              string link = json.GetText("link");
              string content = json.GetText("content");

              LightObject editSection = DataBox.LoadObject(fabricConnection, SectionType.Section, section.Id);

              if (!SectionType.Title.SetWithCheck(editSection.Box, editSection.Id, title))
              {
                state.Operation.Message = "Страница с таким заголовком уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              if (!hideSortKind)
                editSection.Set(SectionType.SortKind, sortKind);
              if (!hideUnitSortKind)
                editSection.Set(SectionType.UnitSortKind, unitSortKind);
              if (!hideSortTime)
                SorterHlp.ParseAndSetSortTime(editSection, json.GetText("sortTime"));

              if (!hideTag1)
                editSection.Set(SectionType.Tags, 0, tag1);
              if (!hideTag2)
                editSection.Set(SectionType.Tags, 1, tag2);
							if (!hideHideInMenu)
								editSection.Set(SectionType.HideInMenu, hideInMenu);
              if (!hideNameInMenu)
                editSection.Set(SectionType.NameInMenu, nameInMenu);
              if (!hideSubtitle)
                editSection.Set(SectionType.Subtitle, subtitle);
              if (!hideRawAnnotation || !hideHtmlAnnotation)
                editSection.Set(SectionType.Annotation, annotation);
              if (!hideLink)
                editSection.Set(SectionType.Link, link);
              if (!hideContent)
                editSection.Set(SectionType.Content, content);

              editSection.Box.Update();

              SiteContext.Default.UpdateStore();

              state.Operation.Message = "Изменения успешно сохранены";
              state.Operation.Status = "success";
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    //public static IHtmlControl GetUniversalTextEdit(EditState state, LightKin section,
    //  bool hideNameInMenu, bool hideThumb, bool hideAnnotation, bool hideContent,
    //  bool showSortKind = false, bool showSortDate = false)
    //{
    //  //LightSection section = SiteContext.Default.Store.Sections.FindSection(section.Id);
    //  //int? parentId = section.GetParentId(SectionType.SubsectionLinks);

    //  string returnUrl = !hideContent ? UrlHlp.ShopUrl("page", section.Id) :
    //    UrlHlp.ReturnParentUrl(SiteContext.Default.Store.Sections, section.Id);

    //  return new HPanel(
    //    DecorEdit.Title("Редактирование страницы"),
    //    EditElementHlp.GetDeletePanel(state, section.Id, "раздел", "Удаление раздела",
    //      delegate
    //      {
    //        if (section is LightSection && ((LightSection)section).Subsections.Length > 0)
    //        {
    //          state.Operation.Message = "Раздел имеющий подразделы не может быть удален";
    //          return false;
    //        }

    //        return true;
    //      }
    //    ),
    //    //GetDeletePanel(state, section),
    //    new HPanel(
    //      DecorEdit.Field("Заголовок страницы", "title", section.Get(SectionType.Title))
    //        .MarginLeft(5),
    //      EditElementHlp.GetImageThumb(section.Id).Hide(hideThumb).MarginLeft(5),
    //      DecorEdit.Field(
    //        new HLabel("Вид сортировки").FontBold(),
    //        new HComboEdit<string>("sortKind", section.Get(SectionType.SortKind),
    //          delegate (string kind)
    //          {
    //            if (kind == "desc")
    //              return "По убыванию даты";
    //            return "";
    //          },
    //          new string[] { "", "desc" }
    //        )
    //      ).MarginLeft(5).Hide(!showSortKind),
    //      DecorEdit.Field("Дата для сортировки", "sortDate", 
    //        section.Get(SectionType.SortDate)?.ToLocalTime().ToShortDateString()
    //      ).MarginLeft(5).Hide(!showSortDate),
    //      DecorEdit.Field("Заголовок в меню", "nameInMenu", section.Get(SectionType.NameInMenu))
    //        .MarginLeft(5).MarginBottom(20).Hide(hideNameInMenu),
    //      DecorEdit.FieldInputBlock("Аннотация",
    //        HtmlHlp.CKEditorCreate("annotation", section.Get(SectionType.Annotation),
    //          "100px", true)
    //      ).Hide(hideAnnotation),
    //      DecorEdit.FieldInputBlock("Текст",
    //        HtmlHlp.CKEditorCreate("content", section.Get(SectionType.Content),
    //          "400px", true)
    //      ).Hide(hideContent),
    //      EditElementHlp.GetDescriptionImagesPanel(section.Id)
    //    ).Margin(0, 10),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.SaveButton()
    //      .CKEditorOnUpdateAll()
    //      .Event("save_section", "editContent",
    //        delegate (JsonData json)
    //        {
    //          string title = json.GetText("title");
    //          if (StringHlp.IsEmpty(title))
    //          {
    //            state.Operation.Message = "Не задан заголовок";
    //            return;
    //          }

    //          string nameInMenu = json.GetText("nameInMenu");
    //          string annotation = json.GetText("annotation");
    //          string content = json.GetText("content");
    //          string rawSortDate = json.GetText("sortDate");
    //          string sortKind = json.GetText("sortKind");

    //          LightObject editSection = DataBox.LoadObject(fabricConnection, SectionType.Section, section.Id);

    //          if (!SectionType.Title.SetWithCheck(editSection.Box, editSection.Id, title))
    //          {
    //            state.Operation.Message = "Страница с таким заголовком уже существует";
    //            return;
    //          }

    //          if (HttpContext.Current.IsInRole("nosave"))
    //          {
    //            state.Operation.Message = "Нет прав на сохранение изменений";
    //            return;
    //          }

    //          if (!hideNameInMenu)
    //            editSection.Set(SectionType.NameInMenu, nameInMenu);
    //          if (!hideAnnotation)
    //            editSection.Set(SectionType.Annotation, annotation);
    //          if (!hideContent)
    //            editSection.Set(SectionType.Content, content);

    //          if (showSortKind)
    //          {
    //            editSection.Set(SectionType.SortKind, sortKind);
    //          }

    //          if (showSortDate)
    //          {
    //            DateTime sortDate;
    //            if (DateTime.TryParse(rawSortDate, out sortDate))
    //              editSection.Set(SectionType.SortDate, sortDate.ToUniversalTime());
    //          }

    //          editSection.Box.Update();

    //          SiteContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("editContent");
    //}

    public static IHtmlControl GetTextEdit(EditState state, LightKin section)
    {
      string returnUrl = UrlHlp.ShopUrl("page", section.Id);

      return new HPanel(
        DecorEdit.Title("Редактирование страницы"),
        EditElementHlp.GetDeletePanel(state, section.Id, "раздел", "Удаление раздела",
          delegate
          {
            if (section is LightSection && ((LightSection)section).Subsections.Length > 0)
            {
              state.Operation.Message = "Раздел имеющий подразделы не может быть удален";
              return false;
            }

            return true;
          }
        ),
        //GetDeletePanel(state, section),
        new HPanel(
          DecorEdit.Field("Заголовок страницы", "title", section.Get(SectionType.Title))
            .MarginLeft(5),
          DecorEdit.Field("Заголовок в меню", "nameInMenu", section.Get(SectionType.NameInMenu))
            .MarginLeft(5).MarginBottom(20),
          DecorEdit.FieldInputBlock("Текст",
            HtmlHlp.CKEditorCreate("content", section.Get(SectionType.Content),
              "400px", true)
          ),
          EditElementHlp.GetDescriptionImagesPanel(state.Option, section.Id)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_section", "editContent",
            delegate (JsonData json)
            {
              string title = json.GetText("title");
              if (StringHlp.IsEmpty(title))
              {
                state.Operation.Message = "Не задан заголовок";
                return;
              }

              string nameInMenu = json.GetText("nameInMenu");
              string content = json.GetText("content");

              LightObject editSection = DataBox.LoadObject(fabricConnection, SectionType.Section, section.Id);

              if (!SectionType.Title.SetWithCheck(editSection.Box, editSection.Id, title))
              {
                state.Operation.Message = "Страница с таким заголовком уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editSection.Set(SectionType.NameInMenu, nameInMenu);
              editSection.Set(SectionType.Content, content);

              editSection.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    [Obsolete]
    public static IHtmlControl GetContactEdit(EditState state, LightKin section)
    {
      string returnUrl = UrlHlp.ShopUrl("page", section.Id);

      return new HPanel(
        DecorEdit.Title("Редактирование контактов"),
        new HPanel(
          DecorEdit.Field("Заголовок страницы", "title", section.Get(SectionType.Title))
            .MarginLeft(5),
          DecorEdit.Field("Заголовок в меню", "nameInMenu", section.Get(SectionType.NameInMenu))
            .MarginLeft(5).MarginBottom(20),
          DecorEdit.FieldInputBlock("Текст выводимый над картой",
            HtmlHlp.CKEditorCreate("content", section.Get(SectionType.Widget),
              "200px", true)
          ),
          EditElementHlp.GetDescriptionImagesPanel(state.Option, section.Id),
          DecorEdit.FieldInputBlock("Виджет карты 2ГИС",
            new HPanel(
              new HTextView(@"Если адрес предприятия изменился, то перейдите <a href='http://api.2gis.ru/widgets/firmsonmap/' target='blank'>по ссылке</a>, получите виджет карты для нового адреса и вставьте его в поле ниже.")
                .Padding(10).BorderLeft(DecorEdit.blockBorder).BorderRight(DecorEdit.blockBorder),
              new HTextArea("script", section.Get(SectionType.Widget))
                .Width("100%").Height(220).Padding(5)
            )
          ),
          DecorEdit.FieldInputBlock("Текст выводимый под картой",
            HtmlHlp.CKEditorCreate("underContent", section.Get(SectionType.UnderContent),
              "400px", true)
          )
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_section", "editContent",
            delegate (JsonData json)
            {
              string title = json.GetText("title");
              if (StringHlp.IsEmpty(title))
              {
                state.Operation.Message = "Не задан заголовок";
                return;
              }

              string nameInMenu = json.GetText("nameInMenu");
              string content = json.GetText("content");
              string script = json.GetText("script");
              string underContent = json.GetText("underContent");

              LightObject editSection = DataBox.LoadObject(fabricConnection, SectionType.Section, section.Id);

              if (!SectionType.Title.SetWithCheck(editSection.Box, editSection.Id, title))
              {
                state.Operation.Message = "Страница с таким заголовком уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }


              editSection.Set(SectionType.NameInMenu, nameInMenu);
              editSection.Set(SectionType.Content, content);
              editSection.Set(SectionType.Widget, script);
              editSection.Set(SectionType.UnderContent, underContent);

              editSection.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }
  }
}
