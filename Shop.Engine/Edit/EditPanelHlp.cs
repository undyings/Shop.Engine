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
  public class EditPanelHlp
  {
    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetFabricParentsPanel(EditState state, ShopStorage shop, LightKin fabric)
    {
      RowLink[] parentRows = fabric.AllParentRows(GroupType.FabricTypeLink);

      int[] groupIdsForAdd = FabricHlp.FindGroupIdsForAddFabric(shop, fabric);

      List<IHtmlControl> controls = new List<IHtmlControl>();

      controls.Add(
        DecorEdit.FieldBlock("Товар находится в группах:",
          new HGrid<RowLink>(parentRows, delegate (RowLink parentRow)
            {
              int linkId = parentRow.Get(LinkType.LinkId);
              int parentId = parentRow.Get(LinkType.ParentId);
              LightGroup parentGroup = shop.FindGroup(parentId);

              return new HPanel(
                new HLabel(parentGroup?.Get(GroupType.Identifier)).Width(300).Padding(10, 0),
                parentRows.Length < 2 ? null :
                  std.Button("Убрать из группы").MarginLeft(20)
                    .Event("remove_from_group", "",
                    delegate
                    {
                      fabricConnection.GetScalar("", "Delete From light_link Where link_id = @linkId",
                        new DbParameter("linkId", linkId)
                      );
                      SiteContext.Default.UpdateStore();
                    },
                    linkId
                  )
              );
            },
            new HRowStyle()
          ).Padding(0, 10)
        ).InlineBlock()
      );

      controls.Add(
        new HPanel(
          new HComboEdit<int>("group_for_add", -1,
            delegate (int groupId) { return shop.FindGroup(groupId)?.Get(GroupType.Identifier); },
            groupIdsForAdd
          ).MarginRight(20),
          std.Button("Добавить в группу").Event("add_in_group", "group_add_container",
            delegate (JsonData json)
            {
              int? addGroupId = ConvertHlp.ToInt(json.GetText("group_for_add"));
              if (addGroupId == null || addGroupId == -1)
              {
                state.Operation.Message = "Группа для добавления не выбрана";
                return;
              }

              LightKin editFabric = DataBox.LoadKin(fabricConnection, FabricType.Fabric, fabric.Id);
              editFabric.AddParentId(GroupType.FabricTypeLink, addGroupId.Value);
              editFabric.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          )
        ).EditContainer("group_add_container")
      );

      controls.Add(
        EditElementHlp.GetDeletePanel(state, fabric.Id, "товар", "Удаление товара", null)
      );

      return new HPanel(controls.ToArray());
    }

    public static IHtmlControl GetSectionMovePanel(EditState state,
      SectionStorage sectionStorage, LightSection moveSection)
    {
      int parentSectionId = -1;
      if (moveSection.ParentSection != null)
        parentSectionId = moveSection.ParentSection.Id;

      LightSection[] groupsForMove = SectionHlp.FindGroupsForMoveSection(sectionStorage, "group", moveSection);
      return new HPanel(
        new HLabel("Родительский раздел"),
        new HComboEdit<int>("parentSection", parentSectionId, delegate (int sectionId)
          {
            LightSection section = sectionStorage.FindSection(sectionId);
            if (section == null)
              return "";
            if (section.IsMenu)
              return "Главная";
            return section.Get(SectionType.Title);
          },
          ArrayHlp.Convert(groupsForMove, delegate (LightSection section) { return section.Id; })
        ),
        EditElementHlp.GetActionPanel(state, string.Format("move_{0}", moveSection.Id),
          "Переместить", "раздел", "Перемещение раздела", null,
          delegate
          {
            LightKin parentSection = DataBox.LoadKin(fabricConnection,
              SectionType.Section, moveSection.ParentSection.Id);
            //fabricConnection.GetScalar("", "Update light_link Set 
          }
        ).InlineBlock()
      ).EditContainer("moveSection");
    }

    //public static IHtmlControl GetSortingEdit(EditState state,
    //  string title, LightObject[] sections, string returnUrl)
    //{
    //  int[] sectionIds = ArrayHlp.Convert(sections,
    //    delegate (LightObject section) { return section.Id; }
    //  );

    //  return new HPanel(
    //    DecorEdit.Title(title),
    //    new HPanel(
    //      new HTextView("Если нужно, чтобы раздел показывался <em>перед</em> другими разделами, то задайте префикс начинающийся с <strong>aa</strong>."
    //      ).Padding(5),
    //      new HTextView("Если нужно, чтобы раздел показывался <em>после</em> других разделов, то задайте префикс начинающийся с <strong>Яя</strong>."
    //      ).Padding(5),
    //      new HGrid<LightObject>(sections,
    //        delegate (LightObject section)
    //        {
    //          return new HPanel(
    //            new HTextEdit(string.Format("prefix_{0}", section.Id), section.Get(SEOProp.SortingPrefix))
    //              .Width(100).Margin(0, 5),
    //            new HLabel(section.Get(SectionType.Title))
    //          ).Padding(5);
    //        },
    //        new HRowStyle().Odd(new HTone().Background("#F9F9F9"))
    //      ).MarginTop(10)
    //    ).Margin(0, 10).MarginBottom(20),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.SaveButton()
    //      .Event("save_sorting", "editContent",
    //        delegate (JsonData json)
    //        {
    //          if (HttpContext.Current.IsInRole("nosave"))
    //          {
    //            state.Operation.Message = "Нет прав на сохранение изменений";
    //            return;
    //          }

    //          ObjectBox editBox = new ObjectBox(fabricConnection,
    //            DataCondition.ForObjects(sectionIds));
    //          foreach (int editId in editBox.AllObjectIds)
    //          {
    //            string prefix = json.GetText(string.Format("prefix_{0}", editId));
    //            if (prefix == null)
    //              continue;
    //            LightObject editSection = new LightObject(editBox, editId);
    //            editSection.Set(SEOProp.SortingPrefix, prefix);
    //          }

    //          editBox.Update();

    //          SiteContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("editContent");
    //}

    public static IHtmlControl GetObjectAdd(EditState state,
      string title, string fieldCaption, string returnUrl,
      int objectType, XmlDisplayName displayName)
    {
      return GetObjectAdd(state, title, fieldCaption, returnUrl,
        delegate (string objectName)
        {
          ObjectBox box = new ObjectBox(fabricConnection, "1=0");
          int? createObjectId = box.CreateUniqueObject(objectType,
            displayName.CreateXmlIds(objectName), null);

          if (createObjectId == null)
          {
            state.Operation.Message = "Объект с таким наименованием уже существует";
            return null;
          }

          return new LightObject(box, createObjectId.Value);
        }
      );
    }

    public static IHtmlControl GetObjectAdd(EditState state,
      string title, string fieldCaption, string returnUrl,
      Getter<LightObject, string> objectCreator)
    {
      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field(fieldCaption, "objectName", "")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить").Event("add_object", "addContent",
            delegate (JsonData json)
            {
              string objectName = json.GetText("objectName");
              if (StringHlp.IsEmpty(objectName))
              {
                state.Operation.Message = "Не задано наименование добавляемого объекта";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              LightObject createObject = objectCreator(objectName);
              if (createObject == null)
                return;

              FabricHlp.SetCreateTime(createObject);
              createObject.Box.Update();

              state.CreatingObjectId = createObject.Id;

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("addContent");
    }
  }
}
