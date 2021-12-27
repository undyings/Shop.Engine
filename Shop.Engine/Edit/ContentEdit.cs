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
  public class ContentEdit
  {
    //[HttpGet, HttpPost]
    //[Route("edit2")]
    //public HttpResponseMessage Route()
    //{
    //  Logger.AddMessage("Edit.Route");
    //  return HWebApiSynchronizeHandler.Process<object>(this.Request, HView);
    //}

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
					if (state.IsRattling(json))
						continue;

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

    static IHtmlControl GetCenterPanel(HttpContext httpContext, EditState state,
      string kind, int? parentId, int? id, out string title)
    {
      title = "";

      switch (kind)
      {
        case "group":
          return EditHlp.GetGroupEdit(state, parentId, id, out title);
        case "fabric":
          return EditHlp.GetFabricEdit(state, parentId, id, out title);
        case "variety":
          return EditHlp.GetVarietyEdit(state, parentId, id, out title);
        case "fabric-feature":
          return FabricFeatureEditHlp.GetFeaturesView(state, id, out title);
        case "payment":
          return EditHlp.GetPaymentWayEdit(state, id, out title);
        case "delivery":
          return EditHlp.GetDeliveryWayEdit(state, id, out title);
        case "news":
          return EditHlp.GetNewsEdit(state, id, out title);
        case "sorting_payment":
          return EditHlp.GetSortingPaymentWays(state, out title);
        case "sorting_delivery":
          return EditHlp.GetSortingDeliveryWays(state, out title);
        case "sorting_group":
          return EditHlp.GetSortingGroupEdit(state, parentId, out title);
        case "sorting_fabric":
          return EditHlp.GetSortingFabricEdit(state, parentId, out title);
        case "sorting_section":
          return EditHlp.GetSortingSectionEdit(state, parentId, out title);
        case "sorting_unit":
          return EditHlp.GetSortingUnitEdit(state, parentId, out title);
        case "sorting_subunit":
          return EditHlp.GetSortingSubunitEdit(state, parentId, out title);
        case "page":
          {
            string design = httpContext.Get("design");
            return EditHlp.GetSectionEdit(state, parentId, id, design, out title);
          }
        case "unit":
          {
            string design = httpContext.Get("design");
            return EditHlp.GetUnitEdit(state, parentId, id, design, out title);
          }
        //case "oplata-i-dostavka":
        //case "offers":
        //  return EditHlp.GetArticleEdit(httpContext, state, kind, out title);
        //case "kontakty":
        //  return EditHlp.GetContactsViewEdit(httpContext, state, out title);
        case "contacts-column":
          return EditHlp.GetContactsColumnEdit(state, out title);
        case "shop-catalog":
          return EditHlp.GetShopCatalogEdit(state, out title);
        case "kind-list":
          return MetaEditHlp.GetKindListEdit(out title);
        case "kind":
          return MetaEditHlp.GetKindEdit(state, id, out title);
        case "property-list":
          return MetaEditHlp.GetPropertyListEdit(out title);
        case "property":
          return MetaEditHlp.GetPropertyEdit(state, id, out title);
        case "category-list":
          return MetaEditHlp.GetCategoryListEdit(out title);
        case "category":
          return MetaEditHlp.GetCategoryEdit(state, id, out title);
        case "feature-list":
          return FeatureEditHlp.GetFeatureListEdit(state, out title);
        case "feature":
          return FeatureEditHlp.GetFeatureEdit(state, id, out title);
        case "feature-value":
          return FeatureEditHlp.GetFeatureValueEdit(state, parentId, id, out title);
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

      string title = "Редактирование";

      IHtmlControl editPanel = new HPanel();
      if (!httpContext.IsInRole("edit"))
      {
        editPanel = EditHlp.GetInfoMessage("Недостаточно прав для редактирования", "/");
      }
      else if (state.Operation.Completed)
      {
        editPanel = EditHlp.GetInfoMessage(state.Operation.Message, state.Operation.ReturnUrl);
      }
      else
      {
        editPanel = GetCenterPanel(httpContext, state, kind, parentId, id, out title);
      }

      HEventPanel mainPanel = new HEventPanel(
        new HPanel(
          editPanel.Background("white"),
          EditElementHlp.GetOperationPopup(state.Operation)
          //std.OperationWarning(state.Operation)
        ).WidthLimit("", "800px").Margin("0 auto")
      ).Width("100%").Background("#fafef9");

      if (!StringHlp.IsEmpty(state.Operation.Status))
      {
        mainPanel.OnClick(";").Event("main_popup_reset", "",
          delegate
          {
            state.Operation.Reset();
          }
        );
      }

      StringBuilder css = new StringBuilder();

      std.AddStyleForFileUploaderButtons(css);

      HElement mainElement = mainPanel.ToHtml("main", css);

			return h.Html
			(
				h.Head(
					h.Element("title", title),
					h.LinkScript("/scripts/fileuploader.js"),
					h.LinkScript("/ckeditor/ckeditor.js?v=4113"),
					//h.LinkScript("https://cdn.ckeditor.com/4.11.3/full-all/ckeditor.js"),
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