using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Shop.Engine
{
  public class TranslitLinks
  {
    readonly Dictionary<Tuple<int, int?>, string> directoryById = new Dictionary<Tuple<int, int?>, string>();
    readonly Dictionary<string, LinkInfo> linkByDirectory = new Dictionary<string, LinkInfo>();

    public string ToDirectory(int id, int? parentId)
    {
      return DictionaryHlp.GetValueOrDefault(directoryById, _.Tuple(id, parentId));
    }

    public LinkInfo FindLink(string kind, string translitId)
    {
      return FindLink(UrlHlp.ToDirectory(kind, translitId));
    }

    public LinkInfo FindLink(string directory)
    {
      return DictionaryHlp.GetValueOrDefault(linkByDirectory, directory);
    }

    public void AddLink(LinkInfo link)
    {
      linkByDirectory[link.Directory] = link;
    }

		public void AddShopLinks(LightGroup parentGroup, LightGroup group)
		{
			AddShopLinks(parentGroup, group, false, false);
		}

		public void AddShopLinks(LightGroup parentGroup, LightGroup group, 
			bool groupOmitInSitemap, bool productOmitInSitemap)
    {
			//string groupDisplay = GroupType.DisplayName(group);
			//string groupDirectory = string.Format("/catalog/{0}", TranslitString(groupDisplay));
			string groupDirectory = UrlHlp.ShopDirectory(group);

      DateTime groupModifyTime = group.Get(ObjectType.ActTill) ?? FabricHlp.RefTime;
      linkByDirectory[groupDirectory] = new LinkInfo(groupDirectory, "catalog", 
        parentGroup?.Id, group.Id, groupModifyTime, groupOmitInSitemap);
      directoryById[new Tuple<int, int?>(group.Id, null)] = groupDirectory;

      foreach (Product product in group.Products)
      {
        string productDisplay = product.ProductName;
        string productDirectory = string.Format("{0}/{1}", groupDirectory, TranslitString(productDisplay));

        //if (product.ProductId == 67)
        //  Logger.AddMessage("Product: {0}, {1}, {2}", group.Id, productDisplay, productDirectory);

        linkByDirectory[productDirectory] = new LinkInfo(
          productDirectory, "product", group.Id, product.ProductId, product.ModifyTime, productOmitInSitemap
        );
        directoryById[new Tuple<int, int?>(product.ProductId, group.Id)] = productDirectory;
        //hack для корзины товаров и на всякий случай
        directoryById[new Tuple<int, int?>(product.ProductId, null)] = productDirectory;
      }
    }

    public void AddDirectory(string kind, int id, string directory, DateTime? modifyTime)
    {
      if (linkByDirectory.ContainsKey(directory))
      {
        for (int i = 1; i < 1000; ++i)
        {
          string attempt = string.Format("{0}{1}", directory, i);
          if (!linkByDirectory.ContainsKey(attempt))
          {
            directory = attempt;
            break;
          }
        }
      }

      linkByDirectory[directory] = new LinkInfo(directory, kind, null, id, modifyTime, false);
      directoryById[new Tuple<int, int?>(id, null)] = directory;
    }

    public void AddLink(string kind, int id, string displayName, DateTime? modifyTime)
    {
      if (StringHlp.IsEmpty(displayName))
        displayName = "-";

      string directory = UrlHlp.ToDirectory(kind, TranslitString(displayName));

      AddDirectory(kind, id, directory, modifyTime);
    }

    public void AddLink(string kind, DateTime? modifyTime)
    {
      string directory = string.Format("/{0}", kind);
      linkByDirectory[directory] = new LinkInfo(directory, kind, null, null, modifyTime, false);
    }

    public LinkInfo[] All
    {
      get { return _.ToArray(linkByDirectory.Values); }
    }

    public static string TranslitString(string displayName)
    {
      StringBuilder translit = new StringBuilder();
      bool prevDash = false;
      foreach (char ch in (displayName).Trim())
      {
        string trCh = TranslitChar(ch);

        if (trCh == "")
          continue;

        bool dash = trCh == "-";
        if (dash && prevDash)
          continue;

        prevDash = dash;

        translit.Append(trCh);
      }
      return translit.ToString();
    }

    public static string TranslitChar(char ch)
    {
      switch (ch)
      {            
        case 'а':  
        case 'А':
          return "a";
        case 'б':
        case 'Б':
          return "b";
        case 'в':
        case 'В':
          return "v";
        case 'г':
        case 'Г':
          return "g";
        case 'д':
        case 'Д':
          return "d";
        case 'е':
        case 'Е':
        case 'ё':
        case 'Ё':
          return "e";
        case 'ж':
        case 'Ж':
          return "zh";
        case 'з':
        case 'З':
          return "z";
        case 'и':
        case 'И':
          return "i";
        case 'й':
        case 'Й':
          return "j";
        case 'к':
        case 'К':
          return "k";
        case 'л':
        case 'Л':
          return "l";
        case 'м':
        case 'М':
          return "m";
        case 'н':
        case 'Н':
          return "n";
        case 'о':
        case 'О':
          return "o";
        case 'п':
        case 'П':
          return "p";
        case 'р':
        case 'Р':
          return "r";
        case 'с':
        case 'С':
          return "s";
        case 'т':
        case 'Т':
          return "t";
        case 'у':
        case 'У':
          return "u";
        case 'ф':
        case 'Ф':
          return "f";
        case 'х':
        case 'Х':
          return "h";
        case 'ц':
        case 'Ц':
          return "ts";
        case 'ч':
        case 'Ч':
          return "ch";
        case 'ш':
        case 'Ш':
          return "sh";
        case 'щ':
        case 'Щ':
          return "sch";
        case 'ъ':
        case 'Ъ':
          return "";
        case 'ы':
        case 'Ы':
          return "y";
        case 'ь':
        case 'Ь':
          return "";
        case 'э':
        case 'Э':
          return "e";
        case 'ю':
        case 'Ю':
          return "yu";
        case 'я':
        case 'Я':
          return "ya";
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
        case 'a':
        case 'A':
        case 'b':
        case 'B':
        case 'c':
        case 'C':
        case 'd':
        case 'D':
        case 'e':
        case 'E':
        case 'f':
        case 'F':
        case 'g':
        case 'G':
        case 'h':
        case 'H':
        case 'i':
        case 'I':
        case 'j':
        case 'J':
        case 'k':
        case 'K':
        case 'l':
        case 'L':
        case 'm':
        case 'M':
        case 'n':
        case 'N':
        case 'o':
        case 'O':
        case 'p':
        case 'P':
        case 'q':
        case 'Q':
        case 'r':
        case 'R':
        case 's':
        case 'S':
        case 't':
        case 'T':
        case 'u':
        case 'U':
        case 'v':
        case 'V':
        case 'w':
        case 'W':
        case 'x':
        case 'X':
        case 'y':
        case 'Y':
        case 'z':
        case 'Z':
        case '_':
        case '-':
          return new string(new char[] { ch }).ToLower();

        case ' ':
          return "-";
      
        default:
          return "";
      }
    }
  }

  public class LinkInfo
  {
    public readonly string Directory;
    public readonly string Kind;
    public readonly int? ParentId;
    public readonly int? Id;
    public readonly DateTime? ModifyTime;
    public readonly bool OmitInSitemap;

    public LinkInfo(string directory, string kind, int? parentId, int? id, 
      DateTime? modifyTime, bool omitInSitemap)
    {
      this.Directory = directory;
      this.Kind = kind;
      this.ParentId = parentId;
      this.Id = id;
      this.ModifyTime = modifyTime;
      this.OmitInSitemap = omitInSitemap;
    }

    public LinkInfo(string directory, string kind, int? id) :
      this (directory, kind, null, id, null, false)
    {
    }
  }

  public class LightLink
  {
    public readonly string Directory;
    public readonly DateTime? ModifyTime;

    public LightLink(string directory, DateTime? modifyTime)
    {
      this.Directory = directory;
      this.ModifyTime = modifyTime;
    }
  }
}
