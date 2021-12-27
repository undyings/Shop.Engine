using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine
{
	public abstract class BaseTunes : TuneContainer<bool>
	{
		public readonly string DesignKind;
		public readonly string DisplayName;

		public BaseTunes(string designKind, string displayName)
		{
			this.DesignKind = designKind;
			this.DisplayName = displayName;
		}
	}

  public class SectionTunes : BaseTunes
  {
    public SectionTunes(string designKind, string displayName) :
			base (designKind, displayName)
    {
    }
  }

  public static class SectionTunesExt
  {
    public static SectionTunes Tile(this SectionTunes tunes)
    {
      return Tile(tunes, EditElementHlp.thumbWidth);
    }

    public static SectionTunes Tile(this SectionTunes tunes, int tileSize)
    {
      return Tile(tunes, tileSize, tileSize);
    }

    public static SectionTunes Tile(this SectionTunes tunes, int tileWidth, int tileHeight)
    {
      return Tile(tunes, tileWidth, tileHeight, false, false);
    }

    public static SectionTunes Tile(this SectionTunes tunes,
      int tileWidth, int tileHeight, bool proportionalTile, bool pngTile)
    {
      tunes.WithSetting("tileWidth", tileWidth.ToString());
      tunes.WithSetting("tileHeight", tileHeight.ToString());
      tunes.WithSetting("fullFillingTile", !proportionalTile ? "true" : "false");
      tunes.WithSetting("jpegTile", !pngTile ? "true" : "false");

      return tunes.SetTune("Tile");
    }

    public static SectionTunes SortKind(this SectionTunes tunes)
    {
      return tunes.SetTune("SortKind");
    }

    public static SectionTunes UnitSortKind(this SectionTunes tunes)
    {
      return tunes.SetTune("UnitSortKind");
    }

    public static SectionTunes SortTime(this SectionTunes tunes)
    {
      return tunes.SetTune("SortTime");
    }

    public static SectionTunes Tag1(this SectionTunes tunes)
    {
      return tunes.SetTune("Tag1");
    }

    public static SectionTunes Tag2(this SectionTunes tunes)
    {
      return tunes.SetTune("Tag2");
    }

    public static SectionTunes NameInMenu(this SectionTunes tunes)
    {
      return tunes.SetTune("NameInMenu");
    }

		public static SectionTunes HideInMenu(this SectionTunes tunes)
		{
			return tunes.SetTune("HideInMenu");
		}

    public static SectionTunes Subtitle(this SectionTunes tunes)
    {
      return tunes.SetTune("Subtitle");
    }

    public static SectionTunes Annotation(this SectionTunes tunes)
    {
      return tunes.SetTune("Annotation");
    }

    public static SectionTunes Link(this SectionTunes tunes)
    {
      return tunes.SetTune("Link");
    }

    public static SectionTunes IsHtmlAnnotation(this SectionTunes tunes)
    {
      return tunes.SetTune("IsHtmlAnnotation");
    }

    public static SectionTunes Content(this SectionTunes tunes)
    {
      return tunes.SetTune("Content");
    }

    public static SectionTunes Widget(this SectionTunes tunes)
    {
      return tunes.SetTune("Widget");
    }

    public static SectionTunes UnderContent(this SectionTunes tunes)
    {
      return tunes.SetTune("UnderContent");
    }

    public static SectionTunes SetTune(this SectionTunes tunes, string tuneName)
    {
      tunes.WithTune(tuneName, true);
      return tunes;
    }
  }
}
