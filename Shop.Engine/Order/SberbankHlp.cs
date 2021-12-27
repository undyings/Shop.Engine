using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Commune.Basis;
using Commune.Html;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Shop.Engine
{
  public class SberbankHlp
  {
    public static string OrderStatusToDisplay(int? orderStatus)
    {
      if (orderStatus == null)
        return "";

      switch (orderStatus)
      {
        case 0:
          return "Заказ зарегистрирован, но не оплачен";
        case 1:
          return "Предавторизованная сумма захолдирована";
        case 2:
          return "Проведена полная авторизация суммы заказа";
        case 3:
          return "Авторизация отменена";
        case 4:
          return "По транзакции была проведена операция возврата";
        case 5:
          return "Инициирована авторизация через ACS банка-эмитента";
        case 6:
          return "Авторизация отклонена";
        default:
          return string.Format("Код {0}", orderStatus);
      }
    }

    public static string ErrorCodeToDisplay(int? errorCode)
    {
      switch (errorCode ?? 0)
      {
        case 0:
          return "Обработка запроса прошла без системных ошибок";
        case 1:
          return "Ожидается [orderId] или [orderNumber]";
        case 2:
          return "Заказ отклонен по причине ошибки в реквизитах платежа";
        case 6:
          return "Незарегистрированный OrderId";
        case 7:
          return "Системная ошибка";
        default:
          return string.Format("Код {0}", errorCode);
      }
    }

    public static void GetOrderStatus(string statusUrl,
      string userName, string password, string orderId,
      out int? orderNumber, out int? orderStatus, out int? amount,
      out int? errorCode, out string errorMessage)
    {
      using (WebClient webClient = new WebClient())
      {
        webClient.QueryString.Add("userName", userName);
        webClient.QueryString.Add("password", password);
        webClient.QueryString.Add("orderId", orderId);

        byte[] answerBytes = webClient.DownloadData(statusUrl);
        string answer = Encoding.UTF8.GetString(answerBytes);

        Logger.AddMessage("OrderStatus.Answer: {0}", answer);

        JsonSerializer jsonSerializer = JsonSerializer.Create();
        JsonData json = new JsonData(jsonSerializer.Deserialize(
          new JsonTextReader(new StringReader(answer))
        ));

        orderNumber = ConvertHlp.ToInt(json.JPath("OrderNumber"));
        orderStatus = ConvertHlp.ToInt(json.JPath("OrderStatus"));
        amount = ConvertHlp.ToInt(json.JPath("Amount"));
        errorCode = ConvertHlp.ToInt(json.JPath("ErrorCode"));
        errorMessage = json.JPath("ErrorMessage")?.ToString();
      }
    }

    public static void RegisterOrder(
      string registerUrl, string userName, string password, 
      int orderNumber, int amount, int sessionTimeoutSecs, string returnUrl,
      out string orderId, out string formUrl, out string errorMessage)
    {
      using (WebClient webClient = new WebClient())
      {
        webClient.QueryString.Add("userName", userName);
        webClient.QueryString.Add("password", password);
        webClient.QueryString.Add("orderNumber", orderNumber.ToString());
        webClient.QueryString.Add("amount", amount.ToString());
				webClient.QueryString.Add("sessionTimeoutSecs", sessionTimeoutSecs.ToString());
				webClient.QueryString.Add("returnUrl", returnUrl);

        Logger.AddMessage("RegisterUrl: {0}", registerUrl);

        string answer = webClient.DownloadString(registerUrl);

        Logger.AddMessage("RegisterOrder.Answer: {0}", answer);

        JsonSerializer jsonSerializer = JsonSerializer.Create();
        JsonData json = new JsonData(jsonSerializer.Deserialize(
          new JsonTextReader(new StringReader(answer))
        ));

        orderId = json.JPath("orderId")?.ToString();
        formUrl = json.JPath("formUrl")?.ToString();
        object errorCode = json.JPath("errorCode");
        errorMessage = json.JPath("errorMessage")?.ToString();

        Logger.AddMessage("Register.Answer: {0}, {1}, {2}, {3}", orderId, formUrl, errorCode, errorMessage);
      }
    }
  }
}
