using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Html;
using Commune.Data;
using Shop.Engine;
using System.Net.Http;

namespace Shop.Prototype
{
  public class ShopState : SiteState
  {
    public string SearchText = "";
    public string SortKind = "";
    public SearchFilter SearchFilter = new SearchFilter();

    //public string PopupHint = "";

    //public readonly WebOperation Operation = new WebOperation();

    public int? SelectedPaymentId = null;
    public int? SelectedDeliveryId = null;

    //public bool EditMode = false;
    //public bool SeoMode = false;

    //public string RedirectUrl = null;
  }

  //public class EditState
  //{
  //  public readonly WebOperation Operation = new WebOperation();

  //  //hack чтобы можно было редактировать только что созданный объект
  //  public int? CreatingObjectId = null;

  //  public int? SelectedLinkedId = null;
  //  public int? SelectedVacantId = null;
  //  public LightObject EditObject = null;
  //}

  public class ShopManagerState
  {
    public readonly WebOperation Operation = new WebOperation();

    public int? SelectedOrderId = null;
  }


}