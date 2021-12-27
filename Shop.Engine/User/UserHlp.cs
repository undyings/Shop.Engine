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
using System.Security.Cryptography;

namespace Shop.Engine
{
  public class UserHlp
  {
    static IDataLayer userConnection
    {
      get
      {
        return SiteContext.Default.UserConnection;
      }
    }

    public static string CalcConfirmationCode(int userId, string login, string salt)
    {
      List<byte> bytes = new List<byte>();
      bytes.AddRange(BitConverter.GetBytes(userId));

      foreach (char ch in salt)
        bytes.AddRange(BitConverter.GetBytes(ch));

      foreach (char ch in login)
        bytes.AddRange(BitConverter.GetBytes(ch));

      return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(bytes.ToArray())).Replace("-", "");
    }

    public static string GetFirstLetter(LightObject user)
    {
      string firstName = user.Get(UserType.FirstName);
      if (!StringHlp.IsEmpty(firstName))
        return firstName.Substring(0, 1);

      string family = user.Get(UserType.Family);
      if (!StringHlp.IsEmpty(family))
        return family.Substring(0, 1);

      return "";
    }

    public static LightObject GetCurrentUser(HttpContext httpContext, UserStorage userStorage)
    {
      string xmlLogin = httpContext.UserName();

      if (httpContext.IsInRole("service"))
        return null;

      if (StringHlp.IsEmpty(xmlLogin))
        return null;

      LightObject user = userStorage.FindUser(xmlLogin);
      if (user == null)
        httpContext.Logout();

      return user;
    }

    public static string UserToString(LightObject user)
    {
      return string.Format("{0} {1}", user.Get(UserType.FirstName), user.Get(UserType.Family)).TrimStart();
    }

    public static LightObject CheckOrCreateVkUser(UserStorage userStorage, JsonData userInfo)
    {
      string uid = userInfo.JPath("response", "uid")?.ToString();

      if (StringHlp.IsEmpty(uid))
        return null;

      string xmlLogin = UserType.Login.CreateXmlIds("vk", uid);

      string firstName = userInfo.JPath("response", "first_name")?.ToString();
      string lastName = userInfo.JPath("response", "last_name")?.ToString();

      LightObject user = DataBox.LoadOrCreateObject(userConnection, UserType.User, xmlLogin);

      if (user.Get(UserType.Family) != lastName)
        user.Set(UserType.Family, lastName);

      if (user.Get(UserType.FirstName) != firstName)
        user.Set(UserType.FirstName, firstName);

      Logger.AddMessage("VkUserUpdate: {0}", user.Box.DataChangeTick);

      if (user.Box.DataChangeTick != 0)
      {
        user.Set(ObjectType.ActTill, DateTime.UtcNow);
        user.Box.Update();
        userStorage.Update();
      }

      return user;
    }

    public static string VkAuthorizeUrl(string applicationId, string redirectUrl)
    {
      return string.Format(
        @"http://oauth.vk.com/authorize?client_id={0}&redirect_uri={1}&response_type=code",
          applicationId, redirectUrl
      );
    }

    public static JsonData VkUserInfo(string applicationId, string secretKey, string code,
      string redirectUri, params string[] fields)
    {
      using (WebClient webClient = new WebClient())
      {
        webClient.QueryString.Add("client_id", applicationId);
        webClient.QueryString.Add("client_secret", secretKey);
        webClient.QueryString.Add("code", code);
        webClient.QueryString.Add("redirect_uri", redirectUri);

        string answer = webClient.DownloadString("https://oauth.vk.com/access_token");

        JsonSerializer jsonSerializer = JsonSerializer.Create();
        JsonData json = new JsonData(jsonSerializer.Deserialize(
          new JsonTextReader(new StringReader(answer))
        ));
        object accessToken = json.JPath("access_token");
        object userId = json.JPath("user_id");

        //Logger.AddMessage("VkAnswer: {0}, {1}, {2}, {3}", code, answer, accessToken, userId);

        webClient.QueryString.Clear();
        webClient.QueryString.Add("uids", userId.ToString());
        webClient.QueryString.Add("fields", string.Join(",", fields));
        webClient.QueryString.Add("access_token", accessToken.ToString());

        byte[] bytes = webClient.DownloadData("https://api.vk.com/method/users.get");

        string userString = Encoding.UTF8.GetString(bytes).Replace("[", "").Replace("]", "");
        return new JsonData(jsonSerializer.Deserialize(
          new JsonTextReader(new StringReader(userString))
        ));
      }
    }

    public static LightObject LoadUser(string auth, string login)
    {
      return DataBox.LoadObject(userConnection, UserType.User, UserType.Login.CreateXmlIds(auth, login));
    }

    public static bool SiteAuthorization(SiteSettings settings, 
      HttpContext httpContext, string login, string password)
    {
      if (login == "admin")
      {
        string adminPassword = settings.AdminPassword;
        if (!StringHlp.IsEmpty(adminPassword) && password == adminPassword)
        {
          httpContext.SetUserAndCookie("admin", "service", "edit", "seo");
          return true;
        }
        return false;
      }

      if (login == "edit")
      {
        string editPassword = settings.EditPassword;
        if (!StringHlp.IsEmpty(editPassword) && password == editPassword)
        {
          httpContext.SetUserAndCookie("admin", "service", "edit");
          return true;
        }

        if (settings.GuestEditModeEnabled)
        {
          httpContext.SetUserAndCookie(login, "edit", "service", "nosave");
          return true;
        }
        return false;
      }

      if (login == "seo")
      {
        string seoPassword = settings.SeoPassword;
        if (!StringHlp.IsEmpty(seoPassword) && password == seoPassword)
        {
          httpContext.SetUserAndCookie("admin", "service", "seo");
          return true;
        }

        if (settings.GuestEditModeEnabled)
        {
          httpContext.SetUserAndCookie(login, "seo", "service", "nosave");
          return true;
        }
        return false;
      }

      return false;
    }

    public static bool DirectAuthorization(HttpContext httpContext, SiteSettings settings)
    {
      string auth = httpContext.Get("auth");
      if (StringHlp.IsEmpty(auth))
        return false;

      if (auth == "logout")
      {
        httpContext.Logout();
        return false;
      }

      return SiteAuthorization(settings, httpContext, auth, httpContext.Get("psw"));

    //  if (auth == "admin")
    //  {
    //    string adminPassword = settings.AdminPassword;
    //    if (!StringHlp.IsEmpty(adminPassword) && httpContext.Get("psw") == adminPassword)
    //    {
    //      httpContext.SetUserAndCookie("admin", "service", "edit", "seo");
    //      return true;
    //    }
    //    return false;
    //  }

    //  if (auth == "edit")
    //  {
    //    string editPassword = settings.EditPassword;
    //    if (!StringHlp.IsEmpty(editPassword) && httpContext.Get("psw") == editPassword)
    //    {
    //      httpContext.SetUserAndCookie("admin", "service", "edit");
    //      return true;
    //    }

    //    if (settings.GuestEditModeEnabled)
    //    {
    //      httpContext.SetUserAndCookie(auth, "edit", "service", "nosave");
    //      return true;
    //    }
    //    return false;
    //  }

    //  if (auth == "seo")
    //  {
    //    string seoPassword = settings.SeoPassword;
    //    if (!StringHlp.IsEmpty(seoPassword) && httpContext.Get("psw") == seoPassword)
    //    {
    //      httpContext.SetUserAndCookie("admin", "service", "seo");
    //      return true;
    //    }

    //    if (settings.GuestEditModeEnabled)
    //    {
    //      httpContext.SetUserAndCookie(auth, "seo", "service", "nosave");
    //      return true;
    //    }
    //    return false;
    //  }

    //  //if (settings.GuestEditModeEnabled && (auth == "edit" || auth == "seo"))
    //  //{
    //  //  httpContext.SetUserAndCookie(auth, auth, "service", "nosave");
    //  //  return true;
    //  //}

    //  return false;
    }
  }
}
