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
  public static class DecorEdit
  {
    public const string propertyColor = "#3f454b";
    public const string propertyMinorColor = "#7f868e";
    public const string iconColor = "#233b9e";

    public static T MediaSmartfon<T>(this T control, params HStyle[] styles)
      where T : IEditExtension
    {
      return control.Media(480, styles);
    }

    public static T MediaTablet<T>(this T control, params HStyle[] styles)
      where T : IEditExtension
    {
      return control.Media(800, styles);
    }

    public static T MediaLaptop<T>(this T control, params HStyle[] styles)
      where T : IEditExtension
    {
      return control.Media(1180, styles);
    }

    public static HLink RedoButton(bool editMode, string caption, string url)
    {
      return new HLink(url,
        std.Button(caption).Block().Color("#000"),
        new HHover().TextDecoration("none")
      ).InlineBlock().Align(null).MarginRight(20).Hide(!editMode)
      .FontFamily("Arial").FontSize(14).LineHeight(14).TextDecoration("none");
    }

    public static HLink RedoButton(string caption, string url)
    {
      return new HLink(url,
        std.Button(caption).Block().Color("#000"),
        new HHover().TextDecoration("none")
      ).InlineBlock().MarginRight(20)
      .FontFamily("Arial").FontSize(14).LineHeight(14).TextDecoration("none");
    }

    public static HPanel AdminMainPanel(SiteSettings settings, HttpContext httpContext)
    {
      string auth = httpContext.Get("auth");

      bool editMode = httpContext.IsInRole("edit");
      bool seoMode = httpContext.IsInRole("seo");

      if (!editMode && !seoMode)
      {
        if (StringHlp.IsEmpty(auth))
          return null;

        return new HPanel(
          new HLabel("", std.BeforeAwesome(@"\f084", 10)).Color(DecorEdit.iconColor),
          new HTextEdit("login", auth).MarginRight(10),
          new HPasswordEdit("password").MarginRight(10),
          std.Button("Войти").FontFamily("Arial").FontSize(14).LineHeight(14)
            .Event("authorize", "authContent", delegate (JsonData json)
            {
              string login = json.GetText("login");
              string password = json.GetText("password");

              UserHlp.SiteAuthorization(settings, httpContext, login, password);
            }
          )
        ).EditContainer("authContent").Align(true).Padding(5);
      }

      return new HPanel(
        std.Button("Выйти").FontFamily("Arial").FontSize(14).LineHeight(14).MarginRight(20)
          .Event("logout", "", delegate { httpContext.Logout(); }),
        DecorEdit.RedoButton(editMode, "Предприятие", UrlHlp.EditUrl("contacts-column", null)),
        DecorEdit.RedoButton(editMode && SiteContext.Default.Store is IShopStore, 
          "Справочник магазина", UrlHlp.EditUrl("shop-catalog", null)
        ),
        DecorEdit.RedoButton(seoMode, "SEO-поля", UrlHlp.SeoUrl("seo-pattern", null)),
        DecorEdit.RedoButton(seoMode, "Все перенаправления", UrlHlp.SeoUrl("redirect-list", null)),
        DecorEdit.RedoButton(seoMode, "SEO виджеты", UrlHlp.SeoUrl("widget-list", null))
      ).Align(true).Padding(5);
    }

    public static HPanel AdminPagePanel(IState state, LightSection section)
    {
      return AdminPagePanel(state, section, null);
    }

    public static HPanel AdminPagePanel(IState state, LightSection section, string fixedDesignKind)
    {
      if (!state.EditMode && !state.SeoMode)
        return null;

      int? parentId = section?.ParentSection?.Id;
      int? id = section?.Id;

      bool isGroup = !StringHlp.IsEmpty(fixedDesignKind);

      string addUrl = UrlHlp.EditUrl(id, "page", null);
      if (isGroup)
        addUrl = string.Format("{0}&design={1}", addUrl, fixedDesignKind);

      return new HPanel(
        DecorEdit.RedoButton(state.EditMode, "Редактировать", UrlHlp.EditUrl(parentId, "page", id)),
        DecorEdit.RedoButton(state.SeoMode, "SEO", UrlHlp.SeoUrl("page", id)),
        DecorEdit.RedoButton(state.EditMode, "Добавить подраздел", addUrl).Hide(!isGroup),
        DecorEdit.RedoButton(state.EditMode, "Сортировать подразделы", UrlHlp.EditUrl(id, "sorting_section", null))
          .Hide(!isGroup)
      ).Align(true).Padding(5);
    }

    public static HPanel AdminSectionPanel(bool editMode, bool seoMode, string kind, LightSection section, bool isGroup)
    {
      if (!editMode && !seoMode)
        return new HPanel().Hide(true);

      int? parentId = section?.ParentSection?.Id;
      int? id = section?.Id;

      return new HPanel(
        DecorEdit.RedoButton(editMode, "Редактировать", UrlHlp.EditUrl(parentId, kind, id)),
        DecorEdit.RedoButton(seoMode, "SEO", UrlHlp.SeoUrl(kind, id)),
        DecorEdit.RedoButton(editMode, "Добавить подраздел", UrlHlp.EditUrl(id, kind, null)).Hide(!isGroup),
        DecorEdit.RedoButton(editMode, "Сортировать подразделы", UrlHlp.EditUrl(id, "sorting_section", null))
          .Hide(!isGroup)
      ).Align(true).Padding(5);
    }

    public static HPanel AdminGroupPanel(bool editMode, int groupSectionId)
    {
      if (!editMode)
        return null;

      return new HPanel(
        DecorEdit.AddIconButton(true, UrlHlp.EditUrl(groupSectionId, "page", null)),
        DecorEdit.SortIconButton(true, UrlHlp.EditUrl(groupSectionId, "sorting_section", null))
      ).InlineBlock().FontSize(14);
    }

    public static HPanel AdminUnitPanel(bool editMode, int sectionId)
    {
      return AdminUnitPanel(editMode, sectionId, "");
    }

    public static HPanel AdminUnitPanel(bool editMode, int parentId, string fixedDesignKind)
    {
      if (!editMode)
        return null;

      string addUrl = UrlHlp.EditUrl(parentId, "unit", null);
      if (!StringHlp.IsEmpty(fixedDesignKind))
        addUrl = string.Format("{0}&design={1}", addUrl, fixedDesignKind);

      return new HPanel(
        DecorEdit.AddIconButton(true, addUrl).Title("Добавить элемент"),
        DecorEdit.SortIconButton(true, UrlHlp.EditUrl(parentId, "sorting_subunit", null)).Title("Сортировать элементы")
      ).InlineBlock();
    }

    public static HPanel AdminPaymentPanel(bool editMode)
    {
      if (!editMode)
        return null;

      return new HPanel(
        DecorEdit.AddIconButton(true, UrlHlp.EditUrl("payment", null)).Title("Добавить способ оплаты"),
        DecorEdit.SortIconButton(true, UrlHlp.EditUrl("sorting_payment", null)).Title("Сортировать способы оплаты")
      ).InlineBlock();
    }

    public static HPanel AdminDeliveryPanel(bool editMode)
    {
      if (!editMode)
        return null;

      return new HPanel(
        DecorEdit.AddIconButton(true, UrlHlp.EditUrl("delivery", null)).Title("Добавить способ доставки"),
        DecorEdit.SortIconButton(true, UrlHlp.EditUrl("sorting_delivery", null)).Title("Сортировать способы доставки")
      ).InlineBlock();
    }

    [Obsolete]
    public static IHtmlControl AdminUnitRedoIcon(bool editMode, int unitId)
    {
      if (!editMode)
        return null;

      return DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("unit", unitId))
        .PositionAbsolute().Left(2).Top(2).Padding(2).Background("#fff");
    }

    public static IHtmlControl AdminUnitRedoBlock(bool editMode, int unitId)
    {
      return AdminUnitRedoBlock(editMode, unitId, null);
    }

    public static IHtmlControl AdminUnitRedoBlock(bool editMode, int unitId, string fixedDesignKind)
    {
      if (!editMode)
        return null;

      return new HPanel(
        new HAnchor(string.Format("unit{0}", unitId)),
        DecorEdit.RedoIconButton(true, UrlHlp.EditUrl("unit", unitId)),
        DecorEdit.AdminUnitPanel(fixedDesignKind != null, unitId, fixedDesignKind)?.MarginLeft(6)
      ).InlineBlock().PositionAbsolute().Left(2).Top(2).Padding(2).Background("#fff").FontSize(14).LineHeight(14);
    }

    public static IHtmlControl AdminProductPanel(SiteState state,
      LightHead group, int productId, bool withFeatures)
    {
      if (!state.EditMode && !state.SeoMode)
        return null;

      return new HPanel(
        state.EditMode ? DecorEdit.RedoButton("Редактировать", UrlHlp.EditUrl(group, "fabric", productId)) : null,
        (state.EditMode && withFeatures) ? 
          DecorEdit.RedoButton("Особенности", UrlHlp.EditUrl("fabric-feature", productId)) : null,
        state.SeoMode ? DecorEdit.RedoButton("SEO", UrlHlp.SeoUrl("fabric", productId)) : null
      );
    }

    public static HPanel AdminPanel(bool editMode, bool seoMode,
      LightHead parent, string kind, int? id)
    {
      if (!editMode && !seoMode)
        return new HPanel().Hide(true);

      return new HPanel(
        DecorEdit.RedoButton(editMode, "Редактировать", UrlHlp.EditUrl(parent, kind, id)).Hide(!editMode),
        DecorEdit.RedoButton(seoMode, "SEO", UrlHlp.SeoUrl(kind, id)).Hide(!seoMode)
      );
    }



    //public static IHtmlControl EditOrSeoButton(bool editMode, bool seoMode,
    //  LightHead parent, string kind, int? id)
    //{
    //  if (seoMode)
    //    return DecorEdit.RedoButton(seoMode, "SEO",
    //      UrlHlp.SeoUrl(kind, id)
    //    );

    //  return DecorEdit.RedoButton(editMode, "Редактировать",
    //    UrlHlp.EditUrl(parent, kind, id)
    //  );
    //}

    public static HLink RedoIconButton(bool isAdmin, string url)
    {
      return new HLink(url,
        new HButton("", std.BeforeAwesome(@"\f044", 0)).Color(iconColor)
      ).VAlign(-1).Hide(!isAdmin).Title("Редактировать");
    }

    public static HLink AddIconButton(bool isAdmin, string url)
    {
      return new HLink(url,
        new HButton("", std.BeforeAwesome(@"\f067", 0)).Color(iconColor)
      ).Margin(0, 6).Hide(!isAdmin).Title("Добавить раздел");
    }

    public static HLink SortIconButton(bool isAdmin, string url)
    {
      return new HLink(url,
        new HButton("", std.BeforeAwesome(@"\f15d", 0)).Color(iconColor)
      ).Margin(0, 6).Hide(!isAdmin).Title("Сортировать разделы в меню");
    }

    public static IHtmlControl Title(string title)
    {
      return new HPanel(
        new HPanel().Height(40).RelativeWidth(50).LinearGradient("to left", "#fafafa", "#ddd"),
        new HPanel().Height(40).RelativeWidth(50).LinearGradient("to right", "#fafafa", "#ddd"),
        new HLabel(title).FontBold().FontSize("1.5em").PositionAbsolute().Width("100%").Left("0").Bottom("9px")
      ).Align(null).PositionRelative().MarginBottom(20)
        //.BorderBottom("1px solid #bbb")
        .Background("#fafafa");
        //.LinearGradient("to top", "#ddd", "#fafafa");
    }

    public static HButton AddButton(string caption)
    {
      return std.Button(caption, 6, 18, std.BeforeAwesome(@"\f067", 6).VAlign(-2).Color("#3cf33d"));
    }

    public static HButton SaveButton()
    {
      return std.Button("Cохранить", 6, 18, std.BeforeAwesome(@"\f1c0", 6).Color("#233b9e"));
    }

    public static IHtmlControl ReturnButton(string returnUrl)
    {
      return ReturnButton("Вернуться", returnUrl);
    }

    public static IHtmlControl ReturnButton(string caption, string url)
    {
      return new HLink(url,
        std.Button(caption, 6, 18, std.BeforeAwesome(@"\f112", 6).Color("#39abf0"))
      );
    }

    public static IHtmlControl FieldCheck(string label, string dataName, bool isChecked)
    {
      return new HPanel(new HInputCheck(dataName, isChecked).MarginRight(10), new HLabel(label));
    }

    public static IHtmlControl Field(string label, string dataName, string dataText)
    {
      return Field(label, new HTextEdit(dataName, dataText));
    }

    public static IHtmlControl Field(string label, IHtmlControl editControl)
    {
      return Field(new HLabel(label).FontBold(), editControl);
    }

    public static IHtmlControl Field(IHtmlControl labelControl, IHtmlControl editControl)
    {
      return new HPanel(
        labelControl.RelativeWidth(30),
        editControl.RelativeWidth(70)
      ).Padding(5, 0);
    }

    public static IHtmlControl FieldArea(string label, IHtmlControl area)
    {
      return new HPanel(
        new HLabel(label).Block().MarginBottom(5).FontBold(),
        area.Width("100%")
      ).Padding(5, 0);
    }

    public static IHtmlControl Caption(IHtmlControl captionPanel)
    {
      return captionPanel.FontBold().Padding(5, 10, 5, 10)
        .Background("#f1f1f1")
        .LinearGradient("to top", "#dddddd", "#f1f1f1");
    }

    public static IHtmlControl FieldBlock(string caption, IHtmlControl block)
    {
      return FieldBlock(new HLabel(caption).Block(), block);
    }

    public static IHtmlControl FieldBlock(IHtmlControl captionPanel, IHtmlControl block)
    {
      return new HPanel(
        Caption(captionPanel),
        block
      ).Align(true).Border(blockBorder).MarginBottom(20);
    }

    public const string blockBorder = "1px solid #bbb";

    public static IHtmlControl FieldInputBlock(string label, IHtmlControl inputBlock)
    {
      return new HPanel(
        new HLabel(label).Block().FontBold().Padding(5, 10, 5, 10)
          .Background("#f1f1f1").Border(blockBorder).BorderBottom("0px"),
        inputBlock
      ).Align(true).MarginBottom(20);
    }

    static IHtmlControl GetCloseButton(SiteState state, string color)
    {
      return new HButton("", std.BeforeAwesome(@"\f00d", 0), new HHover().Color("#808080"))
        .FontSize(21).Color("#ccc").PaddingTop(17).PaddingRight(17)
        .Title("Закрыть")
        .Event("auth_close", "", delegate
          {
            state.PopupHint = "";
          }
        );
    }

    public static IHtmlControl ShowDialog(SiteState state)
    {
      if (StringHlp.IsEmpty(state.Operation.Message))
        return null;

      return new HEventPanel(
        new HTextView(state.Operation.Message), //.CursorDefault(),
        new HButton("",
          std.BeforeAwesome(@"\f00d", 0),
          new HHover().Color("#C51A3C")
        ).PositionAbsolute().Right(8).Top(5).Color(DecorEdit.propertyMinorColor)
      ).BoxSizing().WidthLimit("", "380px").Align(true).ZIndex(2000)
      .Background("#ffefe9").BorderRadius(4)
      .PaddingLeft(25).PaddingRight(25).PaddingTop(20).PaddingBottom(20)
      .Position("fixed").Margin("0 auto").Top(50).Left(0).Right(0) //.Left("50%").MarginLeft(-140).Top(50)
      .BoxShadow("0px 2px 10px 0px rgba(0, 0, 0, 0.5)")
      .OnClickWithStopPropagation()
      .Event("dialog_close", "", delegate
        {
          state.Operation.Reset();
        }
      );
      //.BoxShadow("0px 2px 10px 0px rgba(63, 69, 75, 0.5)");
    }

    public static IHtmlControl GetDialogBox(SiteState state)
    {
      return GetPopupView(state, 480, 240,
        GetDialogPanel(state, "#000", "#fff", state.Operation.Status,
          new HTextView(state.Operation.Message)
            .FontFamily("Arial").FontSize(15).LineHeight(21).Color("#202242")
        ).PaddingLeft(100).PaddingTop(35).PaddingBottom(40).PaddingRight(50)
      );
    }

    public static IHtmlControl GetDialogPanel(SiteState state,
      string color, string background, string iconKind, IHtmlControl dialogControl)
    {
      IHtmlControl icon = GetDialogIcon(iconKind);
      if (icon != null)
        icon.FontSize(40).Left("30px").Top("50%").MarginTop(-20).PositionAbsolute();

      return new HPanel(
        dialogControl,
        icon,
        DecorEdit.GetCloseButton(state, color).Top("0").Right("0").PositionAbsolute()
      ).Align(true).BoxSizing()
        .Color(color).Background(background);
    }

    public static IHtmlControl GetPopupView(int width, IHtmlControl popupPanel)
    {
      HEventPanel wrapperPanel = new HEventPanel(popupPanel)
        .Width(width).OnClickWithStopPropagation().InlineBlock().VAlign(null).Align(null)
        .Media(width, new HStyle().WidthFull());

      //if (!StringHlp.IsEmpty(state.Operation.Message))
      //  wrapperPanel.Event("popup_click5", "", delegate
      //    {
      //      state?.Operation.Reset();
      //    }
      //  );

      return new HPanel("",
        new IHtmlControl[] {
          wrapperPanel
        }, new HBefore().InlineBlock().Height("100%").VAlign(null).Content("")
      ).Position("fixed").ZIndex(1000).Left("0px").Top("0px").Width("100%").Height("100%")
        .Background("rgba(0,0,0,0.64)");
    }

    [Obsolete("Используйте другую перегрузку")]
    public static IHtmlControl GetPopupView(SiteState state, int width, int height,
      IHtmlControl popupPanel)
    {
      DefaultExtensionContainer defaults = new DefaultExtensionContainer(popupPanel);
      defaults.Width(width);

      popupPanel.Media(width, new HStyle().Width("100%").BoxSizing());

      return new HPanel(
        new HEventPanel(
          popupPanel
        ).OnClick("e.stopPropagation();")
        .Position("fixed").Left("50%").Top("50%").MarginLeft(-width / 2).MarginTop(-height / 2)
        .Media(width, new HStyle().Left("0").MarginLeft(0))
      ).Position("fixed").ZIndex(1000).Left("0px").Top("0px").Width("100%").Height("100%").Background("rgba(0,0,0,0.64)");
    }

    static IHtmlControl GetDialogIcon(string iconKind)
    {
      switch (iconKind)
      {
        case DialogIcon.Info:
          return new HLabel("", std.BeforeAwesome(@"\f05a", 0)).Color("#4285f4");
        case DialogIcon.Warning:
          return new HLabel("", std.BeforeAwesome(@"\f071", 0)).Color("#fbbc05");
        case DialogIcon.Question:
          return new HLabel("", std.BeforeAwesome(@"\f059", 0)).Color("#ea4335");
        case DialogIcon.Error:
          return new HLabel("", std.BeforeAwesome(@"\f28e", 0)).Color("#ea4335");
        case DialogIcon.Success:
          return new HLabel("", std.BeforeAwesome(@"\f00c", 0)).Color("#34a853");
        default:
          return null;
      }
    }

  }


}