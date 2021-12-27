using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine
{
  public class UnitTunes : BaseTunes
  {
    public UnitTunes(string designKind, string displayName) :
			base (designKind, displayName)
    {
    }
  }

  public static class UnitTunesExt
  {
    public static UnitTunes Tile(this UnitTunes tunes)
    {
      return Tile(tunes, EditElementHlp.thumbWidth);
    }

    public static UnitTunes Tile(this UnitTunes tunes, int tileSize)
    {
      return Tile(tunes, tileSize, tileSize);
    }

    public static UnitTunes Tile(this UnitTunes tunes, int tileWidth, int tileHeight)
    {
      return Tile(tunes, tileWidth, tileHeight, false, false);
    }

    public static UnitTunes Tile(this UnitTunes tunes,
      int tileWidth, int tileHeight, bool proportionalTile, bool pngTile)
    {
      tunes.WithSetting("tileWidth", tileWidth.ToString());
      tunes.WithSetting("tileHeight", tileHeight.ToString());
      tunes.WithSetting("fullFillingTile", !proportionalTile ? "true" : "false");
      tunes.WithSetting("jpegTile", !pngTile ? "true" : "false");

      return tunes.SetTune("Tile");
    }

		public static UnitTunes AdaptTitle(this UnitTunes tunes)
		{
			return tunes.SetTune("AdaptTitle");
		}

		public static UnitTunes AdaptImage(this UnitTunes tunes)
		{
			return tunes.SetTune("AdaptImage");
		}

    public static UnitTunes ImageAlt(this UnitTunes tunes)
    {
      return tunes.SetTune("ImageAlt");
    }

    public static UnitTunes Tag1(this UnitTunes tunes)
    {
      return tunes.SetTune("Tag1");
    }

    public static UnitTunes Tag2(this UnitTunes tunes)
    {
      return tunes.SetTune("Tag2");
    }

    public static UnitTunes Subtitle(this UnitTunes tunes)
    {
      return tunes.SetTune("Subtitle");
    }

    public static UnitTunes Annotation(this UnitTunes tunes)
    {
      return tunes.SetTune("Annotation");
    }

    public static UnitTunes Content(this UnitTunes tunes)
    {
      return tunes.SetTune("Content");
    }

    public static UnitTunes Link(this UnitTunes tunes)
    {
      return tunes.SetTune("Link");
    }

    public static UnitTunes SortTime(this UnitTunes tunes)
    {
      return tunes.SetTune("SortTime");
    }

    public static UnitTunes Gallery(this UnitTunes tunes)
    {
      return Gallery(tunes, EditElementHlp.thumbWidth);
    }

    public static UnitTunes Gallery(this UnitTunes tunes, int thumbSize)
    {
      return Gallery(tunes, thumbSize, thumbSize);
    }

    public static UnitTunes Gallery(this UnitTunes tunes, int thumbWidth, int thumbHeight)
    {
      return Gallery(tunes, thumbWidth, thumbHeight, false, false);
    }

    public static UnitTunes Gallery(this UnitTunes tunes,
      int thumbWidth, int thumbHeight, bool proportionalThumb, bool pngThumb)
    {
      tunes.WithSetting("thumbWidth", thumbWidth.ToString());
      tunes.WithSetting("thumbHeight", thumbHeight.ToString());
      tunes.WithSetting("fullFillingThumb", !proportionalThumb ? "true" : "false");
      tunes.WithSetting("jpegThumb", !pngThumb ? "true" : "false");

      return tunes.SetTune("Gallery");
    }

    public static UnitTunes SortKind(this UnitTunes tunes)
    {
      return tunes.SetTune("SortKind");
    }

    public static UnitTunes SetTune(this UnitTunes tunes, string tuneName)
    {
      tunes.WithTune(tuneName, true);
      return tunes;
    }
  }
}
