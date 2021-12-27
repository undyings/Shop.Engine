using Commune.Basis;
using Commune.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Engine.Cml
{
	public class CmlHlp
	{
		public static Dictionary<string, ValueItem> CreateValueIndices(ObjectBox propertyBox)
		{
			Dictionary<string, ValueItem> valueIndices = new Dictionary<string, ValueItem>();

			foreach (int propertyId in propertyBox.AllObjectIds)
			{
				LightObject property = new LightObject(propertyBox, propertyId);

				RowPropertyBlank<string> blank = DataBox.Create(propertyId, DataBox.StringValue);

				RowLink[] idRows = property.AllPropertyRows(MetaPropertyType.EnumIds);
				foreach (RowLink row in idRows)
				{
					int index = row.Get(PropertyType.PropertyIndex);
					string guid = property.Get(MetaPropertyType.EnumIds, index);
					string value = property.Get(MetaPropertyType.EnumItems, index);

					if (StringHlp.IsEmpty(guid))
						continue;

					valueIndices[guid] = new ValueItem(blank, guid, value);
				}
			}

			return valueIndices;
		}
	}
}
