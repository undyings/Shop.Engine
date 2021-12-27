using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;
using Commune.Html;

namespace Shop.Engine
{
  public class EditorSelector
  {
    //[Obsolete]
    //public static EditorSelector ForSection()
    //{
    //  EditorSelector selector = new EditorSelector();
    //  selector.AddEditor("", "Текст", SectionEditorHlp.GetTextEdit);
    //  selector.AddEditor("contact", "Контакты", SectionEditorHlp.GetContactEdit);

    //  return selector;
    //}

    //[Obsolete]
    //public static EditorSelector ForUnit()
    //{
    //  EditorSelector selector = new EditorSelector();
    //  selector.AddEditor("", "Текст", UnitEditorHlp.GetTextEdit);
    //  selector.AddEditor("link", "Ссылка", UnitEditorHlp.GetImageLinkEdit);
    //  selector.AddEditor("gallery", "Галерея изображений", UnitEditorHlp.GetGalleryImagesEdit);

    //  return selector;
    //}

    //readonly Dictionary<string, Getter<IHtmlControl, EditState, LightKin>> editorByDesignKind =
    //  new Dictionary<string, Getter<IHtmlControl, EditState, LightKin>>();

    //readonly Dictionary<string, string> displayNameByDesignKind = new Dictionary<string, string>();

		readonly Dictionary<string, BaseTunes> tunesByDesignKind = new Dictionary<string, BaseTunes>();

		public EditorSelector()
    {
    }

    public EditorSelector(params SectionTunes[] tunes)
    {
      foreach (SectionTunes tune in tunes)
      {
				tunesByDesignKind[tune.DesignKind] = tune;

        //AddEditor(tune.DesignKind, tune.DisplayName,
        //  delegate (EditState state, LightKin section)
        //  {
        //    return SectionEditorHlp.GetEditor(state, section, tune);
        //  }
        //);
      }
    }

    public EditorSelector(params UnitTunes[] tunes)
    {
      foreach (UnitTunes tune in tunes)
      {
				tunesByDesignKind[tune.DesignKind] = tune;

        //AddEditor(tune.DesignKind, tune.DisplayName,
        //  delegate (EditState state, LightKin section)
        //  {
        //    return UnitEditorHlp.GetEditor(state, section, tune);
        //  }
        //);
      }
    }

		public EditorSelector(params GroupTunes[] tunes)
		{
			foreach (GroupTunes tune in tunes)
			{
				tunesByDesignKind[tune.DesignKind] = tune;
			}
		}

    //public void AddEditor(string designKind, string displayName,
    //  Getter<IHtmlControl, EditState, LightKin> editor)
    //{
    //  editorByDesignKind[designKind] = editor;
    //  displayNameByDesignKind[designKind] = displayName;
    //}

    public string[] AllKinds
    {
			get { return _.ToArray(tunesByDesignKind.Keys); }
      //get { return _.ToArray(editorByDesignKind.Keys); }
    }

		public string GetDisplayName(string designKind)
		{
			return FindTunes(designKind)?.DisplayName;
		}

		public BaseTunes FindTunes(string designKind)
		{
			return DictionaryHlp.GetValueOrDefault(tunesByDesignKind, designKind);
		}

    //public string GetDisplayName(string designKind)
    //{
    //  return DictionaryHlp.GetValueOrDefault(displayNameByDesignKind, designKind);
    //}

    //public Getter<IHtmlControl, EditState, LightKin> FindEditor(string designKind)
    //{
    //  return DictionaryHlp.GetValueOrDefault(editorByDesignKind, designKind);
    //}
  }
}
