using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class ShopStore : IShopStore
  {
    public static ShopStore Load(IDataLayer fabricConnection, bool disableScripts, 
      ContextTunes tunes, SearchTunes searchTunes)
    {
      ObjectBox paymentBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(PaymentWayType.Payment));
      ObjectBox deliveryBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(DeliveryWayType.Delivery));

      LightObject contacts = DataBox.LoadOrCreateObject(fabricConnection,
        ContactsType.Contacts, ContactsType.Kind.CreateXmlIds, "main");

      LightObject seo = DataBox.LoadOrCreateObject(fabricConnection,
        SEOType.SEO, SEOType.Kind.CreateXmlIds, "main");

      ObjectBox landingBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(LandingType.Landing) +
        " order by xml_ids asc"
      );

      SectionStorage sections = SectionStorage.Load(fabricConnection);
      ShopStorage shop = ShopStorage.Load(fabricConnection);

      SearchModule search = SearchModule.Create(fabricConnection, searchTunes, shop.AllFabricKinds);
      search.Fill(shop.AllProducts);

      ObjectStorage payments = new ObjectStorage(paymentBox);
      ObjectStorage deliveries = new ObjectStorage(deliveryBox);

      ObjectStorage landings = null;
      if (tunes.GetTune("landings"))
      {
        landings = new ObjectStorage(landingBox);
      }

      NewsStorage news = null;
      if (tunes.GetTune("news"))
      {
        int newsCount = ConvertHlp.ToInt(tunes.GetSetting("newsCount")) ?? 0;
        news = NewsStorage.Load(fabricConnection, newsCount);
      }

      WidgetStorage widgets = WidgetStorage.Load(fabricConnection, disableScripts);

      RedirectStorage redirects = RedirectStorage.Load(fabricConnection);

      return new ShopStore(sections, shop, search,
        payments, deliveries, landings, news,
        widgets, redirects, contacts, seo
      );
    }

    public static void FillLinks(TranslitLinks links, IShopStore store, params string[] extraLinkKinds)
    {
      SiteStore.FillLinks(links, store, extraLinkKinds);

      store.Shop.FillLinks(links);

      links.AddLink("cart", null);
      links.AddLink("order", null);

      if (store.Landings != null)
      {
        foreach (LightObject landing in store.Landings.All)
        {
          links.AddLink("landing", landing.Id, landing.Get(LandingType.DisplayName),
            landing.Get(ObjectType.ActTill) ?? FabricHlp.RefTime
          );
        }
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

    readonly SearchModule searchModule;
    public SearchModule SearchModule
    {
      get { return searchModule; }
    }

    readonly SectionStorage sections;
    public SectionStorage Sections
    {
      get { return sections; }
    }
    readonly ShopStorage shop;
    public ShopStorage Shop
    {
      get { return shop; }
    }

    readonly ObjectStorage payments;
    public ObjectStorage Payments
    {
      get { return payments; }
    }
    readonly ObjectStorage deliveries;
    public ObjectStorage Deliveries
    {
      get { return deliveries; }
    }

    readonly ObjectStorage landings;
    public ObjectStorage Landings
    {
      get { return landings; }
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

    public ShopStore(SectionStorage sections, 
      ShopStorage shop, SearchModule searchModule,
      ObjectStorage payments, ObjectStorage deliveries, 
      ObjectStorage landings, NewsStorage news,
      WidgetStorage widgets, RedirectStorage redirects,
      LightObject contacts, LightObject seo)
    {
      this.sections = sections;
      this.shop = shop;
      this.searchModule = searchModule;

      this.payments = payments;
      this.deliveries = deliveries;

      this.landings = landings;
      this.news = news;

      this.seoWidgets = widgets;
      this.redirects = redirects;

      this.contacts = contacts;
      this.seo = seo;

      //FillLinks(Links, this, extraLinkKinds);
    }
  }
}
