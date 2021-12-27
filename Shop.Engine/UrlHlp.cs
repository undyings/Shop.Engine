using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Commune.Basis;
using Commune.Data;
using Commune.Html;

namespace Shop.Engine
{
  public class UrlHlp
  {
    public const string defaultPlaceholderUrl = @"/Images/placeholder.png?v=1";

    static string imagesPath
    {
      get
      {
        return SiteContext.Default.ImagesPath;
      }
    }

    public static string FormatPhone(string phone)
    {
      StringBuilder builder = new StringBuilder();
      foreach (char ch in (phone ?? ""))
      {
        if (ch == ' ' || ch == '(' || ch == ')' || ch == '-')
          continue;

        builder.Append(ch);
      }
      return builder.ToString();
    }

    public static string PhoneAsTelLink(string formatPhone)
    {
      return string.Format("tel:{0}", formatPhone);
    }

    public static string PhoneAsViberLink(string formatPhone)
    {
      return string.Format("viber://add?number={0}", formatPhone);
    }

    public static string PhoneAsWhatsAppLink(string formatPhone)
    {
      return string.Format("whatsapp://send?phone={0}", formatPhone);
    }

    public static string DateToString(DateTime? date)
    {
      return date?.ToLocalTime().ToString("d MMMM yyyy");
    }

    public static string TimeToString(DateTime? date)
    {
      return date?.ToLocalTime().ToString("dd MMM HH:mm");
    }

    public static string GetFIO(LightObject order)
    {
      StringBuilder fio = new StringBuilder();
      fio.Append(order.Get(OrderType.Family));
      string firstName = order.Get(OrderType.FirstName);
      if (!StringHlp.IsEmpty(firstName))
      {
        fio.Append(string.Format(" {0}.", firstName.Substring(0, 1)));
        string patronymic = order.Get(OrderType.Patronymic);
        if (!StringHlp.IsEmpty(patronymic))
          fio.Append(string.Format(" {0}.", patronymic.Substring(0, 1)));
      }
      return fio.ToString();
    }

		public static string GetFullName(LightObject order)
		{
			StringBuilder fullName = new StringBuilder();
			fullName.Append(order.Get(OrderType.Family));
			string firstName = order.Get(OrderType.FirstName);
			if (!StringHlp.IsEmpty(firstName))
			{
				fullName.Append(string.Format(" {0}", firstName));
				string patronymic = order.Get(OrderType.Patronymic);
				if (!StringHlp.IsEmpty(patronymic))
					fullName.Append(string.Format(" {0}", patronymic));
			}
			return fullName.ToString();
		}

    readonly static long refTicks = new DateTime(2016, 1, 1).Ticks;
    public const long ticksInSecond = 10000000;

    public static string FileUrl(string subPaths)
    {
      string filePath = Path.Combine(SiteContext.Default.RootPath, subPaths.Replace('/', '\\').TrimStart('\\'));
      FileInfo imageInfo = new FileInfo(filePath);

      return string.Format(@"{0}?v={1}",
        string.Join("/", subPaths), Math.Max(0, imageInfo.LastWriteTimeUtc.Ticks - refTicks) / ticksInSecond);
    }

    public static string ImageUrl(string imageName)
    {
      return ImageEditUrl(imageName);
    }

    public static string ImageUrl(int objectId, bool isOriginal)
    {
      return ImageUrl(objectId, isOriginal ? "original.jpg" : "thumb.png");
    }

		public static string ImageUrl(int objectId, bool isOriginal, string placeholderUrl)
		{
			return ImageUrlOrPlaceholder(placeholderUrl, objectId.ToString(), isOriginal ? "original.jpg" : "thumb.png");
		}

    public static string ImageUrl(int objectId, string imageName)
    {
      return ImageEditUrl(objectId.ToString(), imageName);
    }

    public static string ImagePath(params string[] imageSubPaths)
    {
			imageSubPaths = imageSubPaths.Where(sp => !StringHlp.IsEmpty(sp)).ToArray();
      return Path.Combine(imagesPath, Path.Combine(imageSubPaths));
    }

		public static string ImageEditUrl(params string[] imageSubPaths)
		{
			return ImageUrlOrPlaceholder(defaultPlaceholderUrl, imageSubPaths);
		}

		static string ImageUrlOrPlaceholder(string placeholderUrl, params string[] imageSubPaths)
    {
			imageSubPaths = imageSubPaths.Where(sp => !StringHlp.IsEmpty(sp)).ToArray();
			FileInfo imageInfo = new FileInfo(ImagePath(imageSubPaths));
      if (!imageInfo.Exists)
        return placeholderUrl;

      return string.Format(@"/images/{0}?v={1}",
        string.Join("/", imageSubPaths), Math.Max(0, imageInfo.LastWriteTimeUtc.Ticks - refTicks) / ticksInSecond);
    }

    public static string GetUrlArg(int? parentId, string kind, int? id)
    {
      List<string> args = new List<string>();
      if (parentId != null)
        args.Add(string.Format("parent={0}", parentId.Value));
      if (!StringHlp.IsEmpty(kind))
        args.Add(string.Format("kind={0}", kind));
      if (id != null)
        args.Add(string.Format("id={0}", id.Value));

      return string.Join("&", args);
    }

    public static string EditUrl(LightHead parent, string kind, int? id)
    {
      return EditUrl(parent != null ? parent.Id : (int?)null, kind, id);
    }

    public static string EditUrl(int? parentId, string kind, int? id)
    {
      return string.Format("/editor?{0}", GetUrlArg(parentId, kind, id));
    }

    public static string EditUrl(string kind, int? id)
    {
      return EditUrl((int?)null, kind, id);
    }

    public static string SeoUrl(string kind, int? id)
    {
      return string.Format("/seo?{0}", GetUrlArg(null, kind, id));
    }

    //public static string PageUrl(int id)
    //{
    //  string directory = SiteContext.Default.Store.Links.ToDirectory(id, null);
    //  if (Site.DirectPageLinks && directory != null && directory.StartsWith("/page"))
    //    return directory.Substring(5);
    //  return directory;
    //}

    public static string ShopUrl(string kind, int? id = null, int? parentId = null)
    {
      if (id == null)
        return string.Format("/{0}", kind);

      string directory = SiteContext.Default.Store.Links.ToDirectory(id.Value, parentId);
      if (directory != null)
        return string.Format("{0}", directory);

      return string.Format("/{0}/{1}", kind, id);
    }

    public static string ToDirectory(string kind, string translitId)
    {
      StringBuilder directory = new StringBuilder();

      if (!StringHlp.IsEmpty(kind))
      {
        directory.Append("/");
        directory.Append(kind.ToLower());
      }

      if (!StringHlp.IsEmpty(translitId))
      {
        if (!translitId.StartsWith("/"))
          directory.Append("/");
        directory.Append(translitId.ToLower());
      }

      return directory.ToString();
    }

    public static string ReturnUnitUrl(LightSection parent, int unitId)
    {
      string returnUrl = (parent != null && parent.IsMenu) ? "/" : UrlHlp.ShopUrl("page", parent?.Id);
      return string.Format("{0}#unit{1}", returnUrl, unitId);
    }

    public static string ReturnParentUrl(SectionStorage sectionStorage, int sectionId)
    {
      LightSection[] branch = SectionHlp.GetSectionBranch(sectionStorage, sectionId);
      if (branch.Length <= 2)
        return "/";

      return UrlHlp.ShopUrl("page", branch[branch.Length - 2].Id);
    }

    public static FileInfo[] GalleryImageInfos(int objectId)
    {
      string directory = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
        objectId.ToString(), "gallery");

      DirectoryInfo directoryInfo = new DirectoryInfo(directory);
      FileInfo[] files = directoryInfo.GetFiles();

      ArrayHlp.Sort(files, delegate (FileInfo file)
        {
          return new object[] { -file.LastWriteTimeUtc.Ticks, file.Name};
        },
        _.ArrayComparison
      );

      return files;
    }

    public static string[] GalleryImageNames(int objectId)
    {
      FileInfo[] infos = GalleryImageInfos(objectId);
      return ArrayHlp.Convert(infos, delegate (FileInfo info)
        {
          return info.Name;
        }
      );
    }

    //public static string[] GalleryImageNames(int objectId)
    //{
    //  string directory = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
    //    objectId.ToString(), "gallery");

    //  string[] imagePathes = Directory.GetFiles(directory);
    //  return ArrayHlp.Convert(imagePathes, delegate (string path)
    //    { return Path.GetFileName(path); }
    //  );
    //}

    public static string GalleryImageUrl(int objectId, string imageName)
    {
      //FileInfo imageInfo = new FileInfo(GalleryImagePath(objectId, imageName));
      //if (!imageInfo.Exists)
      //  return placeholderUrl;

      return ImageEditUrl(objectId.ToString(), "gallery", imageName);

      //return string.Format("/images/{0}/gallery/{1}", objectId, imageName);
    }

    public static string GalleryThumbUrl(int objectId, string imageName)
    {
      string thumbName = Path.ChangeExtension(imageName, "png");
      return ImageEditUrl(objectId.ToString(), "thumb", thumbName);
      //return string.Format("/images/{0}/thumb/{1}", objectId, thumbName);
    }

    public static string GalleryImagePath(int objectId, string imageName)
    {
      return Path.Combine(SiteContext.Default.ImagesPath, objectId.ToString(), "gallery", imageName);
    }

    public static string GalleryThumbPath(int objectId, string imageName)
    {
      string thumbName = Path.ChangeExtension(imageName, "png");
      return Path.Combine(SiteContext.Default.ImagesPath, objectId.ToString(), "thumb", thumbName);
    }

		public static LightGroup[] GetGroupBranch(LightGroup group)
		{
			List<LightGroup> branch = new List<LightGroup>();
			LightGroup refGroup = group;
			while (refGroup != null)
			{
				branch.Insert(0, refGroup);
				refGroup = refGroup.ParentGroup;
			}
			return branch.ToArray();
		}

		public static string ShopDirectory(LightGroup refGroup)
		{
			LightGroup[] branch = GetGroupBranch(refGroup);

			StringBuilder builder = new StringBuilder();
			builder.Append("/catalog");
			foreach (LightGroup group in branch)
			{
				builder.Append("/");
				builder.Append(TranslitLinks.TranslitString(GroupType.DisplayName(group)));
			}

			return builder.ToString();
		}
	}
}
