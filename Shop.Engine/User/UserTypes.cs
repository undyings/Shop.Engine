using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class UserType
  {
    public const int User = 1000;

    public readonly static XmlLogin Auth = XmlLogin.Auth;
    public readonly static XmlLogin Login = XmlLogin.Login;

    public readonly static RowPropertyBlank<string> Email = DataBox.Create(1100, DataBox.StringValue);

    public readonly static RowPropertyBlank<string> Family = DataBox.Create(1101, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> FirstName = DataBox.Create(1102, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Password = DataBox.Create(1103, DataBox.StringValue);

    public readonly static RowPropertyBlank<bool> NotConfirmed = DataBox.Create(1110, DataBox.BoolValue);

    //public readonly static RowPropertyBlank<bool> IsFemale = DataBox.Create(1110, DataBox.BoolValue);
  }


  public class ReviewType
  {
    public const int Review = 2000;

    public readonly static XmlDisplayName ClientName = new XmlDisplayName();

    public readonly static RowPropertyBlank<int> Evaluation = new RowPropertyBlank<int>(2101, DataBox.IntValue);
    public readonly static RowPropertyBlank<string> Text = new RowPropertyBlank<string>(2111, DataBox.StringValue);
    public readonly static RowPropertyBlank<string> Answer = new RowPropertyBlank<string>(2121, DataBox.StringValue);
  }
}
