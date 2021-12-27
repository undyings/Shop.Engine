using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Data;
using Commune.Html;
using Shop.Engine;

namespace Shop.Prototype
{
  public static class Decor
  {
    public const string BedColor = "#000";
    public const string BedMinorColor = "#888888";

    public static T Media664<T>(this T control, params HStyle[] styles)
      where T : IEditExtension
    {
      return control.Media("(max-width: 680px)", styles);
    }

    public static T Media900<T>(this T control, params HStyle[] styles)
      where T : IEditExtension
    {
      return control.Media("(max-width: 900px)", styles);
    }

    public static T Media1024<T>(this T control, params HStyle[] styles)
      where T : IEditExtension
    {
      return control.Media("(max-width: 1023px)", styles);
    }

    public static HAfter DividerAfter()
    {
      return new HAfter().Content("").PositionAbsolute()
        .Background("transparent url(/images/divider.png) center center no-repeat")
        .Size(28, 17).Left("50%").MarginLeft(-14).Bottom("-8px");
    }

    //public static HBefore EcoBefore(string content, int marginRight)
    //{
    //  return new HBefore().Content(content).FontFamily("FontAwesome").MarginRight(marginRight);
    //}

    //public static HAfter EcoAfter(string content, int marginLeft)
    //{
    //  return new HAfter().Content(content).FontFamily("FontAwesome").MarginLeft(marginLeft);
    //}

    public static HButton EcoButton(string caption)
    {
      return EcoButton(caption, null, null);
    }

    public static HButton EcoButton(string caption, HBefore before, HAfter after)
    {
      return EcoButton("", caption, before, after);
    }

    public static HButton EcoButton(string name, string caption, HBefore before, HAfter after)
    {
      return new HButton(name, caption,
        new HHover().Background("#237F35").Color("rgba(255, 255, 255, 0.6)"),
        after,
        before
      ).Background("#71A866").Align(null).Color("rgba(255, 255, 255, 0.9)").BorderRadius(2);
    }

    public const string orderBorder = "1px solid #E5E5DC";
    public const string orderBackground = "#F3F2EB";

    public const string propertyColor = "#3f454b";
    public const string propertyMinorColor = "#7f868e";

    public static IHtmlControl OrderLabel(string caption)
    {
      return new HLabel(caption).Color("#222").FontBold().LineHeight("1.65em");
    }

    public static IHtmlControl OrderEditControl(IHtmlControl editControl)
    {
      return editControl.Width("100%").Padding(7, 10).MarginBottom(15)
        .Background(orderBackground).BorderWithRadius(orderBorder, 2)
        .Color("#555").FontFamily("Arial").FontSize("14px").FontWeight("400").Opacity("0.8");
    }

    public static IHtmlControl OrderEdit(string dataName)
    {
      return OrderEditControl(new HTextEdit(dataName, ""));
    }

    public static IHtmlControl OrderEdit(string dataName, string value)
    {
      return OrderEditControl(new HTextEdit(dataName, value));
    }

    public static IHtmlControl AuthEditControl(IHtmlControl editControl)
    {
      return editControl.Width("100%").Padding(7, 10).MarginBottom(15)
        .BorderWithRadius("1px solid #CCC", 2)
        .Color("#555").FontFamily("Arial").FontSize("14px").FontWeight("400").Opacity("0.8");
    }

    public const string authBackground = "#F6F6F6";

    public static IHtmlControl AuthEdit(string dataName)
    {
      return AuthEditControl(new HTextEdit(dataName));
    }
    public const string TitleColor = "#6CAF22";
    public const string PriceColor = "#71A866";
    public const string TileName = "#000";
    //public const string CartHover = 
    public const string TileAnnotationColor = "#888";
    public const string SeparatorColor = "#E5E5DC";
    public const string SeparatorHeight = "2px";

    public const string headerColor = "#555";

    public static IHtmlControl HeaderLink(string caption, string url)
    {
      return new HLink(url,
        new HLabel(caption, 
          new HHover().Color("#777").BorderBottom("4px solid #9ACD32")
        ).Padding(0, 0, 1, 0).BorderBottom("4px solid transparent").Color(Decor.headerColor)
      ).MarginRight(20); //.Padding(0, 20, 0, 0);
    }

    public static HHover UnderlineHover()
    {
      return new HHover().TextDecoration("underline");
    }

    public static HHover UnderlineHover(string color)
    {
      return new HHover().TextDecoration("underline").Color(color);
    }
  }
}