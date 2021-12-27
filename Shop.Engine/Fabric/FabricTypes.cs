using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class SEOProp
  {
    public readonly static RowPropertyBlank<string> Title = DataBox.Create(101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Description = DataBox.Create(102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Keywords = DataBox.Create(103, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Text = DataBox.Create(104, DataBox.StringValue);

		public readonly static RowPropertyBlank<string> Name = DataBox.Create(110, DataBox.StringValue);
		public readonly static RowPropertyBlank<bool> IsImport = DataBox.Create(115, DataBox.BoolValue);

    public readonly static XmlDisplayName Identifier = new XmlDisplayName();
    public readonly static RowPropertyBlank<string> SortingPrefix = DataBox.Create(1100, DataBox.StringValue);
    public readonly static RowPropertyBlank<DateTime?> SortTime = DataBox.Create(17103, DataBox.DateTimeNullableValue);

		public static string GetDisplayName(LightObject obj)
		{
			string name = obj.Get(SEOProp.Name);
			if (!StringHlp.IsEmpty(name))
				return name;

			return obj.Get(SEOProp.Identifier);
		}

		public static string GetEditName(LightObject obj)
		{
			string identifier = obj.Get(SEOProp.Identifier);
			string name = obj.Get(SEOProp.Name);
			if (StringHlp.IsEmpty(name))
				return identifier;

			return string.Format("{0} ({1})", name, identifier);
		}
	}

  public class GroupType
  {
		public static string DisplayName(LightObject group)
		{
			string name = group.Get(SEOProp.Name);
			if (!StringHlp.IsEmpty(name))
				return name;

			return group.Get(GroupType.Identifier);
		}

    public const int Group = 1000;

    public readonly static XmlDisplayName Identifier = new XmlDisplayName();

		public readonly static RowPropertyBlank<string> DesignKind = SectionType.DesignKind;

		//public readonly static RowPropertyBlank<string> SortingPrefix = DataBox.Create(1100, DataBox.StringValue);
		public readonly static RowPropertyBlank<int?> FabricKind = DataBox.Create(1110, DataBox.IntNullableValue);

    public readonly static LinkKindBlank SubgroupTypeLink = new LinkKindBlank(1500);
    public readonly static LinkKindBlank FabricTypeLink = new LinkKindBlank(1501);
  }

  public class FabricType
  {
		public static string DisplayName(LightObject fabric)
		{
			string name = fabric.Get(SEOProp.Name);
			if (!StringHlp.IsEmpty(name))
				return name;

			return fabric.Get(FabricType.Identifier);
		}

    public const int Fabric = 3000;

    public readonly static XmlDisplayName Identifier = new XmlDisplayName();

    //public readonly static RowPropertyBlank<string> SortingPrefix = DataBox.Create(3100, DataBox.StringValue);

    public readonly static RowPropertyBlank<int> Price = DataBox.Create(3101, DataBox.IntValue);
    public readonly static RowPropertyBlank<string> Annotation = DataBox.Create(3102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Description = DataBox.Create(3103, DataBox.StringValue);
    public readonly static RowPropertyBlank<int> InStockCount = DataBox.Create(3104, DataBox.IntValue);
    //public readonly static RowPropertyBlank<string> IdentifierIn1C = DataBox.Create(3105, DataBox.StringValue);

    public readonly static RowPropertyBlank<int?> Kind = DataBox.Create(3106, DataBox.IntNullableValue);

    public readonly static RowPropertyBlank<bool> NoPresences = DataBox.Create(3110, DataBox.BoolValue);
    public readonly static RowPropertyBlank<int> FeatureMarkups = DataBox.Create(3111, DataBox.IntValue);
    public readonly static RowPropertyBlank<bool> OutOfStock = DataBox.Create(3120, DataBox.BoolValue);

    public readonly static LinkKindBlank VarietyTypeLink = new LinkKindBlank(3500);
  }

  public class VarietyType
  {
    public const int Variety = 4000;

    public readonly static XmlParentDisplayName<int> ParentId = XmlParentDisplayName<int>.ParentId;
    public readonly static XmlParentDisplayName<string> DisplayName = XmlParentDisplayName<string>.DisplayName;

    public readonly static RowPropertyBlank<string> Annotation = DataBox.Create(4101, DataBox.StringValue);
  }

  public class NewsType
  {
    public const int News = 5000;

    public readonly static XmlDisplayName Title = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> Annotation = DataBox.Create(5101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Text = DataBox.Create(5102, DataBox.StringValue);

    public readonly static RowPropertyBlank<bool> IsLink = DataBox.Create(5110, DataBox.BoolValue);
    public readonly static RowPropertyBlank<string> LinkUrl = DataBox.Create(5115, DataBox.StringValue);

    public readonly static RowPropertyBlank<int> PublisherId = DataBox.Create(5121, DataBox.IntValue);
    public readonly static RowPropertyBlank<string> OriginName = DataBox.Create(5122, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> OriginUrl = DataBox.Create(5123, DataBox.StringValue);
  }

  public class PaymentWayType
  {
    public const int Payment = 6000;

    public readonly static XmlDisplayName DisplayName = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> Tags = DataBox.Create(6100, DataBox.StringValue);
    public readonly static RowPropertyBlank<int> CommissionInPip = DataBox.Create(6110, DataBox.IntValue);
  }

  public class DeliveryWayType
  {
    public const int Delivery = 7000;

    public readonly static XmlDisplayName DisplayName = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> Tags = DataBox.Create(7100, DataBox.StringValue);
    public readonly static RowPropertyBlank<int> Cost = DataBox.Create(7101, DataBox.IntValue);
  }

  public class NoteType
  {
    public const int Article = 8000;

    public readonly static XmlKind Kind = new XmlKind();

    public readonly static RowPropertyBlank<string> Title = DataBox.Create(8101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Content = DataBox.Create(8102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Widget = DataBox.Create(8103, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> UnderContent = DataBox.Create(8104, DataBox.StringValue);
  }

  public class ContactsType
  {
    public const int Contacts = 9000;

    public readonly static XmlKind Kind = new XmlKind();

    public readonly static RowPropertyBlank<string> Brand = DataBox.Create(9101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Address = DataBox.Create(9102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Phones = DataBox.Create(9103, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Email = DataBox.Create(9104, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Header = DataBox.Create(9105, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> About = DataBox.Create(9106, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> SocialNetwork = DataBox.Create(9107, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> MapWidget = DataBox.Create(9108, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Alert = DataBox.Create(9110, DataBox.StringValue);
  }

  public class SEOType
  {
    public const int SEO = 10000;

    public readonly static XmlKind Kind = new XmlKind();

    public readonly static RowPropertyBlank<string> MainTitle = DataBox.Create(10101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> MainDescription = DataBox.Create(10102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> MainKeywords = DataBox.Create(10103, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> FooterSeoText = DataBox.Create(10104, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> SectionTitlePattern = DataBox.Create(10111, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> SectionDescriptionPattern = DataBox.Create(10112, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> GroupTitlePattern = DataBox.Create(10201, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> GroupDescriptionPattern = DataBox.Create(10202, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> ProductTitlePattern = DataBox.Create(10301, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> ProductDescriptionPattern = DataBox.Create(10302, DataBox.StringValue);
  }

  public class LandingType
  {
    public const int Landing = 11000;

    public readonly static XmlDisplayName DisplayName = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> SearchText = DataBox.Create(11101, DataBox.StringValue);

    public readonly static RowPropertyBlank<int?> FabricKind = DataBox.Create(11110, DataBox.IntNullableValue);
    public readonly static RowPropertyBlank<string> SortKind = DataBox.Create(11120, DataBox.StringValue);
  }

  public class RedirectType
  {
    public const int Redirect = 12000;

    public readonly static XmlDisplayName From = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> To = DataBox.Create(12101, DataBox.StringValue);
  }

  public class SEOWidgetType
  {
    public const int Widget = 13000;

    public readonly static XmlDisplayName DisplayName = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> Code = DataBox.Create(13101, DataBox.StringValue);
  }

  public class MetaKindType
  {
		public static string DisplayName(LightObject kind)
		{
			if (kind == null)
				return "";

			string name = kind.Get(SEOProp.Name);
			if (!StringHlp.IsEmpty(name))
				return name;

			return kind.Get(MetaKindType.Identifier);
		}

    public const int FabricKind = 14000;

    public readonly static XmlDisplayName Identifier = new XmlDisplayName();

		public readonly static RowPropertyBlank<string> DesignKind = DataBox.Create(14101, DataBox.StringValue);

		public readonly static RowPropertyBlank<string> WithFeatures = DataBox.Create(14111, DataBox.StringValue);

    public readonly static LinkKindBlank PropertyLinks = new LinkKindBlank(14500);

  }

  public class MetaPropertyType
  {
		public static string DisplayName(LightObject property)
		{
			string name = property.Get(SEOProp.Name);
			if (!StringHlp.IsEmpty(name))
				return name;

			return property.Get(MetaPropertyType.Identifier);
		}

		public static string GetShortName(LightObject property)
		{
			string shortName = property.Get(MetaPropertyType.ShortName);
			if (!StringHlp.IsEmpty(shortName))
				return shortName;

			return DisplayName(property);
		}

		public readonly static string[] AllKinds = new string[] { "", "string", "numerical", "enum" };
    public static string KindToDisplay(string kind)
    {
      switch (kind)
      {
        case "string":
          return "строковый";
        case "numerical":
          return "числовой";
        case "enum":
          return "перечисление";
        default:
          return kind;
      }
    }

    public const int Property = 15000;

		public readonly static XmlDisplayName Identifier = new XmlDisplayName();
    //public readonly static XmlDisplayNameWithMarking Identifier = XmlDisplayNameWithMarking.DisplayName;
    //public readonly static XmlDisplayNameWithMarking Marking = XmlDisplayNameWithMarking.Marking;

		public readonly static RowPropertyBlank<string> Kind = DataBox.Create(15101, DataBox.StringValue);

    public readonly static RowPropertyBlank<bool> IsMultiple = DataBox.Create(15102, DataBox.BoolValue);
    public readonly static RowPropertyBlank<bool> WithIcon = DataBox.Create(15103, DataBox.BoolValue);

    public readonly static RowPropertyBlank<string> EnumItems = DataBox.Create(15104, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> EnumIcons = DataBox.Create(15105, DataBox.StringValue);
		public readonly static RowPropertyBlank<string> EnumIds = DataBox.Create(15120, DataBox.StringValue);

    //public readonly static RowPropertyBlank<string> ViewerKind = DataBox.Create(15106, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> MeasureUnit = DataBox.Create(15107, DataBox.StringValue);

    public readonly static RowPropertyBlank<int?> Category = DataBox.Create(15108, DataBox.IntNullableValue);

    public readonly static RowPropertyBlank<bool> IsPrior = DataBox.Create(15109, DataBox.BoolValue);

    public readonly static RowPropertyBlank<string> Hint = DataBox.Create(15110, DataBox.StringValue);

		public readonly static RowPropertyBlank<string> ShortName = DataBox.Create(15115, DataBox.StringValue);



	}

  public class MetaCategoryType
  {
    public readonly static string[] AllKinds = new string[] { "group" };
    public static string KindToDisplay(string kind)
    {
      switch (kind)
      {
        case "group":
          return "группа";
        default:
          return "";
      }
    }

    public const int PropertyCategory = 16000;

    public readonly static XmlDisplayName DisplayName = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> Kind = DataBox.Create(16101, DataBox.StringValue);
  }

  public class SectionType
  {
    public const int Section = 17000;

    public readonly static XmlDisplayName Title = new XmlDisplayName();

    //public readonly static RowPropertyBlank<bool> IsMenu = DataBox.Create(17100, DataBox.BoolValue);
    public readonly static RowPropertyBlank<string> DesignKind = DataBox.Create(17101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> SortKind = DataBox.Create(17102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Tags = DataBox.Create(17104, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> UnitSortKind = DataBox.Create(17105, DataBox.StringValue);
		public readonly static RowPropertyBlank<bool> HideInMenu = DataBox.Create(17106, DataBox.BoolValue);

    public readonly static RowPropertyBlank<string> NameInMenu = DataBox.Create(17110, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Subtitle = DataBox.Create(17111, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Annotation = DataBox.Create(17120, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Link = DataBox.Create(17121, DataBox.StringValue);

    //public readonly static RowPropertyBlank<string> SortingPrefix = DataBox.Create(17130, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Content = DataBox.Create(17200, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Widget = DataBox.Create(17210, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> UnderContent = DataBox.Create(17220, DataBox.StringValue);

    public readonly static LinkKindBlank SubsectionLinks = new LinkKindBlank(17500);
    public readonly static LinkKindBlank UnitForPaneLinks = new LinkKindBlank(17509);

    [Obsolete]
    public readonly static LinkKindBlank UnitLinks = new LinkKindBlank(17510);
  }

  public class UnitType
  {
    public const int Unit = 18000;

    public readonly static XmlParentDisplayName<int> ParentId = XmlParentDisplayName<int>.ParentId;
    public readonly static XmlParentDisplayName<string> DisplayName = XmlParentDisplayName<string>.DisplayName;

    public readonly static RowPropertyBlank<string> DesignKind = SectionType.DesignKind;

    public readonly static RowPropertyBlank<string> ImageAlt = DataBox.Create(18101, DataBox.StringValue);
		public readonly static RowPropertyBlank<string> AdaptTitle = DataBox.Create(18102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Tags = DataBox.Create(18104, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Annotation = DataBox.Create(18110, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Subtitle = DataBox.Create(18111, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Link = DataBox.Create(18120, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Content = DataBox.Create(18130, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> SortKind = DataBox.Create(18140, DataBox.StringValue);

    public readonly static LinkKindBlank SubunitLinks = SectionType.UnitLinks;
  }

  public class FeatureType
  {
    public const int Feature = 19000;

    public readonly static XmlDisplayName Code = new XmlDisplayName();

    public readonly static RowPropertyBlank<string> Marking = new RowPropertyBlank<string>(19101, DataBox.StringValue);

    public readonly static RowPropertyBlank<bool> WithIcon = new RowPropertyBlank<bool>(19111, DataBox.BoolValue);
    //public readonly static RowPropertyBlank<bool> WithCategory = new RowPropertyBlank<bool>(19112, DataBox.BoolValue);
    public readonly static RowPropertyBlank<bool> WithHint = new RowPropertyBlank<bool>(19113, DataBox.BoolValue);

    public readonly static RowPropertyBlank<string> WithFeatures = new RowPropertyBlank<string>(19221, DataBox.StringValue);

    public readonly static LinkKindBlank FeatureValueLinks = new LinkKindBlank(19500);
  }

  public class FeatureValueType
  {
    public const int FeatureValue = 20000;

    public readonly static XmlParentDisplayName<int> ParentId = XmlParentDisplayName<int>.ParentId;
    public readonly static XmlParentDisplayName<string> DisplayName = XmlParentDisplayName<string>.DisplayName;

    //public readonly static RowPropertyBlank<string> Category = new RowPropertyBlank<string>(20101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Hint = new RowPropertyBlank<string>(20102, DataBox.StringValue);

    public readonly static RowPropertyBlank<int> WithFeatures = new RowPropertyBlank<int>(20111, DataBox.IntValue);
  }

}