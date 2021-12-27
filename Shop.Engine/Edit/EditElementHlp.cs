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
  public class EditElementHlp
  {
    const int tilePadding = 12;
    const int tileWidth = 164;
    public static int thumbWidth = 236;
    const int thumbPadding = 12;

    public static IHtmlControl GetDeletePanel(EditState state, int objectId,
      string objectKind, string dialogTitle,
      Getter<bool> actionEnabler)
    {
      string popupDialog = string.Format("delete_{0}", objectId);
      return GetActionPanel(state, popupDialog, "Удалить", objectKind, dialogTitle, actionEnabler,
        delegate
        {
          SQLiteDatabaseHlp.DeleteParentObject(SiteContext.Default.FabricConnection, objectId);
          Logger.AddMessage("{0} '{1}' успешно удален", StringHlp.ToUpper(objectKind, 1), objectId);

          state.Operation.Complete(string.Format("{0} успешно удален", StringHlp.ToUpper(objectKind, 1)), "/");

          SiteContext.Default.UpdateStore();
        }
      );
    }

    public static IHtmlControl GetActionPanel(EditState state, string popupDialog, 
      string actionDisplay, string objectKind, string dialogTitle,
      Getter<bool> actionEnabler, Executter action)
    {
      return new HPanel(
        std.Button(string.Format("{0} {1}", actionDisplay, objectKind))
        .Event("action_object", "",
          delegate (JsonData json)
          {
            state.PopupDialog = popupDialog;
          }
        ),
        EditElementHlp.GetPopupDialog(state, dialogTitle,
          string.Format("Вы уверены, что хотите {0} {1}?", actionDisplay.ToLower(), objectKind), actionDisplay,
          delegate
          {
            if (HttpContext.Current.IsInRole("nosave"))
            {
              state.Operation.Message = "Нет прав на сохранение изменений";
              return;
            }

            if (actionEnabler != null)
            {
              if (!actionEnabler())
                return;
            }

            action();

            //SQLiteDatabaseHlp.DeleteParentObject(SiteContext.Default.FabricConnection, objectId);
            //Logger.AddMessage("{0} '{1}' успешно удален", StringHlp.ToUpper(objectKind, 1), objectId);

            //state.Operation.Complete(string.Format("{0} успешно удален", StringHlp.ToUpper(objectKind, 1)), "/");

            //SiteContext.Default.UpdateStore();
          }
        ).Hide(state.PopupDialog != popupDialog)
      ).Align(false).Margin(5, 10);
    }

    public static IHtmlControl GetPopupDialog(EditState state, 
      string title, string question, string actionCaption, Executter action)
    {
      return new HPanel(
        new HLabel(title).Block().FontBold().MarginBottom(10),
        new HTextView(question),
        new HPanel(
          std.Button(actionCaption, 4, 18, new HHover().Background("#C51A3C").Color("#fff"))
            .MarginRight(24)
            .Event("dialog_action", "", delegate { action(); }),
          std.Button("Отмена", 4, 18).Event("dialog_cancel", "", delegate { state.PopupDialog = ""; })
        ).Align(null).MarginTop(14),
        new HButton("",
          std.AfterAwesome(@"\f00d", 0).FontSize("1.5em"),
          new HHover().Color("#C51A3C")
        ).PositionAbsolute().Right("10px").Top("10px").Color("#dedede")
        .Event("dialog_close", "", delegate { state.PopupDialog = ""; })
      ).Width(290).Align(true)
      .FontSize(14).LineHeight(18)
      .Background("#ffffff")
      .Right("285px").MarginTop(10)
      //.MarginLeft(16).MarginTop(-8)
      .PaddingLeft(13).PaddingRight(17).PaddingTop(11).PaddingBottom(12)
      .PositionAbsolute().ZIndex(10000)
      .BoxShadow("0px 2px 10px 0px rgba(128, 0, 0, 1)");
      //.BoxShadow("0px 2px 10px 0px rgba(63, 69, 75, 0.5)");
    }

    public static IHtmlControl[] GetFilterRows(IList<ISearchIndex> indices, SearchFilter filter)
    {
      List<IHtmlControl> controls = new List<IHtmlControl>();
      for (int i = 0; i < indices.Count; i += 2)
      {
        IHtmlControl filterRow = EditElementHlp.GetFilterRow(indices, i, filter);
        controls.Add(filterRow);
      }
      return controls.ToArray();
    }

    public static IHtmlControl GetFilterRow(IList<ISearchIndex> indices, int index, SearchFilter filter)
    {
      return new HPanel(
        GetFilterField(index < indices.Count ? indices[index] : null, filter).RelativeWidth(50),
        GetFilterField(index + 1 < indices.Count ? indices[index + 1] : null, filter).RelativeWidth(50)
      ).MarginBottom(10)
      .Media(1024, new HStyle(".{0} > div").MediaBlock(true).MarginBottom(10))
      .Media(664, new HStyle(".{0} > div").PaddingLeft(20));
    }

    public static IHtmlControl GetFilterField(ISearchIndex index, SearchFilter filter)
    {
      if (index == null)
        return new HPanel().Hide(true);

      return new HPanel(
        new HLabel(index.Property.Get(MetaPropertyType.Identifier)).Width(150).MarginRight(20)
          .Color(DecorEdit.propertyColor),
        GetFilterEditControl(index, filter)
      ).VAlign(null);

    }

    public static IHtmlControl GetFilterEditControl(ISearchIndex index, SearchFilter filter)
    {
      SearchCondition condition = filter?.FindCondition(index.Property.Id);

      if (index is EnumSearchIndex)
      {
        EnumSearchCondition enumCondition = condition as EnumSearchCondition;
        string value = enumCondition != null ? enumCondition.Value : "";

        return new HComboEdit<string>(string.Format("property_{0}", index.Property.Id),
          value, delegate (string item) { return item; }, ((EnumSearchIndex)index).SortedEnumVariants
        ).MarginLeft("18.7px").WidthLimit("106px", "131px");
      }
      else if (index is NumericalSearchIndex)
      {
        NumericalSearchCondition numericalCondition = condition as NumericalSearchCondition;
        string min = numericalCondition?.Min?.ToString();
        string max = numericalCondition?.Max?.ToString();

        return new HPanel(
          new HLabel("от").Color(DecorEdit.propertyMinorColor),
          new HTextEdit(string.Format("property_{0}_min", index.Property.Id), min).Align(null)
            .Width("3em").MarginLeft(5).MarginRight(5),
          new HLabel("до").Color(DecorEdit.propertyMinorColor),
          new HTextEdit(string.Format("property_{0}_max", index.Property.Id), max).Align(null)
            .Width("3em").MarginLeft(5).MarginRight(5),
          new HLabel(index.Property.Get(MetaPropertyType.MeasureUnit)).Color(DecorEdit.propertyMinorColor)
        ).InlineBlock();
      }

      return new HPanel().Hide(true);
    }

    public static IHtmlControl GetFabricKindCombo(ShopStorage store, int? selectedKindId)
    {
      return DecorEdit.Field(
        new HPanel(
          new HLabel("Вид товара").FontBold(),
          DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("kind-list", null))
            .TargetBlank()
            .MarginLeft(2).Title("Редактировать виды товара в новой вкладке")
        ),
        new HComboEdit<int?>("fabricKind", selectedKindId,
          delegate (int? kindId)
          {
            return MetaKind.ToDisplayName(store.FindFabricKind(kindId));
          },
          store.AllFabricKindIds
        )
      );
    }

		public static IHtmlControl GetDesignKindCombo(EditorSelector selector, string designKind)
		{
			return DecorEdit.Field(
				new HLabel("Вид дизайна").FontBold(),
				new HComboEdit<string>("designKind", designKind,
					delegate (string kind)
					{
						return selector.GetDisplayName(kind);
					},
					selector.AllKinds
				)
			);
		}

    static IHtmlControl GetPropertyEdit(MetaProperty property, LightObject fabric)
    {
      string type = property.Get(MetaPropertyType.Kind);

      string dataName = string.Format("property_{0}", property.Id);

      string value = fabric.Get(property.Blank);

      switch (type)
      {
        case "numerical":
          return new HTextEdit(dataName, value);
        case "enum":
          string[] enumItems = ArrayHlp.Convert(property.AllPropertyRows(MetaPropertyType.EnumItems),
            delegate (RowLink row) { return row.Get(PropertyType.PropertyValue); }
          );
          return std.RowPanel(
            std.DockFill(
              new HComboEdit<string>(dataName, value, null, ArrayHlp.Merge(new string[] { "" }, enumItems )
              )
            ),
            new HLink(UrlHlp.EditUrl("property", property.Id),
              new HButton("", std.BeforeAwesome(@"\f067", 0).VAlign(-1))
                .Title("Добавить вариант в перечисление в новой вкладке")
            ).Padding(0, 4).TargetBlank()
          );
        default:
          return new HTextEdit(dataName, value);
      }

    }

    public static IHtmlControl GetPropertiesPanel(MetaKind kind, LightKin fabric)
    {
      if (kind == null)
        return new HPanel().Hide(true);

      List<IHtmlControl> controls = new List<IHtmlControl>();
      foreach (MetaProperty property in kind.Properties)
      {
        string label = MetaPropertyType.DisplayName(property);
        string measureUnit = property.Get(MetaPropertyType.MeasureUnit);
        if (!StringHlp.IsEmpty(measureUnit))
          label = string.Format("{0}, {1}", label, measureUnit);
        IHtmlControl editControl = GetPropertyEdit(property, fabric);
        if (editControl != null)
          controls.Add(DecorEdit.Field(label, editControl));
      }

      return DecorEdit.FieldBlock(
        new HPanel(
          new HLabel("Характеристики товара"),
          DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("kind", kind.Id))
            .MarginLeft(4).FontBold(false).TargetBlank()
            .Title("Редактировать вид товара в новой вкладке")
        ),
        new HPanel(controls.ToArray()).Padding(5, 8)
      );
    }

    public static IHtmlControl GetImagePanel(string imageUrl, string imageCaption, int size)
    {
      return GetImagePanel(imageUrl, imageCaption, size, size);
    }

    public static IHtmlControl GetImagePanel(string imageUrl, string imageCaption, int width, int height)
    {
      return new HPanel(
        new HSpan("").Height("100%").VAlign(null).InlineBlock(),
        new HImage(imageUrl).VAlign(null).TagAttribute("alt", imageCaption)
          .HeightLimit("", string.Format("{0}px", height))
      ).Size(width, height).Align(null);
    }

    public static IHtmlControl GetTechnicalTile(string url, string caption, int thumbSize)
    {
      return new HPanel(
        new HPanel().Size(thumbSize, thumbSize),
        DecorEdit.RedoButton(true, caption, url).Block()
      ).InlineBlock().Width(tileWidth).Padding(thumbPadding);
    }

    public static IHtmlControl GetVarietyTile(bool isAdmin, Product product, int thumbSize)
    {
      string varietyName = product.VarietyName;
      string shopUrl = UrlHlp.ShopUrl("product", product.ProductId);
      return new HPanel(
        new HLink(shopUrl,
          GetImagePanel(UrlHlp.ImageUrl(product.ImageId, false), product.ProductName, thumbSize)
        ),
        new HLink(shopUrl,
          new HPanel(
            new HLabel(varietyName).PaddingTop(10).Color("#000")
          ).Align(null)
        ).Block().Align(true)
          .LineHeight("1.2em").FontBold(),
        //new HLabel(product.Annotation)
        //  .Block().Color("#888").LineHeight("15px"),
        DecorEdit.RedoButton(isAdmin, "Редактировать",
          UrlHlp.EditUrl(product.Id, "variety", product.VarietyId)
        ).Block()
      ).Width(tileWidth).Padding(thumbPadding).InlineBlock().VAlign(true);
    }

    public static IHtmlControl GetVarietiesPanel(ShopStorage store, LightKin fabric)
    {
      List<IHtmlControl> tileControls = new List<IHtmlControl>();
      foreach (int varietyId in fabric.AllChildIds(FabricType.VarietyTypeLink))
      {
        Product product = store.FindProduct(varietyId);
        if (product == null)
          continue;

        tileControls.Add(EditElementHlp.GetVarietyTile(true, product, tileWidth));
      }

      string caption = tileControls.Count != 0 ? "Разновидности этого товара" :
        "Нет разновидностей этого товара";

      if (tileControls.Count == 0)
      {
        tileControls.Add(
          new HPanel(
            DecorEdit.RedoButton(true, "Добавить", UrlHlp.EditUrl(fabric.Id, "variety", null))
          ).MarginLeft(8).MarginBottom(8)
        );
      }
      else
      {
        tileControls.Add(
          EditElementHlp.GetTechnicalTile(
            UrlHlp.EditUrl(fabric.Id, "variety", null),
            "Добавить", tileWidth
          )
        );
      }

      return DecorEdit.FieldBlock(
        caption,
        new HPanel(tileControls.ToArray())
      );
    }

    public static IHtmlControl GetImageThumb(int objectId)
    {
      return GetImageThumb(objectId, null);
    }

		public static IHtmlControl GetImageThumb(int objectId, TuneContainer<bool> tunes)
		{
			return GetImageThumb(objectId, null, tunes);
		}

    public static IHtmlControl GetImageThumb(int objectId, string subfolder, TuneContainer<bool> tunes)
    {
			List<HAttribute> attributes = new List<HAttribute>();

			if (subfolder != null)
				attributes.Add(new HAttribute("subfolder", subfolder));

      if (tunes != null)
      {
				attributes.AddRange(
					new HAttribute[] {
						new HAttribute("tileWidth", tunes.GetSetting("tileWidth")),
						new HAttribute("tileHeight", tunes.GetSetting("tileHeight")),
						new HAttribute("fullFillingTile", tunes.GetSetting("fullFillingTile")),
						new HAttribute("jpegTile", tunes.GetSetting("jpegTile"))
					}
				);
      }

      string size = string.Format("{0}px", thumbWidth);
      return new HPanel(
        //new HImage(UrlHlp.ImageUrl(objectId, true))
        //  .WidthLimit(size, size),
          //.InlineBlock().WidthFixed(thumbWidth).HeightFixed(thumbWidth),
				new HPanel(
				).InlineBlock().Size(thumbWidth, thumbWidth)
					.Background(UrlHlp.ImageEditUrl(objectId.ToString(), subfolder, "original.jpg"), 
						"no-repeat", "center"
					).BackgroundSize("contain"),
        new HPanel(
          std.Button("Обновить изображение").Event("refresh_thumb", "", delegate { }).MarginBottom(10),
          new HPanel(),
          std.Button("Удалить изображение").Event("delete_thumb", "",
            delegate
            {
              File.Delete(UrlHlp.ImagePath(objectId.ToString(), subfolder, "original.jpg"));
              File.Delete(UrlHlp.ImagePath(objectId.ToString(), subfolder, "thumb.png"));
            }
          ).MarginBottom(10),
          new HFileUploader("/tileUpload", "Загрузить изображение", objectId, attributes.ToArray())
        ).Align(true).Padding(10)
      );
    }

		public static IHtmlControl GetAdaptImage(int objectId)
		{
			string size = string.Format("{0}px", thumbWidth);
			return new HPanel(
				new HPanel(
				).InlineBlock().Size(thumbWidth, thumbWidth)
					.Background(UrlHlp.ImageUrl(objectId, "adapt.jpg"), "no-repeat", "center").BackgroundSize("contain"),
				new HPanel(
					std.Button("Обновить изображение").Event("refresh_adapt", "", delegate { }).MarginBottom(10),
					new HPanel(),
					std.Button("Удалить изображение").Event("delete_adapt", "",
						delegate
						{
							File.Delete(UrlHlp.ImagePath(objectId.ToString(), "adapt.jpg"));
						}
					).MarginBottom(10),
					new HFileUploader("/tileUpload", "Загрузить изображение", objectId,
						new HAttribute("imageName", "adapt.jpg")
					)
				).Align(true).Padding(10)
			);
		}

		public static IHtmlControl GetImageTile(int objectId, string subfolder, 
			string imageName, int index, bool allowDeletion)
    {
			//string imageUrl = string.Format("/images/{0}/{1}", objectId, imageName);
			string imageUrl = UrlHlp.ImageEditUrl(objectId.ToString(), subfolder, imageName);
			return new HPanel(
        EditElementHlp.GetImagePanel(imageUrl, "", tileWidth),
				new HPanel(
					new HLink(imageUrl, "Ссылка"),
					new HButton("", std.BeforeAwesome(@"\f00d", 0).Color("red")).Hide(!allowDeletion)
						.PositionAbsolute().Right(2).Top(0).Title("Удалить изображение")
						.Event(string.Format("delete_{0}", imageName), "",
							delegate (JsonData json)
							{
								string imagePath = UrlHlp.ImagePath(objectId.ToString(), subfolder, imageName);
								if (File.Exists(imagePath))
								{
									File.Delete(imagePath);
									Logger.AddMessage("Файл '{0}' успешно удален", imagePath);
								}
							}
						)
				).PositionRelative()
        //new HPanel(
        //  std.Button("Удалить").Event(string.Format("delete_{0}", imageName), "",
        //    delegate (JsonData json)
        //    {
        //      string imagePath = Path.Combine(SiteContext.Default.ImagesPath,
        //        objectId.ToString(), imageName);
        //      if (File.Exists(imagePath))
        //      {
        //        File.Delete(imagePath);
        //        Logger.AddMessage("Файл '{0}' успешно удален", imagePath);
        //      }
        //    }
        //  )
        //).Align(false)
      ).Width(tileWidth).Padding(tilePadding).InlineBlock().VAlign(true);
    }

    public static IHtmlControl GetPropertiesTable(int objectId, BaseTunes tunes,
			IHtmlControl deletePanel, params IHtmlControl[] propertyControls)
    {
      IHtmlControl[] allControls = propertyControls;
      if (deletePanel != null)
      {
        allControls = ArrayHlp.Merge(propertyControls,
          new IHtmlControl[] { deletePanel.Margin(0).MarginTop(5) }
        );
      }

      return new HXPanel(
        EditElementHlp.GetImageThumb(objectId, tunes),
        std.DockFill(
          new HPanel(allControls).PaddingLeft(10)
        )
      ).MarginBottom(10).MarginRight(10);
    }

    public static IHtmlControl GetButtonsPanel(params IHtmlControl[] buttons)
    {
      foreach (IHtmlControl button in buttons)
        button.MarginRight(20);

      return new HPanel(buttons).PaddingTop(12).PaddingBottom(18).Align(false)
        .Background("#fafafa").LinearGradient("to top", "#fafafa", "#ddd").FontSize("1.25em");
    }

		public static IHtmlControl GetDescriptionImagesPanel(VirtualRowLink option, int objectId)
		{
			return GetDescriptionImagesPanel(option, objectId, null);
		}

    public static IHtmlControl GetDescriptionImagesPanel(VirtualRowLink option, int objectId, string subfolder)
    {
			bool allow = option.Get(EditOptionType.AllowImageDeletion);

			List<IHtmlControl> tileControls = new List<IHtmlControl>();
      int i = -1;
      foreach (string imageName in FabricHlp.GetImageNamesForDescription(objectId, subfolder))
      {
        ++i;
        IHtmlControl tileControl = GetImageTile(objectId, subfolder, imageName, i, allow);
        tileControls.Add(tileControl);
      }

      string caption = tileControls.Count != 0 ? "Изображения для вставки в текст" :
        "Нет изображений для вставки в текст";

			HAttribute[] attributes = new HAttribute[0];
			if (!StringHlp.IsEmpty(subfolder))
				attributes = new HAttribute[] { new HAttribute("subfolder", subfolder) };

      return DecorEdit.FieldBlock(
        caption,
        new HPanel(
          new HPanel(
            tileControls.ToArray()
          ).EditContainer(string.Format("image_dsc_{0}_{1}", objectId, i)),
          new HPanel(
            new HFileUploader("/filesupload", "Загрузить изображение", objectId, attributes)
          ).MarginTop(10).MarginLeft(8).MarginBottom(8),
					new HButton("", std.BeforeAwesome(@"\f1e2", 0)).Title("Разрешить удаление изображений")
						.Color(allow ? "red" : DecorEdit.propertyColor)
						.PositionAbsolute().Top(5).Right(5)
						.Event("images_delete", "", delegate
							{
								bool editAllow = option.Get(EditOptionType.AllowImageDeletion);
								option.Set(EditOptionType.AllowImageDeletion, !editAllow);
							},
							objectId
						)
        )
      ).PositionRelative();
    }

    public static IHtmlControl GetOperationPopup(WebOperation operation)
    {
      if (StringHlp.IsEmpty(operation.Message))
        return null;

      bool isWarning = StringHlp.IsEmpty(operation.Status);

      return new HPanel(
        new HTextView(operation.Message),
        new HButton("",
          std.AfterAwesome(@"\f00d", 0).FontSize("1.5em"),
          new HHover().Color("#C51A3C")
        ).PositionAbsolute().Right("10px").Top("10px").Color("#dedede")
        .Event("operation_reset", "", delegate
          {
            operation.Reset();
          }
        )
      ).Width(200).FontSize(14).LineHeight(18)
      .Background("#ffffff")
      .PaddingLeft(13).PaddingRight(25).PaddingTop(11).PaddingBottom(12)
      .Position("fixed").Left(10).Bottom(10) //.Right(10).Top(10)
      .BorderRadius(5)
      .BoxShadow(isWarning ? "0px 2px 10px 0px rgba(128, 0, 0, 1)" : "0px 2px 10px 0px rgba(0, 96, 0, 1)");
      //.BoxShadow("0px 2px 10px 0px rgba(63, 69, 75, 0.5)");
    }
  }
}