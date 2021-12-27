using Commune.Basis;
using Commune.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shop.Engine.Cml
{
	public class CmlSyncHlp
	{
		public static void CheckOrSet(LightObject obj, RowPropertyBlank<string> property,
			int propertyIndex, string value)
		{
			if (obj.Get(property, propertyIndex) != value)
				obj.Set(property, propertyIndex, value);
		}

		public static void CheckOrSet(LightObject obj, RowPropertyBlank<string> property,	string value)
		{
			CheckOrSet(obj, property, 0, value);
		}

		public static void CheckOrSet(LightObject obj, RowPropertyBlank<bool> property,	bool value)
		{
			if (obj.Get(property) != value)
				obj.Set(property, value);
		}

		public static void CheckOrSet(LightObject obj, RowPropertyBlank<int> property, int value)
		{
			if (obj.Get(property) != value)
				obj.Set(property, value);
		}

		public static int? CheckOrCreateObject(ObjectHeadBox box, int typeId, string xmlIds)
		{
			RowLink row = box.ObjectByXmlIds.AnyRow(xmlIds);
			if (row == null)
			{
				int? createObjectId = box.CreateUniqueObject(typeId, xmlIds, null);
				Logger.AddMessage("CreateObject: {0}, {1}, {2}", typeId, xmlIds, createObjectId);
				return createObjectId;
			}

			return row.Get(ObjectType.ObjectId);
		}

		public static LightParent SyncOrCreateGroup(ParentBox groupBox, string id, string name)
		{
			if (StringHlp.IsEmpty(id))
				return null;

			string xmlIds = GroupType.Identifier.CreateXmlIds(id);

			int? groupId = CheckOrCreateObject(groupBox, GroupType.Group, xmlIds);
			if (groupId == null)
				return null;

			long startChangeTick = groupBox.DataChangeTick;

			LightParent group = new LightParent(groupBox, groupId.Value);

			CheckOrSet(group, SEOProp.Name, name);
			CheckOrSet(group, SEOProp.IsImport, true);

			if (group.Get(ObjectType.ActFrom) == null)
				group.Set(ObjectType.ActFrom, DateTime.UtcNow);

			if (group.Get(ObjectType.ActTill) == null || startChangeTick != groupBox.DataChangeTick)
				group.Set(ObjectType.ActTill, DateTime.UtcNow);

			return group;
		}

		public static LightObject SyncOrCreateProperty(ObjectBox propertyBox,
			string id, string name, string kind, PropertyItem[] items)
		{
			if (StringHlp.IsEmpty(id))
				return null;

			string xmlIds = MetaPropertyType.Identifier.CreateXmlIds(id);

			int? propertyId = CheckOrCreateObject(propertyBox, MetaPropertyType.Property, xmlIds);
			if (propertyId == null)
				return null;

			LightObject property = new LightObject(propertyBox, propertyId.Value);

			CheckOrSet(property, SEOProp.Name, name);
			CheckOrSet(property, SEOProp.IsImport, true);

			//if (StringHlp.IsEmpty(property.Get(MetaPropertyType.Kind)))
			//	property.Set(MetaPropertyType.Kind, "enum");

			int itemLength = items.Length;
			if (itemLength > 100)
				itemLength = 0;

			if (itemLength > 0)
			{
				ArrayHlp.Sort(items, delegate (PropertyItem item) { return item.Value; });

				for (int i = 0; i < items.Length; ++i)
				{
					PropertyItem item = items[i];
					CheckOrSet(property, MetaPropertyType.EnumIds, i, item.Id);
					CheckOrSet(property, MetaPropertyType.EnumItems, i, item.Value);
				}
			}

			int enumIdsCount = property.AllPropertyRows(MetaPropertyType.EnumIds).Length;
			for (int i = enumIdsCount - 1; i >= items.Length; --i)
			{
				property.RemoveProperty(MetaPropertyType.EnumIds, i);
			}
			int enumItemsCount = property.AllPropertyRows(MetaPropertyType.EnumItems).Length;
			for (int i = enumItemsCount - 1; i >= items.Length; --i)
			{
				property.RemoveProperty(MetaPropertyType.EnumItems, i);
			}

			return property;
		}

		public static LightParent SyncOrCreateFabricKind(ParentBox kindBox, ObjectBox propertyBox,
			string id, string name, List<string> propertyGuids)
		{
			if (StringHlp.IsEmpty(id))
				return null;

			string xmlIds = MetaKindType.Identifier.CreateXmlIds(id);

			int? kindId = CheckOrCreateObject(kindBox, MetaKindType.FabricKind, xmlIds);
			if (kindId == null)
				return null;

			LightParent kind = new LightParent(kindBox, kindId.Value);

			CheckOrSet(kind, SEOProp.Name, name);
			CheckOrSet(kind, SEOProp.IsImport, true);

			List<int> propertyIds = new List<int>();
			foreach (string propertyGuid in propertyGuids)
			{
				string propertyXmlIds = MetaPropertyType.Identifier.CreateXmlIds(propertyGuid);
				RowLink propertyRow = propertyBox.ObjectByXmlIds.AnyRow(propertyXmlIds);
				if (propertyRow != null)
					propertyIds.Add(propertyRow.Get(ObjectType.ObjectId));
				else
					Logger.AddMessage("В категории {0}:{1} не найдена ссылка на свойство: {2}",
						id, name, propertyGuid
				  );
			}

			for (int i = 0; i < propertyIds.Count; ++i)
			{
				int propertyId = propertyIds[i];
				if (kind.GetChildId(MetaKindType.PropertyLinks, i) != propertyId)
					kind.SetChildId(MetaKindType.PropertyLinks, i, propertyId);
			}

			int childCount = kind.AllChildRows(MetaKindType.PropertyLinks).Length;
			for (int i = childCount - 1; i >= propertyIds.Count; --i)
			{
				kind.RemoveChildLink(MetaKindType.PropertyLinks, i);
			}

			return kind;
		}

		public static LightObject SyncFabricOffer(ObjectBox fabricBox, 
			string fabricGuid, int? price, int? amount)
		{
			if (StringHlp.IsEmpty(fabricGuid))
				return null;

			string xmlIds = FabricType.Identifier.CreateXmlIds(fabricGuid);

			RowLink fabricRow = fabricBox.ObjectByXmlIds.AnyRow(xmlIds);
			if (fabricRow == null)
				return null;

			long startChangeTick = fabricBox.DataChangeTick;

			LightObject fabric = new LightObject(fabricBox, fabricRow.Get(ObjectType.ObjectId));

			if (price != null)
				CheckOrSet(fabric, FabricType.Price, price.Value);

			if (amount != null)
				CheckOrSet(fabric, FabricType.InStockCount, amount.Value);

			if (startChangeTick != fabricBox.DataChangeTick)
				fabric.Set(ObjectType.ActTill, DateTime.UtcNow);

			return fabric;
		}

		public static LightObject SyncOrCreateFabric(
			ParentBox groupBox, ParentBox kindBox, Dictionary<string, ValueItem> valueIndices,
			ObjectBox fabricBox, string fabricGuid, string fabricName, 
			List<string> groupGuids, string categoryGuid, string description,	List<Option> properties)
		{
			if (StringHlp.IsEmpty(fabricGuid))
				return null;

			string xmlIds = FabricType.Identifier.CreateXmlIds(fabricGuid);

			int? fabricId = CheckOrCreateObject(fabricBox, FabricType.Fabric, xmlIds);
			if (fabricId == null)
				return null;

			long startChangeTick = fabricBox.DataChangeTick;

			LightObject fabric = new LightObject(fabricBox, fabricId.Value);

			CheckOrSet(fabric, SEOProp.Name, fabricName);
			CheckOrSet(fabric, SEOProp.IsImport, true);

			string kindXmlIds = MetaKindType.Identifier.CreateXmlIds(categoryGuid);
			RowLink kindRow = kindBox.ObjectByXmlIds.AnyRow(kindXmlIds);
			if (kindRow != null)
			{
				int kindId = kindRow.Get(ObjectType.ObjectId);
				if (fabric.Get(FabricType.Kind) != kindId)
					fabric.Set(FabricType.Kind, kindId);
			}

			if (fabric.Get(FabricType.Description) != description)
				fabric.Set(FabricType.Description, description);

			List<int> groupIds = new List<int>();
			foreach (string groupGuid in groupGuids)
			{
				string groupXmlIds = GroupType.Identifier.CreateXmlIds(groupGuid);
				RowLink groupRow = groupBox.ObjectByXmlIds.AnyRow(groupXmlIds);
				if (groupRow != null)
					groupIds.Add(groupRow.Get(ObjectType.ObjectId));
			}

			RowLink[] links = groupBox.ChildsByChildIdWithKind.Rows(GroupType.FabricTypeLink.Kind, fabricId.Value);

			CollectionSynchronizer.Synchronize(links,
				delegate (RowLink link) { return link.Get(LinkType.ParentId); },
				groupIds, delegate (int groupId) { return groupId; },
				delegate (RowLink removeLink)
				{
					groupBox.ChildTable.RemoveRow(removeLink);
				},
				delegate (int addGroupId)
				{
					RowLink addLink = groupBox.ChildTable.NewRow();
					addLink.Set(LinkType.ParentId, addGroupId);
					addLink.Set(LinkType.ChildId, fabricId.Value);
					addLink.Set(LinkType.TypeId, GroupType.FabricTypeLink.Kind);

					groupBox.ChildsByObjectIdWithKind.AddInArray(addLink);
				},
				delegate (RowLink link, int groupId)
				{
				}
			);

			foreach (Option property in properties)
			{
				ValueItem value = DictionaryHlp.GetValueOrDefault(valueIndices, property.Item2);
				if (value == null)
				{
					Logger.AddMessage("Для товара {0}:{1} не найдено значение {2}",
						fabricGuid, fabricName, property.Item2
					);
					continue;
				}

				CheckOrSet(fabric, value.Property, value.Value);
			}

			if (fabric.Get(ObjectType.ActFrom) == null)
				fabric.Set(ObjectType.ActFrom, DateTime.UtcNow);

			if (fabric.Get(ObjectType.ActTill) == null || startChangeTick != fabricBox.DataChangeTick)
				fabric.Set(ObjectType.ActTill, DateTime.UtcNow);

			return fabric;
		}

		public static void SyncObjectImage(int objectId, string rootPath, string imagePath)
		{
			ApplicationHlp.CheckAndCreateFolderPath(UrlHlp.ImagePath(), objectId.ToString());

			string originalPath = UrlHlp.ImagePath(objectId.ToString(), "original.jpg");
			string thumbPath = UrlHlp.ImagePath(objectId.ToString(), "thumb.png");

			if (StringHlp.IsEmpty(imagePath))
			{
				if (File.Exists(originalPath))
					File.Delete(originalPath);
				if (File.Exists(thumbPath))
					File.Delete(thumbPath);
				return;
			}

			string fullPath = Path.Combine(rootPath, imagePath);

			if (!File.Exists(fullPath))
			{
				Logger.AddMessage("Для объекта {0} не найдена картинка {1}", objectId, imagePath);
				return;
			}

			long newLength = new FileInfo(fullPath).Length;
			long oldLength = File.Exists(originalPath) ? new FileInfo(originalPath).Length : 0;

			if (newLength != oldLength)
			{
				File.Copy(fullPath, originalPath, true);
				Bitmap thumbImage = FabricHlp.GetThumbnailImage(Image.FromFile(fullPath), 
					EditElementHlp.thumbWidth, EditElementHlp.thumbWidth, false, true
				);

				thumbImage.Save(thumbPath, ImageFormat.Png);				
			}
		}

	}
}
