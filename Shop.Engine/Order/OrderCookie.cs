using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Commune.Html;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class OrderCookie
  {
    readonly HttpCookie order;
    public OrderCookie(HttpCookie order)
    {
      this.order = order;
    }

    public OrderCookie(string cookie) :
      this(new HttpCookie("shop_order", cookie))
    {
    }

    public string AsString()
    {
      return order.Value;
    }

    public VirtualRowLink GetValue(string productKey)
    {
      string json = order.Values[productKey];
      if (StringHlp.IsEmpty(json))
        return null;

      return OrderHlp.JsonToRowLink(json);
    }

    public int GetCount(int productId)
    {
      VirtualRowLink row = GetValue(productId.ToString());
      if (row == null)
        return 0;

      return row.Get(OrderProductType.Count);
    }

		public int GetPrice(int productId)
		{
			VirtualRowLink row = GetValue(productId.ToString());
			if (row == null)
				return 0;

			return row.Get(OrderProductType.Price);
		}

    public void SetValue(string productKey, VirtualRowLink row)
    {
      string json = OrderHlp.JsonFromRowLink(row);
      order.Values[productKey] = json;

      //Logger.AddMessage("Product: {0}, {1}", productKey, json);
    }

    //public void SetCount(int productId, int count)
    //{
    //  VirtualRowLink row = GetValue(productId);
    //  row.Set(OrderProductType.Count, count);
    //  SetValue(productId, row);
    //  //order.Values[productId.ToString()] = count.ToString();
    //}

    public void Increment(Product product)
    {
      string productKey = product.ProductId.ToString();

      VirtualRowLink row = GetValue(productKey);
      if (row == null)
      {
        row = new VirtualRowLink();
        row.Set(OrderProductType.ProductKey, productKey);
        row.Set(OrderProductType.ProductId, product.ProductId);
        row.Set(OrderProductType.Price, product.Get(FabricType.Price));
      }

      int count = row.Get(OrderProductType.Count);
      row.Set(OrderProductType.Count, count + 1);
      SetValue(productKey, row);
    }

    public void Increment(string productKey)
    {
      VirtualRowLink row = GetValue(productKey);
      if (row == null)
      {
        Logger.AddMessage("OrderCookie.Increment.Empty: {0}", productKey);
        return;
      }

      row.Set(OrderProductType.Count, row.Get(OrderProductType.Count) + 1);
      SetValue(productKey, row);      
    }

    public void Decrement(string productKey)
    {
      VirtualRowLink row = GetValue(productKey);
      if (row == null)
        return;

      int count = row.Get(OrderProductType.Count);
      if (count == 0)
        return;

      row.Set(OrderProductType.Count, count - 1);
      SetValue(productKey, row);
    }

    public void Clear()
    {
      order.Values.Clear();
			order.Expires = DateTime.UtcNow.AddDays(-1);
    }

    public int GetAllProductCount(ShopStorage store, out int orderPrice)
    {
      orderPrice = 0;
      int allCount = 0;
      foreach (VirtualRowLink row in AllProducts(store))
      {
        int count = row.Get(OrderProductType.Count);
        int price = row.Get(OrderProductType.Price);

        allCount += count;
        orderPrice += count * price;
      }

      return allCount;
    }

    public IEnumerable<VirtualRowLink> AllProducts(ShopStorage store)
    {
      foreach (string productKey in order.Values.Keys)
      {
        VirtualRowLink row = GetValue(productKey);
        if (row == null)
          continue;

        int productId = row.Get(OrderProductType.ProductId);
        Product product = store.FindProduct(productId);
        if (product == null)
          continue;

        int count = row.Get(OrderProductType.Count);
        if (count == 0)
          continue;

        yield return row;
      }
    }
  }
}
