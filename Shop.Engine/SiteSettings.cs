using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Shop.Engine
{
  [Obfuscation(ApplyToMembers = true, Exclude = true)]
  public class SiteSettings
  {
    public string AdminPassword = "admin";
    public string EditPassword = null;
    public string SeoPassword = null;
    public bool GuestEditModeEnabled = false;
    public bool DisableScripts = false;

    public string SiteHost = "";
    public string Organization = "";

    public string FullUrl(string relativeUrl)
    {
      return string.Format("{0}{1}", SiteHost, relativeUrl);
    }

		public string MySqlConnectionString;

    public string MailFrom;
    public string SmtpHost;
    public int SmtpPort;
    public string SmtpUserName;
    public string SmtpPassword;
    public string MailTo;
    public string MailForOrder;

    public string VkApplicationId;
    public string VkSecretKey;

    public int SberbankPaymentId;
    public string SberbankRegisterUrl;
    public string SberbankStatusUrl;
    public string SberbankUserName;
    public string SberbankPassword;

		public string CmlImportFolder;
		public string CmlExportFolder;

    public SiteSettings()
    {
    }
  }

	public class SyncTimes
	{
		public DateTime? ImportFabricsLastTime;
		public DateTime? ImportOffersLastTime;
		//public DateTime? ExportOrdersLastTime;
	}
}
