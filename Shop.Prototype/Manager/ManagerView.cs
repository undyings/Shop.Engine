using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Basis;
using Commune.Html;
using Commune.Data;
using Shop.Engine;
using System.Web.Http;
using System.Net.Http;

namespace Shop.Prototype
{
  public class ManagerView
  {
    public static HtmlResult<HElement> HView(object _state, JsonData[] jsons, HttpRequestMessage context)
    {
      ShopManagerState state = _state as ShopManagerState;
      if (state == null)
      {
        state = new ShopManagerState();
      }


      foreach (JsonData json in jsons)
      {
        try
        {
          state.Operation.Reset();
          Logger.AddMessage("ContentEdit.Json: {0}", json.ToText());

          HElement cachePage = Page(state, HttpContext.Current);

          hevent eventh = cachePage.FindEvent(json, true);
          if (eventh != null)
          {
            eventh.Execute(json);
          }
        }
        catch (Exception ex)
        {
          Logger.WriteException(ex);
        }
      }

      var page = Page(state, HttpContext.Current);
      return new HtmlResult<HElement>
      {
        Html = page,
        State = state,
        RefreshPeriod = TimeSpan.FromSeconds(5)
      };
    }

 
    static readonly HBuilder h = null;

    static HElement Page(ShopManagerState state, HttpContext httpContext)
    {
      int? parentId = httpContext.GetUInt("parent");
      string kind = httpContext.Get("kind");
      int? id = httpContext.GetUInt("id");

      IHtmlControl editPanel = new HPanel();
      string login = httpContext.UserName();
      if (login != "manager")
      {
        editPanel = EditHlp.GetInfoMessage("Этот режим доступен только менеджерам", "/");
      }
      else if (state.Operation.Completed)
      {
        editPanel = EditHlp.GetInfoMessage(state.Operation.Message, state.Operation.ReturnUrl);
      }
      else
      {
        //switch (kind)
        //{
        //  case "order":
        //    editPanel = ManagerHlp.GetOrderProcessingView(httpContext, store, state);
        //    break;
        //}
      }

      IHtmlControl mainPanel = new HPanel(
        editPanel,
        std.OperationWarning(state.Operation)
      ); //.WidthLimit("", "800px");

      string title = "Управление заказами";

      StringBuilder css = new StringBuilder();

      HElement mainElement = mainPanel.ToHtml("main", css);

      return h.Html
      (
        h.Head(
          h.Element("title", title),
          //h.LinkScript("js/fileuploader.js"),
          //h.LinkScript("ckeditor/ckeditor.js"),
          //HtmlHlp.CKEditorUpdateAll(),
          //h.LinkCss("css/fileuploader.css"),
          h.LinkCss("css/font-awesome.css")
        ),
        h.Body(
          HtmlHlp.CKEditorUpdateAll(),
          h.Css(h.Raw(css.ToString())),
          mainElement
        )
      );
    }
  }
}