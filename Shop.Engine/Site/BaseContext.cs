using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Commune.Basis;
using Commune.Data;
using Commune.Task;

namespace Shop.Engine
{
  public abstract class BaseContext : IContext
  {
    public volatile static IContext Default = null;

    protected readonly static object lockObj = new object();

    readonly SiteSettings siteSettings;
    public SiteSettings SiteSettings
    {
      get { return siteSettings; }
    }

    readonly TaskPull pull;
    public TaskPull Pull
    {
      get { return pull; }
    }

    readonly string rootPath;
    public string RootPath
    {
      get { return rootPath; }
    }
    readonly string imagesPath;
    public string ImagesPath
    {
      get { return imagesPath; }
    }
    readonly IDataLayer userConnection;
    public IDataLayer UserConnection
    {
      get { return userConnection; }
    }
    readonly IDataLayer fabricConnection;
    public IDataLayer FabricConnection
    {
      get { return fabricConnection; }
    }

    readonly IDataLayer orderConnection;
    public IDataLayer OrderConnection
    {
      get { return orderConnection; }
    }

    readonly EditorSelector sectionEditorSelector;
    public EditorSelector SectionEditorSelector
    {
      get { return sectionEditorSelector; }
    }

    readonly EditorSelector unitEditorSelector;
    public EditorSelector UnitEditorSelector
    {
      get { return unitEditorSelector; }
    }

		readonly ContextTunes contextTunes;
		public ContextTunes ContextTunes
		{
			get { return contextTunes; }
		}

    public BaseContext(string rootPath,
      EditorSelector sectionEditorSelector, EditorSelector unitEditorSelector,
      IDataLayer userConnection, IDataLayer fabricConnection, IDataLayer orderConnection, 
			ContextTunes contextTunes)
    {
      this.rootPath = rootPath;
      this.imagesPath = Path.Combine(RootPath, "Images");
      this.userConnection = userConnection;
      this.fabricConnection = fabricConnection;
      this.orderConnection = orderConnection;
      this.sectionEditorSelector = sectionEditorSelector;
      this.unitEditorSelector = unitEditorSelector;
			this.contextTunes = contextTunes;

      string settingsPath = Path.Combine(rootPath, "SiteSettings.config");
			this.siteSettings = XmlSerialization.SafeLoad<SiteSettings>(settingsPath);
      //if (!File.Exists(settingsPath))
      //  this.siteSettings = new SiteSettings();
      //else
      //  this.siteSettings = XmlSerialization.Load<SiteSettings>(settingsPath);

      this.pull = new TaskPull(
        new ThreadLabel[] { Labels.Service },
        TimeSpan.FromMinutes(15)
      );

      Pull.StartTask(Labels.Service, SiteTasks.SitemapXmlChecker(this, rootPath));

      if (contextTunes.GetTune("reviews"))
      {
        this.reviewsCache = new Cache<LightObject[], long>(
          delegate
          {
            ObjectBox reviewBox = new ObjectBox(userConnection,
              DataCondition.ForTypes(ReviewType.Review) + " order by act_from desc"
            );

            return ArrayHlp.Convert(reviewBox.AllObjectIds, delegate (int reviewId)
              { return new LightObject(reviewBox, reviewId); }
            );
          },
          delegate { return userDataChangeTick; }
        );
      }

      if (contextTunes.GetTune("users"))
      {
        this.userStorage = new UserStorage(userConnection);
      }
    }

    public abstract IStore Store { get; }

    protected long dataChangeTick = 0;
    public void UpdateStore()
    {
      lock (lockObj)
        dataChangeTick++;
    }

    readonly UserStorage userStorage = null;
    public UserStorage UserStorage
    {
      get { return userStorage; }
    }

    readonly RawCache<LightObject[]> reviewsCache = null;
    public LightObject[] Reviews
    {
      get
      {
        if (reviewsCache == null)
          return new LightObject[0];

        lock (lockObj)
          return reviewsCache.Result;
      }
    }

    protected long userDataChangeTick = 0;
    public void UpdateUserData()
    {
      lock (lockObj)
        userDataChangeTick++;
    }
  }
}
