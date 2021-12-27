using System;
using System.Collections.Generic;
using System.Web;
using Commune.Basis;
using Commune.Html;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;

namespace Shop.Prototype
{
  public class ViewPanelHlp
  {
    public static IHtmlControl GetSearchFilterPanel(ShopState state, IShopStore store, MetaKind kind)
    {
      if (kind == null)
        return new HPanel().Hide(true);

      List<IHtmlControl> controls = new List<IHtmlControl>();
      controls.Add(
        new HPanel(
          new HLabel("Сортировать:").MarginRight(20).Color(Decor.propertyMinorColor),
          ViewElementHlp.GetFilterItem("По наименованию", state.SortKind == "").MarginRight(20)
            .Event("sort_by_name", "", delegate
            {
              state.SortKind = "";
            }),
          ViewElementHlp.GetFilterItem("Сначала дешевые", state.SortKind == "cheap").MarginRight(20)
            .Event("sort_by_cheap", "", delegate
            {
              state.SortKind = "cheap";
            }),
          ViewElementHlp.GetFilterItem("Сначала дорогие", state.SortKind == "pricey")
            .Event("sort_by_pricey", "", delegate
            {
              state.SortKind = "pricey";
            })
        ).MarginBottom(15)
      );
      //SearchIndexStorage storage = store.SearchModule.FindIndexStorage(kind.Id);
      //if (storage != null)
      //{
      //  ISearchIndex[] priorIndices = storage.PriorSearchIndices;
      //  controls.Add(
      //    new HPanel(
      //      ViewElementHlp.GetFilterRows(priorIndices, null)
      //    ) //.Background("#ffffff").Padding(5)
      //  );

      //  ISearchIndex[] minorIndices = storage.MinorSearchIndices;
      //  if (minorIndices.Length != 0)
      //  {
      //    controls.Add(
      //      new HSpoilerPanel(
      //        new HImage("/images/bot.png", new HHover().Cursor("hand")).VAlign(-2), 
      //        new HImage("/images/top.png", new HHover().Cursor("hand")).VAlign(-2),
      //        true,
      //        new HLabel("Другие характеристики", new HHover().Cursor("hand").Color(Decor.propertyColor))
      //          .MarginRight(4).FontBold().Color(Decor.propertyMinorColor),
      //        new HPanel(
      //          ViewElementHlp.GetFilterRows(minorIndices, null)
      //        ).MarginTop(15)
      //      ).MarginTop(15)
      //    );
      //  }

      //  controls.Add(
      //    new HPanel(
      //      Decor.EcoButton("filterButton", "Применить", null, null).Padding(8, 35)
      //      .Event("apply_filter", "editContent",
      //        delegate(JsonData json)
      //        {
      //          state.SearchFilter = SearchHlp.GetFilterFromJson(storage, json);
      //        }
      //      )
      //    ).MarginTop(20).MarginBottom(20)
      //  );
      //}

      return new HPanel(
        controls.ToArray()
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetFabricPropertiesTable(ShopStorage store, LightObject fabric,
      ShopState state)
    {
      MetaKind kind = store.FindFabricKind(fabric.Get(FabricType.Kind));

      if (kind == null)
        return new HPanel().Hide(true);

      List<IHtmlControl> rows = new List<IHtmlControl>();
      rows.Add(
        new HLabel("Основные характеристики").Block().MarginBottom(24)
          .FontSize(20).FontBold().LineHeight(20)
      );
      LightObject category = null;
      foreach (MetaProperty property in kind.Properties)
      {
        LightObject prevCategory = category;
        category = store.FindPropertyCategory(property.Get(MetaPropertyType.Category));

        string value = fabric.Get(property.Blank);
        if (StringHlp.IsEmpty(value))
          continue;

        if (category != null && category != prevCategory)
        {
          HPanel categoryRow = new HPanel(
            new HLabel(category.Get(MetaCategoryType.DisplayName)).FontBold()
          ).PaddingTop(40).PaddingBottom(10).MarginBottom(3)
          .BorderBottom("1px solid #E5E5DC");

          rows.Add(categoryRow);
        }

        string measureUnit = property.Get(MetaPropertyType.MeasureUnit);
        if (!StringHlp.IsEmpty(measureUnit))
          value = string.Format("{0} {1}", value, measureUnit);

        string popupKind = string.Format("property_{0}", property.Id);
        string displayName = property.Get(MetaPropertyType.Identifier);
        string hint = property.Get(MetaPropertyType.Hint);

        HPanel propertyRow = new HPanel(
          new HPanel(
            new HLabel(displayName),
            new HButton("", std.BeforeAwesome(@"\f059", 0)
              .Color("#9fd0f3").FontSize("1.25em").VAlign(-1) //.Color("#bfe0f3")
            ).Hide(StringHlp.IsEmpty(hint)).MarginLeft(12)
              .Event("propertyPopup", "", delegate
                {
                  state.PopupHint = popupKind;
                },
                property.Id
              ),
            ViewElementHlp.GetHintPanel(state, popupKind, displayName, hint)
          ).InlineBlock().Width(472)
          .Media900(new HStyle().Width(240)),
          new HLabel(value)
        ).PaddingTop(10).PaddingBottom(8);

        rows.Add(propertyRow);
      }

      return new HPanel(
        rows.ToArray()
      ).Align(true).MarginTop(35)
      .LineHeight("1em").Color(Decor.propertyColor);
    }

    public static IHtmlControl GetNewsColumn(bool isAdmin, IShopStore store)
    {
      return new HPanel(
        new HLabel("Эконовости").Block().MarginBottom(20).Align(null)
          .FontSize("22px").Color(Decor.TitleColor),
        new HGrid<LightObject>(store.News.Actual,
          delegate (LightObject news)
          {
            string title = news.Get(NewsType.Annotation);
            if (StringHlp.IsEmpty(title))
              title = news.Get(NewsType.Title);
            return new HPanel(
              new HLabel(UrlHlp.DateToString(news.Get(ObjectType.ActFrom)))
                .Block().MarginBottom(10)
                .FontSize("11px").Color("#999"),
              new HLink(UrlHlp.ShopUrl("news", news.Id),
                new HLabel(title, Decor.UnderlineHover()).Color("#555")
              ).Block().TextDecoration("none")
            ).MarginBottom(30);
          },
          new HRowStyle()
        ),
        new HLink(UrlHlp.ShopUrl(Site.Novosti),
          new HLabel("Все новости").FontBold().Color(Decor.PriceColor)
        ).Block().TextDecoration("none")
      );
    }

    public static IHtmlControl GetLeftPanel(ShopState state, IShopStore store, string kind, int? parentId, int? id)
    {
      if ((kind == "catalog" && id == null) || kind == "news")
        return GetNewsColumn(state.EditMode, store);

      if (kind == "catalog")
      {
        LightGroup group = store.Shop.FindGroup(id);
        MetaKind fabricKind = group != null ? store.Shop.FindFabricKind(group.Get(GroupType.FabricKind)) : null;
        if (fabricKind != null && StringHlp.IsEmpty(fabricKind.Get(MetaKindType.DesignKind)))
        {
          SearchIndexStorage storage = store.SearchModule.FindIndexStorage(fabricKind.Id);
          if (storage != null)
            return ViewFilterHlp.GetFilterColumn(state, storage);
        }
      }

      //LightGroup parentGroup = FabricHlp.GetParent(store.Shop, kind, parentId, id);
      LightGroup parentGroup = store.Shop.FindGroup(parentId);

      string returnUrl = "/";
      if (parentGroup != null)
      {
        returnUrl = UrlHlp.ShopUrl("group", parentGroup.Id);
      }

      LightGroup treeGroup = parentGroup;
      if (treeGroup != null && treeGroup.Subgroups.Length == 0)
        treeGroup = treeGroup.ParentGroup;
      LightKin[] groups = treeGroup != null ? treeGroup.Subgroups : store.Shop.RootGroups;

      return new HPanel(
        new HLink(returnUrl,
          new HButton("", new HBefore().FontFamily("FontAwesome").Content(@"\f190")
            .FontSize("4em").VAlign(null).Align(null).Color("#71A866").Opacity("0.8")
          )
        ).Hide(kind != "group" && kind != "product")
          .Block().Align(null).PaddingBottom(18).PaddingTop(8),
        new HGrid<LightKin>(groups,
          delegate (LightKin group)
          {
            string groupUrl = string.Format(UrlHlp.ShopUrl("group", group.Id));
            if (group.Id == id)
            {
              return new HLabel(group.Get(GroupType.Identifier))
                .Block().Align(null).Padding(4, 0).Color("#71A866").TextDecoration("underline").Opacity("0.8");
            }

            return new HLink(groupUrl,
              new HLabel(group.Get(GroupType.Identifier)),
              new HHover().Color("#DE2C3B").TextDecoration("underline")
            ).Block().Align(null).Padding(4, 0).Color("#0F0F0F").TextDecoration("none").Opacity("0.8");
          },
          null
        )
      ).CssAttribute("line-height", "1.65em");
    }

    public static IHtmlControl GetSearchPanel(ShopState state)
    {
      return std.RowPanel(
        std.DockFill(
          new HTextEdit("searchText").Placeholder("Поиск товара...")
            .Padding("4px 0.5em").Border("1px", "solid", "#cccccc", "15px").TabIndex(1)
            .FontSize("1.2em")
            .OnKeyDown("if (e.keyCode == 13) $('.searchButton').click();")
        ).PaddingRight(18),
        new HPanel(
          new HButton("searchButton", " ",
            std.BeforeAwesome(@"\f002", 0).FontSize("2em").Color("#008000")
          )
          .Event("search_fabrics", "searchContent",
            delegate (JsonData json)
            {
              string searchText = json.GetText("searchText");
              if (searchText == null || searchText.Length < 3)
              {
                state.Operation.Message = "Поисковый запрос должен содержать не менее 3 символов";
                return;
              }
              state.SearchText = searchText;
            }
          )
        ).Padding(0, 18)
      ).EditContainer("searchContent").MarginBottom("23px");
    }

    public static IHtmlControl GetVarietiesPanel(
      bool isAdmin, ShopStorage store, LightKin fabric, int? viewVarietyId)
    {
      List<IHtmlControl> tileControls = new List<IHtmlControl>();
      foreach (int varietyId in fabric.AllChildIds(FabricType.VarietyTypeLink))
      {
        Product product = store.FindProduct(varietyId);
        if (product == null || varietyId == viewVarietyId)
          continue;

        tileControls.Add(ViewElementHlp.GetVarietyTile(isAdmin, product));
      }

      if (isAdmin)
        tileControls.Add(
          ViewElementHlp.GetTechnicalTile(
            UrlHlp.EditUrl(fabric.Id, "variety", null),
            "Добавить"
          )
        );

      if (tileControls.Count == 0)
        return new HPanel();

      if (tileControls.Count < 4)
      {
        while (tileControls.Count < 4)
        {
          tileControls.Add(ViewElementHlp.GetPlaceholderTile());
        }
      }

      return new HPanel(
        new HLabel("Разновидности этого товара").Block().Align(null)
          .Upper().Color("#71A866").PaddingTop("1em"),
        new HPanel(
          tileControls.ToArray()
        )
      ).Align(true);
    }

    public static IHtmlControl GetCartTable(HttpContext httpContext,
      OrderCookie orderForView, ShopStorage store, int orderPrice)
    {
      IEnumerable<VirtualRowLink> rows = orderForView.AllProducts(store);

      string gridBorder = "1px solid #DDD";

      return new HGrid<VirtualRowLink>(
        std.RowPanel(
          std.DockFill(new HLabel("Ваши покупки").MarginLeft(44)).Padding(7, 0),
          new HLabel("Кол-во").Width(108).Align(null),
          new HLabel("Цена").Width(60).Align(null),
          new HLabel("Сумма").Width(60).Align(null).MarginRight(7)
        ).Border(gridBorder).FontBold(),
        rows,
        delegate (VirtualRowLink row)
        {
          string productKey = row.Get(OrderProductType.ProductKey);
          int productId = row.Get(OrderProductType.ProductId);
          int price = row.Get(OrderProductType.Price);
          int count = row.Get(OrderProductType.Count);

          string productName = BedHlp.GetProductName(store, productId, row);

          Product product = store.FindProduct(productId);

          return std.RowPanel(
            new HPanel(new HImage(UrlHlp.ImageUrl(product.ImageId, false))
              .Size(32, 32)
            ).Height(60).Padding(0, 6),
            std.DockFill(
              new HLink(UrlHlp.ShopUrl("product", product.ProductId),
                new HLabel(productName)
                  .FontBold().Color("#71A866")
              )
            ),
            new HPanel(
              Decor.EcoButton("–").Padding(0, 6).FontBold()
                .Event("product_dec", "",
                  delegate (JsonData json)
                  {
                    OrderCookie order = OrderHlp.GetOrCreateOrderCookie(httpContext);
                    order.Decrement(productKey);
                  },
                  product.ProductId
                ),
              new HLabel(count.ToString())
                .Width("36px").Align(null).FontBold(),
              Decor.EcoButton("+").Padding(0, 6).FontBold().MarginRight(18)
                .Event("fabric_inc", "",
                  delegate (JsonData json)
                  {
                    OrderCookie order = OrderHlp.GetOrCreateOrderCookie(httpContext);
                    order.Increment(productKey);
                  },
                  product.ProductId
                )
            ).NoWrap(),
            new HLabel(price).Width("60px").Align(null),
            new HLabel(price * count)
              .Width("60px").MarginRight(7).Align(null).FontBold()
          ).Border(gridBorder).BorderTop("0px");
        },
        new HRowStyle().Odd(new HTone().Background("#F9F9F9")),
        std.RowPanel(
          std.DockFill(new HLabel("Итого:").Align(false)).Padding(7, 0),
          new HLabel("").Width(180),
          new HLabel(orderPrice).Width(60).Align(null).MarginRight(7)
        ).MarginTop("30px").FontBold().Border(gridBorder).BorderLeft("0px").BorderRight("0px")
      ).LineHeight("24px");
    }


  }
}