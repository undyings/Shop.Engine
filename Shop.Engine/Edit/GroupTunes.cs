using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine
{
	public class GroupTunes : BaseTunes
	{
		public GroupTunes(string designKind, string displayName) :
			base (designKind, displayName)
		{
		}
	}

	public static class GroupTunesExt
	{
		public static GroupTunes Tile(this GroupTunes tunes)
		{
			return Tile(tunes, EditElementHlp.thumbWidth);
		}

		public static GroupTunes Tile(this GroupTunes tunes, int tileSize)
		{
			return Tile(tunes, tileSize, tileSize);
		}

		public static GroupTunes Tile(this GroupTunes tunes, int tileWidth, int tileHeight)
		{
			return Tile(tunes, tileWidth, tileHeight, false, false);
		}

		public static GroupTunes Tile(this GroupTunes tunes,
			int tileWidth, int tileHeight, bool proportionalTile, bool pngTile)
		{
			tunes.WithSetting("tileWidth", tileWidth.ToString());
			tunes.WithSetting("tileHeight", tileHeight.ToString());
			tunes.WithSetting("fullFillingTile", !proportionalTile ? "true" : "false");
			tunes.WithSetting("jpegTile", !pngTile ? "true" : "false");

			return tunes.SetTune("Tile");
		}

		public static GroupTunes SetTune(this GroupTunes tunes, string tuneName)
		{
			tunes.WithTune(tuneName, true);
			return tunes;
		}
	}
}
