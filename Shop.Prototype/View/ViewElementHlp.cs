using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Basis;
using Commune.Html;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;

namespace Shop.Prototype
{
  public class ViewElementHlp
  {
    public const int thumbSize = 164;
    public const int thumbPadding = 12;
    public const int tileWidth = thumbSize;
    public const int thumbLargeSize = 236;

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

    public static HButton GetFilterItem(string caption, bool selected)
    {
      HButton button = new HButton(caption).Padding(5, 14).MarginBottom(5)
          .Background("#f8f8f8").Border("1px", "solid", "#e5e5e5", "1px");

      if (selected)
        button.Color(Decor.propertyColor).FontBold() //.Background("#e5e5e5")
          .Background("#f1f1f1")
          .LinearGradient("to top right", "#dddddd", "#f1f1f1");
      else
        button.Color(Decor.propertyMinorColor);

      return button;
    }

    public static IHtmlControl[] GetFilterRows(IList<ISearchIndex> indices, SearchFilter filter)
    {
      List<IHtmlControl> controls = new List<IHtmlControl>();
      for (int i = 0; i < indices.Count; i += 2)
      {
        IHtmlControl filterRow = ViewElementHlp.GetFilterRow(indices, i, filter);
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
      .Media1024(new HStyle(".{0} > div").MediaBlock(true).MarginBottom(10))
      .Media664(new HStyle(".{0} > div").PaddingLeft(20));
    }

    public static IHtmlControl GetFilterField(ISearchIndex index, SearchFilter filter)
    {
      if (index == null)
        return new HPanel().Hide(true);

      return new HPanel(
        new HLabel(index.Property.Get(MetaPropertyType.Identifier)).Width(150).MarginRight(20)
          .Color(Decor.propertyColor),
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
        NumericalSearchIndex numericalIndex = (NumericalSearchIndex)index;

        NumericalSearchCondition numericalCondition = condition as NumericalSearchCondition;

        string measureUnit = index.Property.Get(MetaPropertyType.MeasureUnit);

        return new HPanel(
          new HIonSlider(
            string.Format("property_{0}", index.Property.Id),
            true, numericalIndex.Min ?? 0, numericalIndex.Max ?? 0, numericalIndex.Accuracy, 
            measureUnit, false, "$('.filterButton').click();"
          )
        );

        //return new HPanel(
        //  new HLabel("от").Color(Decor.propertyMinorColor),
        //  new HTextEdit(string.Format("property_{0}_min", index.Property.Id), min).Align(null)
        //    .Width("3em").MarginLeft(5).MarginRight(5),
        //  new HLabel("до").Color(Decor.propertyMinorColor),
        //  new HTextEdit(string.Format("property_{0}_max", index.Property.Id), max).Align(null)
        //    .Width("3em").MarginLeft(5).MarginRight(5),
        //  new HLabel(index.Property.Get(MetaPropertyType.MeasureUnit)).Color(Decor.propertyMinorColor)
        //).InlineBlock();
      }

      return new HPanel().Hide(true);
    }

    public static IHtmlControl GetTechnicalTile(string url, string caption)
    {
      return EditElementHlp.GetTechnicalTile(url, caption, thumbSize);
    }

    public static IHtmlControl GetVarietyTile(bool isAdmin, Product product)
    {
      return EditElementHlp.GetVarietyTile(isAdmin, product, thumbSize);
    }

    public static IHtmlControl GetViewTitle(string title)
    {
      return new HPanel(
        new HH1(
          title,
          Decor.DividerAfter()
        ).PaddingBottom(24).CssAttribute("position", "relative")
        .FontFamily("PT Sans").FontBold(true).FontSize("2.25em").Color("#6CAF22")
      ).Align(null).BorderBottom("1px solid #E5E5DC")
      .CssAttribute("margin-bottom", "40px").LineHeight("1.1");
    }

    public static IHtmlControl GetViewCompletion(string title, string message)
    {
      return new HPanel(
        GetViewTitle(title),
        new HLabel(message).Block().Align(null)
          .FontFamily("PT Sans").FontSize("22px").FontBold().Color(Decor.PriceColor)
      );
    }

    public static IHtmlControl GetAuthHeader(string kind)
    {
      return new HPanel(
        new HPanel(
          GetAuthLink(kind == "login", "Войти", UrlHlp.ShopUrl("login")),
          GetAuthLink(kind == "register", "Зарегистрироваться", UrlHlp.ShopUrl("register")),
          GetAuthLink(kind == "passwordreset", "Забыли пароль?", UrlHlp.ShopUrl("passwordreset"))
        ).InlineBlock().MarginBottom("1.65em")
      ).FontSize("110%").LineHeight("1.65em").Align(null);
    }

    static IHtmlControl GetAuthLink(bool isSelected, string caption, string url)
    {
      if (isSelected)
        return new HLabel(caption).Color("rgba(0, 0, 0, 0.4)").Padding(0, 8);

      return new HLink(url,
        new HLabel(caption).Color(Decor.PriceColor).Padding(0, 12)
      ).TextDecoration("none");
    }

    public static IHtmlControl GetGroupTile(bool editMode, bool seoMode,
      LightKin parentGroup, LightKin group)
    {
      string groupName = group.Get(GroupType.Identifier);
      string shopUrl = UrlHlp.ShopUrl("group", group.Id);
      return new HPanel(
        new HLink(shopUrl,
          EditElementHlp.GetImagePanel(UrlHlp.ImageUrl(group.Id, false), groupName, thumbSize)
        ),
        new HLink(shopUrl,
          new HH2(groupName).Padding(10, 0).FontSize(14).LineHeight("1.2em").FontBold().Color("#000")
        ).Block().Align(null),
        DecorEdit.AdminPanel(editMode, seoMode, parentGroup, "group", group.Id)
      ).Width(tileWidth).Padding(thumbPadding).InlineBlock().VAlign(true);
    }

    public static IHtmlControl GetProductTile(HttpContext httpContext,
      bool editMode, bool seoMode, LightKin parentGroup, Product product, OrderCookie orderForView)
    {
      string productName = product.ProductName;
      string shopUrl = UrlHlp.ShopUrl("product", product.ProductId, parentGroup?.Id);

      MetaKind kind = shop.FindFabricKind(product.Get(FabricType.Kind));
      bool isOption = kind != null && kind.Get(MetaKindType.DesignKind) == "bed";

      int productCount = orderForView.GetCount(product.ProductId);
      return new HPanel(
        new HLink(shopUrl,
          EditElementHlp.GetImagePanel(UrlHlp.ImageUrl(product.ImageId, false), productName, thumbSize)
        ),
        new HPanel(
          new HLabel(string.Format("{0} руб", product.Get(FabricType.Price)))
            .Padding(4, 0).FloatLeft()
            .FontSize("18px").FontFamily("PT Sans").FontBold(true).Color("#71A866"),
          Decor.EcoButton("+", null,
            std.AfterAwesome(@"\f07a", 1).FontWeight("normal")
          ).Hide(isOption)
          .Width(36).Padding(7, 0).FloatRight().Title("Добавить в корзину").FontBold()
            .Event(string.Format("product_{0}_add", product.ProductId), "", delegate (JsonData json)
            {
              OrderCookie order = OrderHlp.GetOrCreateOrderCookie(httpContext);
              order.Increment(product);
            }),
          new HLabel(productCount, new HBefore().Content("×").MarginRight(2).FontSize("75%"))
            .Hide(isOption)
            .FloatRight().Hide(productCount == 0).Padding(4, 0).MarginRight(8)
            .FontSize("18px").FontFamily("PT Sans").FontBold().Color("#888")
        ),
        new HLink(shopUrl,
          new HPanel(
            new HH2(productName).Padding(10, 0).FontSize(14).LineHeight("1.2em").FontBold().Color("#000")
          )
        ).Block().Align(true),
        new HLabel(product.Annotation)
          .Block().Color("#888").LineHeight("15px"),
        DecorEdit.AdminPanel(editMode, seoMode, parentGroup, "fabric", product.Id)
      ).Width(tileWidth).Padding(thumbPadding).InlineBlock().VAlign(true);
    }

    public static IHtmlControl GetPlaceholderTile()
    {
      return new HPanel(
        new HPanel().Size(thumbSize, thumbSize)
      ).InlineBlock().Width(tileWidth).Padding(thumbPadding);
    }

    public static IHtmlControl GetUserControlForHeader(HttpContext httpContext, string login)
    {
      if (StringHlp.IsEmpty(login))
        return Decor.HeaderLink("Войти", UrlHlp.ShopUrl("login"));

      string displayName = "";
      if (login == "admin")
      {
        displayName = "Администратор";
      }
      else if (login == "manager")
      {
        displayName = "Менеджер";
      }
      else if (login == "seo")
      {
        displayName = "SEO";
      }
      else if (login == "edit")
      {
        displayName = "Редактор";
      }
      else
      {
        LightObject user = UserHlp.LoadUser("", login);
        if (user == null)
        {
          httpContext.Logout();
          return Decor.HeaderLink("Войти", UrlHlp.ShopUrl("login"));
        }
        displayName = user.Get(UserType.Family);
      }

      return new HPanel(
        new HLabel(displayName).Color(Decor.headerColor),
        new HButton("", std.AfterAwesome(@"\f08b", 4)).Event("logout", "",
          delegate (JsonData json)
          {
            httpContext.Logout();
          }
        ).Title("Выйти").MarginRight(20)
      ).BorderBottom("4px solid transparent"); //.PaddingRight(20);
    }

    public static IHtmlControl GetHintPanel(ShopState state, string popupKind, string title, string text)
    {
      return new HPanel(
        GetHintPopup(title, text)
      ).InlineBlock().Size(10, 10).Hide(state.PopupHint != popupKind);
    }

    public static IHtmlControl GetHintPopup(string title, string text)
    {
      return new HPanel(
        new HLabel(title).Block().FontBold().MarginBottom(10),
        new HTextView(text),
        new HButton("",
          std.AfterAwesome(@"\f00d", 0).FontSize("1.5em"),
          new HHover().Color("#C51A3C")
        ).PositionAbsolute().Right("10px").Top("10px").Color("#dedede")
      ).Width(230).FontSize(11).LineHeight(14)
      .Background("#ffffff")
      .MarginLeft(16).MarginTop(-8)
      .PaddingLeft(13).PaddingRight(17).PaddingTop(11).PaddingBottom(12)
      .PositionAbsolute()
      .BorderRadius(5)
      .BoxShadow("0px 2px 10px 0px rgba(63, 69, 75, 0.5)");
    }
  }
}