using Commune.Html;
using Commune.Data;
using Shop.Engine;
using NitroBolt.Wui;

namespace Shop.Engine
{
  public class EditState
  {
    public readonly WebOperation Operation = new WebOperation();

    //hack чтобы можно было редактировать только что созданный объект
    public int? CreatingObjectId = null;

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

		public string PopupDialog = "";
  }

  public class EditOptionType
  {
    // MetaEdit
    public readonly static FieldBlank<int?> SelectedLinkedId = new FieldBlank<int?>("SelectedLinkedId");
    public readonly static FieldBlank<int?> SelectedVacantId = new FieldBlank<int?>("SelectedVacantId");
    public readonly static FieldBlank<LightObject> EditObject = new FieldBlank<LightObject>("EditObject");

		// панель изображений
		public readonly static FieldBlank<bool> AllowImageDeletion = new FieldBlank<bool>("AllowImageDeletion");

    // галерея
    public readonly static FieldBlank<int?> MovableImageIndex = new FieldBlank<int?>("MovableImageIndex");
    public readonly static FieldBlank<bool> AllowDeleteImage = new FieldBlank<bool>("AllowDeleteImage");
  }
}
