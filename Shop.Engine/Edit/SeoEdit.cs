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
//using System.Web.Http;
using System.Net.Http;

namespace Shop.Engine
{
  public class SeoEdit
  {
    public static HtmlResult<HElement> HView(object _state, JsonData[] jsons, HttpRequestMessage context)
    {
      EditState state = _state as EditState;
      if (state == null)
      {
        state = new EditState();
      }


      foreach (JsonData json in jsons)
      {
        try
        {
          state.Operation.Reset();
          //Logger.AddMessage("ContentEdit.Json: {0}", json.ToText());

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
          state.Operation.Message = string.Format("Непредвиденная ошибка: {0}", ex.Message);
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

    static IHtmlControl GetCenterPanel(EditState state,
      string kind, int? parentId, int? id, out string title)
    {
      title = "";

      switch (kind)
      {
        case "group":
        case "fabric":
        case "page":
          return SeoEditorHlp.GetSEOObjectEdit(state, kind, id, out title);
        case "landing":
          return SeoEditorHlp.GetLandingEdit(state, id, out title);
        case "landing-list":
          return SeoEditorHlp.GetLandingListEdit(state, out title);
        case "seo-pattern":
          return SeoEditorHlp.GetSEOPatternEdit(state, out title);
        case "redirect":
          return SeoEditorHlp.GetRedirectEdit(state, id, out title);
        case "redirect-list":
          return SeoEditorHlp.GetRedirectListEdit(out title);
        case "widget":
          return SeoEditorHlp.GetWidgetEdit(state, id, out title);
        case "widget-list":
          return SeoEditorHlp.GetWidgetListEdit(state, out title);
        default:
          return new HPanel();
      }
    }

    static HElement Page(EditState state, HttpContext httpContext)
    {
      int? parentId = httpContext.GetUInt("parent");
      string kind = httpContext.Get("kind");
      int? id = httpContext.GetUInt("id");
      if (id == null)
        id = state.CreatingObjectId;

      string title = "SEO поля";

      IHtmlControl editPanel = new HPanel();
      if (!httpContext.IsInRole("seo"))
      {
        editPanel = EditHlp.GetInfoMessage("Недостаточно прав для редактирования SEO полей", "/");
      }
      else if (state.Operation.Completed)
      {
        editPanel = EditHlp.GetInfoMessage(state.Operation.Message, state.Operation.ReturnUrl);
      }
      else
      {
        editPanel = GetCenterPanel(state, kind, parentId, id, out title);
      }

      IHtmlControl mainPanel = new HPanel(
        new HPanel(
          editPanel.Background("white"),
          std.OperationWarning(state.Operation)
        ).WidthLimit("", "800px").Margin("0 auto")
      ).Width("100%").Background("#fafef9");

      StringBuilder css = new StringBuilder();

      std.AddStyleForFileUploaderButtons(css);

      HElement mainElement = mainPanel.ToHtml("main", css);

      return h.Html
      (
        h.Head(
          h.Element("title", title),
          h.LinkScript("/scripts/fileuploader.js"),
          h.LinkScript("/ckeditor/ckeditor.js"),
          HtmlHlp.CKEditorUpdateAll(),
          h.LinkCss("/css/fileuploader.css"),
          h.LinkCss("/css/static.css"),
          h.LinkCss("/css/font-awesome.css")
        ),
        h.Body(
          //HtmlHlp.CKEditorUpdateAll(),
          h.Css(h.Raw(css.ToString())),
          mainElement
        )
      );
    }
  }
}