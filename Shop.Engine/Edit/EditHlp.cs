using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using System.IO;
using Commune.Html;
using NitroBolt.Wui;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class EditHlp
  {
    static IStore store
    {
      get
      {
        return SiteContext.Default.Store;
      }
    }

    static ShopStorage shop
    {
      get { return ((IShopStore)store).Shop; }
    }

    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    public static IHtmlControl GetSortingPaymentWays(EditState state, out string title)
    {
      title = "Сортировка способов оплаты";

      LightObject[] allPayments = ((IShopStore)store).Payments.All;

      return SorterHlp.GetAlhabetSortingEdit(state, title, allPayments, "/order");
    }

    public static IHtmlControl GetSortingDeliveryWays(EditState state, out string title)
    {
      title = "Сортировка способов доставки";

      LightObject[] allDeliveries = ((IShopStore)store).Deliveries.All;

      return SorterHlp.GetAlhabetSortingEdit(state, title, allDeliveries, "/order");
    }

    public static IHtmlControl GetSortingGroupEdit(EditState state, int? parentGroupId, out string title)
    {
      title = "Сортировка групп";

      LightGroup parentGroup = shop.FindGroup(parentGroupId);
      if (parentGroupId != null && parentGroup == null)
        return GetInfoMessage("Неверный аргумент", "/");

      LightKin[] subgroups = parentGroup != null ? parentGroup.Subgroups : shop.RootGroups;

      string returnUrl = parentGroupId != null ? UrlHlp.ShopUrl("group", parentGroupId) : "/";
      return SorterHlp.GetAlhabetSortingEdit(state, title, subgroups, returnUrl);
    }

    public static IHtmlControl GetSortingFabricEdit(EditState state, int? parentGroupId, out string title)
    {
      title = "Сортировка товаров";

      LightGroup parentGroup = shop.FindGroup(parentGroupId);
      if (parentGroup == null)
        return GetInfoMessage("Неверный аргумент", "/");

      //int[] editFabricIds = parentGroup.AllChildIds(GroupType.FabricTypeLink);
      //List<LightKin> fabrics = new List<LightKin>();
      //foreach (int fabricId in editFabricIds)
      //{
      //  LightKin fabric = shop.FindFabric(fabricId);
      //  if (fabric != null)
      //    fabrics.Add(fabric);
      //}

      string returnUrl = UrlHlp.ShopUrl("group", parentGroupId);

      return SorterHlp.GetAlhabetSortingEdit(state, title, parentGroup.Products, returnUrl);
    }

    public static IHtmlControl GetSortingSectionEdit(EditState state, int? parentSectionId, out string title)
    {
      title = "Сортировка разделов";

      LightSection parentSection = store.Sections.FindSection(parentSectionId);
      if (parentSection == null)
        return GetInfoMessage("Неверный аргумент", "/");

      string sortKind = parentSection.Get(SectionType.SortKind);
      string returnUrl = parentSection.IsMenu ? "/" : UrlHlp.ShopUrl("page", parentSectionId);

      return SorterHlp.GetSortingEdit(state, title, sortKind, parentSection.Subsections, returnUrl);
    }

    [Obsolete]
    public static IHtmlControl GetSortingUnitEdit(EditState state, int? parentSectionId, out string title)
    {
      title = "Сортировка элементов";

      LightSection parentSection = store.Sections.FindSection(parentSectionId);
      if (parentSection == null)
        return GetInfoMessage("Неверный аргумент", "/");

      string unitSortKind = parentSection.Get(SectionType.UnitSortKind);
      string returnUrl = parentSection.IsMenu ? "/" : UrlHlp.ShopUrl("page", parentSectionId);

      return SorterHlp.GetSortingEdit(state, title, unitSortKind, parentSection.Units, returnUrl);
    }

    public static IHtmlControl GetSortingSubunitEdit(EditState state, int? parentUnitId, out string title)
    {
      title = "Сортировка элементов";

      LightUnit parentUnit = store.Sections.FindUnit(parentUnitId);
      if (parentUnit == null)
        return GetInfoMessage("Неверный аргумент", "/");

      LightSection parentSection = FabricHlp.ParentSectionForUnit(store.Sections, parentUnit);

      string returnUrl = UrlHlp.ReturnUnitUrl(parentSection, parentUnit.Id);
      string sortKind = parentUnit.Get(UnitType.SortKind);

      return SorterHlp.GetSortingEdit(state, title, sortKind, parentUnit.Subunits, returnUrl);
    }

    public static IHtmlControl GetUnitEdit(
      EditState state, int? parentId, int? unitId, string fixedDesignKind, out string title)
    {
      EditorSelector selector = SiteContext.Default.UnitEditorSelector;
      SectionStorage sections = SiteContext.Default.Store.Sections;

      title = "Редактирование элемента раздела";

      if (unitId == null)
        unitId = state.CreatingObjectId;

      if (unitId == null)
      {
        LightKin parent = sections.FindUnit(parentId);
        if (parent == null)
          parent = sections.FindSection(parentId);

        if (parent == null)
          return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");

        title = "Добавление раздела";
        return UnitEditorHlp.GetUnitAdd(selector, state, title, parent, fixedDesignKind);
      }

      LightKin unit = sections.FindUnit(unitId);
      if (unit == null)
        return EditHlp.GetInfoMessage("Элемент раздела не существует", "/");

      string designKind = unit.Get(SectionType.DesignKind);

			BaseTunes tunes = selector.FindTunes(designKind);
			if (tunes == null)
				return UnitEditorHlp.GetTextEdit(state, unit);

			return UnitEditorHlp.GetEditor(state, unit, tunes);

      //Getter<IHtmlControl, EditState, LightKin> editor = selector.FindEditor(designKind);
      //if (editor == null)
      //  editor = new Getter<IHtmlControl, EditState, LightKin>(UnitEditorHlp.GetTextEdit);

      //return editor(state, unit);
    }

    public static IHtmlControl GetSectionEdit(
      EditState state, int? parentId, int? sectionId, string fixedDesignKind, out string title)
    {
      EditorSelector selector = SiteContext.Default.SectionEditorSelector;
      SectionStorage sections = SiteContext.Default.Store.Sections;

      title = "Редактирование раздела";

      if (sectionId == null)
        sectionId = state.CreatingObjectId;

      if (sectionId == null)
      {
        LightSection parent = sections.FindSection(parentId);
        if (parent == null)
          return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");

        title = "Добавление раздела";
        return SectionEditorHlp.GetSectionAdd(selector, state, title, parent, fixedDesignKind);
      }

      LightSection section = sections.FindSection(sectionId);
      if (section == null)
        return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");

      string designKind = section.Get(SectionType.DesignKind);

			BaseTunes tunes = selector.FindTunes(designKind);
			if (tunes == null)
				return SectionEditorHlp.GetTextEdit(state, section);

			return SectionEditorHlp.GetEditor(state, section, tunes);

			//Getter<IHtmlControl, EditState, LightKin> editor = selector.FindEditor(designKind);
   //   if (editor == null)
   //     editor = new Getter<IHtmlControl, EditState, LightKin>(SectionEditorHlp.GetTextEdit);

   //   return editor(state, section);
    }

    public static IHtmlControl GetDeliveryWayEdit(EditState state, int? deliveryId, out string title)
    {
      title = "Редактирование способа доставки";

      if (deliveryId == null)
        deliveryId = state.CreatingObjectId;

      string returnUrl = UrlHlp.ShopUrl("order");

      if (deliveryId == null)
      {
        title = "Добавление способа доставки";
        return EditPanelHlp.GetObjectAdd(state, title, "Способ доставки", returnUrl, 
          DeliveryWayType.Delivery, DeliveryWayType.DisplayName);
        //return GetDeliveryWayAdd(state, out title);
      }

      ObjectStorage deliveries = ((IShopStore)store).Deliveries;

      LightObject delivery = deliveries.Find(deliveryId.Value);
      if (delivery == null)
        return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Способ доставки", "deliveryName", delivery.Get(DeliveryWayType.DisplayName)),
          DecorEdit.Field("Стоимость доставки", "deliveryCost", delivery.Get(DeliveryWayType.Cost).ToString()),
          DecorEdit.Field("Признак 1", "deliveryTag1", delivery.Get(DeliveryWayType.Tags, 0)),
          DecorEdit.Field("Признак 2", "deliveryTag2", delivery.Get(DeliveryWayType.Tags, 1)),
          EditElementHlp.GetDeletePanel(state, delivery.Id, 
            "способ доставки", "Удаление способа доставки", null
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton().CKEditorOnUpdateAll()
          .Event("save_delivery", "editContent",
            delegate (JsonData json)
            {
              string deliveryName = json.GetText("deliveryName");
              if (StringHlp.IsEmpty(deliveryName))
              {
                state.Operation.Message = "Не задано наименование способа доставки";
                return;
              }

              string costText = json.GetText("deliveryCost");
              int? deliveryCost = ConvertHlp.ToInt(costText);
              if (!StringHlp.IsEmpty(costText) && deliveryCost == null)
              {
                state.Operation.Message = "Цена доставки должна быть целым положительным числом";
                return;
              }

              LightObject editDelivery = DataBox.LoadObject(fabricConnection,
                DeliveryWayType.Delivery, delivery.Id);
              if (!DeliveryWayType.DisplayName.SetWithCheck(editDelivery.Box, delivery.Id, deliveryName))
              {
                state.Operation.Message = "Способ доставки с таким наименованием уже существует";
                return;
              }

              string tag1 = json.GetText("deliveryTag1");
              string tag2 = json.GetText("deliveryTag2");

              editDelivery.Set(DeliveryWayType.Cost, deliveryCost ?? 0);
              editDelivery.Set(DeliveryWayType.Tags, 0, tag1);
              editDelivery.Set(DeliveryWayType.Tags, 1, tag2);

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editDelivery.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");

    }

    public static IHtmlControl GetPaymentWayEdit(EditState state, int? paymentId, out string title)
    {
      title = "Редактирование способа оплаты";

      if (paymentId == null)
        paymentId = state.CreatingObjectId;

      string returnUrl = UrlHlp.ShopUrl("order");

      if (paymentId == null)
      {
        title = "Добавление способа оплаты";
        return EditPanelHlp.GetObjectAdd(state, title, "Способ оплаты", returnUrl,
          PaymentWayType.Payment, PaymentWayType.DisplayName);
        //return GetPaymentWayAdd(state, out title);
      }

      ObjectStorage payments = ((IShopStore)store).Payments;

      LightObject payment = payments.Find(paymentId.Value);
      if (payment == null)
        return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Способ оплаты", "paymentName", payment.Get(PaymentWayType.DisplayName)),
          DecorEdit.Field("Признак", "paymentTag", payment.Get(PaymentWayType.Tags, 0)),
          DecorEdit.Field("Комиссия, %/100", "paymentCommission", payment.Get(PaymentWayType.CommissionInPip).ToString()),
          EditElementHlp.GetDeletePanel(state, payment.Id, 
            "способ оплаты", "Удаление способа оплаты", null
          )
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton().CKEditorOnUpdateAll()
          .Event("save_payment", "editContent",
            delegate (JsonData json)
            {
              string paymentName = json.GetText("paymentName");
              if (StringHlp.IsEmpty(paymentName))
              {
                state.Operation.Message = "Не задано наименование способа оплаты";
                return;
              }

              LightObject editPayment = DataBox.LoadObject(fabricConnection, 
                PaymentWayType.Payment, payment.Id);
              if (!PaymentWayType.DisplayName.SetWithCheck(editPayment.Box, payment.Id, paymentName))
              {
                state.Operation.Message = "Способ оплаты с таким наименованием уже существует";
                return;
              }

              int? commissionInPip = json.GetInt("paymentCommission");
              editPayment.Set(PaymentWayType.CommissionInPip, commissionInPip ?? 0);

              string tag = json.GetText("paymentTag");
              editPayment.Set(PaymentWayType.Tags, tag);

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editPayment.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");

    }

    public static IHtmlControl GetNewsEdit(
      EditState state, int? newsId, out string title)
    {
      title = "Редактирование новости";

      if (newsId == null)
        newsId = state.CreatingObjectId;

      string returnUrl = UrlHlp.ShopUrl(Site.Novosti);

      if (newsId == null)
      {
        title = "Добавление новости";
        return EditPanelHlp.GetObjectAdd(state, title, "Заголовок новости", returnUrl,
          delegate(string newsTitle)
          {
            if (HttpContext.Current.IsInRole("nosave"))
            {
              state.Operation.Message = "Нет прав на сохранение изменений";
              return null;
            }

            ObjectBox box = new ObjectBox(fabricConnection, "1=0");
            int createNewsId = box.CreateObject(NewsType.News,
              NewsType.Title.CreateXmlIds(newsTitle), DateTime.UtcNow);
            box.Update();
            return new LightObject(box, createNewsId);
          }
        );
        //return GetNewsAdd(state, out title);
      }

      LightObject news = DataBox.LoadObject(fabricConnection, NewsType.News, newsId.Value);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          EditElementHlp.GetPropertiesTable(newsId.Value, null,
            EditElementHlp.GetDeletePanel(state, news.Id,
              "новость", "Удаление новости", null
            ),
            DecorEdit.FieldArea("Заголовок",
              new HTextEdit("newsTitle", news.Get(NewsType.Title))
            ),
            DecorEdit.FieldArea("Аннотация", 
              new HTextArea("newsAnnotation", news.Get(NewsType.Annotation)).Height("4em")
            ),
            DecorEdit.FieldArea("Дата",
              new HTextEdit("newsData", news.Get(ObjectType.ActFrom)?.ToLocalTime().ToShortDateString())
            )
          ),
          DecorEdit.FieldInputBlock("Описание новости",
            HtmlHlp.CKEditorCreate("newsText", news.Get(NewsType.Text),
              "400px", true)
          ),
          EditElementHlp.GetDescriptionImagesPanel(state.Option, newsId.Value)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton().CKEditorOnUpdateAll()
          .Event("save_news", "editContent",
            delegate (JsonData json)
            {
              string newsTitle = json.GetText("newsTitle");
              if (StringHlp.IsEmpty(newsTitle))
              {
                state.Operation.Message = "Не задан заголовок новости";
                return;
              }

              string newsAnnotation = json.GetText("newsAnnotation");
              string newsText = json.GetText("newsText");

              LightObject editNews = DataBox.LoadObject(fabricConnection, NewsType.News, newsId.Value);

              if (!NewsType.Title.SetWithCheck(editNews.Box, editNews.Id, newsTitle))
              {
                state.Operation.Message = "Новость с таким наименованием уже существует";
                return;
              }

              editNews.Set(NewsType.Annotation, newsAnnotation);
              editNews.Set(NewsType.Text, newsText);

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editNews.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(UrlHlp.ShopUrl(Site.Novosti, news.Id))
        )
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetShopCatalogEdit(EditState state, out string title)
    {
      title = "Справочник магазина";

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          new HPanel(
            DecorEdit.RedoButton("Виды товаров", UrlHlp.EditUrl("kind-list", null))
          ).MarginBottom(5),
          new HPanel(
            DecorEdit.RedoButton("Свойства товаров", UrlHlp.EditUrl("property-list", null))
          ).MarginBottom(5),
          new HPanel(
            DecorEdit.RedoButton("Категории свойств", UrlHlp.EditUrl("category-list", null))
          ).MarginBottom(5),
          new HPanel(
            DecorEdit.RedoButton("Особенности товаров", UrlHlp.EditUrl("feature-list", null))
          ).MarginBottom(5)
        ).MarginLeft(10).PaddingBottom(10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton("/")
        )
      );
    }

    public static IHtmlControl GetContactsColumnEdit(EditState state, out string title)
    {
      title = "Редактирование информации о предприятии";

      LightObject contacts = store.Contacts;

      string returnUrl = "/";

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          DecorEdit.Field("Наименование предприятия", "brand", contacts.Get(ContactsType.Brand)),
          DecorEdit.Field("Адрес предприятия", "address", contacts.Get(ContactsType.Address)),
          new HPanel(
            new HLabel("Телефоны предприятия").RelativeWidth(30).FontBold(),
            new HTextEdit("phone1", contacts.Get(ContactsType.Phones, 0)).RelativeWidth(34),
            new HPanel().RelativeWidth(2),
            new HTextEdit("phone2", contacts.Get(ContactsType.Phones, 1)).RelativeWidth(34)
          ).Padding(5, 0),
          new HPanel(
            new HLabel("Социальные сети").RelativeWidth(30).FontBold(),
            new HTextEdit("social1", contacts.Get(ContactsType.SocialNetwork, 0)).RelativeWidth(34),
            new HPanel().RelativeWidth(2),
            new HTextEdit("social2", contacts.Get(ContactsType.SocialNetwork, 1)).RelativeWidth(34)
          ).Padding(5, 0),
          DecorEdit.Field("Электронная почта", "email", contacts.Get(ContactsType.Email)),
          DecorEdit.Field("О нас", "header", contacts.Get(ContactsType.Header)),
          DecorEdit.FieldArea("Оповещение",
            new HTextArea("alert", contacts.Get(ContactsType.Alert)).Height("6em")
          ).MarginTop(10),
          DecorEdit.FieldArea("О предприятии",
            new HTextArea("about", contacts.Get(ContactsType.About)).Height("3em")
          ).MarginTop(10),
          DecorEdit.FieldArea("Виджет карты для контактов",
            new HTextArea("mapWidget", contacts.Get(ContactsType.MapWidget)).Height(240)
          ).MarginTop(10)
          //DecorEdit.FieldInputBlock("О предприятии",
          //  HtmlHlp.CKEditorCreate("about", contacts.Get(ContactsType.About),
          //    "240px", true)
          //).MarginTop(10)
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_article", "editContent",
            delegate (JsonData json)
            {
              string brand = json.GetText("brand");
              string address = json.GetText("address");
              string phone1 = json.GetText("phone1");
              string phone2 = json.GetText("phone2");
              string social1 = json.GetText("social1");
              string social2 = json.GetText("social2");
              string email = json.GetText("email");
              string header = json.GetText("header");
              string alert = json.GetText("alert");
              string about = json.GetText("about");
              string mapWidget = json.GetText("mapWidget");

              LightObject editContacts = DataBox.LoadObject(fabricConnection, 
                ContactsType.Contacts, ContactsType.Kind.CreateXmlIds("main"));

              editContacts.Set(ContactsType.Brand, brand);
              editContacts.Set(ContactsType.Address, address);
              editContacts.Set(ContactsType.Phones, 0, phone1);
              editContacts.Set(ContactsType.Phones, 1, phone2);
              editContacts.Set(ContactsType.SocialNetwork, 0, social1);
              editContacts.Set(ContactsType.SocialNetwork, 1, social2);
              editContacts.Set(ContactsType.Email, email);
              editContacts.Set(ContactsType.Header, header);
              editContacts.Set(ContactsType.Alert, alert);
              editContacts.Set(ContactsType.About, about);
              editContacts.Set(ContactsType.MapWidget, mapWidget);

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editContacts.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");

    }

    //public static IHtmlControl GetContactsViewEdit(HttpContext httpContext,
    //  ShopEditState state, out string pageTitle)
    //{
    //  pageTitle = "Редактирование страницы контактов";

    //  string articleKind = "kontakty";

    //  LightObject article = store.FindNote(articleKind);

    //  string returnUrl = UrlHlp.ShopUrl(articleKind);

    //  return new HPanel(
    //    DecorEdit.Title(pageTitle),
    //    new HPanel(
    //      DecorEdit.Field("Заголовок страницы", "title", article.Get(NoteType.Title))
    //        .MarginLeft(5).MarginBottom(20),
    //      DecorEdit.FieldInputBlock("Текст выводимый над картой",
    //        HtmlHlp.CKEditorCreate("content", article.Get(NoteType.Content),
    //          "200px", true)
    //      ),
    //      EditElementHlp.GetDescriptionImagesPanel(httpContext, article.Id),
    //      DecorEdit.FieldInputBlock("Виджет карты 2ГИС",
    //        new HPanel(
    //          new HTextView(@"Если адрес предприятия изменился, то перейдите <a href='http://api.2gis.ru/widgets/firmsonmap/' target='blank'>по ссылке</a>, получите виджет карты для нового адреса и вставьте его в поле ниже.")
    //            .Padding(10).BorderLeft(DecorEdit.blockBorder).BorderRight(DecorEdit.blockBorder),
    //          new HTextArea("script", article.Get(NoteType.Widget))
    //            .Width("100%").Height(220).Padding(5)
    //        )
    //      ),
    //      DecorEdit.FieldInputBlock("Текст выводимый под картой",
    //        HtmlHlp.CKEditorCreate("underContent", article.Get(NoteType.UnderContent),
    //          "400px", true)
    //      )
    //    ).Margin(0, 10),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.SaveButton()
    //      .CKEditorOnUpdateAll()
    //      .Event("save_article", "editContent",
    //        delegate (JsonData json)
    //        {
    //          string title = json.GetText("title");
    //          if (StringHlp.IsEmpty(title))
    //          {
    //            state.Operation.Message = "Не задан заголовок";
    //            return;
    //          }

    //          string content = json.GetText("content");
    //          string script = json.GetText("script");
    //          string underContent = json.GetText("underContent");

    //          LightObject editArticle = DataBox.LoadObject(fabricConnection, NoteType.Article, article.Id);

    //          editArticle.Set(NoteType.Title, title);
    //          editArticle.Set(NoteType.Content, content);
    //          editArticle.Set(NoteType.Widget, script);
    //          editArticle.Set(NoteType.UnderContent, underContent);

    //          editArticle.Box.Update();

    //          ShopContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("editContent");

    //}

    //public static IHtmlControl GetArticleEdit(HttpContext httpContext,
    //  ShopEditState state, string articleKind, out string pageTitle)
    //{
    //  pageTitle = "Редактирование статьи";

    //  if (StringHlp.IsEmpty(articleKind))
    //    return GetInfoMessage("Неверный аргумент", "/");

    //  LightObject article = store.FindNote(articleKind);

    //  string returnUrl = UrlHlp.ShopUrl(articleKind, null);

    //  return new HPanel(
    //    DecorEdit.Title(pageTitle),
    //    new HPanel(
    //      DecorEdit.Field("Заголовок статьи", "title", article.Get(NoteType.Title))
    //        .MarginLeft(5).MarginBottom(20),
    //      DecorEdit.FieldInputBlock("Текст статьи",
    //        HtmlHlp.CKEditorCreate("content", article.Get(NoteType.Content),
    //          "400px", true)
    //      ),
    //      EditElementHlp.GetDescriptionImagesPanel(httpContext, article.Id)
    //    ).Margin(0, 10),
    //    EditElementHlp.GetButtonsPanel(
    //      DecorEdit.SaveButton()
    //      .CKEditorOnUpdateAll()
    //      .Event("save_article", "editContent",
    //        delegate (JsonData json)
    //        {
    //          string title = json.GetText("title");
    //          if (StringHlp.IsEmpty(title))
    //          {
    //            state.Operation.Message = "Не задан заголовок";
    //            return;
    //          }

    //          string content = json.GetText("content");

    //          LightObject editArticle = DataBox.LoadObject(fabricConnection, NoteType.Article, article.Id);

    //          editArticle.Set(NoteType.Title, title);
    //          editArticle.Set(NoteType.Content, content);

    //          editArticle.Box.Update();

    //          ShopContext.Default.UpdateStore();
    //        }
    //      ),
    //      DecorEdit.ReturnButton(returnUrl)
    //    )
    //  ).EditContainer("editContent");
    //}

    public static IHtmlControl GetVarietyEdit(
      EditState state, int? fabricId, int? varietyId, out string title)
    {
      title = "Редактирование разновидности товара";

      if (varietyId == null)
        varietyId = state.CreatingObjectId;

      if (fabricId == null)
      {
        return EditHlp.GetInfoMessage("Неверный формат запроса", "/");
      }

      LightKin fabric = shop.FindFabric(fabricId.Value);
      if (fabric == null)
        return EditHlp.GetInfoMessage("Не найден товар", "/");

      int? groupId = fabric.GetParentId(GroupType.FabricTypeLink);
      if (groupId == null)
        return EditHlp.GetInfoMessage("Не найдена группа товара", "/");

      LightObject variety = null;
      if (varietyId != null)
      {
        variety = shop.FindVariety(varietyId.Value);
        if (variety == null)
          return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");
      }

      string returnUrl = UrlHlp.EditUrl(groupId, "fabric", fabricId);

      if (varietyId == null)
      {
        title = "Добавление разновидности товара";
        return EditPanelHlp.GetObjectAdd(state, title, "Наименование", returnUrl,
          delegate(string varietyName)
          {
            KinBox box = new KinBox(fabricConnection, "1=0");
            int? createVarietyId = box.CreateUniqueObject(VarietyType.Variety,
              VarietyType.DisplayName.CreateXmlIds(fabric.Id, varietyName), null);
            if (createVarietyId == null)
            {
              state.Operation.Message = "Разновидность с таким наименованием уже существует";
              return null;
            }
            LightKin addVariety = new LightKin(box, createVarietyId.Value);

            addVariety.AddParentId(FabricType.VarietyTypeLink, fabric.Id);

            addVariety.Box.Update();

            return addVariety;
          }
        );
        //return GetVarietyAdd(state, groupId.Value, fabricId.Value, out title);
      }

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          EditElementHlp.GetPropertiesTable(varietyId.Value, null,
            EditElementHlp.GetDeletePanel(state, varietyId.Value, 
              "разновидность", "Удаление разновидности", null
            ),
            DecorEdit.Field("Наименование", "varietyName", variety.Get(VarietyType.DisplayName)),
            DecorEdit.FieldArea("Аннотация",
              new HTextArea("varietyAnnotation", variety.Get(VarietyType.Annotation)).Height("4em")
            )
          )
        ).Margin(0, 10).MarginBottom(10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_variety", "editContent",
            delegate (JsonData json)
            {
              string varietyName = json.GetText("varietyName");
              if (StringHlp.IsEmpty(varietyName))
              {
                state.Operation.Message = "Не задано наименование разновидности";
                return;
              }

              LightObject editVariety = DataBox.LoadObject(fabricConnection, 
                VarietyType.Variety, varietyId.Value);
              if (!VarietyType.DisplayName.SetWithCheck(editVariety.Box, varietyId.Value, varietyName))
              {
                state.Operation.Message = "Разновидность товара с таким наименованием уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              editVariety.Set(ObjectType.ActTill, DateTime.UtcNow);

              string varietyAnnotation = json.GetText("varietyAnnotation");
              editVariety.Set(VarietyType.Annotation, varietyAnnotation);

              editVariety.Box.Update();

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    static IHtmlControl GetFabricAdd(EditState state, int? parentId, out string title)
    {
      title = "Добавление товара";

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          EditElementHlp.GetFabricKindCombo(shop, null),
					DecorEdit.Field("Наименование товара", "identifier", "")
          //DecorEdit.Field("Наименование товара", "name", "")
        ).Margin(0, 10).MarginBottom(20),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.AddButton("Добавить товар")
          .Event("add_fabric", "addContent",
            delegate (JsonData json)
            {
							string editIdentifier = json.GetText("identifier");
              //string editName = json.GetText("name");
              string fabricKind = json.GetText("fabricKind");
              if (StringHlp.IsEmpty(editIdentifier))
              {
                state.Operation.Message = "Не задано наименование товара";
                return;
              }

              KinBox box = new KinBox(fabricConnection, "1=0");
              int? createFabricId = box.CreateUniqueObject(FabricType.Fabric,
                FabricType.Identifier.CreateXmlIds(editIdentifier), null);
              if (createFabricId == null)
              {
                state.Operation.Message = "Товар с таким наименованием уже существует";
                return;
              }

              if (HttpContext.Current.IsInRole("nosave"))
              {
                state.Operation.Message = "Нет прав на сохранение изменений";
                return;
              }

              LightKin addFabric = new LightKin(box, createFabricId.Value);
							FabricHlp.SetCreateTime(addFabric);

							//addFabric.Set(SEOProp.Name, editName);

              addFabric.Set(FabricType.Kind, ConvertHlp.ToInt(fabricKind));

              addFabric.AddParentId(GroupType.FabricTypeLink, parentId.Value);

              addFabric.Box.Update();

              state.CreatingObjectId = addFabric.Id;

              SiteContext.Default.UpdateStore();
            }
          ),
          DecorEdit.ReturnButton(UrlHlp.ShopUrl("group", parentId))
        )
      ).EditContainer("addContent");
    }

    public static IHtmlControl GetFabricEdit(
      EditState state, int? parentId, int? fabricId, out string title)
    {
      title = "Редактирование товара";

      if (fabricId == null)
        fabricId = state.CreatingObjectId;

      if (parentId == null)
      {
        return EditHlp.GetInfoMessage("Неверный формат запроса", "/");
      }

      LightGroup group = shop.FindGroup(parentId.Value);
      if (group == null)
        return EditHlp.GetInfoMessage("Не найдена группа", "/");

      LightKin fabric = null;
      if (fabricId != null)
      {
        fabric = shop.FindFabric(fabricId.Value);
        if (fabric == null)
          return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");
      }

      if (fabricId == null)
      {
        return GetFabricAdd(state, parentId, out title);
      }

      LightGroup[] parentGroups = FabricHlp.FindParentGroups(shop, fabric);

      LightObject[] varieties = FabricHlp.GetVarieties(shop, fabric);

      string returnUrl = UrlHlp.ShopUrl("product", varieties.Length > 0 ? varieties[0].Id : fabric.Id);
      string returnGroupUrl = UrlHlp.ShopUrl("group", parentId);

      MetaKind kind = shop.FindFabricKind(fabric.Get(FabricType.Kind));

			string specialName = fabric.Get(SEOProp.Name);
			bool withSpecialName = !StringHlp.IsEmpty(specialName);

      return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          EditElementHlp.GetPropertiesTable(fabricId.Value, null,
            DecorEdit.Field("Вид товара", new HLabel(MetaKind.ToDisplayName(kind))),
						DecorEdit.Field(withSpecialName ? "Идентификатор" : "Наименование",
							"identifier", fabric.Get(FabricType.Identifier)
						),
            DecorEdit.Field("Наименование", "name", specialName)
							.Display(withSpecialName ? "block" : "none"),
            DecorEdit.Field("Цена", "fabricPrice", fabric.Get(FabricType.Price).ToString()),
            DecorEdit.FieldArea("Аннотация",
              new HTextArea("fabricAnnotation", fabric.Get(FabricType.Annotation)).Height("4em")
            )
          ),
          EditElementHlp.GetVarietiesPanel(shop, fabric),
          EditElementHlp.GetPropertiesPanel(kind, fabric),
          DecorEdit.FieldInputBlock("Описание товара",
            HtmlHlp.CKEditorCreate("fabricDescription", fabric.Get(FabricType.Description),
              "400px", true)
          ),
          EditElementHlp.GetDescriptionImagesPanel(state.Option, fabricId.Value)
        ).Margin(0, 10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton()
          .CKEditorOnUpdateAll()
          .Event("save_fabric", "editContent2",
            delegate (JsonData json)
            {
              try
              {
								if (fabric.Get(SEOProp.IsImport))
								{
									state.Operation.Message = "Импортированный товар не может быть изменен";
									return;
								}

								string editIdentifier = json.GetText("identifier");
                string editName = json.GetText("name");
                if (StringHlp.IsEmpty(editIdentifier))
                {
                  state.Operation.Message = "Не задано наименование товара";
                  return;
                }

                int? fabricPrice = ConvertHlp.ToInt(json.GetText("fabricPrice"));
                if (fabricPrice == null || fabricPrice < 0)
                {
                  state.Operation.Message = "Цена товара должна быть целым положительным числом";
                  return;
                }

                string fabricAnnotation = json.GetText("fabricAnnotation");
                string fabricDescription = json.GetText("fabricDescription");

                LightKin editFabric = DataBox.LoadKin(fabricConnection, FabricType.Fabric, fabric.Id);
                if (!GroupType.Identifier.SetWithCheck(editFabric.Box, fabric.Id, editIdentifier))
                {
                  state.Operation.Message = "Товар с таким наименованием уже существует";
                  return;
                }

                if (HttpContext.Current.IsInRole("nosave"))
                {
                  state.Operation.Message = "Нет прав на сохранение изменений";
                  return;
                }

							  editFabric.Set(SEOProp.Name, editName);

                editFabric.Set(ObjectType.ActTill, DateTime.UtcNow);

                editFabric.Set(FabricType.Price, fabricPrice.Value);
                editFabric.Set(FabricType.Annotation, fabricAnnotation);
                editFabric.Set(FabricType.Description, fabricDescription);

                if (kind != null)
                {
                  foreach (MetaProperty property in kind.Properties)
                  {
                    string dataName = string.Format("property_{0}", property.Id);
                    string propertyValue = json.GetText(dataName);
                    editFabric.Set(property.Blank, propertyValue);
                  }
                }

                editFabric.Box.Update();

								state.Operation.Message = "Изменения успешно сохранены";
								state.Operation.Status = "success";
							}
              catch (Exception ex)
              {
                Logger.WriteException(ex);
                state.Operation.Message = ex.Message;
              }
              SiteContext.Default.UpdateStore();
						}
          ),
          DecorEdit.ReturnButton(returnUrl)
          //new HLink(returnUrl,
          //  std.Button("Вернуться в товар", std.BeforeAwesome(@"\f112", 6).Color("#39abf0"))
          //),
          //new HLink(returnGroupUrl,
          //  std.Button("Вернуться в группу", std.BeforeAwesome(@"\f122", 6).Color("#39abf0"))
          //)
        ),
        EditPanelHlp.GetFabricParentsPanel(state, shop, fabric)
      ).EditContainer("editContent2");
    }

		public static IHtmlControl GetGroupEdit(
      EditState state, int? parentId, int? groupId, out string title)
    {
      title = "Редактирование группы";

      if (groupId == null)
        groupId = state.CreatingObjectId;

      if (parentId != null && shop.FindGroup(parentId.Value) == null)
        return EditHlp.GetInfoMessage("Неверный формат запроса", "/");

      LightGroup group = null;
      if (groupId != null)
      {
        group = shop.FindGroup(groupId.Value);
        if (group == null)
          return EditHlp.GetInfoMessage("Неверный аргумент запроса", "/");
      }

      string returnUrl = parentId == null ? "/catalog" : UrlHlp.ShopUrl("group", parentId);

      if (groupId == null)
      {
        title = "Добавление группы";
        return EditPanelHlp.GetObjectAdd(state, title, "Наименование группы", returnUrl,
          delegate(string groupName)
          {
            KinBox box = new KinBox(fabricConnection, "1=0");
            int? createGroupId = box.CreateUniqueObject(GroupType.Group,
              GroupType.Identifier.CreateXmlIds(groupName), null);
            if (createGroupId == null)
            {
              state.Operation.Message = "Группа с таким наименованием уже существует";
              return null;
            }
            LightKin addGroup = new LightKin(box, createGroupId.Value);
            FabricHlp.SetCreateTime(addGroup);

            if (parentId != null)
              addGroup.AddParentId(GroupType.SubgroupTypeLink, parentId.Value);

            addGroup.Box.Update();
            return addGroup;
          }
        );
        //return GetGroupAdd(state, parentId, groupId, out title);
      }

			EditorSelector selector = ((IShopContext)SiteContext.Default).GroupEditorSelector;

			return new HPanel(
        DecorEdit.Title(title),
        new HPanel(
          EditElementHlp.GetPropertiesTable(groupId.Value, 
						selector.FindTunes(group.Get(GroupType.DesignKind)),
            EditElementHlp.GetDeletePanel(state, groupId.Value, "группу", "Удаление группы",
              delegate
              {
                if (group.Subgroups.Length != 0 || group.Products.Length != 0)
                {
                  state.Operation.Message = "Непустая группа не может быть удалена.";
                  return false;
                }

                return true;
              }
            ),
            DecorEdit.Field("Наименование", "groupName", group.Get(GroupType.Identifier)),
            EditElementHlp.GetFabricKindCombo(shop, group.Get(GroupType.FabricKind)),
						EditElementHlp.GetDesignKindCombo(selector, group.Get(GroupType.DesignKind))
          )
        ).Margin(0, 10).MarginBottom(10),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.SaveButton().Event("save_group", "editContent",
            delegate(JsonData json)
            {
              try
              {
                string groupName = json.GetText("groupName");
                string fabricKind = json.GetText("fabricKind");
								string designKind = json.GetText("designKind");

                if (StringHlp.IsEmpty(groupName))
                {
                  state.Operation.Message = "Не задано наименование группы";
                  return;
                }

                LightKin editGroup = DataBox.LoadKin(fabricConnection,
                  GroupType.Group, groupId.Value);
                if (!GroupType.Identifier.SetWithCheck(editGroup.Box, groupId.Value, groupName))
                {
                  state.Operation.Message = "Группа с таким наименованием уже существует";
                  return;
                }

                if (HttpContext.Current.IsInRole("nosave"))
                {
                  state.Operation.Message = "Нет прав на сохранение изменений";
                  return;
                }

                editGroup.Set(GroupType.FabricKind, ConvertHlp.ToInt(fabricKind));
								editGroup.Set(GroupType.DesignKind, designKind);

                editGroup.Set(ObjectType.ActTill, DateTime.UtcNow);
                
                editGroup.Box.Update();
              }
              catch (Exception ex)
              {
                Logger.WriteException(ex);
                state.Operation.Message = ex.Message;
              }
              SiteContext.Default.UpdateStore();

							state.Operation.Message = "Изменения успешно сохранены";
							state.Operation.Status = "success";
						}
          ),
          DecorEdit.ReturnButton(returnUrl)
        )
      ).EditContainer("editContent");
    }

    public static IHtmlControl GetInfoMessage(string message, string returnUrl)
    {
      return new HPanel(
        DecorEdit.Title(message).MarginBottom(0),       
        //new HPanel(
        //  new HLabel(message).FontSize("150%")
        //).Padding(5, 10).Border("2px", "solid", HtmlHlp.ColorToHtml(Color.LightGray), "2px"),
        EditElementHlp.GetButtonsPanel(
          DecorEdit.ReturnButton(returnUrl)
        )
      ).Width("100%"); //.WidthLimit("", "0");
    }
  }
}