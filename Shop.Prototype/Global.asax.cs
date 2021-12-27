using Commune.Basis;
using Commune.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using System.Web.SessionState;
using System.Collections.Specialized;
using System.Globalization;
using Commune.Html;
using Commune.Task;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Shop.Engine
{
  public class WebApiApplication : System.Web.HttpApplication
  {
    const string connectionStringFormat = "Data Source={0};Pooling=true;FailIfMissing=false;UseUTF16Encoding=True;";
    protected void Application_Start(object sender, EventArgs e)
    {
      string appPath = HttpContext.Current.Server.MapPath("");
      string logFolder = ApplicationHlp.CheckAndCreateFolderPath(appPath, "Logs");

      try
      {
        //string appPath = HttpContext.Current.Server.MapPath("");
        //string logFolder = ApplicationHlp.CheckAndCreateFolderPath(appPath, "Logs");

        Logger.EnableLogging(Path.Combine(logFolder, "shop.log"), 2);

        GlobalConfiguration.Configure(WebApiConfig.Register);

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        ServicePointManager.ServerCertificateValidationCallback +=
             delegate (object snd, X509Certificate certificate, X509Chain chain,
                           SslPolicyErrors sslPolicyErrors)
             {
               return true;
             };

        string databaseFolder = ApplicationHlp.CheckAndCreateFolderPath(appPath, "Data");

        IDataLayer userConnection = new SQLiteDataLayer(string.Format(
          connectionStringFormat, Path.Combine(databaseFolder, "user.db3")));

        IDataLayer fabricConnection = new SQLiteDataLayer(string.Format(
          connectionStringFormat, Path.Combine(databaseFolder, "fabric.db3")));

        IDataLayer orderConnection = new SQLiteDataLayer(string.Format(
          connectionStringFormat, Path.Combine(databaseFolder, "order.db3")));

        Logger.AddMessage("Подключения к базам данных успешно созданы");

        SQLiteDatabaseHlp.CheckAndCreateDataBoxTables(userConnection);
        SQLiteDatabaseHlp.CheckAndCreateDataBoxTables(fabricConnection);
        SQLiteDatabaseHlp.CheckAndCreateDataBoxTables(orderConnection);

        MetaHlp.ReserveDiapasonForMetaProperty(fabricConnection);

        FabricHlp.CheckAndCreateMenu(fabricConnection, "top", "bottom");

        SiteContext.Default = new ShopContext(appPath,
					new EditorSelector(), new EditorSelector(),
					new EditorSelector(), new EditorSelector(),
					userConnection, fabricConnection, orderConnection,
          new ContextTunes().News(5).Landings().Users(), new SearchTunes(),
					"novosti", "register", "login", "passwordreset"
				);
      }
      catch (Exception ex)
      {
        Logger.WriteException(ex, "Ошибка создания подключения к базе данных:");
      }
    }

    protected void Session_Start(object sender, EventArgs e)
    {

    }

    static bool IsClientCached(HttpRequest request, DateTime contentModified)
    {
      string header = request.Headers["If-Modified-Since"];

      if (header != null)
      {
        DateTime isModifiedSince;
        if (DateTime.TryParse(header, out isModifiedSince))
        {
          return isModifiedSince > contentModified;
        }
      }

      return false;
    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
      string path = (this.Context.Request.Path ?? "").ToLower();
      //Logger.AddMessage("originalPath: {0}", path);

      NameValueCollection query = Context.Request.QueryString;
      string originalArgs = StringHlp.Join("&", query.AllKeys, delegate (string key)
      {
        return string.Format("{0}={1}", key, query[key]);
      });

      LightObject redirect = SiteContext.Default.Store.Redirects.Find(path);
      if (redirect != null)
      {
        Context.Response.Status = "301 Moved Permanently";
        Context.Response.StatusCode = 301;
        Context.Response.AddHeader("Location", redirect.Get(RedirectType.To));
        return;
      }
      //return;

      //bool isIndexJs = path.Contains("index.html.js");

      //if (path.Contains(".") && !isIndexJs)
      //  return;

      //string[] dirs = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
      //if (dirs.Length == 0 || (isIndexJs && dirs.Length == 1))
      //  return;

      //string directory = !isIndexJs ? path : path.Substring(0, path.Length - 14);

      //LinkInfo link = ShopContext.Default.Store.Links.FindLink(directory);
      //if (link == null)
      //{
      //  Response.StatusCode = 404;
      //  return;
      //}

      //DateTime modifyTime = link.ModifyTime ?? ShopContext.Default.Store.CreateTime;

      //if (!isIndexJs && IsClientCached(Request, modifyTime))
      //{
      //  Response.StatusCode = 304;
      //  Response.SuppressContent = true;
      //  return;
      //}
      //else
      //{
      //  Context.Response.AddHeader("Last-Modified",
      //    modifyTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
      //  //Response.Cache.SetLastModified(modifyTime);
      //}

      //string kind = dirs[0];
      //int? id = ShopContext.Default.Store.Links.FromDirectory(directory);
      //if (dirs.Length > 1)
      //{
      //  id = ShopContext.Default.Store.Links.FromDirectory(kind, dirs[1]);
      //  if (id == null)
      //    id = ConvertHlp.ToInt(dirs[1]);

      //  if (id == null)
      //  {
      //    Context.Response.StatusCode = 404;
      //    return;
      //  }
      //}


      //if (id == null)
      //{
      //  Context.Response.StatusCode = 404;
      //  return;
      //}

      //string argPath = string.Format("/{0}?{1}", 
      //  isIndexJs ? "index.html.js" : "", UrlHlp.GetUrlArg(null, link.Kind, link.Id));

      //if (!StringHlp.IsEmpty(originalArgs))
      //  argPath = string.Format("{0}&{1}", argPath, originalArgs);

      //this.Context.RewritePath(argPath);

      //Logger.AddMessage("Path: {0}, {1}", this.Context.Request.Path, this.Context.Request.RawUrl);

    }

    //protected void Application_EndRequest(object sender, EventArgs e)
    //{
    //  try
    //  {
    //    if (Response.StatusCode != 200)
    //      return;

    //    string rawUrl = (Request.RawUrl ?? "").ToLower();
    //    if (rawUrl.Contains("."))
    //      return;

    //    int indexOf = rawUrl.IndexOf("?");
    //    if (indexOf >= 0)
    //      rawUrl = rawUrl.Substring(0, indexOf);

    //    LinkInfo link = ShopContext.Default.Store.Links.FindLink(rawUrl);
    //    if (link == null)
    //      return;

    //    if (link.ModifyTime == null)
    //      return;

    //    //Logger.AddMessage("Last-Modified: {0}, {1}", rawUrl, link.ModifyTime);

    //    Response.Headers.Set("Last-Modified",
    //      link.ModifyTime.Value.ToString("ddd, dd MMM yyyy hh:mm:ss", CultureInfo.InvariantCulture) + " GMT"
    //    );
    //  }
    //  catch (Exception ex)
    //  {
    //    Logger.WriteException(ex);
    //  }

    //  //Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
    //  //Context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(0));
    //  //Context.Response.Cache.SetMaxAge(new TimeSpan(0));
    //  //Context.Response.AddHeader("Last-Modified", DateTime.Now.ToLongDateString());
    //}

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {
      AuthHlp.SetUserFromCookie(HttpContext.Current);
    }

    protected void Application_Error(object sender, EventArgs e)
    {

    }

    protected void Session_End(object sender, EventArgs e)
    {

    }

    protected void Application_End(object sender, EventArgs e)
    {
      Logger.AddMessage("Application.End");
      SiteContext.Default.Pull.Finish();
    }
  }
}