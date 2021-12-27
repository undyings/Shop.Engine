using Commune.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Engine.Cml
{
	public class PropertyItem
	{
		public readonly string Id;
		public readonly string Value;

		public PropertyItem(string id, string value)
		{
			this.Id = id;
			this.Value = value;
		}
	}

	public class ValueItem
	{
		public readonly RowPropertyBlank<string> Property;
		public readonly string ValueGuid;
		public readonly string Value;

		public ValueItem(RowPropertyBlank<string> property, string valueGuid, string value)
		{
			this.Property = property;
			this.ValueGuid = valueGuid;
			this.Value = value;
		}
	}

}
