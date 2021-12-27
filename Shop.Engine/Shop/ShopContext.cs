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
  public class ShopContext : BaseContext, IShopContext
  {
		readonly EditorSelector groupEditorSelector;
		public EditorSelector GroupEditorSelector
		{
			get { return groupEditorSelector; }
		}
		readonly EditorSelector fabricEditorSelector;
		public EditorSelector FabricEditorSelector
		{
			get { return fabricEditorSelector; }
		}
		readonly SearchTunes searchTunes;
		public SearchTunes SearchTunes
		{
			get { return searchTunes; }
		}

		public ShopContext(string rootPath,
			EditorSelector sectionEditorSelector, EditorSelector unitEditorSelector,
			EditorSelector groupEditorSelector, EditorSelector fabricEditorSelector,
			IDataLayer userConnection, IDataLayer fabricConnection, IDataLayer orderConnection,
			ContextTunes contextTunes, SearchTunes searchTunes, params string[] extraLinkKinds) :
			base(rootPath, sectionEditorSelector, unitEditorSelector,
				userConnection, fabricConnection, orderConnection, contextTunes
			)
		{
			this.groupEditorSelector = groupEditorSelector;
			this.fabricEditorSelector = fabricEditorSelector;
			this.searchTunes = searchTunes;

			this.lightStoreCache = new Cache<ShopStore, long>(
				delegate
				{
					ShopStore store = ShopStore.Load(fabricConnection, SiteSettings.DisableScripts,
						contextTunes, searchTunes
					);
					ShopStore.FillLinks(store.Links, store, extraLinkKinds);
					return store;
				},
				delegate { return dataChangeTick; }
			);
		}

    readonly RawCache<ShopStore> lightStoreCache;
    public override IStore Store
    {
      get
      {
        lock (lockObj)
          return lightStoreCache.Result;
      }
    }
  }

}
