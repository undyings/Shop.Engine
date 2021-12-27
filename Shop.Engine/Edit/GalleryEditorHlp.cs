using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroBolt.Wui;
using Commune.Html;
using Commune.Data;
using Commune.Basis;
using System.IO;

namespace Shop.Engine
{
  public class GalleryEditorHlp
  {
    readonly static char[] charSortOrder = new char[] {
      '!', '#', '$', '%', '&', '(', ')', '@', '^', '_', '`', '~', '+', '=',
      '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
      'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
      'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
      'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й',
      'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф',
      'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я'
    };

    static IHtmlControl GetImageTile(EditState state, int objectId, string imageName, int index)
    {
      int tileWidth = 164;

      string thumbUrl = UrlHlp.GalleryThumbUrl(objectId, imageName);
      string imageUrl = UrlHlp.GalleryImageUrl(objectId, imageName);
      return new HPanel(
        EditElementHlp.GetImagePanel(thumbUrl, "", tileWidth),
        new HPanel(
          new HLink(imageUrl, "Ссылка"),
          //new HLabel(File.GetLastWriteTimeUtc(UrlHlp.GalleryImagePath(objectId, imageName)).Ticks),
          new HPanel(
            new HPanel(
              new HButton("", std.BeforeAwesome(@"\f00c", 0)).Color(DecorEdit.iconColor)
                .Title("Выбрать для перемещения")
                .Event(string.Format("time_gallery_{0}", imageName), "",
                  delegate
                  {
                    state.Option.Set(EditOptionType.MovableImageIndex, index);
                  }
                ).Hide(state.Option.Get(EditOptionType.MovableImageIndex) == index)
            ).InlineBlock(),
            new HPanel(
              new HButton("", std.BeforeAwesome(@"\f05e", 0)).Color(DecorEdit.propertyMinorColor)
                .Title("Отменить перемещение")
                .Event("paste_cancel", "", delegate
                {
                  state.Option.Set(EditOptionType.MovableImageIndex, null);
                },
                index
              ).Hide(state.Option.Get(EditOptionType.MovableImageIndex) != index)
            ).InlineBlock(),
            new HButton("", std.BeforeAwesome(@"\f00d", 0).Color("red"))
              .PositionAbsolute().Right(0).Top(0)
              .Title("Удалить").Hide(!state.Option.Get(EditOptionType.AllowDeleteImage))
              .Event(string.Format("delete_gallery_{0}", imageName), "",
                delegate (JsonData json)
                {
                  string thumbPath = Path.Combine(SiteContext.Default.ImagesPath,
                    objectId.ToString(), "thumb", imageName);
                  string thumbPngPath = Path.ChangeExtension(thumbPath, "png");
                  string thumbJpgPath = Path.ChangeExtension(thumbPath, "jpg");

                  string imagePath = Path.Combine(SiteContext.Default.ImagesPath,
                    objectId.ToString(), "gallery", imageName);

                  if (File.Exists(thumbPngPath))
                    File.Delete(thumbPngPath);

                  if (File.Exists(thumbJpgPath))
                    File.Delete(thumbJpgPath);

                  if (File.Exists(imagePath))
                    File.Delete(imagePath);

                  Logger.AddMessage("Изображение '{0}' успешно удалено", imagePath);
                }
            )
          ).PositionRelative()
        ).PositionRelative()
      ).Width(tileWidth).Margin(8).InlineBlock().VAlign(true)
        .EditContainer(string.Format("{0}_{1}", imageName, index));
    }

    static bool MoveImage(int galleryId, int? movableImageIndex, int pasteIndex)
    {
      string[] imageNames = UrlHlp.GalleryImageNames(galleryId);

      if (movableImageIndex == null || movableImageIndex < 0 || movableImageIndex > imageNames.Length - 1)
        return false;
      if (pasteIndex < 0 || pasteIndex > imageNames.Length)
        return false;

      string moveImageName = imageNames[movableImageIndex.Value];
      string moveImagePath = UrlHlp.GalleryImagePath(galleryId, moveImageName);
      if (pasteIndex == 0)
      {
        //DateTime time = File.GetLastWriteTimeUtc(UrlHlp.GalleryImagePath(galleryId, imageNames[0]));
        File.SetLastWriteTimeUtc(moveImagePath, DateTime.UtcNow);
        return true;
      }

      if (pasteIndex == imageNames.Length)
      {
        DateTime time = File.GetLastWriteTimeUtc(UrlHlp.GalleryImagePath(galleryId, imageNames[imageNames.Length - 1]));
        File.SetLastWriteTimeUtc(moveImagePath, time.AddSeconds(-1));
        return true;
      }

      FileInfo prev = new FileInfo(UrlHlp.GalleryImagePath(galleryId, imageNames[pasteIndex - 1]));
      FileInfo next = new FileInfo(UrlHlp.GalleryImagePath(galleryId, imageNames[pasteIndex]));

      File.SetLastWriteTimeUtc(moveImagePath,
        new DateTime((prev.LastWriteTimeUtc.Ticks + next.LastWriteTimeUtc.Ticks) / 2)
      );

      if (prev.LastWriteTimeUtc.Ticks - next.LastWriteTimeUtc.Ticks > 1)
        return true;

      string prevName = Path.GetFileNameWithoutExtension(prev.Name);
      string nextName = Path.GetFileNameWithoutExtension(next.Name);

      string newImageName;
      char addChar = charSortOrder[charSortOrder.Length / 2];
      if (nextName.StartsWith(prevName) && nextName.Length > prevName.Length)
      {
        char nextChar = nextName[prevName.Length];
        int indexOf = Array.IndexOf(charSortOrder, nextChar);
        if (indexOf >= 0)
          addChar = charSortOrder[indexOf / 2];
      }

      newImageName = string.Format("{0}.jpg", prevName + addChar);
      string newImagePath = UrlHlp.GalleryImagePath(galleryId, newImageName);

      string moveThumbPath = UrlHlp.GalleryThumbPath(galleryId, moveImageName);
      string newThumbPath = UrlHlp.GalleryThumbPath(galleryId, newImageName);

      File.Move(moveImagePath, newImagePath);
      File.Move(moveThumbPath, newThumbPath);

      return true;
    }

    static IHtmlControl PasteBlock(EditState state, int galleryId, int index)
    {
      return new HPanel(
        new HButton("", std.BeforeAwesome(@"\f0ea", 0)).Block().MarginTop(20).Color(DecorEdit.iconColor)
          .Title("Переместить изображение")
          .Event("paste_image", "", delegate
            {
              MoveImage(galleryId, state.Option.Get(EditOptionType.MovableImageIndex), index);
              state.Option.Set(EditOptionType.MovableImageIndex, null);
            },
            index
          )
      ).InlineBlock().MarginLeft(4).MarginRight(4);
    }

    public static IHtmlControl GetGalleryPanel(EditState state, int galleryId, BaseTunes tunes)
    {
      int? movableImageIndex = state.Option.Get(EditOptionType.MovableImageIndex);
      bool imageMovingStarted = movableImageIndex != null;

      List<IHtmlControl> tileControls = new List<IHtmlControl>();
      int i = -1;
      string[] imageNames = UrlHlp.GalleryImageNames(galleryId);
      foreach (string imageName in imageNames)
      {
        ++i;

        bool isMovableImage = movableImageIndex == i || movableImageIndex == i - 1;

        if (imageMovingStarted && !isMovableImage)
          tileControls.Add(PasteBlock(state, galleryId, i));

        IHtmlControl tileControl = GetImageTile(state, galleryId, imageName, i);
        if (imageMovingStarted)
        {
          int margin = movableImageIndex == i ? 24 : 0;
          tileControl.MarginLeft(margin).MarginRight(margin);
        }
        tileControls.Add(tileControl);
      }

      if (imageMovingStarted && movableImageIndex != imageNames.Length - 1)
        tileControls.Add(PasteBlock(state, galleryId, imageNames.Length));

      return DecorEdit.FieldBlock(
        "Галерея изображений",
        new HPanel(
          new HPanel(
            tileControls.ToArray()
          ).EditContainer(string.Format("gallery_{0}_{1}", galleryId, i)).PaddingLeft(8),
          new HPanel(
            new HFileUploader("/galleryUpload", "Загрузить изображения", galleryId,
              new HAttribute("thumbWidth", tunes.GetSetting("thumbWidth")),
              new HAttribute("thumbHeight", tunes.GetSetting("thumbHeight")),
              new HAttribute("fullFillingThumb", tunes.GetSetting("fullFillingThumb")),
              new HAttribute("jpegThumb", tunes.GetSetting("jpegThumb"))
            ),
            std.Button(state.Option.Get(EditOptionType.AllowDeleteImage) ? "Запретить удаление" : "Разрешить удаление")
              .PositionAbsolute().Top(0).Right(10)
              .Event("allow_delete_image", "", delegate
                {
                  bool allow = state.Option.Get(EditOptionType.AllowDeleteImage);
                  state.Option.Set(EditOptionType.AllowDeleteImage, !allow);
                }
              )
          ).PositionRelative().MarginTop(10).MarginLeft(8).MarginBottom(8)
        )
      );
    }
  }
}
