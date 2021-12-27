using System;
using System.Collections.Generic;
using System.IO;
using Commune.Basis;
using Commune.Data;
using Commune.Task;

namespace Shop.Engine
{
  public class SiteContext : BaseContext
  {
    public SiteContext(string rootPath,
      EditorSelector sectionEditorSelector, EditorSelector unitEditorSelector,
      IDataLayer userConnection, IDataLayer fabricConnection,
      ContextTunes tunes, params string[] extraLinkKinds) :
      base (rootPath, sectionEditorSelector, unitEditorSelector, userConnection, fabricConnection, null, tunes)
    {
      this.lightStoreCache = new Cache<IStore, long>(
        delegate
        {
          LightObject contacts = DataBox.LoadOrCreateObject(fabricConnection,
            ContactsType.Contacts, ContactsType.Kind.CreateXmlIds, "main");

          LightObject seo = DataBox.LoadOrCreateObject(fabricConnection,
            SEOType.SEO, SEOType.Kind.CreateXmlIds, "main");

          SectionStorage sections = SectionStorage.Load(fabricConnection);

          NewsStorage news = null;
          if (tunes.GetTune("news"))
          {
            int newsCount = ConvertHlp.ToInt(tunes.GetSetting("newsCount")) ?? 0;
            news = NewsStorage.Load(fabricConnection, newsCount);
          }

          WidgetStorage widgets = WidgetStorage.Load(fabricConnection, SiteSettings.DisableScripts);

          RedirectStorage redirects = RedirectStorage.Load(fabricConnection);

          return new SiteStore(sections, news, widgets, redirects, contacts, seo, extraLinkKinds);
        },
        delegate { return dataChangeTick; }
      );
    }

    readonly RawCache<IStore> lightStoreCache;
    public override IStore Store
    {
      get
      {
        lock (lockObj)
          return lightStoreCache.Result;
      }
    }
  }

  //public class SiteContext : IContext
  //{
  //  public volatile static IContext Default = null;

  //  protected readonly static object lockObj = new object();

  //  readonly SiteSettings siteSettings;
  //  public SiteSettings SiteSettings
  //  {
  //    get { return siteSettings; }
  //  }

  //  readonly TaskPull pull;
  //  public TaskPull Pull
  //  {
  //    get { return pull; }
  //  }

  //  readonly string rootPath;
  //  public string RootPath
  //  {
  //    get { return rootPath; }
  //  }
  //  readonly string imagesPath;
  //  public string ImagesPath
  //  {
  //    get { return imagesPath; }
  //  }
  //  readonly IDataLayer userConnection;
  //  public IDataLayer UserConnection
  //  {
  //    get { return userConnection; }
  //  }
  //  readonly IDataLayer fabricConnection;
  //  public IDataLayer FabricConnection
  //  {
  //    get { return fabricConnection; }
  //  }

  //  public IDataLayer OrderConnection
  //  {
  //    get { throw new Exception("SiteContext not supported OrderConnection"); }
  //  }

  //  readonly EditorSelector sectionEditorSelector;
  //  public EditorSelector SectionEditorSelector
  //  {
  //    get { return sectionEditorSelector; }
  //  }

  //  readonly EditorSelector unitEditorSelector;
  //  public EditorSelector UnitEditorSelector
  //  {
  //    get { return unitEditorSelector; }
  //  }

  //  public UserStorage UserStorage
  //  {
  //    get { throw new NotImplementedException(); }
  //  }

  //  public SiteContext(string rootPath, 
  //    EditorSelector sectionEditorSelector, EditorSelector unitEditorSelector,
  //    IDataLayer userConnection, IDataLayer fabricConnection)
  //  {
  //    this.rootPath = rootPath;
  //    this.imagesPath = Path.Combine(RootPath, "Images");
  //    this.userConnection = userConnection;
  //    this.fabricConnection = fabricConnection;
  //    this.sectionEditorSelector = sectionEditorSelector;
  //    this.unitEditorSelector = unitEditorSelector;

  //    string settingsPath = Path.Combine(rootPath, "SiteSettings.config");
  //    if (!File.Exists(settingsPath))
  //      this.siteSettings = new SiteSettings();
  //    else
  //      this.siteSettings = XmlSerialization.Load<SiteSettings>(settingsPath);

  //    this.pull = new TaskPull(
  //      new ThreadLabel[] { Labels.Service },
  //      TimeSpan.FromMinutes(15)
  //    );

  //    Pull.StartTask(Labels.Service, SiteTasks.SitemapXmlChecker(this, rootPath));

  //    this.lightStoreCache = new Cache<IStore, long>(
  //      delegate
  //      {
  //        LightObject contacts = DataBox.LoadOrCreateObject(fabricConnection,
  //          ContactsType.Contacts, ContactsType.Kind.CreateXmlIds, "main");

  //        LightObject seo = DataBox.LoadOrCreateObject(fabricConnection,
  //          SEOType.SEO, SEOType.Kind.CreateXmlIds, "main");

  //        SectionStorage sections = SectionStorage.Load(fabricConnection);

  //        NewsStorage news = NewsStorage.Load(fabricConnection, 5);

  //        WidgetStorage widgets = WidgetStorage.Load(fabricConnection, siteSettings.DisableScripts);

  //        RedirectStorage redirects = RedirectStorage.Load(fabricConnection);

  //        return new SiteStore(sections, news, widgets, redirects, contacts, seo);
  //      },
  //      delegate { return dataChangeTick; }
  //    );
  //  }

  //  readonly RawCache<IStore> lightStoreCache;
  //  public IStore Store
  //  {
  //    get
  //    {
  //      lock (lockObj)
  //        return lightStoreCache.Result;
  //    }
  //  }

  //  long dataChangeTick = 0;
  //  public void UpdateStore()
  //  {
  //    lock (lockObj)
  //      dataChangeTick++;
  //  }
  //}
}
