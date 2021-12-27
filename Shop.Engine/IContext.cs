using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;
using Commune.Task;

namespace Shop.Engine
{
  public interface IContext
  {
    TaskPull Pull { get; }
    SiteSettings SiteSettings { get; }

    string RootPath { get; }
    string ImagesPath { get; }

    IDataLayer FabricConnection { get; }
    IDataLayer UserConnection { get; }
    IDataLayer OrderConnection { get; }

    UserStorage UserStorage { get; }

    EditorSelector SectionEditorSelector { get; }
    EditorSelector UnitEditorSelector { get; }

		ContextTunes ContextTunes { get; }

    IStore Store { get; }
    void UpdateStore();
  }

	public interface IShopContext : IContext
	{
		EditorSelector GroupEditorSelector { get; }
		EditorSelector FabricEditorSelector { get; }
		SearchTunes SearchTunes { get; }
	}

  public interface IShopStore : IStore
  {
    SearchModule SearchModule { get; }

    ShopStorage Shop { get; }

    ObjectStorage Payments { get; }
    ObjectStorage Deliveries { get; }

    ObjectStorage Landings { get; }
  }

  public interface IStore : ISeoStore
  {
    SectionStorage Sections { get; }
    NewsStorage News { get; }

    TranslitLinks Links { get; }

    DateTime CreateTime { get; }
  }

  public interface ISeoStore
  {
    LightObject Contacts { get; }
    LightObject SEO { get; }

    WidgetStorage SeoWidgets { get; }
    RedirectStorage Redirects { get; }
  }
}
