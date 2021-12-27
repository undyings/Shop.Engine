using Commune.Basis;
using Commune.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Engine.Cml
{
	public class BoxIndex
	{
		public readonly ParentBox Box;
		readonly RowPropertyBlank<string> keyProperty;

		readonly RawCache<Dictionary<string, LightParent>> objectByKeyCache;

		public LightParent Find(string key)
		{
			return DictionaryHlp.GetValueOrDefault(objectByKeyCache.Result, key);
		}

		public LightParent CheckOrCreateObject(int typeKind, string key, string xmlIds)
		{
			if (StringHlp.IsEmpty(key) || StringHlp.IsEmpty(xmlIds))
				return null;

			LightParent obj = this.Find(key);
			if (obj == null)
			{
				RowLink row = Box.ObjectByXmlIds.AnyRow(xmlIds);
				if (row == null)
				{
					int? createObjId = Box.CreateUniqueObject(typeKind, xmlIds, null);
					if (createObjId == null)
						return null;
					return new LightParent(Box, createObjId.Value);
				}

				obj = new LightParent(Box, row.Get(ObjectType.ObjectId));
				obj.Set(keyProperty, key);
				return obj;
			}

			if (obj.Get(ObjectType.XmlObjectIds) != xmlIds)
				obj.Set(ObjectType.XmlObjectIds, xmlIds);

			return obj;
		}

		public BoxIndex(ParentBox objectBox, RowPropertyBlank<string> keyProperty)
		{
			this.Box = objectBox;
			this.keyProperty = keyProperty;

			this.objectByKeyCache = new Cache<Dictionary<string, LightParent>, long>(
				delegate
				{
					int[] objectIds = objectBox.AllObjectIds;
					Dictionary<string, LightParent> objectByKey = new Dictionary<string, LightParent>(objectIds.Length);
					foreach (int objectId in objectIds)
					{
						string key = objectBox.PropertiesByObjectIdWithKind.Get(keyProperty, 0, objectId) ?? "";
						objectByKey[key] = new LightParent(objectBox, objectId);
					}
					return objectByKey;
				},
				delegate { return objectBox.DataChangeTick; }
			);
		}
	}
}
