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
using System.IO;
using System.Net.Mail;

namespace Shop.Prototype
{
  public class ViewHlp
  {
    static IDataLayer orderConnection
    {
      get
      {
        return SiteContext.Default.OrderConnection;
      }
    }

    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    static IDataLayer userConnection
    {
      get
      {
        return SiteContext.Default.UserConnection;
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

    public static IHtmlControl GetLeftColumn(ShopState state, string login, string kind, int? parentId, int? id)
    {
      return new HPanel(
        new HPanel(
          new HLink("/",
            new HImage(@"/Images/logo.png")
          ),
          new HLabel("",
            Decor.DividerAfter()
          ).Block().PositionRelative()
            .MarginTop(24).MarginBottom(24).BorderBottom("1px solid #E5E5DC")
        ).Align(null).MarginTop(4),
        ViewPanelHlp.GetLeftPanel(state, store, kind, parentId, id)
      ).Width(245).Padding(0, 5).Media664(new HStyle().MediaBlock(true).MarginBottom(30));
    }

    public static IHtmlControl GetHeaderView(ShopState state, HttpContext httpContext, 
      string login, OrderCookie orderForView)
    {
      int orderPrice = 0;
      int allProductCount = orderForView.GetAllProductCount(shop, out orderPrice);

      LightObject contacts = store.Contacts;

      List<IHtmlControl> controls = new List<IHtmlControl>();
      controls.AddRange(new IHtmlControl[] {
        new HLabel(contacts.Get(ContactsType.Phones, 0)).FontWeight("700").Padding(0, 20, 5, 0),
        new HLabel(contacts.Get(ContactsType.Header))
          .Color(Decor.PriceColor).Padding(0, 0, 5, 0),
        std.DockFill().Media900(new HStyle().MediaBlock().Height(5).Padding(0))
      });

      //controls.Add(
      //  std.Button("Redirect").Event("redirect", "editContent", delegate
      //  {
      //    state.RedirectUrl = "/page/oplata-i-dostavka"; // "http://basketball.ru/";
      //    Logger.AddMessage("Redirection: {0}", state.RedirectUrl);
      //  })
      //);

      LightSection menu = store.Sections.FindMenu("top");
      if (menu != null)
      {
        if (state.EditMode)
        {
          controls.AddRange(new IHtmlControl[] {
            DecorEdit.AddIconButton(state.EditMode, UrlHlp.EditUrl(menu.Id, "page", null)),
            DecorEdit.SortIconButton(state.EditMode, UrlHlp.EditUrl(menu.Id, "sorting_section", null))
          });
        }

        foreach (LightSection section in menu.Subsections)
        {
          controls.Add(
            Decor.HeaderLink(section.NameInMenu, UrlHlp.ShopUrl("page", section.Id))
          );
        }
      }

      controls.AddRange(new IHtmlControl[] {
        ViewElementHlp.GetUserControlForHeader(httpContext, login),
        Decor.HeaderLink("Корзина " + (allProductCount != 0 ? string.Format("({0})", allProductCount) : ""),
          UrlHlp.ShopUrl("cart", null)).Padding(0)
      });      

      return new HPanel(
        std.RowPanel(
          controls.ToArray()
          //new HLabel(contacts.Get(ContactsType.Phones, 0)).FontWeight("700").Padding(0, 20, 5, 0),
          //new HLabel(contacts.Get(ContactsType.Header))
          //  .Color(Decor.PriceColor).Padding(0, 0, 5, 0),
          //std.DockFill().Media900(new HStyle().MediaBlock().Height(5).Padding(0)),
          //Decor.HeaderLink("Оплата и доставка", UrlHlp.ShopUrl("oplata-i-dostavka")),
          //Decor.HeaderLink("Контакты", UrlHlp.ShopUrl("kontakty")),
          //ViewElementHlp.GetUserControlForHeader(httpContext, login),
          //Decor.HeaderLink("Корзина " + (allProductCount != 0 ? string.Format("({0})", allProductCount) : ""),
          //  UrlHlp.ShopUrl("cart", null)).Padding(0)
        ).InlineBlock().WidthLimit("", "1024px").NoWrap().Padding(10, 0, 0, 0)
        .Media664(new HStyle(".{0} > div").MediaBlock().Padding(5).FontSize("1.5em").Wrap())
      ).Align(null).FontSize("13px").LineHeight("1.65em").MarginBottom(32);
    }

    public static IHtmlControl GetCopyrightView()
    {
      return new HPanel(
        new HPanel(
          new HLink("http://webkrokus.ru", 
            new HLabel("Крокус",
              new HHover().Color(Decor.PriceColor).TextDecoration("underline")
            ).PaddingBottom(1).FontBold().Color("#f3f2eb")
          ),
          new HLabel("Челябинск @ 2016").Color("#B1B0A7").MarginLeft("0.35em")
        ).InlineBlock().Width("75%").Align(true).Padding(18, 0)
      ).Align(null).Background("#443F1E");
    }

    public static IHtmlControl GetFooterView(HttpContext httpContext, ShopState state)
    {
      LightObject contacts = store.Contacts;

      string seoText = store.SEO.Get(SEOType.FooterSeoText);

      List<IHtmlControl> controls = new List<IHtmlControl>();
      controls.AddRange(new IHtmlControl[] {
        new HLabel("ПОКУПАТЕЛЯМ").Block().MarginBottom(11)
          .Color("#EEE").FontFamily("Tahoma").FontSize("22px"),
        new HLink(UrlHlp.ShopUrl(Site.Novosti, null),
          new HLabel("Новости").Color("#B1B0A7")
        ).Block().MarginBottom(6)
      });
      LightSection menu = store.Sections.FindMenu("bottom");
      if (menu != null)
      {
        foreach (LightSection section in menu.Subsections)
        {
          controls.Add(
            new HLink(UrlHlp.ShopUrl("page", section.Id),
              new HLabel(section.NameInMenu).Color("#B1B0A7")
            )
          );
        }

        if (state.EditMode)
        {
          controls.Add(new HPanel(
            DecorEdit.AddIconButton(true, UrlHlp.EditUrl(menu.Id, "page", null)),
            DecorEdit.SortIconButton(true, UrlHlp.EditUrl(menu.Id, "sorting_section", null))
          ));
        }
      }
      controls.AddRange(new IHtmlControl[] {
        DecorEdit.RedoButton(state.SeoMode, "Посадочные страницы", UrlHlp.SeoUrl("landing-list", null)),
        DecorEdit.RedoButton(state.SeoMode, "Все перенаправления", UrlHlp.SeoUrl("redirect-list", null)),
        DecorEdit.RedoButton(state.SeoMode, "SEO виджеты", UrlHlp.SeoUrl("widget-list", null))
      });

      return new HPanel(
        new HPanel(
          new HPanel(
            new HPanel(
              new HH4(contacts.Get(ContactsType.Brand))
                .Color("#EEE").FontSize("22px").FontBold(false),
              new HTextView(contacts.Get(ContactsType.Address)).MarginTop(11).MarginBottom("1.65em")
                .Color("#B1B0A7"),
              new HLabel(contacts.Get(ContactsType.Phones, 0)).Block().FontBold().Color("#F3F2EB"),
              new HLabel(contacts.Get(ContactsType.Phones, 1)).Block().FontBold().Color("#F3F2EB"),
              new HLabel(contacts.Get(ContactsType.Email)).Block().FontBold().Color("#F3F2EB"),
              DecorEdit.RedoButton(state.EditMode, "Предприятие", UrlHlp.EditUrl("contacts-column", null)),
              DecorEdit.RedoButton(state.SeoMode, "SEO шаблоны", UrlHlp.SeoUrl("seo-pattern", null))
            ).RelativeWidth(25).Align(true, true),
            new HPanel(
              controls.ToArray()
              //new HLabel("ПОКУПАТЕЛЯМ").Block().MarginBottom(11)
              //  .Color("#EEE").FontFamily("Tahoma").FontSize("22px"),
              //new HLink(UrlHlp.ShopUrl("newslist", null),
              //  new HLabel("Новости").Color("#B1B0A7")
              //).Block().MarginBottom(6),
              //new HLink(UrlHlp.ShopUrl("offers"),
              //  new HLabel("Публичная оферта").Color("#B1B0A7")
              //),
              //DecorEdit.RedoButton(state.SeoMode, "Посадочные страницы", UrlHlp.SeoUrl("landing-list", null)),
              //DecorEdit.RedoButton(state.SeoMode, "Все перенаправления", UrlHlp.SeoUrl("redirect-list", null)),
              //DecorEdit.RedoButton(state.SeoMode, "SEO виджеты", UrlHlp.SeoUrl("widget-list", null))
            ).RelativeWidth(25).Align(true, true),
            new HPanel(
              new HTextView(contacts.Get(ContactsType.About)),
              new HTextView(seoText).MarginTop("1em").Hide(StringHlp.IsEmpty(seoText))
            ).RelativeWidth(50).Align(true, true).Color("#FFF")
          ).Media900(new HStyle(".{0} > div").MediaBlock(true).PaddingLeft(20).MarginBottom(20))
        ).WidthLimit("", "1024px").Margin("0 auto")
      ).Padding(40, 20, 20, 20).Background("#847939").LineHeight("1.65em");
    }

    public static IHtmlControl GetCenterPanel(HttpContext httpContext, ShopState state,
      OrderCookie orderForView, string kind, int? parentId, int? id,
      out string title, out string description)
    {
      title = "";
      description = "";

      switch (kind)
      {
				case "catalog":
        case "group":
          return ViewCenterHlp.GetGroupView(httpContext, state, orderForView, id,
            out title, out description);
        case "product":
          return ViewCenterHlp.GetProductView(httpContext, state, orderForView, parentId, id,
            out title, out description);
        case "cart":
          return ViewCenterHlp.GetCartView(httpContext, orderForView, out title);
        case "search":
          return ViewCenterHlp.GetSearchView(httpContext, orderForView, state.SearchText, out title);
        case "order":
          return ViewCenterHlp.GetOrderView(httpContext, state, orderForView, out title);
        case "payment":
          return ViewCenterHlp.GetPaymentView(httpContext, state, httpContext.Get("orderId"), out title);
        case "register":
          return ViewCenterHlp.GetRegisterPanel(state, out title);
        case "login":
          return ViewCenterHlp.GetLoginPanel(httpContext, state, out title);
        case "passwordreset":
          return ViewCenterHlp.GetPasswordResetPanel(httpContext, state, out title);
        case "page":
          return ViewCenterHlp.GetSectionView(state.EditMode, id, out title);
        //case "oplata-i-dostavka":
        //case "offers":
        //  return ViewCenterHlp.GetNoteView(state.EditMode, kind, out title);
        //case "kontakty":
        //  return ViewCenterHlp.GetContactsView(state.EditMode, out title);
        case "landing":
          return ViewCenterHlp.GetLandingView(httpContext, state, orderForView, id, out title, out description);
      }

      if (kind == Site.Novosti)
      {
        if (id == null)
          return ViewCenterHlp.GetNewsList(state.EditMode, out title);
        return ViewCenterHlp.GetNewsView(state.EditMode, id, out title);
      }

      return null;
    }

  }
}