using Commune.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Engine
{
  public class TileBar
  {
    readonly VirtualRowLink option;
    readonly FieldBlank<int> tileIndexBlank;
    public readonly int TileCount;
    public readonly int ShowTileCount;
		bool round;

    public TileBar(VirtualRowLink option, FieldBlank<int> tileIndexBlank, 
			int tileCount, int showTileCount, bool round)
    {
      this.option = option;
      this.tileIndexBlank = tileIndexBlank;
      this.TileCount = tileCount;
      this.ShowTileCount = showTileCount;
			this.round = round;
    }

    public int CurrentIndex
    {
      get
      {
        return option.Get(tileIndexBlank);
      }
    }

		public int EndIndex
		{
			get
			{
				return Math.Min(CurrentIndex + ShowTileCount, TileCount) - 1;
			}
		}

    public bool PrevPossible()
    {
			if (round)
				return TileCount > 2;

      int currentIndex = CurrentIndex;
      return currentIndex > 0;
    }

    public bool NextPossible()
    {
			if (round)
				return TileCount > 1;

      int currentIndex = CurrentIndex;
      return currentIndex + ShowTileCount < TileCount;
    }

		public int PrevIndex()
		{
			return (CurrentIndex + TileCount - 1) % TileCount;
		}

		public int NextIndex()
		{
			return (CurrentIndex + TileCount + 1) % TileCount;
		}

    public bool Prev()
    {
      if (!PrevPossible())
        return false;

      option.Set(tileIndexBlank, PrevIndex());
      return true;
    }

    public bool Next()
    {
      if (!NextPossible())
        return false;

      option.Set(tileIndexBlank, NextIndex());
      return true;
    }
  }
}
