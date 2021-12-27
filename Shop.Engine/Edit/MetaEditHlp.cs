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
  public class MetaEditHlp
  {
		static IShopContext context
		{
			get
			{
				return (IShopContext)SiteContext.Default;
			}
		}

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

    public static IHtmlControl GetCategoryListEdit(out string title)
    {
      title = "Категории свойств";

      ObjectBox categoryBox = shop.propertyCategoryBox;

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HGrid<int>(categoryBox.AllObjectIds,
            delegate (int categoryId)
            {
              LightObject category = new LightObject(categoryBox, categoryId);

              return new HPanel(
                DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("category", categoryId)).MarginRight(10),
                new HLabel(
                  string.Format("{0} ({1})", 
                    category.Get(MetaCategoryType.DisplayName),
                    MetaCategoryType.KindToDisplay(category.Get(MetaCategoryType.Kind))
                  )
                )
              ).Padding(5, 0);
            },
            new HRowStyle()
          ),
          new HPanel(
            DecorEdit.RedoButton(true, "Добавить категорию", UrlHlp.EditUrl("category", null))
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("Все свойства", UrlHlp.EditUrl("property-list", null))
        )
      );
    }

    public static IHtmlControl GetCategoryEdit(EditState state, int? categoryId, out string title)
    {
      if (categoryId == null)
        categoryId = state.CreatingObjectId;

      title = categoryId == null ? "Добавить категорию свойств" : "Редактировать категорию свойств";

      LightObject category = shop.FindPropertyCategory(categoryId);

      if (categoryId != null && category == null)
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      string returnUrl = UrlHlp.EditUrl("category-list", null);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Наименование", "name", category?.Get(MetaCategoryType.DisplayName)),
          DecorEdit.Field("Тип категории",
            new HComboEdit<string>("type", category?.Get(MetaCategoryType.Kind),
              MetaCategoryType.KindToDisplay, MetaCategoryType.AllKinds
            )
          )
          //new HPanel(
          //  std.Button("Удалить").Event("delete_category", "",
          //    delegate (JsonData json)
          //    {
          //      if (redirectId == null)
          //        return;

          //      SQLiteDatabaseHlp.DeleteObject(fabricConnection, redirectId.Value);
          //      Logger.AddMessage("Перенаправление '{0}' успешно удалено", redirectId.Value);

          //      state.Operation.Complete("Перенаправление успешно удалено", returnUrl);

          //      ShopContext.Default.UpdateStore();
          //    }
          //  )
          //).Hide(redirectId == null).Align(false).MarginTop(10)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          (category != null ? DecorEdit.SaveButton() : DecorEdit.AddButton("Добавить"))
          .Event("save_category", "editContent",
            delegate (JsonData json)
            {
              string editName = json.GetText("name");
              string editType = json.GetText("type");

              if (StringHlp.IsEmpty(editName))
              {
                state.Operation.Message = "Не задано наименование категории";
                return;
              }
              if (StringHlp.IsEmpty(editType))
              {
                state.Operation.Message = "Не задан тип категории";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              LightObject editCategory = null;
              if (categoryId != null)
              {
                editCategory = DataBox.LoadObject(fabricConnection,
                  MetaCategoryType.PropertyCategory, categoryId.Value);

                if (!MetaCategoryType.DisplayName.SetWithCheck(editCategory.Box, categoryId.Value, editName))
                {
                  state.Operation.Message = "Другая категория с таким наименованием уже существует";
                  return;
                }
              }
              else
              {
                ObjectBox box = new ObjectBox(fabricConnection, "1=0");
                int? createCategoryId = box.CreateUniqueObject(MetaCategoryType.PropertyCategory,
                  MetaCategoryType.DisplayName.CreateXmlIds(editName), null);

                //Logger.AddMessage("Redirect.Edit: {0}, {1}", editFrom, editTo);

                if (createCategoryId == null)
                {
                  state.Operation.Message = "Категория с таким наименованием уже существует";
                  return;
                }

                editCategory = new LightObject(box, createCategoryId.Value);
                editCategory.Set(ObjectType.ActFrom, DateTime.UtcNow);

                state.CreatingObjectId = editCategory.Id;

              }

              editCategory.Set(MetaCategoryType.Kind, editType);
              editCategory.Set(ObjectType.ActTill, DateTime.UtcNow);

              editCategory.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton("Все категории", returnUrl)
        )
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetKindListEdit(out string title)
    {
      title = "Виды товаров";

      ParentBox kindBox = shop.fabricKindBox;

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HGrid<int>(kindBox.AllObjectIds,
            delegate (int kindId)
            {
              LightObject kind = new LightObject(kindBox, kindId);

              return new HPanel(
                DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("kind", kindId)).VAlign(-2).MarginRight(10),
                new HLabel(SEOProp.GetEditName(kind))
              ).Padding(5, 0);
            },
            new HRowStyle()
          ),
          new HPanel(
            DecorEdit.RedoButton(true, "Добавить вид товара", UrlHlp.EditUrl("kind", null))
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("Справочник", UrlHlp.EditUrl("shop-catalog", null))
        )
      );
    }

    static IHtmlControl GetKindAdd(EditState state, out string title)
    {
      title = "Добавление вида товара";

      string returnUrl = UrlHlp.EditUrl("kind-list", null);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
					DecorEdit.Field("Наименование", "identifier", "")
          //DecorEdit.Field("Наименование", "name", "")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить")
          .Event("add_kind", "addContent",
            delegate (JsonData json)
            {
							string editIdentifier = json.GetText("identifier");
              //string editName = json.GetText("name");

							if (StringHlp.IsEmpty(editIdentifier))
							{
								state.Operation.Message = "Не задано наименование свойства";
								return;
							}

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              ParentBox box = new ParentBox(fabricConnection, "1=0");
              int? createKindId = box.CreateUniqueObject(MetaKindType.FabricKind,
                MetaKindType.Identifier.CreateXmlIds(editIdentifier), null);

              if (createKindId == null)
              {
                state.Operation.Message = "Вид товара с таким наименованием уже существует";
                return;
              }

              LightParent editKind = new LightParent(box, createKindId.Value);
              state.CreatingObjectId = editKind.Id;

							//editKind.Set(SEOProp.Name, editName);
              editKind.Set(ObjectType.ActFrom, DateTime.UtcNow);
              editKind.Set(ObjectType.ActTill, DateTime.UtcNow);

              editKind.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton("Виды товаров", returnUrl)
        )
      ).EditContainer("addContent");
    }

    public static IHtmlControl GetKindEdit(EditState state, int? kindId, out string title)
    {
      if (kindId == null)
        kindId = state.CreatingObjectId;

      if (kindId == null)
        return GetKindAdd(state, out title);

      title = "Редактировать вид товара";

      ParentBox kindBox = shop.fabricKindBox;
      ObjectBox propertyBox = shop.fabricPropertyBox;

      if (kindId != null && !kindBox.ObjectById.Exist(kindId.Value))
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      LightParent editKind = state.Option.Get(EditOptionType.EditObject) as LightParent;
      if (editKind == null)
      {
        editKind = DataBox.LoadKin(fabricConnection, MetaKindType.FabricKind, kindId.Value);
        state.Option.Set(EditOptionType.EditObject, editKind);
      }

      List<int> linkedPropertyIds = new List<int>(editKind.AllChildIds(MetaKindType.PropertyLinks));
      List<int> vacantPropertyIds = new List<int>();
      foreach (int propertyId in propertyBox.AllObjectIds)
      {
        if (linkedPropertyIds.IndexOf(propertyId) < 0)
          vacantPropertyIds.Add(propertyId);
      }

      LightObject[] vacantProperties = MetaHlp.SortProperties(shop, vacantPropertyIds.ToArray());

      string returnUrl = UrlHlp.EditUrl("kind-list", null);

			string specialName = editKind.Get(SEOProp.Name);
			bool withSpecialName = !StringHlp.IsEmpty(specialName);

			return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
					DecorEdit.Field(withSpecialName ? "Идентификатор" : "Наименование",
						"identifier", editKind.Get(MetaKindType.Identifier)
					),
          DecorEdit.Field("Наименование", "name", editKind.Get(SEOProp.Name))
						.Display(withSpecialName ? "block" : "none"),
          DecorEdit.Field("Вид дизайна", "designKind", editKind.Get(MetaKindType.DesignKind)).MarginBottom(20),
          new HPanel(
            DecorEdit.Caption(new HLabel("Наличие и наценка для особенностей товара").Block()),
            new HPanel(
            new HGrid<int>(new int[] { 0, 1, 2, 3, 4 },
              delegate(int index)
              {
                return DecorEdit.Field("Особенность", string.Format("withFeature{0}", index),
                  editKind.Get(MetaKindType.WithFeatures, index)
                );
              },
              new HRowStyle()
            )).MarginLeft(10)
          ).MarginBottom(5),
          new HPanel(
            new HPanel(
              new HGrid<int>(
                DecorEdit.Caption(new HLabel("Свойства товара").Block()),
                linkedPropertyIds,
                delegate (int propertyId)
                {
                  LightObject property = new LightObject(propertyBox, propertyId);

                  return std.RowPanel(
                    new HInputRadio("linked", propertyId, false,
                      delegate (JsonData json)
                      {
                        state.Option.Set(EditOptionType.SelectedLinkedId, propertyId);
                      }
                    ).MarginLeft(5).MarginRight(8),
                    std.DockFill(
                      new HTextView(MetaHlp.PropertyToDisplay(shop, property))
                    )
                  ).Padding(5);
                },
                new HRowStyle().Even(new HTone().Background("#F9F9F9")),
                null
              )
            ).RelativeWidth(45).VAlign(true),
            new HPanel(
              new HPanel(
                new HButton("", std.BeforeAwesome(@"\f112", 0))
                  .Title("Добавить свойство в начало или перед выбранным свойством товара")
                  .Event("insert_before", "editContent", delegate(JsonData json)
                    {
                      int? vacantId = state.Option.Get(EditOptionType.SelectedVacantId);
                      if (vacantId == null)
                      {
                        state.Operation.Message = "Не выбрано свойство для вставки";
                        return;
                      }
                      int insertIndex = GetInsertIndex(linkedPropertyIds, 
                        state.Option.Get(EditOptionType.SelectedLinkedId), true
                      );
                      editKind.InsertChildLink(MetaKindType.PropertyLinks, insertIndex, vacantId.Value);
                      state.Option.Set(EditOptionType.SelectedVacantId, null);
                    }
                  )
              ).Align(null).MarginTop(32),
              new HPanel(
                new HButton("", std.BeforeAwesome(@"\f064", 0)).Transform("rotate(180deg)")
                  .Title("Добавить свойство в конец или после выбранного свойства товара")
                  .Event("insert_after", "editContent", delegate(JsonData json)
                    {
                      int? vacantId = state.Option.Get(EditOptionType.SelectedVacantId);
                      if (vacantId == null)
                      {
                        state.Operation.Message = "Не выбрано свойство для вставки";
                        return;
                      }
                      int insertIndex = GetInsertIndex(linkedPropertyIds, 
                        state.Option.Get(EditOptionType.SelectedLinkedId), false
                      );
                      editKind.InsertChildLink(MetaKindType.PropertyLinks, insertIndex, vacantId.Value);
                      state.Option.Set(EditOptionType.SelectedVacantId, null);
                    }
                  )
              ).Align(null).MarginTop(5),
              new HPanel(
                new HButton("", std.BeforeAwesome(@"\f178", 0))
                  .Title("Убрать выбранное свойство из товара")
                  .Event("make_vacant", "editContent", delegate(JsonData json)
                    {
                      int? linkedId = state.Option.Get(EditOptionType.SelectedLinkedId);
                      if (linkedId == null)
                      {
                        state.Operation.Message = "Не выбрано свойство для удаления из товара";
                        return;
                      }
                      int removeIndex = linkedPropertyIds.IndexOf(linkedId.Value);
                      if (removeIndex < 0)
                        return;

                      editKind.RemoveChildLink(MetaKindType.PropertyLinks, removeIndex);
                      state.Option.Set(EditOptionType.SelectedLinkedId, null);
                    }
                  )
              ).Align(null).MarginTop(10)
            ).RelativeWidth(10).FontSize("2em"),
            new HPanel(
              new HGrid<LightObject>(
                DecorEdit.Caption(new HLabel("Несвязанные свойства").Block()),
                vacantProperties,
                delegate (LightObject property)
                {
                  return std.RowPanel(
                    new HInputRadio("vacant", property.Id, false,
                      delegate (JsonData json)
                      {
                        state.Option.Set(EditOptionType.SelectedVacantId, property.Id);
                      }
                    ).MarginLeft(5).MarginRight(8),
                    std.DockFill(
                      new HTextView(MetaHlp.PropertyToDisplay(shop, property))
                    )
                  ).Padding(5);
                },
                new HRowStyle().Even(new HTone().Background("#F9F9F9")),
                null
              )
            ).RelativeWidth(45).VAlign(true)
          ),
          new HPanel(
            DecorEdit.RedoButton(true, "Свойства товаров", UrlHlp.EditUrl("property-list", null))
              .TargetBlank().Title("Редактировать свойства товаров в новой вкладке")
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .Event("save_kind", "editContent",
            delegate (JsonData json)
            {
							string editIdentifier = json.GetText("identifier");
              string editName = json.GetText("name");
              string editDesignKind = json.GetText("designKind");

              if (StringHlp.IsEmpty(editIdentifier))
              {
                state.Operation.Message = "Не задано наименование вида товара";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              if (!MetaKindType.Identifier.SetWithCheck(editKind.Box, kindId.Value, editIdentifier))
              {
                state.Operation.Message = "Другой вид товара с таким идентификатором уже существует";
                return;
              }

							editKind.Set(SEOProp.Name, editName);
              editKind.Set(MetaKindType.DesignKind, editDesignKind);
              editKind.Set(ObjectType.ActTill, DateTime.UtcNow);

              for (int i = 0; i < 5; ++i)
              {
                string editFeature = json.GetText(string.Format("withFeature{0}", i));
                editKind.Set(MetaKindType.WithFeatures, i, editFeature);
              }

              editKind.Box.Update();

              SiteContext.Default.UpdateStore();

							state.Operation.Message = "Изменения успешно сохранены";
							state.Operation.Status = "success";
						}
          ),
          DecorEdit.ReturnButton("Виды товаров", returnUrl)
        )
      ).EditContainer("editContent");
    }

    static int GetInsertIndex(List<int> idList, int? id, bool before)
    {
      int defaultIndex = before ? 0 : idList.Count;

      if (id == null)
        return defaultIndex;

      int insertIndex = idList.IndexOf(id.Value);
      if (insertIndex < 0)
        return defaultIndex;

      return before ? insertIndex : insertIndex + 1;
    }

    public static IHtmlControl GetPropertyListEdit(out string title)
    {
      title = "Свойства товаров";

      ObjectBox propertyBox = shop.fabricPropertyBox;

      LightObject[] sortProperties = MetaHlp.SortProperties(shop, propertyBox.AllObjectIds);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HGrid<LightObject>(sortProperties,
            delegate (LightObject property)
            {
              return new HPanel(
                DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("property", property.Id)).VAlign(-2).MarginRight(10),
                new HTextView(MetaHlp.PropertyToDisplay(shop, property)).InlineBlock()
                  //.FontBold(property.Get(MetaPropertyType.IsPrior))
              ).Padding(5, 0);
            },
            new HRowStyle()
          ),
          new HPanel(
            DecorEdit.RedoButton(true, "Добавить свойство", UrlHlp.EditUrl("property", null))
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("Виды товаров", UrlHlp.EditUrl("kind-list", null))
        )
      );
    }

    static IHtmlControl GetPropertyAdd(EditState state, out string title)
    {
      title = "Добавить свойство";

      string returnUrl = UrlHlp.EditUrl("property-list", null);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
					DecorEdit.Field("Наименование", "name", ""),
					DecorEdit.Field("Идентификатор", "identifier", ""),
					DecorEdit.Field("Вид свойства",
            new HComboEdit<string>("type", "",
              ArrayHlp.Convert(context.SearchTunes.All, delegate (SearchTune tune)
							{
								return new Option(tune.PropertyKind, tune.DisplayName);
							})
            )
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить")
          .Event("add_property", "addContent",
            delegate (JsonData json)
            {
							string editName = json.GetText("name");
							string editIdentifier = json.GetText("identifier");
              string editType = json.GetText("type");

							bool withoutIdentifier = StringHlp.IsEmpty(editIdentifier);

							// hack идентификатор необязателен
							if (withoutIdentifier)
								editIdentifier = editName;

              if (StringHlp.IsEmpty(editIdentifier) || (!withoutIdentifier && StringHlp.IsEmpty(editName)))
              {
                state.Operation.Message = "Не задано найменование свойства";
                return;
              }
              if (StringHlp.IsEmpty(editType))
              {
                state.Operation.Message = "Не задан тип свойства";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              ObjectBox box = new ObjectBox(fabricConnection, "1=0");
              int? createPropertyId = box.CreateUniqueObject(MetaPropertyType.Property,
                MetaPropertyType.Identifier.CreateXmlIds(editIdentifier), null);

              if (createPropertyId == null)
              {
                state.Operation.Message = "Свойство с таким идентификатором уже существует";
                return;
              }

              LightObject editProperty = new LightObject(box, createPropertyId.Value);
              editProperty.Set(ObjectType.ActFrom, DateTime.UtcNow);

              state.CreatingObjectId = editProperty.Id;

              editProperty.Set(ObjectType.ActTill, DateTime.UtcNow);

							if (!withoutIdentifier)
								editProperty.Set(SEOProp.Name, editName);

              editProperty.Set(MetaPropertyType.Kind, editType);

              editProperty.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton("Все свойства", returnUrl)
        )
      ).EditContainer("addContent");
    }

    public static IHtmlControl GetPropertyEdit(EditState state, int? propertyId, out string title)
    {
      if (propertyId == null)
        propertyId = state.CreatingObjectId;

      title = propertyId == null ? "Добавить свойство" : "Редактировать свойство";

      ObjectBox propertyBox = shop.fabricPropertyBox;

      if (propertyId != null && !propertyBox.ObjectById.Exist(propertyId.Value))
        return EditHlp.GetInfoMessage("Неверный аргумент", "/");

      if (propertyId == null)
        return GetPropertyAdd(state, out title);

      LightObject viewProperty = state.Option.Get(EditOptionType.EditObject);
      //Logger.AddMessage("EditProperty: {0}, {1}", propertyId, viewProperty != null);
      if (viewProperty == null)
      {
        viewProperty = DataBox.LoadKin(fabricConnection, MetaPropertyType.Property, propertyId.Value);
        state.Option.Set(EditOptionType.EditObject, viewProperty);
      }

      string returnUrl = UrlHlp.EditUrl("property-list", null);

      string propertyKind = viewProperty.Get(MetaPropertyType.Kind);
			string valueType = context.SearchTunes.FindIndexType(propertyKind)?.IndexType;

			string specialName = viewProperty.Get(SEOProp.Name);
			bool withSpecialName = !StringHlp.IsEmpty(specialName);

			return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
					DecorEdit.Field(withSpecialName ? "Идентификатор" : "Наименование",
						"identifier", viewProperty.Get(MetaPropertyType.Identifier)
					),
          DecorEdit.Field("Наименование", "name", viewProperty.Get(SEOProp.Name))
						.Display(withSpecialName ? "block" : "none"),
					DecorEdit.Field("Короткое наименование", "shortName", viewProperty.Get(MetaPropertyType.ShortName)),
					DecorEdit.Field("Вид свойства",
						new HComboEdit<string>("kind", propertyKind,
							ArrayHlp.Convert(context.SearchTunes.All, delegate (SearchTune tune)
							{
								return new Option(tune.PropertyKind, tune.DisplayName);
							})
						)
					),
					DecorEdit.Field("Тип значения", new HLabel(MetaPropertyType.KindToDisplay(valueType))),
          DecorEdit.Field(new HLabel("Важное свойство").FontBold().Title("Выводится вверху фильтра"),
            new HPanel(
              new HInputCheck("prior", viewProperty.Get(MetaPropertyType.IsPrior)).Width("100%")
            )
          ),
          DecorEdit.Field(
            new HPanel(
              new HLabel("Категория").FontBold(),
              DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("category-list", null))
                .TargetBlank()
                .MarginLeft(2).Title("Редактировать категории свойств в новой вкладке")
            ),
            new HComboEdit<int?>("category", viewProperty.Get(MetaPropertyType.Category),
              delegate(int? categoryId) 
              {
                return shop.FindPropertyCategory(categoryId)?.Get(MetaCategoryType.DisplayName);
              }, shop.AllPropertyCategoryIds
            )
          ),
          DecorEdit.Field("Единица измерения", "unit", viewProperty.Get(MetaPropertyType.MeasureUnit))
            .Hide(valueType != "numerical"),
          DecorEdit.Field("Множественное значение",
            new HPanel(
              new HInputCheck("multiple", viewProperty.Get(MetaPropertyType.IsMultiple)).Width("100%")
            )
          ).MarginBottom(20),
          DecorEdit.FieldBlock("Варианты для перечисления",
            new HPanel(
              new HPanel(
                new HTextEdit("variant"),
                new HButton("", std.BeforeAwesome(@"\f067", 6).Color("#3cf33d").VAlign(-1)).MarginLeft(5)
                  .Event("add_variant", "editContent", delegate(JsonData json)
                    {
                      string variant = json.GetText("variant");
                      if (StringHlp.IsEmpty(variant))
                      {
                        state.Operation.Message = "Новый вариант для перечисления не задан";
                        return;
                      }
                      LightObject editProperty = state.Option.Get(EditOptionType.EditObject);
                      editProperty.AddProperty(MetaPropertyType.EnumItems, variant);
                    }
                  )
              ),
              new HGrid<RowLink>(viewProperty.AllPropertyRows(MetaPropertyType.EnumItems),
                delegate(RowLink row)
                {
                  return std.RowPanel(
                    new HLabel(row.Get(PropertyType.PropertyValue)),
                    std.DockFill(),
                    new HButton(" ", std.AfterAwesome(@"\f00d", 6).Color("#f05120"))
                      .Event("remove_variant", "editContent", delegate(JsonData json)
                        {
                          LightObject editProperty = state.Option.Get(EditOptionType.EditObject);
                          editProperty.RemoveProperty(MetaPropertyType.EnumItems, 
                            row.Get(PropertyType.PropertyIndex));
                        },
                        row.Get(PropertyType.PropertyIndex)
                      )
                  ).NoWrap().Padding(5);
                },
                new HRowStyle()
              )
            ).Padding(8)
          ).Hide(valueType != "enum"),
          DecorEdit.FieldInputBlock("Подсказка",
            HtmlHlp.CKEditorCreate("hint", viewProperty.Get(MetaPropertyType.Hint),
              "250px", true)
          )
          //new HPanel(
          //  std.Button("Удалить").Event("delete_redirect", "",
          //    delegate (JsonData json)
          //    {
          //      if (propertyId == null)
          //        return;

        //      SQLiteDatabaseHlp.DeleteObject(fabricConnection, propertyId.Value);
        //      Logger.AddMessage("Свойство товара '{0}' успешно удалено", propertyId.Value);

        //      state.Operation.Complete("SEO виджет успешно удален", returnUrl);

        //      ShopContext.Default.UpdateStore();
        //    }
        //  )
        //).Hide(property == null).Align(false).MarginTop(10)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_property", "editContent",
            delegate (JsonData json)
            {
							string editIdentifier = json.GetText("identifier");
              string editName = json.GetText("name");
							string editShortName = json.GetText("shortName");
							string editKind = json.GetText("kind");
							//string editViewer = json.GetText("viewer");
              //string editMarking = json.GetText("marking");
              string editMeasureUnit = json.GetText("unit");
              bool editPrior = json.GetText("prior")?.ToLower() == "true";
              bool editMultiple = json.GetText("multiple")?.ToLower() == "true";
              string editHint = json.GetText("hint");

              int? categoryId = ConvertHlp.ToInt(json.GetText("category"));

              if (StringHlp.IsEmpty(editIdentifier))
              {
                state.Operation.Message = "Не задано найменование свойства";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              string xmlIds = MetaPropertyType.Identifier.CreateXmlIds(editIdentifier);

              LightObject editProperty = state.Option.Get(EditOptionType.EditObject);

              if (!editProperty.Box.ObjectUniqueChecker.IsUniqueKey(editProperty.Id,
                editProperty.Get(ObjectType.TypeId), xmlIds, null))
              {
                state.Operation.Message = "Другое свойство с таким идентификатором уже существует";
                return;
              }

              editProperty.Set(ObjectType.XmlObjectIds, xmlIds);

              editProperty.Set(ObjectType.ActTill, DateTime.UtcNow);

							editProperty.Set(SEOProp.Name, editName);
							editProperty.Set(MetaPropertyType.ShortName, editShortName);
							editProperty.Set(MetaPropertyType.Kind, editKind);
              editProperty.Set(MetaPropertyType.IsPrior, editPrior);
              editProperty.Set(MetaPropertyType.MeasureUnit, editMeasureUnit);
              editProperty.Set(MetaPropertyType.IsMultiple, editMultiple);
              editProperty.Set(MetaPropertyType.Category, categoryId);
              editProperty.Set(MetaPropertyType.Hint, editHint);

              editProperty.Box.Update();

              SiteContext.Default.UpdateStore();

							state.Operation.Message = "Изменения успешно сохранены";
							state.Operation.Status = "success";
						},
            propertyId.Value
          ),
          DecorEdit.ReturnButton("Все свойства", UrlHlp.EditUrl("property-list", null))
        )
      ).EditContainer("editContent");
    }

  }
}