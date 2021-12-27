using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class Product : LightKin
  {
    public readonly LightObject Variety;
    public Product(KinBox fabricBox, int fabricId, LightObject variety) :
      base(fabricBox, fabricId)
    {
      this.Variety = variety;
    }

    //public int? ParentGroupId
    //{
    //  get
    //  {
    //    return GetParentId(GroupType.FabricTypeLink);
    //  }
    //}

    public int ProductId
    {
      get
      {
        if (Variety != null)
          return Variety.Id;
        return Id;
      }
    }

    public int ImageId
    {
      get
      {
        if (Variety != null)
          return Variety.Id;
        return Id;
      }
    }

    public string ProductName
    {
      get
      {
				string name = FabricType.DisplayName(this);
				if (Variety != null)
        {
          return string.Format("{0} - {1}",
            name, VarietyName);
        }
				return name;
      }
    }

    public int? VarietyId
    {
      get
      {
        if (Variety != null)
          return Variety.Id;
        return null;
      }
    }

    public string VarietyName
    {
      get
      {
        if (Variety != null)
          return Variety.Get(VarietyType.DisplayName);
        return "";
      }
    }

    public string Annotation
    {
      get
      {
        if (Variety != null)
        {
          string varietyAnnotation = Variety.Get(VarietyType.Annotation);
          if (!StringHlp.IsEmpty(varietyAnnotation))
            return varietyAnnotation;
        }
        return this.Get(FabricType.Annotation);
      }
    }

    public DateTime ModifyTime
    {
      get
      {
        DateTime fabricTime = this.Get(ObjectType.ActTill) ?? FabricHlp.RefTime;
        if (Variety != null)
        {
          DateTime varietyTime = this.Get(ObjectType.ActTill) ?? FabricHlp.RefTime;
          if (varietyTime > fabricTime)
            return varietyTime;
        }

        return fabricTime;
      }
    }
  }
}
