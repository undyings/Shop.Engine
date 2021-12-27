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
  public class ViewFilterHlp
  {
    public static IHtmlControl GetFilterColumn(ShopState state, SearchIndexStorage storage)
    {
      SearchFilter filter = state.SearchFilter;

      List<IHtmlControl> propertyBlocks = new List<IHtmlControl>();
      foreach (ISearchIndex index in storage.PriorSearchIndices)
      {
        IHtmlControl block = GetPropertyBlock(index, filter, true);
        if (block != null)
          propertyBlocks.Add(block);
      }

      foreach (ISearchIndex index in storage.MinorSearchIndices)
      {
        IHtmlControl block = GetPropertyBlock(index, filter, false);
        if (block != null)
          propertyBlocks.Add(block);
      }

      propertyBlocks.Add(
        Decor.EcoButton("filterButton", "Применить", null, null).Padding(8, 35)
        .Event("apply_filter", "filterContent",
          delegate (JsonData json)
          {
            state.SearchFilter = SearchHlp.GetFilterFromJson2(storage, json);
          }
        ).Display("none")
      );

      return new HPanel(
        new HPanel(
          propertyBlocks.ToArray()
        ).WidthLimit("", "222px").InlineBlock().Align(true)
        .FontFamily("Arial").FontSize(13).LineHeight("1em").Color("#818181")
      ).EditContainer("filterContent").Align(null);
    }

    public static IHtmlControl GetPropertyBlock(ISearchIndex index, SearchFilter filter, bool isPriorProperty)
    {
      SearchCondition condition = filter.FindCondition(index.Property.Id);

      IHtmlControl propertyPanel = null;
      if (index is NumericalSearchIndex)
        propertyPanel = GetDiapasonPanel((NumericalSearchIndex)index);
      else if (index is EnumSearchIndex)
        propertyPanel = GetMultiEnumPanel((EnumSearchIndex)index);

      if (propertyPanel == null)
        return null;

      return new HSpoilerPanel(string.Format("property_{0}", index.Property.Id),
        new HButton("", std.BeforeAwesome(@"\f0da", 6)),
        new HButton("", std.BeforeAwesome(@"\f0d7", 6)),
        //new HImage("/images/bot.png", new HHover().Cursor("hand")).VAlign(-2),
        //new HImage("/images/top.png", new HHover().Cursor("hand")).VAlign(-2),
        false,
        new HLabel(
          index.Property.Get(MetaPropertyType.Identifier), 
          new HHover().Cursor("hand").Color(Decor.propertyColor)
        ).MarginRight(4).FontBold().Color("#535353").FontSize(14),
        propertyPanel,
        isPriorProperty
      ).MarginTop(15);
    }

    static IHtmlControl GetDiapasonPanel(NumericalSearchIndex index)
    {
      string measureUnit = index.Property.Get(MetaPropertyType.MeasureUnit);

      if (index.Min == index.Max)
        return null;

      return new HPanel(
        new HIonSlider(
          string.Format("property_{0}", index.Property.Id),
          true, index.Min ?? 0, index.Max ?? 0, index.Accuracy, 
          measureUnit, false, "$('.filterButton').click();",
          new HAttribute("force_edges", "true")
        )
      ).PaddingRight(6).MarginTop(8);
    }

    static IHtmlControl GetMultiEnumPanel(EnumSearchIndex index)
    {
      List<IHtmlControl> controls = new List<IHtmlControl>();
      int i = -1;
      foreach (string variant in index.SortedEnumVariants)
      {
        i++;

        if (StringHlp.IsEmpty(variant))
          continue;

        HCheckButton variantControl = new HCheckButton(
          string.Format("property_{0}_{1}", index.Property.Id, i),
          new HLabel(variant).BoxSizing().Width(105).Padding(3), 
          new HPanel(
            new HLabel(variant)
            .BoxSizing().Width(87),
            new HLabel("", std.AfterAwesome(@"\f00d", 0).VAlign(1)).MarginRight(1)
              .Width(12).PositionAbsolute().Right("0px").Top("50%").MarginTop(-7)
          ).PositionRelative().BoxSizing().Width(105).Padding(3).Background("#bf2525").Color("white"),
          false, "$('.filterButton').click();"
        ).MarginRight(6).MarginBottom(4);
        //HCheckEdit variantControl = new HCheckEdit(
        //  string.Format("property_{0}_{1}", index.Property.Id, i),
        //  false, new HTone().Size(40, 40).Background("green"), new HTone().Size(30, 30).Margin(5).Background("white"),
        //  new HAfter().Content(variant)
        //).Margin(5);
        //HButton variantControl = new HButton(variant)
        //  .Event("filter_property", "",
        //    delegate(JsonData json)
        //    {
        //      condition.InvertSelection(variant);
        //    },
        //    index.Property.Id
        //  );
        //if (condition.IsSelection(variant))
        //  variantControl.Color("white").Background("#bf2525");
        //else
        //  variantControl.Color("#818181");

        controls.Add(variantControl);
      }

      return new HPanel(
        controls.ToArray()
      ).MarginTop(8);
    }
  }
}