//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.Web;
//using NitroBolt.Wui;
//using System.IO;
//using Shop.Prototype;
//using Commune.Basis;

//namespace Shop.Engine
//{
//  public class HSync : HWebSynchronizeHandler
//  {
//    public HSync()
//      : base(new Dictionary<string, Func<object, JsonData[], HContext, HtmlResult<HElement>>>
//        {
//          { "index", MainView.HView },
//          { "default", MainView.HView },
//          { "main", MainView.HView },
//          { "edit", ContentEdit.HView },
//          { "seo", SeoEdit.HView },
//          { "manager", ManagerView.HView }
//        },
//        true)
//    {

//    }

//    public static HElement OnFirstTransformer(HElement element)
//    {
//      return element;
//      //return ToHtmlElement(element);
//    }
    
//    static HElement ToHtmlElement(HElement element)
//    {
//      HAttribute[] htmlAttrs = GetHtmlAttributes(element.Attributes);

//      List<HObject> htmlNodes = new List<HObject>();
//      foreach (HObject node in element.Nodes)
//      {
//        if (!(node is HElement))
//        {
//          htmlNodes.Add(node);
//          continue;
//        }
//        htmlNodes.Add(ToHtmlElement((HElement)node));
//      }

//      return new HElement(element.Name, ArrayHlp.Merge(htmlAttrs, htmlNodes.ToArray()));
//    }

//    static HAttribute[] GetHtmlAttributes(HAttribute[] attributes)
//    {
//      List<HAttribute> htmlAttrs = new List<HAttribute>(attributes.Length);
//      foreach (HAttribute attr in attributes)
//      {
//        string name = attr.Name.ToString();
//        if (name.StartsWith("on") || name.StartsWith("data-"))
//          continue;

//        htmlAttrs.Add(attr);
//      }

//      return htmlAttrs.ToArray();
//    }
//  }
//}
