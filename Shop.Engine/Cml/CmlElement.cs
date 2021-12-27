using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Basis;

namespace Shop.Engine.Cml
{
  public interface ICmlElement
  {
    string Name { get; }
    void ToXmlText(StringBuilder builder, int elementLevel);
  }

  public class CmlElement : ICmlElement
  {
    readonly string name;
    public string Name
    {
      get { return name; }
    }

    public readonly ICmlElement[] Elements;

    public void ToXmlText(StringBuilder builder, int elementLevel)
    {
			for (int i = 0; i < elementLevel; ++i)
				builder.Append("\t");

      if (Elements.Length == 0)
      {
        CmlWriterHlp.AddEmptyElement(builder, name);
      }
      else
      {
        CmlWriterHlp.AddStartElement(builder, name);
				builder.AppendLine();
        foreach (ICmlElement element in Elements)
        {
          element.ToXmlText(builder, elementLevel + 1);
        }
				for (int i = 0; i < elementLevel; ++i)
					builder.Append("\t");
				CmlWriterHlp.AddEndElement(builder, name);
      }
      builder.AppendLine();
    }

    public CmlElement(string name, params ICmlElement[] elements)
    {
      this.name = name;
      this.Elements = elements;
    }
  }

  public class CmlText : ICmlElement
  {
    readonly string name;
    public string Name
    {
      get { return name; }
    }

    public readonly string Text;

    public void ToXmlText(StringBuilder builder, int elementLevel)
    {
			for (int i = 0; i < elementLevel; ++i)
				builder.Append("\t");

			if (StringHlp.IsEmpty(Text))
        CmlWriterHlp.AddEmptyElement(builder, name);
      else
      {
        CmlWriterHlp.AddStartElement(builder, name);
        builder.Append(Text);
        CmlWriterHlp.AddEndElement(builder, name);
      }
      builder.AppendLine();
    }

    public CmlText(string name, string text)
    {
      this.name = name;
      this.Text = text;
    }

    public CmlText(string name, object value) :
      this(name, value?.ToString())
    {
    }
  }
}
