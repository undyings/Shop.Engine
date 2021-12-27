using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Basis;
using Commune.Html;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;

namespace Shop.Prototype
{
  public class ViewBedHlp
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

    public static IHtmlControl GetBedView(HttpContext httpContext, 
      ShopState state, LightGroup group, Product product)
    {
      string productName = product.ProductName;

      return new HPanel(
        ViewElementHlp.GetViewTitle(productName),
        new HPanel(
          new HPanel(
            new HEventPanel(
              new HImage(UrlHlp.ImageUrl(product.ImageId, true))
            ).OnClick(";").Event("popupPhotoView", "", delegate (JsonData json)
              {
                state.PopupHint = "photoView";
              })
          ).Padding(18).BoxShadow("rgba(0, 0, 0, 0.1) 0px 0px 8px 0px"),
          GetFeatureColumn(httpContext, state,group, product).PositionAbsolute().Top(0).Right(0),
          GetPopupPhotoView(state, product),
          GetPopupClothView(state, product)
        ).PositionRelative().PaddingRight(228),
        new HTextView(product.Get(FabricType.Description)).MarginTop(30).LineHeight("1.65em")
      );
    }

    static IHtmlControl GetFeatureColumn(HttpContext httpContext, ShopState state, LightGroup group, Product product)
    {
      VirtualRowLink option = state.Option;

      LightFeature widthFeature = shop.FindFeature("Ширина кровати");
      LightFeature lengthFeature = shop.FindFeature("Длина кровати");

      List<Tuple<string, string>> sizes = new List<Tuple<string, string>>();
      if (widthFeature != null && lengthFeature != null)
      {
        foreach (LightObject widthValue in widthFeature.FeatureValues)
          foreach (LightObject heightValue in lengthFeature.FeatureValues)
          {
            sizes.Add(_.Tuple(
              string.Format("{0},{1}", widthValue.Id, heightValue.Id),
              string.Format("{0}x{1}", 
                widthValue.Get(FeatureValueType.DisplayName), heightValue.Get(FeatureValueType.DisplayName)
              )
            ));
          }

        if (option.Get(OptionType.SelectWidthId) == 0 && widthFeature.FeatureValues.Length > 0)
          option.Set(OptionType.SelectWidthId, widthFeature.FeatureValues[0].Id);
        if (option.Get(OptionType.SelectLengthId) == 0 && lengthFeature.FeatureValues.Length > 0)
          option.Set(OptionType.SelectLengthId, lengthFeature.FeatureValues[0].Id);
      }

      LightFeature clothFeature = shop.FindFeature("Ткань");
      LightObject selectCloth = clothFeature.FindValue(option.Get(OptionType.SelectClothId));
      LightObject selectWidth = widthFeature.FindValue(option.Get(OptionType.SelectWidthId));
      LightObject selectLength = lengthFeature.FindValue(option.Get(OptionType.SelectLengthId));

      int price = BedHlp.CalcBedPrice(product, selectCloth, selectWidth);

      return new HPanel(
        new HPanel(
          new HLabel("Размер:").Width(60),
          new HComboEdit<string>("size", sizes.Count > 0 ? sizes[0].Item1 : "", sizes.ToArray()
          ).Width(128).OnClick("$('.applyButton').click();")
        ).MarginBottom(20),
        new HPanel(
          new HLabel("Ткань:").Width(60).PaddingTop(12),
          new HButton(selectCloth == null ? "Не выбрана" : selectCloth.Get(FeatureValueType.DisplayName))
            .BoxSizing().Size(128, 40).Align(null).PaddingTop(12).BorderRadius(2)
            .Color("#fff").TextShadow("0 0 15px black, 0 0 5px black").Background("lightgray")
            .BackgroundImage(selectCloth == null ? "" : UrlHlp.ImageUrl(selectCloth.Id, true))
            .BackgroundSize("100% auto")
            .Event("popupClothView", "", delegate
            {
              state.PopupHint = "clothView";
            }
          )
        ).MarginBottom(20),
        new HButton("applyButton", "Применить").Display("none")
          .Event("apply_features", "contentEdit",
            delegate (JsonData json)
            {
              string size = json.GetText("size");
              if (StringHlp.IsEmpty(size))
                return;

              string[] parts = size.Split(',');
              if (parts.Length != 2)
                return;

              int? widthId = ConvertHlp.ToInt(parts[0]);
              int? lengthId = ConvertHlp.ToInt(parts[1]);

              if (widthId != null)
                state.Option.Set(OptionType.SelectWidthId, widthId.Value);
              if (lengthId != null)
                state.Option.Set(OptionType.SelectLengthId, lengthId.Value);
            }
          ),
        new HLabel(string.Format("{0}{1} руб", selectWidth == null || selectCloth == null ? "от " : "", price))
          .Padding(4, 0).MarginBottom(15)
          .FontSize("18px").FontFamily("PT Sans").FontBold(true).Color("#71A866"),
        new HPanel(
          Decor.EcoButton("Купить").BoxSizing().Width("100%").PaddingTop(8).PaddingBottom(8).BorderRadius(2)
            .Event(string.Format("product_{0}_add", product.ProductId), "contentEdit", 
            delegate (JsonData json)
            {
              if (!state.Operation.Validate(selectCloth == null, "Не выбрана ткань"))
                return;

              if (!state.Operation.Validate(selectWidth == null || selectLength == null, "Не выбран размер"))
                return;

              OrderCookie order = OrderHlp.GetOrCreateOrderCookie(httpContext);
              string productKey = StringHlp.Join("_", "{0}",
                new object[] { product.ProductId, selectWidth.Id, selectLength.Id, selectCloth.Id }
              );

              VirtualRowLink productRow = order.GetValue(productKey);
              if (productRow == null)
              {
                productRow = new VirtualRowLink();
                productRow.Set(OrderProductType.ProductKey, productKey);
                productRow.Set(OrderProductType.ProductId, product.ProductId);
                productRow.Set(OrderProductType.Price, price);
                productRow.Set(OptionType.SelectWidthId, selectWidth.Id);
                productRow.Set(OptionType.SelectLengthId, selectLength.Id);
                productRow.Set(OptionType.SelectClothId, selectCloth.Id);
              }
              productRow.Set(OrderProductType.Count, productRow.Get(OrderProductType.Count) + 1);

              order.SetValue(productKey, productRow);

              state.RedirectUrl = "/cart";
            })
          ),
        std.OperationWarning(state.Operation),
        DecorEdit.RedoButton(state.EditMode, "Редактировать", UrlHlp.EditUrl(group.Id, "fabric", product.Id)).MarginTop(5),
        DecorEdit.RedoButton(state.EditMode, "Особенности", UrlHlp.EditUrl("fabric-feature", product.Id)).MarginTop(5)
      ).EditContainer("contentEdit").InlineBlock().BoxSizing().Width(208)
        .Padding(18, 10).BoxShadow("rgba(0, 0, 0, 0.1) 0px 0px 8px 0px");
    }

    static IHtmlControl GetCloseButton(ShopState state)
    {
      return new HButton("", std.AfterAwesome(@"\f00d", 1).FontSize(17), new HHover().Color("#777"))
        .PositionAbsolute().Top(10).Right(10)
        .Size(25, 25).BoxSizing().PaddingTop(1)
        .Color("#aaa").Background("#fff").BorderRadius("50%").Border("2px solid #ccc")
        .Event("photoView_close", "", delegate { state.ResetPopup(); });
    }

    static IHtmlControl GetPopupPhotoView(ShopState state, Product product)
    {
      if (state.PopupHint != "photoView")
        return null;

      return DecorEdit.GetPopupView(state, 1000, 620,
        new HPanel(
          new HImage(UrlHlp.ImageUrl(product.ImageId, true)),
          GetCloseButton(state)
        ).BoxSizing().Width(1000).Height(620).PaddingTop(10).PaddingBottom(10).Align(null)
          .PositionRelative()
          .Background("#fff").BorderRadius(10)
      );
    }

    static IHtmlControl GetPopupClothView(ShopState state, Product product)
    {
      if (state.PopupHint != "clothView")
        return null;

      LightFeature categoryFeature = shop.FindFeature("Категория ткани");
      LightFeature materialFeature = shop.FindFeature("Материал ткани");
      LightFeature colorFeature = shop.FindFeature("Цвет");

      Tuple<int, string>[] allColors = BedHlp.GetComboItems("Все цвета", colorFeature.FeatureValues);
      Tuple<int, string>[] allMaterials = BedHlp.GetComboItems("Все материалы", materialFeature.FeatureValues);

      int filterColorId = state.Option.Get(OptionType.FilterColorId);
      int filterMaterialId = state.Option.Get(OptionType.FilterMaterialId);

      Dictionary<int, List<LightObject>> clothesByCategoryId = BedHlp.FindClothValues(
        shop, product, filterColorId, filterMaterialId);

      return DecorEdit.GetPopupView(state, 1000, 620,
        new HPanel(
          new HLabel("Выберите ткань обивки").Color(Decor.BedColor).FontSize(24).MarginBottom(20),
          new HPanel(
            new HComboEdit<int>("filterColorId", filterColorId, allColors)
              .Width(140).MarginRight(10)
              .OnChange("$('.clothFilterButton').click();"),
            new HComboEdit<int>("filterMaterialId", filterMaterialId, allMaterials)
              .Width(140)
              .OnChange("$('.clothFilterButton').click();"),
            new HButton("clothFilterButton", "Фильтр").Display("none")
              .Event("apply_filter", "filterContainer",
                delegate (JsonData json)
                {
									Logger.AddMessage("ApplyFilter: {0}", json.ToText());
                  state.Option.Set(OptionType.FilterColorId, json.GetInt("filterColorId") ?? 0);
                  state.Option.Set(OptionType.FilterMaterialId, json.GetInt("filterMaterialId") ?? 0);
                }
              )
          ).EditContainer("filterContainer").Align(true).MarginBottom(10),
          new HPanel(
            ArrayHlp.Convert(clothesByCategoryId.Keys, delegate (int categoryId)
              {
                LightObject category = categoryFeature.FindValue(categoryId);
                return GetCategoryBlock(state, product, materialFeature, colorFeature, category,
                  clothesByCategoryId[categoryId].ToArray()
                );
              }
            )
          ).Align(true).CssAttribute("overflow-y", "scroll").Height(460),
          GetCloseButton(state)
        ).BoxSizing().Width(1000).Height(620).Padding(40).Align(null)
          .PositionRelative()
          .Background("#fff").BorderRadius(10)
      );
    }

    static IHtmlControl GetCategoryBlock(ShopState state, Product product,
      LightFeature materialFeature, LightFeature colorFeature, LightObject category, LightObject[] clothes)
    {
      int price = product.Get(FabricType.FeatureMarkups, category.Id);

      return new HPanel(
        std.RowPanel(
          std.DockFill(new HLabel(category?.Get(FeatureValueType.DisplayName))),
          new HLabel(string.Format("от {0} р.", price)).NoWrap().MarginRight(24)
        ).FontSize(18),
        new HPanel(
          ArrayHlp.Convert(clothes, delegate(LightObject cloth)
          {
            return GetClothTile(state, materialFeature, colorFeature, cloth);
          })
        )
      ).PaddingTop(25).PaddingBottom(15).BorderTop(Decor.orderBorder);
    }

    static IHtmlControl GetClothTile(ShopState state,
      LightFeature materialFeature, LightFeature colorFeature, LightObject cloth)
    {
      int materialId = cloth.Get(FeatureValueType.WithFeatures, 1);
      int colorId = cloth.Get(FeatureValueType.WithFeatures, 2);

      LightObject material = materialFeature.FindValue(materialId);
      LightObject color = colorFeature.FindValue(colorId);

      return new HPanel(
        new HEventPanel(
          //new HImage(UrlHlp.ImageUrl(cloth.Id, true))
        ).OnClick(";").CursorPointer().BoxSizing().Size(129, 86)
          .BackgroundImage(UrlHlp.ImageUrl(cloth.Id, true))
          .BackgroundSize("100% auto")
          .Event("select_cloth", "", delegate
          {
            state.Option.Set(OptionType.SelectClothId, cloth.Id);
            state.ResetPopup();
          },
          cloth.Id),
        new HLabel(cloth.Get(FeatureValueType.DisplayName)).Block().MarginTop(3).MarginBottom(2),
        new HPanel(
          new HLabel(material?.Get(FeatureValueType.DisplayName)).Color(Decor.BedMinorColor).FontSize(13)
        ),
        new HPanel(
          new HLabel(color?.Get(FeatureValueType.DisplayName)).Color(Decor.BedMinorColor).FontSize(13)
        )
      ).InlineBlock().VAlign(true).MarginTop(10).MarginBottom(10).MarginRight(20);
    }

    //static IHtmlControl GetClothView(ShopState state)
    //{

    //}
  }
}