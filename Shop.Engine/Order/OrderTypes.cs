using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class OrderType
  {
    public const int Order = 1000;

    public readonly static XmlLogin Auth = XmlLogin.Auth;
    public readonly static XmlLogin Login = XmlLogin.Login;

    public readonly static RowPropertyBlank<string> Family = DataBox.Create(1101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> FirstName = DataBox.Create(1102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Patronymic = DataBox.Create(1103, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> EMail = DataBox.Create(1104, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Phone = DataBox.Create(1105, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Address = DataBox.Create(1106, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Comment = DataBox.Create(1107, DataBox.StringValue);

    public readonly static RowPropertyBlank<int> PaymentKind = DataBox.Create(1108, DataBox.IntValue);
    public readonly static RowPropertyBlank<int> DeliveryKind = DataBox.Create(1109, DataBox.IntValue);
    public readonly static RowPropertyBlank<int> DeliveryCost = DataBox.Create(1111, DataBox.IntValue);
    public readonly static RowPropertyBlank<int> Commission = DataBox.Create(1112, DataBox.IntValue);

    public readonly static RowPropertyBlank<string> Products = DataBox.Create(1110, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> DocumentNumber = DataBox.Create(1120, DataBox.StringValue);

  }

  public class XmlLogin : XmlUniqueProperty<string>
  {
    readonly static XmlFieldBlank xmlIds = new XmlFieldBlank(
      ObjectType.XmlObjectIds, "Auth", "Login");

    public readonly static XmlLogin Auth = new XmlLogin("Auth", null);
    public readonly static XmlLogin Login = new XmlLogin("Login", null);

    public XmlLogin(string propertyKind, Getter<string, string> propertyConverter) :
      base(xmlIds, propertyKind, propertyConverter)
    {
    }

    public string CreateXmlIds(string auth, string login)
    {
      return xmlField.Create(auth, login);
    }
  }

  // Хранится в виде Json в OrderCookie
  public class OrderProductType
  {
    public readonly static FieldBlank<string> ProductKey = new FieldBlank<string>("productKey");
    public readonly static FieldBlank<int> ProductId = new FieldBlank<int>("productId", IntStringConverter.Default);
    //public readonly static FieldBlank<string> Option = new FieldBlank<string>("option");
    public readonly static FieldBlank<int> Price = new FieldBlank<int>("price", IntStringConverter.Default);
    public readonly static FieldBlank<int> Count = new FieldBlank<int>("count", IntStringConverter.Default);
  }
}
