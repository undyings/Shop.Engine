using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Drawing;
using Commune.Basis;
using NitroBolt.Wui;
using Commune.Html;
using Commune.Data;
using System.Net.Http;
using System.Text;
using System.Drawing.Imaging;

namespace Shop.Engine
{
  public class HttpLoader
  {
    static readonly HBuilder h = null;

    public static HttpResponseMessage Gis()
    {
      LightObject contact = SiteContext.Default.Store.Sections.ContactPage;

      HElement htmlElement = h.Html
      (
        h.Head(
          h.title("")
        ),
        h.Body(
          h.Raw(contact?.Get(SectionType.Widget))
        )
       );

      return new HttpResponseMessage()
      {
        Content = new StringContent(htmlElement.ToHtmlText(), Encoding.UTF8, "text/html")
      };
    }

    public static HttpResponseMessage Gis2()
    {
      LightObject contact = SiteContext.Default.Store.Contacts;

      HElement htmlElement = h.Html
      (
        h.Head(
					h.Meta("robots", "noindex,nofollow")
				),
        h.Body(
          h.Raw(contact?.Get(ContactsType.MapWidget))
        )
       );

      return new HttpResponseMessage()
      {
        Content = new StringContent(htmlElement.ToHtmlText(), Encoding.UTF8, "text/html")
      };
    }

    public static HttpResponseMessage AvatarUploader(string folder, string avatarName, int avatarSize)
    {
      HttpContext httpContext = HttpContext.Current;

      int? objectId = httpContext.GetUInt("objectId"); // ?? state.CreatingObjectId;

      if (objectId == null)
        return null;

      LightObject currentUser = UserHlp.GetCurrentUser(httpContext, SiteContext.Default.UserStorage);
      if (currentUser == null || currentUser.Id != objectId)
        return null;

      string path = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
        folder, objectId.ToString());

      if (httpContext.Request.Files.Count == 0)
        return null;

      HttpPostedFile file = httpContext.Request.Files[0];

      using (Bitmap originalImage = (Bitmap)Bitmap.FromStream(file.InputStream))
      using (Bitmap thumbImage = FabricHlp.GetThumbnailImage(originalImage, avatarSize))
      {
        thumbImage.Save(Path.Combine(path, string.Format("{0}.png", avatarName)), ImageFormat.Png);
      }

      return new HttpResponseMessage()
      {
        Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "text/json")
      };
    }

    public static HttpResponseMessage TileUploader()
    {
      HttpContext httpContext = HttpContext.Current;

      return TileUploader("",
				httpContext.Get("imageName"),
        httpContext.GetUInt("tileWidth") ?? EditElementHlp.thumbWidth,
        httpContext.GetUInt("tileHeight") ?? EditElementHlp.thumbWidth,
        httpContext.Get("fullFillingTile") == "true",
        httpContext.Get("jpegTile") == "true"
      );
    }

    [Obsolete()]
    public static HttpResponseMessage TileUploader(int thumbSize)
    {
      return TileUploader("", null, thumbSize, thumbSize, false, false);
    }

    //public static HttpResponseMessage TileUploader(int thumbSize, bool fullFillingThumb, bool jpegThumb)
    //{
    //  return TileUploader("", thumbSize, thumbSize, fullFillingThumb, jpegThumb);
    //}

    public static HttpResponseMessage TileUploader(string folder, string imageName,
      int thumbWidth, int thumbHeight, bool fullFillingThumb, bool jpegThumb)
    {
      try
      {
        HttpContext httpContext = HttpContext.Current;

        int? objectId = httpContext.GetUInt("objectId"); // ?? state.CreatingObjectId;
        if (objectId == null)
          return null;

				string subfolder = httpContext.Get("subfolder");

				string path = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
					folder, objectId.ToString(), subfolder);

				Logger.AddMessage("TileUploader: {0}, {1}", subfolder, path);


        if (httpContext.Request.Files.Count == 0)
          return null;

        HttpPostedFile file = httpContext.Request.Files[0];

				using (Bitmap originalImage = (Bitmap)Bitmap.FromStream(file.InputStream))
				{
					if (!StringHlp.IsEmpty(imageName))
					{
						file.SaveAs(Path.Combine(path, imageName));
					}
					else
					{
						using (Bitmap thumbImage = FabricHlp.GetThumbnailImage(originalImage,
							thumbWidth, thumbHeight, fullFillingThumb, !jpegThumb))
						{
							ImageFormat imageFormat = jpegThumb ? ImageFormat.Jpeg : ImageFormat.Png;
							thumbImage.Save(Path.Combine(path, "thumb.png"), imageFormat);
							file.SaveAs(Path.Combine(path, "original.jpg"));
						}
					}
				}

        return new HttpResponseMessage()
        {
          Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "text/json")
        };
      }
      catch (Exception ex)
      {
        Logger.WriteException(ex);
        throw;
      }
    }

    static readonly object lockObj = new object();
    //static DateTime lastAlbumTime = DateTime.UtcNow;
    //static DateTime lastGalleryUploadTime = DateTime.UtcNow;

    //static void SetImageModifyTime(string imagePath)
    //{
    //  lock (lockObj)
    //  {
    //    try
    //    {
    //      DateTime modifyTime = File.GetLastWriteTimeUtc(imagePath);
    //      //Logger.AddMessage("SetImage: {0}, {1}, {2}", imagePath, modifyTime, lastGalleryUploadTime);

    //      if (modifyTime - lastGalleryUploadTime > TimeSpan.FromSeconds(300))
    //      {
    //        lastAlbumTime = modifyTime;
    //        return;
    //      }
    //      //Logger.AddMessage("SetLast: {0}", lastAlbumTime.Ticks);

    //      File.SetLastWriteTimeUtc(imagePath, lastAlbumTime);
    //    }
    //    catch (Exception ex)
    //    {
    //      Logger.WriteException(ex);
    //    }
    //    finally
    //    {
    //      lastGalleryUploadTime = DateTime.UtcNow;
    //    }
    //  }
    //}

    static void SaveGalleryImage(int galleryId, HttpPostedFile file, string filePath)
    {
      lock (lockObj)
      {
        FileInfo[] infos = UrlHlp.GalleryImageInfos(galleryId);

        file.SaveAs(filePath);

        if (infos.Length > 0 && DateTime.UtcNow - infos[0].LastWriteTimeUtc < TimeSpan.FromHours(8))
          File.SetLastWriteTimeUtc(filePath, infos[0].LastWriteTimeUtc);
      }
    }

    public static HttpResponseMessage GalleryUploader()
    {
      HttpContext httpContext = HttpContext.Current;

      return GalleryUploader(
        httpContext.GetUInt("thumbWidth") ?? EditElementHlp.thumbWidth,
        httpContext.GetUInt("thumbHeight") ?? EditElementHlp.thumbWidth,
        httpContext.Get("fullFillingThumb") == "true",
        httpContext.Get("jpegThumb") == "true"
      );
    }

    public static HttpResponseMessage GalleryUploader(int thumbWidth, int thumbHeight,
      bool fullFillingThumb, bool jpegThumb)
    {
      try
      {
        HttpContext httpContext = HttpContext.Current;

        int? objectId = httpContext.GetUInt("objectId"); // ?? state.CreatingObjectId;

        if (objectId == null)
          return null;

        string imagePath = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
          objectId.ToString(), "gallery");
        string thumbPath = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
          objectId.ToString(), "thumb");

        if (httpContext.Request.Files.Count == 0)
          return null;

        for (var i = 0; i < httpContext.Request.Files.Count; ++i)
        {
          HttpPostedFile file = httpContext.Request.Files[i];

          string title = Path.GetFileNameWithoutExtension(file.FileName);
          if (title.Length > 100)
            title = title.Substring(0, 100);

          using (Bitmap originalImage = (Bitmap)Bitmap.FromStream(file.InputStream))
          using (Bitmap thumbImage = FabricHlp.GetThumbnailImage(originalImage, 
            thumbWidth, thumbHeight, fullFillingThumb, !jpegThumb))
          {
            ImageFormat imageFormat = jpegThumb ? ImageFormat.Jpeg : ImageFormat.Png;
            thumbImage.Save(Path.Combine(thumbPath, string.Format("{0}.png", title)), imageFormat);
            string filePath = Path.Combine(imagePath, string.Format("{0}.jpg", title));

            SaveGalleryImage(objectId.Value, file, filePath);
            //file.SaveAs(filePath);

            ////hack чтобы загруженные вместе изображения сортировались по наименованию
            //SetImageModifyTime(filePath);
          }
        }

        return new HttpResponseMessage()
        {
          Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "text/json")
        };
      }
      catch (Exception ex)
      {
        Logger.WriteException(ex);
        throw;
      }
    }

    //public static HttpResponseMessage TileLargeUploader(int thumbLargeSize)
    //{
    //  HttpContext httpContext = HttpContext.Current;

    //  int? objectId = httpContext.GetUInt("objectId");

    //  if (objectId == null)
    //    return null;

    //  string path = FabricHlp.CheckAndCreateImageFolder(objectId.Value);

    //  if (httpContext.Request.Files.Count == 0)
    //    return null;

    //  HttpPostedFile file = httpContext.Request.Files[0];

    //  using (Bitmap originalImage = (Bitmap)Bitmap.FromStream(file.InputStream))
    //  using (Bitmap thumbImage = FabricHlp.GetThumbnailImage(originalImage, thumbLargeSize))
    //  {
    //    thumbImage.Save(Path.Combine(path, "thumb.png"), System.Drawing.Imaging.ImageFormat.Png);
    //    file.SaveAs(Path.Combine(path, "original.jpg"));
    //  }

    //  return new HttpResponseMessage()
    //  {
    //    Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "text/json")
    //  };
    //}

    public static HttpResponseMessage FilesUploader()
    {
      HttpContext httpContext = HttpContext.Current;

      int? objectId = httpContext.GetUInt("objectId");
      if (objectId == null)
        return null;

			string subfolder = httpContext.Get("subfolder");

			//string path = FabricHlp.CheckAndCreateImageFolder(objectId.Value);
			string path = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
				objectId.Value.ToString(), subfolder
			);

			for (var i = 0; i < httpContext.Request.Files.Count; ++i)
      {
        HttpPostedFile file = httpContext.Request.Files[i];

        string title = Path.GetFileNameWithoutExtension(file.FileName);
        if (title.Length > 100)
          title = title.Substring(0, 100);

        file.SaveAs(Path.Combine(path, title + Path.GetExtension(file.FileName)));
      }

      return new HttpResponseMessage()
      {
        Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "text/json")
      };
    }
  }
}
