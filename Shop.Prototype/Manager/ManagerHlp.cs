using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using System.IO;
using Commune.Html;
using NitroBolt.Wui;
using Shop.Engine;
using Commune.Data;
using Commune.Basis;

namespace Shop.Prototype
{
  public class ManagerHlp
  {
    static IDataLayer orderConnection
    {
      get
      {
        return SiteContext.Default.OrderConnection;
      }
    }

    public static IHtmlControl GetOrderProcessingView(HttpContext httpContext, 
      IShopStore store, ShopManagerState state)
    {
      ObjectBox orderBox = new ObjectBox(orderConnection,
        string.Format("{0} and act_till is null order by act_from desc",
          DataCondition.ForTypes(OrderType.Order))
      );

      return new HPanel(
        ViewElementHlp.GetViewTitle("Заказы"),
        std.RowPanel(),
        new HPanel(
          new HPanel(
            new HGrid<int>(orderBox.AllObjectIds,
              delegate(int orderId)
              {
                LightObject order = new LightObject(orderBox, orderId);
                OrderCookie cookie = new OrderCookie(order.Get(OrderType.Products));
                int orderPrice;
                cookie.GetAllProductCount(store.Shop, out orderPrice);
                return std.RowPanel(
                  new HInputRadio("orders", orderId, state.SelectedOrderId == orderId,
                    delegate(JsonData json)
                    {
                      state.SelectedOrderId = orderId;
                    }
                  ).MarginLeft(5).MarginRight(8),
                  new HLabel(UrlHlp.TimeToString(order.Get(ObjectType.ActFrom))).Width(150),
                  std.DockFill(new HLabel(UrlHlp.GetFIO(order))),
                  new HLabel(orderPrice).Width(100).Align(false)
                ).Padding(5);
              },
              new HRowStyle().Odd(new HTone().Background("#F9F9F9"))
            )
          ).RelativeWidth(50),
          new HPanel(
          ).RelativeWidth(50)
        )
      );
    }
  }
}