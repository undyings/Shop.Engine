using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine
{
  public class ContextTunes : TuneContainer<bool>
  {
  }

  public static class ContextTunesExt
  {
    public static ContextTunes Users(this ContextTunes tunes)
    {
      tunes.WithTune("users", true);
      return tunes;
    }

    public static ContextTunes Reviews(this ContextTunes tunes)
    {
      tunes.WithTune("reviews", true);
      return tunes;
    }

    public static ContextTunes News(this ContextTunes tunes, int loadNewsCount)
    {
      tunes.WithTune("news", true);
      tunes.WithSetting("newsCount", loadNewsCount.ToString());
      return tunes;
    }

    public static ContextTunes Landings(this ContextTunes tunes)
    {
      tunes.WithTune("landings", true);
      return tunes;
    }
  }
}
