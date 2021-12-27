using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Commune.Basis;
using Commune.Data;
using Commune.Task;
using Shop.Engine.Cml;
using NitroBolt.Wui;

namespace Shop.Engine
{
  public class Labels
  {
    public readonly static ThreadLabel Service = new ThreadLabel("Service", ThreadPriority.Lowest);
		public readonly static ThreadLabel CmlSync = new ThreadLabel("CmlSync", ThreadPriority.Lowest);
  }

  public class SiteTasks
  {
		//public static IEnumerable<Step> Parse1CFile(IContext context)
		//{
		//	while (!context.Pull.IsFinishing)
		//	{
		//	}
		//}

    public static IEnumerable<Step> CleaningSessions(IContext context, 
      TimeSpan editTimeout, TimeSpan userTimeout, TimeSpan guestTimeout)
    {
      Logger.AddMessage("CleaningSessions стартовал");

      while (!context.Pull.IsFinishing)
      {
        yield return new WaitStep(TimeSpan.FromMinutes(1));

        foreach (string key in HWebApiSynchronizeHandler.Frames.Keys)
        {
          HWebApiSynchronizeHandler.HFrame frame;
          if (!HWebApiSynchronizeHandler.Frames.TryGetValue(key, out frame))
            continue;

          HWebApiSynchronizeHandler.HUpdate update;
          if (!frame.Updates.TryGetValue(frame.Cycle, out update))
            continue;

          IState state = update?.State as IState;
          if (state == null)
            continue;

          TimeSpan waitTime = DateTime.UtcNow - state.AccessTime;

          bool editMode = state.EditMode || state.SeoMode || 
            !StringHlp.IsEmpty(state.BlockHint) || !StringHlp.IsEmpty(state.PopupHint);

          if (editMode && waitTime < editTimeout)
            continue;

          if (state.UserMode && waitTime < userTimeout)
            continue;

          if (waitTime < guestTimeout)
            continue;

          HWebApiSynchronizeHandler.Frames.TryRemove(key, out frame);
        }
      }
    }

		public static IEnumerable<Step> ImportFabricsFromCml(IContext context, bool groupFromCategory)
		{
			Logger.AddMessage("ImportFabricsFromCml стартовал: {0}", context.SiteSettings.CmlImportFolder);

			if (StringHlp.IsEmpty(context.SiteSettings.CmlImportFolder))
				yield return new FinishStep("");

			string lastTimesFilePath = Path.Combine(context.RootPath, "SyncTimes.config");
			string fabricsFilePath = Path.Combine(context.RootPath, context.SiteSettings.CmlImportFolder, "import.xml");

			while (!context.Pull.IsFinishing)
			{
				yield return new WaitStep(TimeSpan.FromMinutes(1));

				try
				{
					if (File.Exists(fabricsFilePath))
					{
						FileInfo fileInfo = new FileInfo(fabricsFilePath);
						DateTime updateTime = fileInfo.LastWriteTimeUtc;

						SyncTimes syncTimes = XmlSerialization.SafeLoad<SyncTimes>(lastTimesFilePath);

						DateTime? lastTime = syncTimes.ImportFabricsLastTime;
						if (lastTime == null || (updateTime > lastTime.Value && DateTime.UtcNow > updateTime.AddMinutes(3)))
						{
							Logger.AddMessage("Начата синхронизация товаров с 1С");

							using (FileStream stream = new FileStream(fabricsFilePath, FileMode.Open))
							{
								CmlParserHlp.ParseFabricsXml(context.FabricConnection, stream,
									context.SiteSettings.CmlImportFolder, groupFromCategory
								);
							}

							syncTimes.ImportFabricsLastTime = updateTime;
							XmlSerialization.Save(syncTimes, lastTimesFilePath);

							context.UpdateStore();

							Logger.AddMessage("Закончена синхронизация товаров с 1С");
						}
					}
				}
				catch (Exception ex)
				{
					Logger.WriteException(ex);
				}
			}
		}

		public static IEnumerable<Step> ImportOffersFromCml(IContext context)
		{
			Logger.AddMessage("ImportOffersFromCml стартовал: {0}", context.SiteSettings.CmlImportFolder);

			if (StringHlp.IsEmpty(context.SiteSettings.CmlImportFolder))
				yield return new FinishStep("");

			string lastTimesFilePath = Path.Combine(context.RootPath, "SyncTimes.config");
			string fabricsFilePath = Path.Combine(context.RootPath, context.SiteSettings.CmlImportFolder, "import.xml");
			string offersFilePath = Path.Combine(context.RootPath, context.SiteSettings.CmlImportFolder, "offers.xml");

			while (!context.Pull.IsFinishing)
			{
				yield return new WaitStep(TimeSpan.FromMinutes(1));

				try
				{
					if (File.Exists(offersFilePath) && File.Exists(fabricsFilePath))
					{
						SyncTimes syncTimes = XmlSerialization.SafeLoad<SyncTimes>(lastTimesFilePath);
						FileInfo fabricsfileInfo = new FileInfo(fabricsFilePath);

						if (fabricsfileInfo.LastWriteTimeUtc == syncTimes.ImportFabricsLastTime)
						{
							FileInfo fileInfo = new FileInfo(offersFilePath);
							DateTime updateTime = fileInfo.LastWriteTimeUtc;

							DateTime? lastTime = syncTimes.ImportOffersLastTime;
							if (lastTime == null || updateTime > lastTime.Value)
							{
								Logger.AddMessage("Начата синхронизация предложений с 1С");

								using (FileStream stream = new FileStream(offersFilePath, FileMode.Open))
								{
									CmlParserHlp.ParseOffersXml(context.FabricConnection, stream);
								}

								syncTimes.ImportOffersLastTime = updateTime;
								XmlSerialization.Save(syncTimes, lastTimesFilePath);

								context.UpdateStore();

								Logger.AddMessage("Закончена синхронизация предложений с 1С");
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.WriteException(ex);
				}
			}
		}

    public static IEnumerable<Step> UnloadingOrdersTo1C(IContext context, 
      string unloadingFolder, TimeSpan unloadingPeriod)
    {
      Logger.AddMessage("UnloadingOrdersTo1C стартовал: {0}, {1}", unloadingFolder, unloadingPeriod);

      yield return new WaitStep(TimeSpan.FromMinutes(1));

      IDataLayer orderConnection = context.OrderConnection;

      if (orderConnection.GetTable("",
        @"Select * From light_primary_key Where table_name = @tableName",
        new DbParameter("tableName", CmlWriterHlp.UnloadingOrdersCounterName)
        ).Rows.Count == 0
      )
      {
        orderConnection.GetScalar("",
          string.Format("Insert Into light_primary_key values ('{0}', {1})",
          CmlWriterHlp.UnloadingOrdersCounterName, 0));
        Logger.AddMessage("Добавлен max_primary_key для {0}", CmlWriterHlp.UnloadingOrdersCounterName);
      };

      while (!context.Pull.IsFinishing)
      {
        try
        {
          int lastOrderId = ConvertHlp.ToInt(orderConnection.GetScalar("", 
            "Select max_primary_key From light_primary_key Where table_name = @tableName",
            new DbParameter("tableName", CmlWriterHlp.UnloadingOrdersCounterName)
          )) ?? 0;


        }
        catch (Exception ex)
        {
          Logger.WriteException(ex);
        }

        yield return new WaitStep(unloadingPeriod);
      }
    }

    public static IEnumerable<Step> SitemapXmlChecker(IContext context,
      string sitemapDirectory, Getter<IEnumerable<LightLink>, LinkInfo[]> linksGetter)
    {
      string sitemapPath = Path.Combine(sitemapDirectory, "sitemap.xml");

      Logger.AddMessage("SitemapXmlChecker стартовал: {0}", sitemapPath);

      while (!context.Pull.IsFinishing)
      {
        if (File.Exists(sitemapPath))
          yield return new WaitStep(TimeSpan.FromMinutes(3));

        if (File.Exists(sitemapPath) &&
          context.Store.CreateTime - File.GetLastWriteTimeUtc(sitemapPath) < TimeSpan.FromHours(1))
        {
          FileInfo file = new FileInfo(sitemapPath);
          if (file.Length > 10)
            continue;
        }

        StringBuilder xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        string siteHost = context.SiteSettings.SiteHost;

        AddUrl(xml, siteHost, null);

        foreach (LightLink link in linksGetter(context.Store.Links.All))
        {
          AddUrl(xml, string.Format("{0}{1}", siteHost, link.Directory), link.ModifyTime);
        }

        xml.AppendLine("</urlset>");

        File.WriteAllText(sitemapPath, xml.ToString(), Encoding.UTF8);

        Logger.AddMessage("sitemap.xml успешно обновлен");
      }
    }

    public static IEnumerable<Step> SitemapXmlChecker(IContext context, string sitemapDirectory)
    {
      string sitemapPath = Path.Combine(sitemapDirectory, "sitemap.xml");

      Logger.AddMessage("SitemapXmlChecker стартовал: {0}", sitemapPath);

      while (!context.Pull.IsFinishing)
      {
        if (File.Exists(sitemapPath))
          yield return new WaitStep(TimeSpan.FromMinutes(3));

        if (File.Exists(sitemapPath) &&
          context.Store.CreateTime - File.GetLastWriteTimeUtc(sitemapPath) < TimeSpan.FromHours(1))
        {
          FileInfo file = new FileInfo(sitemapPath);
          if (file.Length > 10)
            continue;
        }

        StringBuilder xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        string siteHost = context.SiteSettings.SiteHost;

        AddUrl(xml, siteHost, null);

        foreach (LinkInfo link in context.Store.Links.All)
        {
          if (link.OmitInSitemap)
            continue;

          string directory = link.Directory;
          ////hack нет хорошего решения для прямых ссылок на разделы
          //if (Site.DirectPageLinks && directory != null && directory.StartsWith("/page"))
          //  directory = directory.Substring(5);
          AddUrl(xml, string.Format("{0}{1}", siteHost, directory), link.ModifyTime);
        }

        xml.AppendLine("</urlset>");

        File.WriteAllText(sitemapPath, xml.ToString(), Encoding.UTF8);

        Logger.AddMessage("sitemap.xml успешно обновлен");
      }
    }

    public static void AddUrl(StringBuilder xml, string url, DateTime? modifyTime)
    {
      xml.AppendLine("  <url>");
      xml.AppendFormat("    <loc>{0}</loc>{1}", url, Environment.NewLine);
      if (modifyTime != null)
        xml.AppendFormat("    <lastmod>{0}</lastmod>{1}", 
          modifyTime.Value.ToString("yyyy-MM-dd"), Environment.NewLine);
      xml.AppendLine("  </url>");
    }

  }
}
