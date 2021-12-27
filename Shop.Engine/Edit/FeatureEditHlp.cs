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
  public class FeatureEditHlp
  {
    static IShopStore store
    {
      get
      {
        return (IShopStore)SiteContext.Default.Store;
      }
    }

    static ShopStorage shop
    {
      get { return store.Shop; }
    }

    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetFeatureListEdit(EditState state, out string title)
    {
      title = "Особенности товаров";

      ParentBox featureBox = shop.featureBox;

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HGrid<int>(featureBox.AllObjectIds,
            delegate (int featureId)
            {
              LightParent feature = new LightParent(featureBox, featureId);
              return new HPanel(
                DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("feature", feature.Id)).VAlign(-2).MarginRight(10),
                new HTextView(feature.Get(FeatureType.Code)).InlineBlock()
              ).Padding(5, 0);
            },
            new HRowStyle()
          ),
          new HPanel(
            DecorEdit.RedoButton(true, "Добавить особенность", UrlHlp.EditUrl("feature", null))
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("Справочник", UrlHlp.EditUrl("shop-catalog", null))
        )        
      );
    }

    public static IHtmlControl GetFeatureEdit(EditState state, int? featureId, out string title)
    {
      if (featureId == null)
        featureId = state.CreatingObjectId;

      title = featureId == null ? "Добавить особенность" : "Редактировать особенность";

      ParentBox featureBox = shop.featureBox;

      if (featureId != null && !featureBox.ObjectById.Exist(featureId.Value))
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      if (featureId == null)
        return GetFeatureAdd(state, out title);

      LightParent feature = new LightParent(featureBox, featureId.Value);

      string returnUrl = UrlHlp.EditUrl("feature-list", null);

      bool withIcon = feature.Get(FeatureType.WithIcon);
      //bool withCategory = feature.Get(FeatureType.WithCategory);
      bool withHint = feature.Get(FeatureType.WithHint);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HPanel(
            DecorEdit.Field("Код", new HLabel(feature.Get(FeatureType.Code))),
            DecorEdit.Field("Примечание", "marking", feature.Get(FeatureType.Marking)),
            DecorEdit.FieldCheck("С иконкой", "withIcon", withIcon),
            //DecorEdit.FieldCheck("С категорией", "withCategory", withCategory),
            DecorEdit.FieldCheck("С подсказкой", "withHint", withHint),
            DecorEdit.Field("С особенностью", "withFeature1", feature.Get(FeatureType.WithFeatures, 0)),
            DecorEdit.Field("С особенностью", "withFeature2", feature.Get(FeatureType.WithFeatures, 1)),
            DecorEdit.Field("С особенностью", "withFeature3", feature.Get(FeatureType.WithFeatures, 2)),
            EditElementHlp.GetButtonsPanel(
              DecorEdit.SaveButton().Event("save_feature", "featureContent",
                delegate (JsonData json)
                {
                  string editMarking = json.GetText("marking");
                  bool editWithIcon = json.GetBool("withIcon");
                  //bool editWithCategory = json.GetBool("withCategory");
                  bool editWithHint = json.GetBool("withHint");
                  string withFeature1 = json.GetText("withFeature1");
                  string withFeature2 = json.GetText("withFeature2");
                  string withFeature3 = json.GetText("withFeature3");

                  if (HttpContext.Current.IsInRole("nosave"))
                  {
                    state.Operation.Message = "Нет прав на сохранение изменений";
                    return;
                  }

                  LightObject editFeature = DataBox.LoadObject(fabricConnection, FeatureType.Feature, featureId.Value);

                  editFeature.Set(FeatureType.Marking, editMarking);
                  editFeature.Set(FeatureType.WithIcon, editWithIcon);
                  //editFeature.Set(FeatureType.WithCategory, editWithCategory);
                  editFeature.Set(FeatureType.WithHint, editWithHint);
                  editFeature.Set(FeatureType.WithFeatures, 0, withFeature1);
                  editFeature.Set(FeatureType.WithFeatures, 1, withFeature2);
                  editFeature.Set(FeatureType.WithFeatures, 2, withFeature3);

                  editFeature.Box.Update();

                  SiteContext.Default.UpdateStore();
                }
              ),
              DecorEdit.ReturnButton(UrlHlp.EditUrl("feature-list", null))
            ).MarginTop(10)
          ).EditContainer("featureContent"),
          new HPanel(
            new HLabel("Значения особенности").FontBold().MarginBottom(5),
            new HGrid<int>(feature.AllChildIds(FeatureType.FeatureValueLinks),
              delegate (int valueId)
              {
                LightObject featureValue = new LightObject(shop.featureValueBox, valueId);
                return new HPanel(
                  new HImage(UrlHlp.ImageUrl(valueId, true)).WidthLimit("", "20px").MarginRight(5).Hide(!withIcon),
                  new HLabel(featureValue.Get(FeatureValueType.DisplayName)).MarginRight(10).VAlign(true),
                  DecorEdit.RedoIconButton(true, UrlHlp.EditUrl(feature.Id, "feature-value", valueId))
                    .PositionAbsolute().Left(2).Top(0).Padding(2).Background("#fff")
                ).PositionRelative().PaddingLeft(24).MarginTop(5).MarginBottom(5);
              },
              new HRowStyle()
            ),
            DecorEdit.RedoButton("Добавить значение", UrlHlp.EditUrl(feature.Id, "feature-value", null))
          ).MarginTop(10)
          //new HPanel(
          //  DecorEdit.Field("Значение", "
          //  DecorEdit.AddButton("Добавить").Event("add_feature_value", "featureValueAddContent",
          //    delegate (JsonData json)
          //    {

          //    }
          //  )
          //).EditContainer("featureValueAddContent")
        ).Margin(0, 10).MarginBottom(20)
      );
    }

    static IHtmlControl GetFeatureAdd(EditState state, out string title)
    {
      title = "Добавить особенность";

      string returnUrl = UrlHlp.EditUrl("feature-list", null);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Код", "code", "")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить")
          .Event("add_feature", "editContent",
            delegate (JsonData json)
            {
              string editCode = json.GetText("code");

              if (StringHlp.IsEmpty(editCode))
              {
                state.Operation.Message = "Не задан код особенности";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              ObjectBox box = new ObjectBox(fabricConnection, "1=0");
              int? createFeatureId = box.CreateUniqueObject(FeatureType.Feature,
                FeatureType.Code.CreateXmlIds(editCode), null);

              if (createFeatureId == null)
              {
                state.Operation.Message = "Особенность с таким кодом уже существует";
                return;
              }

              LightObject editFeature = new LightObject(box, createFeatureId.Value);
              editFeature.Set(ObjectType.ActFrom, DateTime.UtcNow);

              state.CreatingObjectId = editFeature.Id;

              editFeature.Set(ObjectType.ActTill, DateTime.UtcNow);

              editFeature.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton("Все особенности", returnUrl)
        )
      ).EditContainer("editContent");
    }

    static LightFeature FindWithFeature(ShopStorage shop, LightParent feature, int withFeatureIndex)
    {
      string withFeature = feature.Get(FeatureType.WithFeatures, withFeatureIndex);
      if (StringHlp.IsEmpty(withFeature))
        return null;

      return shop.FindFeature(withFeature);
    }

    static IHtmlControl GetWithFeatureField(ShopStorage shop, 
      LightParent feature, LightObject featureValue, int withFeatureIndex)
    {
      LightFeature withFeature = FindWithFeature(shop, feature, withFeatureIndex);
      if (withFeature == null)
        return null;

      HComboEdit<int> combo = new HComboEdit<int>(string.Format("withFeature{0}", withFeatureIndex + 1),
        featureValue.Get(FeatureValueType.WithFeatures, withFeatureIndex),
        ArrayHlp.Convert(withFeature.FeatureValues, delegate(LightObject value)
          {
            return _.Tuple(value.Id, value.Get(FeatureValueType.DisplayName));
          }
        )
      );

      return DecorEdit.Field(string.Format("Особенность {0}", withFeatureIndex + 1), combo);
    }

    public static IHtmlControl GetFeatureValueEdit(EditState state, int? featureId, int? valueId, out string title)
    {
      title = "Редактирование значения особенности товара";

      if (featureId == null)
        return EditHlp.GetInfoMessage("Неверный формат запроса", "/");

      if (!shop.featureBox.ObjectById.Exist(featureId.Value))
        return EditHlp.GetInfoMessage("Не найдена особенность товара", "/");

      if (valueId == null)
        valueId = state.CreatingObjectId;

      LightParent feature = new LightParent(shop.featureBox, featureId.Value);

      if (valueId == null)
        return GetFeatureValueAdd(state, feature, out title);

      if (!shop.featureValueBox.ObjectById.Exist(valueId.Value))
        return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");

      LightObject featureValue = new LightObject(shop.featureValueBox, valueId.Value);

      string returnUrl = UrlHlp.EditUrl("feature", featureId);

      bool withIcon = feature.Get(FeatureType.WithIcon);
      //bool withCategory = feature.Get(FeatureType.WithCategory);
      bool withHint = feature.Get(FeatureType.WithHint);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HXPanel(
            EditElementHlp.GetImageThumb(valueId.Value).Hide(!withIcon),
            std.DockFill(
              new HPanel(
                EditElementHlp.GetDeletePanel(state, valueId.Value,
                  "значение", "Удаление значения", null
                ).Margin(0).MarginTop(5),
                DecorEdit.Field("Особенность", new HLabel(feature.Get(FeatureType.Code))),
                DecorEdit.Field("Значение", "name", featureValue.Get(FeatureValueType.DisplayName)),
                //DecorEdit.Field("Категория", "category", featureValue.Get(FeatureValueType.Category)).Hide(!withCategory),
                DecorEdit.FieldArea("Подсказка", 
                  new HTextArea("hint", featureValue.Get(FeatureValueType.Hint)).Height("4em")
                ).Hide(!withHint),
                GetWithFeatureField(shop, feature, featureValue, 0),
                GetWithFeatureField(shop, feature, featureValue, 1),
                GetWithFeatureField(shop, feature, featureValue, 2)
              ).PaddingLeft(10)
            )
          ).MarginBottom(10).MarginRight(10)
        ).Margin(0, 10).MarginBottom(10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_feature_value", "editContent",
            delegate (JsonData json)
            {
              string editName = json.GetText("name");
              //string editCategory = json.GetText("category");
              string editHint = json.GetText("hint");
              int? editWithFeature1 = json.GetInt("withFeature1");
              int? editWithFeature2 = json.GetInt("withFeature2");
              int? editWithFeature3 = json.GetInt("withFeature3");

              if (StringHlp.IsEmpty(editName))
              {
                state.Operation.Message = "Не задано наименование значения";
                return;
              }

              LightObject editValue = DataBox.LoadObject(fabricConnection,
                FeatureValueType.FeatureValue, valueId.Value);
              
              if (!FeatureValueType.DisplayName.SetWithCheck(editValue.Box, valueId.Value, editName))
              {
                state.Operation.Message = "Значение с таким наименованием уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              //if (withCategory)
              //  editValue.Set(FeatureValueType.Category, editCategory);
              if (withHint)
                editValue.Set(FeatureValueType.Hint, editHint);

              if (editWithFeature1 != null)
                editValue.Set(FeatureValueType.WithFeatures, 0, editWithFeature1.Value);
              if (editWithFeature2 != null)
                editValue.Set(FeatureValueType.WithFeatures, 1, editWithFeature2.Value);
              if (editWithFeature3 != null)
                editValue.Set(FeatureValueType.WithFeatures, 2, editWithFeature3.Value);

              editValue.Set(ObjectType.ActTill, DateTime.UtcNow);

              editValue.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    static IHtmlControl GetFeatureValueAdd(EditState state, LightParent feature, out string title)
    {
      title = "Добавление значения особенности товара";

      string returnUrl = UrlHlp.EditUrl("feature", feature.Id);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Особенность", new HLabel(feature.Get(FeatureType.Code))),
          DecorEdit.Field("Значение", "name", "")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить")
          .Event("add_feature_value", "editContent",
            delegate (JsonData json)
            {
              string editName = json.GetText("name");

              if (StringHlp.IsEmpty(editName))
              {
                state.Operation.Message = "Не задано наименование";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              KinBox box = new KinBox(fabricConnection, "1=0");
              int? createValueId = box.CreateUniqueObject(FeatureValueType.FeatureValue,
                FeatureValueType.DisplayName.CreateXmlIds(feature.Id, editName), null);

              if (createValueId == null)
              {
                state.Operation.Message = "Такое значение уже существует";
                return;
              }

              LightKin editValue = new LightKin(box, createValueId.Value);
              editValue.Set(ObjectType.ActFrom, DateTime.UtcNow);
              editValue.Set(ObjectType.ActTill, DateTime.UtcNow);
              editValue.AddParentId(FeatureType.FeatureValueLinks, feature.Id);

              state.CreatingObjectId = editValue.Id;
              
              editValue.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton("Все значения", returnUrl)
        )
      ).EditContainer("editContent");
    }
  }
}
