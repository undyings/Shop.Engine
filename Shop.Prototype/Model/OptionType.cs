using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Data;

namespace Shop.Prototype
{
  public class OptionType
  {
    public readonly static FieldBlank<int> FilterColorId = new FieldBlank<int>("FilterColorId");
    public readonly static FieldBlank<int> FilterMaterialId = new FieldBlank<int>("FilterMaterialId");

    public readonly static FieldBlank<int> SelectWidthId = new FieldBlank<int>("selectWidth", IntStringConverter.Default);
    public readonly static FieldBlank<int> SelectLengthId = new FieldBlank<int>("selectLength", IntStringConverter.Default);
    //public readonly static FieldBlank<string> SelectSize = new FieldBlank<string>("selectSize");
    public readonly static FieldBlank<int> SelectClothId = new FieldBlank<int>("selectClothId", IntStringConverter.Default);
  }
}