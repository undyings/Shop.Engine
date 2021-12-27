using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using System.IO;
using Commune.Html;
using NitroBolt.Wui;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  class FabricFeatureEditHlp
  {
    static IStore store
    {
      get
      {
        return SiteContext.Default.Store;
      }
    }

    static ShopStorage shop
    {
      get { return ((IShopStore)store).Shop; }
    }

    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetFeaturesView(EditState state, int? fabricId, out string title)
    {
      title = "";

      LightKin fabric = null;
      if (fabricId != null)
      {
        fabric = shop.FindFabric(fabricId.Value);
        if (fabric == null)
          return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");
      }

      MetaKind kind = shop.FindFabricKind(fabric.Get(FabricType.Kind));

      title = string.Format("Особенности товара '{0}'", FabricType.DisplayName(fabric));

			List<IHtmlControl> featureBlocks = new List<IHtmlControl>();
			List<LightFeature> features = new List<LightFeature>();

			if (kind != null)
			{
				foreach (RowLink featureRow in kind.AllPropertyRows(MetaKindType.WithFeatures))
				{
					int index = featureRow.Get(PropertyType.PropertyIndex);
					string featureCode = kind.Get(MetaKindType.WithFeatures, index);
					LightFeature feature = shop.FindFeature(featureCode);
					if (feature == null)
						continue;

					features.Add(feature);
				}

				foreach (RowLink featureRow in kind.AllPropertyRows(MetaKindType.WithFeatures))
				{
					int index = featureRow.Get(PropertyType.PropertyIndex);
					string featureCode = kind.Get(MetaKindType.WithFeatures, index);
					LightFeature feature = shop.FindFeature(featureCode);
					if (feature == null)
						continue;

					featureBlocks.Add(
						DecorEdit.FieldBlock(
							new HPanel(
								new HLabel(featureCode).Width(255),
								new HLabel("Цена/наценка").Width(200).FontBold(false),
								new HLabel("Под заказ").FontBold(false)
							),
							new HGrid<LightObject>(feature.FeatureValues, delegate (LightObject value)
								{
									return DecorEdit.Field(value.Get(FeatureValueType.DisplayName),
										new HPanel(
											new HTextEdit(string.Format("markup_{0}", value.Id),
												fabric.Get(FabricType.FeatureMarkups, value.Id).ToString()
											).MarginLeft(15).MarginRight(60),
											new HInputCheck(string.Format("nopresence_{0}", value.Id),
												fabric.Get(FabricType.NoPresences, value.Id)
											)
										).VAlign(true)
									);
								},
								new HRowStyle()
							).MarginLeft(10)
						)
					);
				}
			}

      string returnUrl = UrlHlp.ShopUrl("product", fabric.Id);

      return new HPanel(
        DecorEdit.Title(title),
        DecorEdit.Field("Базовая цена", "price", fabric.Get(FabricType.Price).ToString()),
        DecorEdit.Field("Нет в продаже", new HPanel(
          new HInputCheck("outOfStock", fabric.Get(FabricType.OutOfStock))
        )),
        DecorEdit.Field("Под заказ", new HPanel(
          new HInputCheck("nopresence", fabric.Get(FabricType.NoPresences))
        )),
        new HPanel(
          featureBlocks.ToArray()
        ),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .Event("save_fabric", "editContent",
            delegate (JsonData json)
            {
              try
              {
                int? fabricPrice = json.GetInt("price");
                if (fabricPrice == null || fabricPrice < 0)
                {
                  state.Operation.Message = "Базовая цена товара должна быть целым положительным числом";
                  return;
                }

                bool outOfStock = json.GetBool("outOfStock");
                bool noPresence = json.GetBool("nopresence");

                if (HttpContext.Current.IsInRole("nosave"))
                {
                  state.Operation.Message = "Нет прав на сохранение изменений";
                  return;
                }

                LightKin editFabric = DataBox.LoadKin(fabricConnection, FabricType.Fabric, fabric.Id);

                editFabric.Set(ObjectType.ActTill, DateTime.UtcNow);

                editFabric.Set(FabricType.Price, fabricPrice.Value);
                editFabric.Set(FabricType.OutOfStock, outOfStock);
                editFabric.Set(FabricType.NoPresences, noPresence);

                foreach (LightFeature feature in features)
                {
                  foreach (LightObject value in feature.FeatureValues)
                  {
                    string markupName = string.Format("markup_{0}", value.Id);
                    int markup = json.GetInt(markupName) ?? 0;
                    editFabric.Set(FabricType.FeatureMarkups, value.Id, markup);

                    string nopresenceName = string.Format("nopresence_{0}", value.Id);
                    bool valueNoPresence = json.GetBool(nopresenceName);
                    editFabric.Set(FabricType.NoPresences, value.Id, valueNoPresence);
                  }
                }

                editFabric.Box.Update();
              }
              catch (Exception ex)
              {
                Logger.WriteException(ex);
                state.Operation.Message = ex.Message;
              }
              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }
  }
}
