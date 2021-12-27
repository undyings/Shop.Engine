using System;
using System.Collections.Generic;
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
  public class SeoPanelHlp
  {
    public static HPanel GetCommonPropertyPanel(string kind, LightObject seo)
    {
      return new HPanel(
        DecorEdit.FieldArea("Титул страницы",
          new HTextEdit("title", seo.Get(SEOProp.Title))
        ),
        DecorEdit.FieldArea("H1 заголовок страницы",
          new HTextEdit("heading", seo.Get(FabricType.Identifier))
        ),
        DecorEdit.FieldArea("SEO описание",
          new HTextArea("description", seo.Get(SEOProp.Description))
        ),
        DecorEdit.FieldArea("SEO текст",
          new HTextArea("seoText", seo.Get(SEOProp.Text))
        ).Hide(kind != "fabric")
      ).Margin(0, 10).MarginBottom(20);
    }

    public static IHtmlControl GetSortKindCombo(string selectedKind)
    {
      return DecorEdit.Field("Сортировать:",
        new HComboEdit<string>("sortKind", selectedKind,
          delegate (string kind)
          {
            return SearchHlp.SortKindToDisplay(kind);
          },
          SearchHlp.AllSortKinds
        )
      );
    }

    public static IHtmlControl GetEditFilterPanel(SearchModule search, LightObject landing, MetaKind kind)
    {
      if (kind == null)
        return new HPanel().Hide(true);

      List<IHtmlControl> controls = new List<IHtmlControl>();

      SearchIndexStorage storage = search.FindIndexStorage(kind.Id);
      if (storage != null)
      {
        SearchFilter filter = SearchHlp.FilterFromLandingPage(storage, landing);

        ISearchIndex[] allIndices = storage.AllSearchIndices;
        controls.Add(
          new HPanel(
            EditElementHlp.GetFilterRows(allIndices, filter)
          ) //.Background("#ffffff").Padding(5)
        );
      }

      return DecorEdit.FieldBlock("Фильтр",
        new HPanel(
          controls.ToArray()
        ).PaddingLeft(8).PaddingRight(8).PaddingTop(10)
      ).MarginTop(10);
    }

  }
}