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
  public class SeoHlp
  {
    static IStore store
    {
      get
      {
        return SiteContext.Default.Store;
      }
    }

    public static void GroupSeo(LightGroup group, out string title, out string description)
    {
      if (group == null)
      {
        title = store.SEO.Get(SEOType.MainTitle);
        description = store.SEO.Get(SEOType.MainDescription);
        return;
      }

      string heading = group.Get(GroupType.Identifier);

      title = FabricHlp.GetSeoTitle(group,
          (store.SEO.Get(SEOType.GroupTitlePattern) ?? "").Replace("<<group>>", heading));
      description = FabricHlp.GetSeoDescription(group,
          (store.SEO.Get(SEOType.GroupDescriptionPattern) ?? "").Replace("<<group>>", heading));
    }

		public static void ProductSeo(LightGroup group, Product product, out string title, out string description)
		{
			ProductSeo(group, product, product.ProductName, out title, out description);
		}

    public static void ProductSeo(LightGroup group, Product product, string productName,
			out string title, out string description)
    {
      title = product.Get(SEOProp.Title);
      if (StringHlp.IsEmpty(title))
        title = group?.Get(SEOType.ProductTitlePattern)?.Replace("<<product>>", productName);
      if (StringHlp.IsEmpty(title))
        title = store.SEO.Get(SEOType.ProductTitlePattern)?.Replace("<<product>>", productName);

      description = product.Get(SEOProp.Description);
      if (StringHlp.IsEmpty(description))
        description = group?.Get(SEOType.ProductDescriptionPattern)?.Replace("<<product>>", productName);
      if (StringHlp.IsEmpty(description))
        description = store.SEO.Get(SEOType.ProductDescriptionPattern)?.Replace("<<product>>", productName);
    }

    public static void SectionSeo(LightSection section, out string title, out string description)
    {
      string sectionName = section.Get(SectionType.Title);
      title = FabricHlp.GetSeoTitle(section,
        (store.SEO.Get(SEOType.SectionTitlePattern) ?? "").Replace("<<title>>", sectionName)
      );

      description = FabricHlp.GetSeoDescription(section,
        (store.SEO.Get(SEOType.SectionDescriptionPattern) ?? "").Replace("<<title>>", sectionName)
      );
    }
  }
}