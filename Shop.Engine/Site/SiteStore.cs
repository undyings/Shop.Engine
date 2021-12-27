using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class SiteStore : IStore
  {
    public static void FillLinks(TranslitLinks links, IStore store, string[] extraLinkKinds)
    {
      store.Sections.FillLinks(links);

      if (store.News != null)
      {
        store.News.FillLinks(links);
        links.AddLink(Site.Novosti, null);
      }

      foreach (string kind in extraLinkKinds)
      {
        links.AddLink(kind, null);
      }
    }

    readonly LightObject contacts;
    public LightObject Contacts
    {
      get { return contacts; }
    }
    readonly LightObject seo;
    public LightObject SEO
    {
      get { return seo; }
    }

    readonly TranslitLinks links = new TranslitLinks();
    public TranslitLinks Links
    {
      get { return links; }
    }

    readonly DateTime createTime = DateTime.UtcNow;
    public DateTime CreateTime
    {
      get { return createTime; }
    }

    readonly SectionStorage sections;
    public SectionStorage Sections
    {
      get { return sections; }
    }

    readonly NewsStorage news;
    public NewsStorage News
    {
      get { return news; }
    }

    readonly WidgetStorage seoWidgets;
    public WidgetStorage SeoWidgets
    {
      get { return seoWidgets; }
    }
    readonly RedirectStorage redirects;
    public RedirectStorage Redirects
    {
      get { return redirects; }
    }

    public SiteStore(SectionStorage sections, NewsStorage news,
      WidgetStorage widgets, RedirectStorage redirects,
      LightObject contacts, LightObject seo, params string[] extraLinkKinds)
    {
      this.sections = sections;
      this.news = news;

      this.seoWidgets = widgets;
      this.redirects = redirects;

      this.contacts = contacts;
      this.seo = seo;

      FillLinks(Links, this, extraLinkKinds);
    }
  }
}
