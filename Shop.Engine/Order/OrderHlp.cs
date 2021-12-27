using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Commune.Basis;
using Commune.Data;
using NitroBolt.Wui;

namespace Shop.Engine
{
  public class OrderHlp
  {
		public static int GetDeliveryCost(LightObject delivery, int productPrice)
		{
			int baseCost = delivery?.Get(DeliveryWayType.Cost) ?? 0;
			int? thresholdCost = ConvertHlp.ToInt(delivery?.Get(DeliveryWayType.Tags, 1));
			if (thresholdCost != null && productPrice >= thresholdCost.Value)
				return 0;
			return baseCost;
		}

    public static VirtualRowLink JsonToRowLink(string json)
    {
      VirtualRowLink row = new VirtualRowLink();

      if (StringHlp.IsEmpty(json))
        return row;

      string content = json.TrimStart('{').TrimEnd('}');
      string[] fields = content.Split(',');

      foreach (string field in fields)
      {
        string[] parts = field.Split(':');
        if (parts.Length != 2)
          continue;
        string name = parts[0].Trim(' ', '"');
        string value = parts[1].Trim(' ', '"');

        ((IRowLink)row).SetValue(name, value);
      }

      return row;
    }

    public static string JsonFromRowLink(VirtualRowLink row)
    {
      StringBuilder builder = new StringBuilder();
      builder.Append("{ ");
      builder.Append(
        StringHlp.Join(", ", row.fieldByName.Keys, delegate (string name)
          {
            string value = row.fieldByName[name]?.ToString();
            return string.Format("\"{0}\": \"{1}\"", name, value);
          }
        )
      );
      builder.Append(" }");
      return builder.ToString();
    }

    public static OrderCookie GetOrderCookie(HttpContext httpContext)
    {
      HttpCookie order = httpContext.Request.Cookies.Get("shop_order");
      if (order == null)
        return new OrderCookie("");
      return new OrderCookie(order);
    }

    public static OrderCookie GetOrCreateOrderCookie(HttpContext httpContext)
    {
      HttpCookie order = httpContext.Request.Cookies.Get("shop_order");
      if (order == null)
      {
        order = new HttpCookie("shop_order");
				order.SameSite = (SameSiteMode)(-1);
				order.Expires = DateTime.UtcNow.AddDays(1);
        httpContext.Request.Cookies.Add(order);
      }

			order.SameSite = (SameSiteMode)(-1);
			httpContext.Response.Cookies.Add(order);

      return new OrderCookie(order);
    }
  }
}
