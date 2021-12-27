using Commune.Html;
using Commune.Data;
using NitroBolt.Wui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Engine
{
  public class Site
  {
    public static string Novosti = "novosti";
    public static bool DirectPageLinks = false;
    public static bool AddFolderForNews = false;
  }


	public interface IState
  {
    DateTime AccessTime { get; set; }
    bool EditMode { get; set; }
    bool SeoMode { get; set; }
    bool UserMode { get; set; }
    string PopupHint { get; set; }
    string BlockHint { get; set; }
		VirtualRowLink Option { get; }
	}

  public class SiteState : IState
  {
    readonly object lockObj = new object();

    DateTime accessTime = DateTime.UtcNow;
    public DateTime AccessTime
    {
      get
      {
        lock (lockObj)
          return accessTime;
      }
      set
      {
        lock (lockObj)
          accessTime = value;
      }
    }

    volatile bool editMode = false;
    public bool EditMode
    {
      get { return editMode; }
      set { editMode = value; }
    }

    volatile bool seoMode = false;
    public bool SeoMode
    {
      get { return seoMode; }
      set { seoMode = value; }
    }

    volatile bool userMode = false;
    public bool UserMode
    {
      get { return userMode; }
      set { userMode = value; }
    }

    public volatile bool ModeratorMode = false;

    volatile string popupHint = "";
    public string PopupHint
    {
      get { return popupHint; }
      set { popupHint = value; }
    }

    volatile string blockHint = "";
    public string BlockHint
    {
      get { return blockHint; }
      set { blockHint = value; }
    }

    readonly VirtualRowLink option = new VirtualRowLink();
		public VirtualRowLink Option
		{
			get { return option; }
		}

    volatile string lastJson = "";
    public bool IsRattling(JsonData json)
    {
			if (json?.JPath("data", "command") == null)
				return true;

			string jsonAsStr = json.ToString();
      if (lastJson == jsonAsStr)
        return true;

      lastJson = jsonAsStr;
      return false;
    }

    public void ResetPopup()
    {
      this.PopupHint = "";
      this.Operation.Reset();
    }

    public void SetBlockHint(string hint)
    {
      if (BlockHint == hint)
        BlockHint = "";
      else
        BlockHint = hint;
    }

    public readonly WebOperation Operation = new WebOperation();

    public volatile string RedirectUrl = null;

    public volatile int GalleryIndex = 0;

    public volatile object Tag = null;

    public void ShowDialog(string popupHint, string iconKind, string message)
    {
      this.PopupHint = popupHint;
      this.Operation.Status = iconKind;
      this.Operation.Message = message;
    }

    public volatile int OperationCounter = 0;
    public string Actual(string name)
    {
      return string.Format("{0}_{1}", name, OperationCounter);
    }
  }
}
