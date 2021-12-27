using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;
using System.IO;

namespace Shop.Engine.Cml
{
  public class CmlWriterHlp
  {
    public const string UnloadingOrdersCounterName = "unloading_order";

    const string dateFormat = "yyyy-MM-dd";
    const string timeFormat = "hh:mm:ss";

		public static void ExportOrder(string exportPath, IShopStore store, LightObject order)
		{
			ICmlElement cmlOrder = CmlWriterHlp.OrderToCml(store, order);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			cmlOrder.ToXmlText(builder, 0);

			string filePath = Path.Combine(exportPath, string.Format("{0}_order.xml", order.Id));
			File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
		}

		public static string FormatPhone(string phone)
		{
			if (StringHlp.IsEmpty(phone))
				return "";

			if (phone.StartsWith("+7"))
				phone = "8" + phone.Substring(2);

			List<char> numbers = new List<char>();
			foreach (char ch in phone)
			{
				if (char.IsNumber(ch))
					numbers.Add(ch);				
			}

			if (numbers.Count == 10)
				numbers.Insert(0, '8');

			return new string(numbers.ToArray());
		}

		public static ICmlElement OrderToCml(IShopStore store, LightObject order)
		{
			int orderId = order.Get(ObjectType.ObjectId);
			DateTime? utcTime = order.Get(ObjectType.ActFrom);

			string phone = order.Get(OrderType.Phone);
			string family = order.Get(OrderType.Family);
			string firstName = order.Get(OrderType.FirstName);
			string patronymic = order.Get(OrderType.Patronymic);

			//string fio = UrlHlp.GetFIO(order);

			OrderCookie orderCookie = new OrderCookie(order.Get(OrderType.Products));

			int orderPrice;
			orderCookie.GetAllProductCount(store.Shop, out orderPrice);

			LightObject payment = store.Payments.Find(order.Get(OrderType.PaymentKind));
			LightObject delivery = store.Deliveries.Find(order.Get(OrderType.DeliveryKind));

			List<ICmlElement> productElements = new List<ICmlElement>();
			foreach (VirtualRowLink productRow in orderCookie.AllProducts(store.Shop))
			{
				int productId = productRow.Get(OrderProductType.ProductId);
				Product product = store.Shop.FindProduct(productId);
				if (product == null)
					continue;

				productElements.Add(
					ProductToCml(product, productRow)
				);
			}

			return new CmlElement("Документ",
				new CmlText("Ид", orderId),
				new CmlText("Номер", orderId),
				new CmlText("Дата", utcTime?.ToString(dateFormat)),
				new CmlText("Время", utcTime?.ToString(timeFormat)),
				new CmlElement("Контрагент",
					new CmlText("Ид", string.Format("{0}#{1}", FormatPhone(phone), family)),
					new CmlText("Наименование", UrlHlp.GetFIO(order)),
					new CmlText("ПолноеНаименование", UrlHlp.GetFullName(order)),
					new CmlText("ИНН", ""),
					new CmlText("КПП", ""),
					new CmlElement("Контакты",
						Contact("Электронная почта", order.Get(OrderType.EMail)),
						Contact("Телефон", phone)
					)
				),
				new CmlText("Сумма", orderPrice),
				new CmlText("ПримечаниеПокупателя", order.Get(OrderType.Comment)),
				new CmlText("Комментарий", string.Format("[Номер документа на сайте: {0}]", orderId)),
				new CmlElement("ЗначенияРеквизитов",
					Requisite("Отменен", "false"),
					Requisite("Оплачен", "false"),
					Requisite("Адрес доставки", order.Get(OrderType.Address)),
					Requisite("Номер документа", order.Get(OrderType.DocumentNumber)),
					Requisite("Вид оплаты", payment?.Get(PaymentWayType.DisplayName)),
					Requisite("Вид доставки", delivery?.Get(DeliveryWayType.DisplayName))
				),
				new CmlElement("Товары",
					productElements.ToArray()
				)
			);
		}

		public static ICmlElement ProductToCml(Product product, VirtualRowLink orderRow)
		{
			int count = orderRow.Get(OrderProductType.Count);
			int price = orderRow.Get(OrderProductType.Price);

			return new CmlElement("Товар",
				new CmlText("Ид", product.Get(FabricType.Identifier)),
				new CmlText("Наименование", product.ProductName),
				new CmlText("Количество", count),
				new CmlText("Цена", price),
				new CmlText("Сумма", count * price)
			);
		}

		//public static ICmlElement OrderToCML(ShopStorage store, LightObject order, string versionNumber)
  //  {
  //    OrderCookie orderCookie = new OrderCookie(order.Get(OrderType.Products));
  //    List<ICmlElement> productElements = new List<ICmlElement>();
  //    foreach (VirtualRowLink productRow in orderCookie.AllProducts(store))
  //    {
  //      int productId = productRow.Get(OrderProductType.ProductId);
  //      Product product = store.FindProduct(productId);
  //      if (product == null)
  //        continue;

  //      productElements.Add(
  //        ProductToCML(product, "", orderCookie.GetCount(product.ProductId))
  //      );
  //    }

  //    int orderId = order.Id;
  //    DateTime utcTime = order.Get(ObjectType.ActFrom) ?? DateTime.UtcNow;

  //    int orderPrice;
  //    orderCookie.GetAllProductCount(store, out orderPrice);

  //    string family = order.Get(OrderType.Family);
  //    string firstName = order.Get(OrderType.FirstName);
  //    string patronymic = order.Get(OrderType.Patronymic);

  //    List<string> fioParts = new List<string>(3);
  //    if (!StringHlp.IsEmpty(family))
  //      fioParts.Add(family);
  //    if (!StringHlp.IsEmpty(firstName))
  //      fioParts.Add(firstName);
  //    if (!StringHlp.IsEmpty(patronymic))
  //      fioParts.Add(patronymic);

  //    string fio = string.Join(" ", fioParts);

  //    return new CmlElement("Контейнер",
  //      new CmlElement("Документ",
  //        new CmlText("Ид", orderId),
  //        new CmlText("НомерВерсии", versionNumber),
  //        new CmlText("ПометкаУдаления", "false"),
  //        new CmlText("Номер", orderId),
  //        new CmlText("Номер1С", orderId),
  //        new CmlText("Дата", utcTime.ToString(dateFormat)),
  //        new CmlText("Дата1С", utcTime.ToString(dateFormat)),
  //        new CmlText("Время", utcTime.ToString(timeFormat)),
  //        new CmlText("ХозОперация", "Заказ товара"),
  //        new CmlElement("Контрагенты",
  //          new CmlElement("Контрагент",
  //            new CmlText("Ид", ""),
  //            new CmlText("НомерВерсии", versionNumber),
  //            new CmlText("ПометкаУдаления", "false"),
  //            new CmlText("Наименование", fio),
  //            new CmlText("Полное наименование", fio),
  //            new CmlText("Роль", "Покупатель"),
  //            new CmlText("ИНН", ""),
  //            new CmlText("КПП", ""),
  //            new CmlText("КодПоОКПО", ""),
  //            new CmlElement("Контакты",
  //              Contact("Телефон рабочий", order.Get(OrderType.Phone)),
  //              Contact("Электронная почта", order.Get(OrderType.EMail))
  //            )
  //          )
  //        ),
  //        new CmlText("Валюта", "руб"),
  //        new CmlText("Курс", "1"),
  //        new CmlText("Сумма", orderPrice),
  //        new CmlText("Роль", "Продавец"),
  //        new CmlText("Комментарий", string.Format("[Номер документа на сайте: {0}]", order.Id)),
  //        new CmlElement("ЗначенияРеквизитов",
  //          Requisite("Отменен", "false"),
  //          Requisite("Проведен", "false"),
  //          Requisite("Статуса заказа ИД", "N")
  //        ),
  //        new CmlElement("Товары",
  //          productElements.ToArray()
  //        )
  //      )
  //    );
  //  }

  //  public static ICmlElement ProductToCML(Product product, string catalogId, int count)
  //  {
  //    string idIn1C = product.Get(FabricType.Identifier);
  //    int price = product.Get(FabricType.Price);

  //    return new CmlElement("Товар",
  //      new CmlText("Ид", idIn1C),
  //      new CmlText("Наименование", product.ProductName),
  //      new CmlElement("СтавкиНалогов",
  //        new CmlElement("СтавкаНалога",
  //          new CmlText("Наименование", "НДС"),
  //          new CmlText("Ставка", "0")
  //        )
  //      ),
  //      new CmlElement("ЗначенияРеквизитов",
  //        Requisite("ВидНоменклатуры", "Товар"),
  //        Requisite("ТипНоменклатуры", "Товар"),
  //        Requisite("СвойствоКорзины#CATALOG.XML_ID", catalogId),
  //        Requisite("СвойствоКорзины#PRODUCT.XML_ID", idIn1C)
  //      ),
  //      new CmlElement("Единица",
  //        new CmlText("Ид", "796"),
  //        new CmlText("НаименованиеКраткое", "шт"),
  //        new CmlText("Код", "796"),
  //        new CmlText("НаименованиеПолное", "Штука")
  //      ),
  //      new CmlText("Коэффициент", "1"),
  //      new CmlText("Количество", count.ToString()),
  //      new CmlText("Цена", price.ToString()),
  //      new CmlText("Сумма", (price * count).ToString())
  //    );
  //  }

    public static CmlElement Contact(string type, string value)
    {
      return new CmlElement("Контакт",
        new CmlText("Тип", type),
        new CmlText("Значение", value)
      );
    }

    public static CmlElement Requisite(string name, string value)
    {
      return new CmlElement("ЗначениеРеквизита",
        new CmlText("Наименование", name),
        new CmlText("Значение", value)
      );
    }

    public static void AddStartElement(StringBuilder builder, string name)
    {
      builder.Append("<");
      builder.Append(name);
      builder.Append(">");
    }

    public static void AddEndElement(StringBuilder builder, string name)
    {
      builder.Append("</");
      builder.Append(name);
      builder.Append(">");
    }

    public static void AddEmptyElement(StringBuilder builder, string name)
    {
      builder.Append("<");
      builder.Append(name);
      builder.Append("/>");
    }
  }
}
