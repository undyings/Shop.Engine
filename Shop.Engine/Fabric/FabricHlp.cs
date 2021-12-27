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
  public class FabricHlp
  {
    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    static IStore store
    {
      get { return SiteContext.Default.Store; }
    }

    public readonly static DateTime RefTime = new DateTime(2016, 5, 20);

    public static LightSection ParentSectionForUnit(SectionStorage sections, LightKin unit)
    {
      //int? parentId = unit.GetParentId(SectionType.UnitLinks);
      //Logger.AddMessage("Parent1: {0}, {1}", unit.Id, parentId);
      //if (parentId != null)
      //  return sections.FindSection(parentId);

      RowLink[] parentRows = unit.AllParentRows(SectionType.UnitForPaneLinks);
      //int? parentId = unit.GetParentId(SectionType.UnitForPaneLinks);
      //Logger.AddMessage("ParentId: {0}", parentId);
      if (parentRows.Length > 0)
        return sections.FindSection(parentRows[0].Get(LinkType.ParentId));

      int? parentId = unit.GetParentId(UnitType.SubunitLinks);
      if (parentId != null)
      {
        LightUnit parentUnit = sections.FindUnit(parentId);
        if (parentUnit != null)
          return ParentSectionForUnit(sections, parentUnit);
        else
          return sections.FindSection(parentId);
      }

      return null;
    }

		public static LightGroup[] GetGroupBranch(ShopStorage shop, int? groupId)
		{
			List<LightGroup> branch = new List<LightGroup>();
			FillGroupBranch(branch, shop, groupId);
			return branch.ToArray();
		}

		static void FillGroupBranch(List<LightGroup> branch, ShopStorage shop, int? groupId)
		{
			LightGroup group = shop.FindGroup(groupId);
			if (group == null)
				return;

			branch.Insert(0, group);

			LightGroup parent = group.ParentGroup;
			if (parent != null)
				FillGroupBranch(branch, shop, parent.Id);
		}

		public static LightGroup[] FindParentGroups(ShopStorage shop, LightKin obj)
    {
      RowLink[] groupRows = obj.AllParentRows(GroupType.FabricTypeLink);
      List<LightGroup> groups = new List<LightGroup>();
      foreach (RowLink groupRow in groupRows)
      {
        int parentId = groupRow.Get(LinkType.ParentId);
        LightGroup group = shop.FindGroup(parentId);
        if (group != null)
          groups.Add(group);
      }
      return groups.ToArray();
    }

    public static int[] FindGroupIdsForAddFabric(ShopStorage shop, LightKin addFabric)
    {
      LightGroup[] parentGroups = FindParentGroups(shop, addFabric);
      Dictionary<int, LightKin> parentGroupById = _.MakeUniqueIndex(parentGroups,
        delegate (LightKin group) { return group.Id; });

      List<int> groupIdsForAdd = new List<int>();
      groupIdsForAdd.Add(-1);
      foreach (LightGroup rootGroup in shop.RootGroups)
      {
        FillGroupIdsForAddFabric(groupIdsForAdd, rootGroup, parentGroupById);
      }

      return groupIdsForAdd.ToArray();
    }

    static void FillGroupIdsForAddFabric(List<int> groupsForAdd,
      LightGroup group, Dictionary<int, LightKin> parentGroupById)
    {
			if (!parentGroupById.ContainsKey(group.Id))
				groupsForAdd.Add(group.Id);

      //if (parentGroupById.ContainsKey(parentGroup.Id))
      //  return;

      //if (parentGroup.Subgroups.Length == 0)
      //{
      //  groupsForAdd.Add(parentGroup.Id);
      //  return;
      //}

      foreach (LightGroup subGroup in group.Subgroups)
      {
        FillGroupIdsForAddFabric(groupsForAdd, subGroup, parentGroupById);

      }
    }

    public static void SetCreateTime(LightHead obj)
    {
      DateTime utcTime = DateTime.UtcNow;
      obj.Set(ObjectType.ActFrom, utcTime);
      obj.Set(ObjectType.ActTill, utcTime);
    }

    public static void CheckAndCreateMenu(IDataLayer fabricConnection, params string[] menuNames)
    {
      ObjectHeadBox box = new ObjectHeadBox(fabricConnection, DataCondition.ForTypes(SectionType.Section));

      bool isCreated = false;
      foreach (string menuName in menuNames)
      {
        string xmlIds = SectionType.Title.CreateXmlIds(menuName);
        if (box.ObjectByXmlIds.Exist(xmlIds))
          continue;

        int menuId = box.CreateObject(SectionType.Section, xmlIds, null);
        FabricHlp.SetCreateTime(new LightHead(box, menuId));

        isCreated = true;
      }

      if (isCreated)
        box.Update();
    }

    public static LightUnit CheckAndCreateUnit(LightSection section, string designKind, int paneIndex)
    {
      LightUnit unit = section.UnitForPane(paneIndex);
      if (unit != null)
        return unit;

      KinBox box = new KinBox(fabricConnection, "1=0");
      int? createUnitId = box.CreateUniqueObject(UnitType.Unit,
        UnitType.ParentId.CreateXmlIds(section.Id, paneIndex.ToString()), null);
      if (createUnitId == null)
      {
        Logger.AddMessage("Не удалось создать Unit: {0}, {1}", section.Id, paneIndex);
        return null;
      }

      LightKin addUnit = new LightKin(box, createUnitId.Value);
      FabricHlp.SetCreateTime(addUnit);

      addUnit.Set(SectionType.DesignKind, designKind);

      addUnit.SetParentId(SectionType.UnitForPaneLinks, paneIndex, section.Id);

      addUnit.Box.Update();

      SiteContext.Default.UpdateStore();

      return store.Sections.FindSection(section.Id).UnitForPane(paneIndex);
    }

		public static IEnumerable<Product> SortProducts(IEnumerable<Product> products,
			string sortKind, bool emptyFilter)
		{
			if (emptyFilter && StringHlp.IsEmpty(sortKind))
				return products;

			if (StringHlp.IsEmpty(sortKind) || sortKind == "alphabet")
				return _.SortBy(products, delegate (Product product) { return product.ProductName; });

			if (sortKind == "cheap")
				return _.SortBy(products, delegate (Product product)
					{ return new object[] { product.Get(FabricType.Price), product.ProductName }; },
					_.ArrayComparison
				);

      if (sortKind == "pricey")
        return _.SortBy(products, delegate(Product product)
					{ return new object[] { new CompareReverser(product.Get(FabricType.Price)), product.ProductName }; },
					_.ArrayComparison
				);

      return products;
    }

    public static string GetSeoTitle(LightObject obj, string pattern)
    {
      string seoTitle = obj.Get(SEOProp.Title);
      if (!StringHlp.IsEmpty(seoTitle))
        return seoTitle;

      return pattern;
    }

    public static string GetSeoDescription(LightObject obj, string pattern)
    {
      string description = obj.Get(SEOProp.Description);
      if (!StringHlp.IsEmpty(description))
        return description;

      return pattern;
    }

    public static IEnumerable<Product> SearchProducts(IEnumerable<Product> products, string searchText)
    {
      if (StringHlp.IsEmpty(searchText) || searchText.Length < 3)
        yield break;

      foreach (Product product in products)
      {
        string productName = product.ProductName;
        string annotation = product.Annotation;

        if (ContainsText(productName, searchText) || ContainsText(annotation, searchText))
          yield return product;
      }
    }

    static bool ContainsText(string text, string search)
    {
      if (StringHlp.IsEmpty(text))
        return false;

      return text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    //public static LightGroup GetParent(ShopStorage store, string kind, int? parentId, int? id)
    //{
    //  if (kind == "group")
    //    return GetParentForGroup(store, id);
    //  if (kind == "product")
    //    return GetParentForProduct(store, id);
    //  return null;
    //}

    public static LightGroup GetParentForGroup(ShopStorage store, int? groupId)
    {
      if (groupId == null)
        return null;

      LightGroup group = store.FindGroup(groupId.Value);
      if (group == null || group.ParentGroup == null)
        return null;
      return group.ParentGroup;
    }

    //public static LightGroup GetParentForProduct(ShopStorage store, int? productId)
    //{
    //  if (productId == null)
    //    return null;

    //  Product product = store.FindProduct(productId.Value);
    //  if (product == null)
    //    return null;

    //  return store.FindGroup(product.ParentGroupId);
    //}

    public static LightObject[] GetVarieties(ShopStorage store, LightKin fabric)
    {
      int[] varietyIds = fabric.AllChildIds(FabricType.VarietyTypeLink);
      if (varietyIds.Length == 0)
        return new LightObject[0];

      List<LightObject> varieties = new List<LightObject>(varietyIds.Length);
      foreach (int varietyId in varietyIds)
      {
        LightObject variety = store.FindVariety(varietyId);
        if (variety != null)
          varieties.Add(variety);
      }
      return varieties.ToArray();
    }

    public static void MakeTransparentBackground(Bitmap image)
    {
      Color? average = GetAverageColor(image);
      if (average == null)
        return;

			//hack чтобы не убирать черный фон
			if (average.Value.R + average.Value.G + average.Value.B < 200)
				return;

      Color etalon = average.Value;

      {
        int transparentCount = 0;
        for (int y = 0; y < image.Height; ++y)
        {
          for (int x = 0; x < image.Width; ++x)
          {
            Color color = image.GetPixel(x, y);
            if (IsNearColor(etalon, color))
            {
              image.SetPixel(x, y, Color.FromArgb(0));
              transparentCount++;
            }
          }
        }
      }
    }

    public static Bitmap GetThumbnailImage(Image sourceImage, int width, int height,
      bool isFullFilling, bool transparentBackground)
    {
      if (isFullFilling)
        return GetFullFillThumbnailImage(sourceImage, width, height);

      return GetProportionsThumbnailImage(sourceImage, width, height, transparentBackground);
    }

    public static Bitmap GetFullFillThumbnailImage(Image sourceImage, int width, int height)
    {
      float shiftX = 0;
      float shiftY = 0;

      int thumbWidth = sourceImage.Width * height / sourceImage.Height;
      int thumbHeight = sourceImage.Height * width / sourceImage.Width;

      if (thumbWidth > width)
        shiftX = (sourceImage.Width - width * sourceImage.Height / height) / 2f;
      else
        shiftY = (sourceImage.Height - height * sourceImage.Width / width) / 2f;

      Bitmap thumbImage = new Bitmap(width, height);
      using (Graphics gr = Graphics.FromImage(thumbImage))
      {
        gr.SmoothingMode = SmoothingMode.HighQuality;
        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
        gr.DrawImage(sourceImage, new RectangleF(0, 0, width, height), 
          new RectangleF(shiftX, shiftY, sourceImage.Width - shiftX * 2, sourceImage.Height - shiftY * 2),
          GraphicsUnit.Pixel
        );
      }

      return thumbImage;
    }

    public static Bitmap GetProportionsThumbnailImage(Image sourceImage, 
      int width, int height, bool transparentBackground)
    {
      int thumbWidth = sourceImage.Width * height / sourceImage.Height;
      int thumbHeight = sourceImage.Height * width / sourceImage.Width;

      if (thumbWidth > width)
        thumbWidth = width;
      else
        thumbHeight = height;

      Bitmap thumbImage = new Bitmap(thumbWidth, thumbHeight);
      using (Graphics gr = Graphics.FromImage(thumbImage))
      {
        gr.SmoothingMode = SmoothingMode.HighQuality;
        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
        gr.DrawImage(sourceImage, new Rectangle(0, 0, thumbImage.Width, thumbImage.Height));
      }

      if (transparentBackground)
        MakeTransparentBackground(thumbImage);

      return thumbImage;
    }

    public static Bitmap GetThumbnailImage(Image sourceImage, int width)
    {
      int height = sourceImage.Height * width / sourceImage.Width;
      if (height > width)
      {
        int calcHeight = height;
        height = width;
        width = width * height / calcHeight;
      }

      Bitmap thumbImage = new Bitmap(width, height);
      using (Graphics gr = Graphics.FromImage(thumbImage))
      {
        gr.SmoothingMode = SmoothingMode.HighQuality;
        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
        gr.DrawImage(sourceImage, new Rectangle(0, 0, thumbImage.Width, thumbImage.Height));
      }

      MakeTransparentBackground(thumbImage);

      //Color? average = GetAverageColor(thumbImage);
      //if (average == null)
      //  return thumbImage;

      //Color etalon = average.Value;

      //{
      //  int transparentCount = 0;
      //  for (int y = 0; y < thumbImage.Height; ++y)
      //  {
      //    for (int x = 0; x < thumbImage.Width; ++x)
      //    {
      //      Color color = thumbImage.GetPixel(x, y);
      //      if (IsNearColor(etalon, color))
      //      {
      //        thumbImage.SetPixel(x, y, Color.FromArgb(0));
      //        transparentCount++;
      //      }
      //    }
      //  }
      //}

      return thumbImage;
    }

    static Color? GetAverageColor(Bitmap thumbImage)
    {
      Color[] corners = new Color[] { thumbImage.GetPixel(0, 0),
        thumbImage.GetPixel(0, thumbImage.Height - 1),
        thumbImage.GetPixel(thumbImage.Width - 1, 0),
        thumbImage.GetPixel(thumbImage.Width - 1, thumbImage.Height - 1)
      };

      int alpha = 0;
      int red = 0;
      int green = 0;
      int blue = 0;
      for (int i = 0; i < corners.Length; ++i)
      {
        alpha += corners[i].A;
        red += corners[i].R;
        green += corners[i].G;
        blue += corners[i].B;
      }

      Color average = Color.FromArgb(alpha / 4, red / 4, green / 4, blue / 4);
      foreach (Color corner in corners)
      {
        if (!IsNearColor(average, corner))
          return null;
      }

      if (average.A == 0)
        return null;

      return average;
    }

    static bool IsNearColor(Color etalon, Color color)
    {
      int dAlpha = 0; // (int)etalon.A - color.A;
      int dRed = (int)etalon.R - color.R;
      int dGreen = (int)etalon.G - color.G;
      int dBlue = (int)etalon.B - color.B;
      return (dAlpha * dAlpha + dRed * dRed + dGreen * dGreen + dBlue * dBlue) < 28;
    }

    //public static Bitmap GetThumbnailImage(Image sourceImage, int width, bool setTransparent)
    //{
    //  int height = sourceImage.Height * width / sourceImage.Width;
    //  if (height > width)
    //    height = width;

    //  Bitmap thumbImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    //  using (Graphics gr = Graphics.FromImage(thumbImage))
    //  {
    //    gr.SmoothingMode = SmoothingMode.HighQuality;
    //    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
    //    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
    //    gr.DrawImage(sourceImage, new Rectangle(0, 0, thumbImage.Width, thumbImage.Height));
    //  }

    //  if (setTransparent)
    //  {
    //    Color makeTransparent = thumbImage.GetPixel(0, 0);
    //    int transparentCount = 0;
    //    for (int y = 0; y < thumbImage.Height; ++y)
    //    {
    //      for (int x = 0; x < thumbImage.Width; ++x)
    //      {
    //        Color color = thumbImage.GetPixel(x, y);
    //        //if (color.R == makeTransparent.R && color.G == makeTransparent.G && color.B == makeTransparent.B)
    //        if (color.R > 253 && color.G > 253 && color.B > 253)
    //        {
    //          thumbImage.SetPixel(x, y, Color.FromArgb(0));
    //          transparentCount++;
    //        }
    //      }
    //    }
    //    Logger.AddMessage("Transparent: {0}, {1}", makeTransparent, transparentCount);
    //    //thumbImage.MakeTransparent(thumbImage.GetPixel(0, 0));
    //  }

    //  return thumbImage;
    //}

    //public static string CheckAndCreateImageFolder(int objectId)
    //{
    //  return ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
    //    objectId.ToString());
    //}

    public static IEnumerable<string> GetImageNamesForDescription(int objectId, string subfolder)
    {
			//string directory = CheckAndCreateImageFolder(objectId);

			string directory = ApplicationHlp.CheckAndCreateFolderPath(SiteContext.Default.ImagesPath,
				objectId.ToString(), subfolder
			);

			string[] imageNames = Directory.GetFiles(directory);
      foreach (string imagePath in imageNames)
      {
        string imageFile = Path.GetFileName(imagePath);
        if (imageFile != "thumb.png" && imageFile != "original.jpg")
          yield return imageFile;
      }
    }

    //public static KinBox LoadKinBox(IDataLayer fabricConnection, 
    //  string conditionWithoutWhere, params DbParameter[] conditionParameters)
    //{
    //  return new KinBox(fabricConnection, conditionWithoutWhere, conditionParameters);
    //}

    //public static ParentBox LoadParentBox(IDataLayer fabricConnection,
    //  string conditionWithoutWhere, params DbParameter[] conditionParameters)
    //{
    //  return new ParentBox(fabricConnection, conditionWithoutWhere, conditionParameters);
    //}

    //public static ObjectBox LoadObjectBox(IDataLayer fabricConnection,
    //  string conditionWithoutWhere, params DbParameter[] conditionParameters)
    //{
    //  return new ObjectBox(fabricConnection, conditionWithoutWhere, conditionParameters);
    //}

    //public static ObjectHeadBox LoadHeadBox(IDataLayer fabricConnection,
    //  string conditionWithoutWhere, params DbParameter[] conditionParameters)
    //{
    //  return new ObjectHeadBox(fabricConnection, conditionWithoutWhere, conditionParameters);
    //}

    //public static LightObject LoadObject(IDataLayer fabricConnection, int typeId, int objectId)
    //{
    //  ObjectBox box = new ObjectBox(fabricConnection, DataCondition.ForTypeObjects(typeId, objectId));
    //  int[] allObjectIds = box.AllObjectIds;
    //  if (allObjectIds.Length == 0)
    //    return null;
    //  return new LightObject(box, allObjectIds[0]);
    //}

    //public static LightKin LoadKin(IDataLayer fabricConnection, int typeId, int objectId)
    //{
    //  KinBox box = new KinBox(fabricConnection, DataCondition.ForTypeObjects(typeId, objectId));
    //  int[] allObjectIds = box.AllObjectIds;
    //  if (allObjectIds.Length == 0)
    //    return null;
    //  return new LightKin(box, allObjectIds[0]);
    //}
  }
}
