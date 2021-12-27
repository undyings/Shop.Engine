using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  //public class XmlDisplayName : XmlUniqueName
  //{
  //  public XmlDisplayName() :
  //    base(new XmlFieldBlank(ObjectType.XmlObjectIds, "DisplayName"), "DisplayName")
  //  {
  //  }

  //  public string CreateXmlIds(string displayName)
  //  {
  //    return xmlField.Create(displayName);
  //  }
  //}

  public class XmlKind : XmlUniqueName
  {
    public XmlKind() :
      base(new XmlFieldBlank(ObjectType.XmlObjectIds, "Kind"), "Kind")
    {
    }

    public string CreateXmlIds(string kind)
    {
      return xmlField.Create(kind);
    }
  }

  public class XmlParentDisplayName<T> : XmlUniqueProperty<T>
  {
    readonly static XmlFieldBlank xmlIds = new XmlFieldBlank(
      ObjectType.XmlObjectIds, "ParentId", "DisplayName");

    public readonly static XmlParentDisplayName<int> ParentId = new XmlParentDisplayName<int>(
      "ParentId", delegate (string xmlParentId) { return int.Parse(xmlParentId); });
    public readonly static XmlParentDisplayName<string> DisplayName = 
      new XmlParentDisplayName<string>("DisplayName", null);

    public XmlParentDisplayName(string propertyKind, Getter<T, string> propertyConverter) :
      base(xmlIds, propertyKind, propertyConverter)
    {
    }

    public string CreateXmlIds(int parentId, string displayName)
    {
      return xmlIds.Create(parentId.ToString(), displayName);
    }
  }

  public class XmlDisplayNameWithMarking : XmlUniqueProperty<string>
  {
    public readonly static XmlFieldBlank xmlIds = new XmlFieldBlank(
      ObjectType.XmlObjectIds, "DisplayName", "Marking");

    public readonly static XmlDisplayNameWithMarking DisplayName =
      new XmlDisplayNameWithMarking("DisplayName");

    public readonly static XmlDisplayNameWithMarking Marking =
      new XmlDisplayNameWithMarking("Marking");

    public XmlDisplayNameWithMarking(string propertyKind) :
      base(xmlIds, propertyKind, null)
    {
    }

    public string CreateXmlIds(string displayName, string marking)
    {
      return xmlIds.Create(displayName, marking);
    }
  }

}
