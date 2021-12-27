using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Commune.Basis;
using Commune.Html;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;
using System.Web.Http;
using System.Net;
using System.Net.Http;

namespace Shop.Prototype
{
  public class MainView
  {
    //public delegate HtmlResult<HElement> HViewHandler<TState>(
    //  TState state, JsonData[] jsons, HttpRequestMessage message);

    public static Func<object, JsonData[], HttpRequestMessage, HtmlResult<HElement>> HViewCreator(
      string directory)
    {
      return delegate(object _state, JsonData[] jsons, HttpRequestMessage context)
      {
        ShopState state = _state as ShopState;
        if (state == null)
        {
          state = new ShopState();
        }

        LinkInfo link = store.Links.FindLink(directory);

        //Logger.AddMessage("Directory: {0}, {1}", directory, link != null);

        if (link == null)
        {
          return new HtmlResult
          {
            RawResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound }
          };
        }

        //Logger.AddMessage("Json: {0}", jsons.Length);

        foreach (JsonData json in jsons)
        {
          try
          {
						//Logger.AddMessage("MainView.Json: {0}", json?.ToText());
						//Logger.AddMessage("Json.Name: {0}", json?.JPath("data", "command"));

						if (state.IsRattling(json))
							continue;

						state.Operation.Reset();

            HElement cachePage = Page(HttpContext.Current, state, link.Kind, link.ParentId, link.Id);

            hevent eventh = cachePage.FindEvent(json, true);
            if (eventh != null)
            {
              eventh.Execute(json);
            }
          }
          catch (Exception ex)
          {
            Logger.WriteException(ex);
            state.Operation.Message = string.Format("Непредвиденная ошибка: {0}", ex.Message);
          }

        }

        //Logger.AddMessage("Redirect: {0}", state.RedirectUrl);

        //if (!StringHlp.IsEmpty(state.RedirectUrl))
        //{
        //  HttpResponseMessage response = new HttpResponseMessage();
        //  response.StatusCode = HttpStatusCode.Redirect;
        //  response.Headers.Add("Location", state.RedirectUrl);

        //  return new HtmlResult
        //  {
        //    RawResponse = response
        //  };
        //}

        HElement page = Page(HttpContext.Current, state, link.Kind, link.ParentId, link.Id);
        return HtmlHlp.FirstHtmlResult(page, state, TimeSpan.FromHours(1));
      };
    }

    static readonly HBuilder h = null;

    static IShopStore store
    {
      get
      {
        return (IShopStore)SiteContext.Default.Store;
      }
    }

    static HElement Page(HttpContext httpContext, ShopState state, string kind, int? parentId, int? id)
    {
      //string kind = httpContext.Get("kind") ?? "group";
      //int? id = httpContext.GetUInt("id");

      //Logger.AddMessage("Page: {0}, {1}, {2}", kind, httpContext.Get("kind"), id);

      if (!StringHlp.IsEmpty(state.SearchText))
      {
        kind = "search";
        id = null;
      }

      UserHlp.DirectAuthorization(httpContext, SiteContext.Default.SiteSettings);

      string login = httpContext.UserName();

      state.EditMode = httpContext.IsInRole("edit");
      state.SeoMode = httpContext.IsInRole("seo");

      OrderCookie orderForView = OrderHlp.GetOrderCookie(httpContext);

      string title;
      string description;

      IHtmlControl centerPanel = ViewHlp.GetCenterPanel(httpContext, 
        state, orderForView, kind, parentId, id, out title, out description);

      if (centerPanel == null)
        return null;

      HEventPanel mainPanel = new HEventPanel(
        DecorEdit.AdminMainPanel(SiteContext.Default.SiteSettings, httpContext)
          ?.InlineBlock().WidthLimit("", "1024px"),
        ViewHlp.GetHeaderView(state, httpContext, login, orderForView),
        new HPanel(
          new HXPanel(
            ViewHlp.GetLeftColumn(state, login, kind, parentId, id),
            new HPanel(
              ViewPanelHlp.GetSearchPanel(state),
              centerPanel
            ).PaddingLeft(32) //.WidthLimit("", "800px")
            .Media664(new HStyle().MediaBlock().PaddingLeft(5))
          ).Align(true).WidthLimit("", "1045px").Margin("0 auto")
          .Media("(min-width: 1045px)", new HStyle(".{0}").Width(1045))
        ).MarginBottom(30),
        ViewHlp.GetFooterView(httpContext, state),
        ViewHlp.GetCopyrightView()
      ).Width("100%").FontFamily("Arial").FontSize("14px")
        .Background("#fafef9 url(/images/bg.png) no-repeat");


      if (!StringHlp.IsEmpty(state.PopupHint))
      {
        mainPanel.OnClick(";");
        mainPanel.Event("popup_reset", "", delegate
        {
          state.PopupHint = "";
        });
      }

      StringBuilder css = new StringBuilder();

      HElement mainElement = mainPanel.ToHtml("main", css);

      return h.Html
      (
        h.Head(
          h.Element("title", title),
          h.MetaDescription(description),
          h.LinkCss("/css/static.css"),
          h.LinkCss("/css/font-awesome.css"),
          h.LinkShortcutIcon("/images/favicon.ico"),
          //h.LinkCss("/ionslider/normalize.css"),
          h.LinkCss("/ionslider/ion.rangeSlider.css"),
          h.LinkCss("/ionslider/ion.rangeSlider.skinSimple.css"),
          h.LinkScript("/ionslider/ion.rangeSlider.min.js"),
          h.Raw(store.SeoWidgets.WidgetsCode)
        ),
        h.Body(
          h.Css(h.Raw(css.ToString())),
          HtmlHlp.RedirectScript(state.RedirectUrl),
          mainElement
        )
       );
    }
  }
}