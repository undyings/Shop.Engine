using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;


namespace Shop.Engine
{
  public class WidgetStorage
  {
    public static WidgetStorage Load(IDataLayer fabricConnection, bool disableScripts)
    {
      ObjectBox widgetBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(SEOWidgetType.Widget)
      );

      return new WidgetStorage(widgetBox, disableScripts);
    }

    public readonly ObjectBox widgetBox;

    public readonly string WidgetsCode = "";

    public WidgetStorage(ObjectBox widgetBox, bool disableScripts)
    {
      this.widgetBox = widgetBox;

      if (!disableScripts)
      {
        this.WidgetsCode = StringHlp.Join(Environment.NewLine, widgetBox.AllObjectIds,
          delegate (int widgetId)
          {
            LightObject widget = new LightObject(widgetBox, widgetId);
            return widget.Get(SEOWidgetType.Code);
          }
        ) + Environment.NewLine;
      }
    }
  }
}
