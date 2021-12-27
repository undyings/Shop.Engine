using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commune.Basis;
using Commune.Html;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;
using System.IO;
using System.Net.Mail;

namespace Shop.Prototype
{
  public class ViewCenterHlp
  {
    static IDataLayer orderConnection
    {
      get
      {
        return SiteContext.Default.OrderConnection;
      }
    }

    static IDataLayer fabricConnection
    {
      get
      {
        return SiteContext.Default.FabricConnection;
      }
    }

    static IDataLayer userConnection
    {
      get
      {
        return SiteContext.Default.UserConnection;
      }
    }

    static IShopStore store
    {
      get
      {
        return (IShopStore)SiteContext.Default.Store;
      }
    }

    static ShopStorage shop
    {
      get { return store.Shop; }
    }

    public static IHtmlControl GetLandingView(HttpContext httpContext,
      ShopState state, OrderCookie orderForView, int? landingId,
      out string title, out string description)
    {
      title = "";
      description = "";

      LightObject landing = store.Landings.Find(landingId);
      if (landing == null)
        return new HPanel();

      title = landing.Get(SEOProp.Title);
      description = landing.Get(SEOProp.Description);

      string heading = landing.Get(LandingType.DisplayName);
      string searchText = landing.Get(LandingType.SearchText);

      IEnumerable<Product> filterProducts = null;
      {
        SearchIndexStorage storage = store.SearchModule.FindIndexStorage(landing.Get(LandingType.FabricKind));
        SearchFilter filter = SearchHlp.FilterFromLandingPage(storage, landing);
        if (filter != null)
        {
          filterProducts = SearchHlp.FindProducts(store.SearchModule, shop, storage.KindId, filter);
        }
      }

      if (filterProducts == null)
        filterProducts = FabricHlp.SearchProducts(shop.AllProducts, searchText);

      string sortKind = landing.Get(LandingType.SortKind);
      IEnumerable<Product> sortedProducts = FabricHlp.SortProducts(filterProducts, sortKind, false);

      List<IHtmlControl> tileControls = new List<IHtmlControl>();
      foreach (Product product in sortedProducts)
      {
        IHtmlControl tile = ViewElementHlp.GetProductTile(httpContext, false, false, null, product, orderForView);
        tileControls.Add(tile);
      }

      if (tileControls.Count < 4)
      {
        while (tileControls.Count < 4)
        {
          tileControls.Add(ViewElementHlp.GetPlaceholderTile());
        }
      }

      return new HPanel(
        ViewElementHlp.GetViewTitle(heading),
        new HPanel(
          new HPanel(
            tileControls.ToArray()
          ).Align(true).Media664(new HStyle().Align(null))
        ),
        DecorEdit.AdminPanel(state.EditMode, state.SeoMode, landing, "landing", landing.Id)
      ); //.Width("100%");
    }

    public static IHtmlControl GetGroupView(HttpContext httpContext,
      ShopState state, OrderCookie orderForView, int? groupId,
      out string title, out string description)
    {
      LightGroup parentGroup = shop.FindGroup(groupId);
      LightKin[] groups = parentGroup != null ? parentGroup.Subgroups : shop.RootGroups;

      string heading = parentGroup?.Get(GroupType.Identifier);

      if (parentGroup != null)
      {
        title = FabricHlp.GetSeoTitle(parentGroup, 
          (store.SEO.Get(SEOType.GroupTitlePattern) ?? "").Replace("<<group>>", heading));
        description = FabricHlp.GetSeoDescription(parentGroup,
          (store.SEO.Get(SEOType.GroupDescriptionPattern) ?? "").Replace("<<group>>", heading));
      }
      else
      {
        title = store.SEO.Get(SEOType.MainTitle);
        description = store.SEO.Get(SEOType.MainDescription);
      }

      List<IHtmlControl> tileControls = new List<IHtmlControl>();

      MetaKind kind = parentGroup != null ? shop.FindFabricKind(parentGroup.Get(GroupType.FabricKind)) : null;

      SearchFilter filter = state.SearchFilter;
      if (kind != null && filter != null && filter.Conditions.Length != 0)
      {
        List<Product> findedProducts = SearchHlp.FindProducts(store.SearchModule, shop, kind.Id, filter);

        IEnumerable<Product> sortedProducts = FabricHlp.SortProducts(findedProducts,
          state.SortKind, false);

        foreach (Product product in sortedProducts)
        {
          IHtmlControl tile = ViewElementHlp.GetProductTile(httpContext, state.EditMode, state.SeoMode,
            parentGroup, product, orderForView);
          tileControls.Add(tile);
        }

        return new HPanel(
          ViewElementHlp.GetViewTitle(heading).Hide(parentGroup == null),
          ViewPanelHlp.GetSearchFilterPanel(state, store, kind),
          new HPanel(
            new HPanel(
              tileControls.ToArray()
            ).Align(true).Media664(new HStyle().Align(null))
          )
        ); //.Width("100%");
      }

      foreach (LightKin group in groups)
      {
        IHtmlControl tile = ViewElementHlp.GetGroupTile(state.EditMode, state.SeoMode, parentGroup, group);
        tileControls.Add(tile);
      }

      if (parentGroup != null)
      {
        IEnumerable<Product> sortedProducts = FabricHlp.SortProducts(parentGroup.Products,
          state.SortKind, true);
        foreach (Product product in sortedProducts)
        {
          IHtmlControl tile = ViewElementHlp.GetProductTile(httpContext, state.EditMode, state.SeoMode,
            parentGroup, product, orderForView);
          tileControls.Add(tile);
        }
      }

      if (state.EditMode && (parentGroup == null || parentGroup.Products.Length == 0))
      {
        tileControls.Add(
          ViewElementHlp.GetTechnicalTile(
            UrlHlp.EditUrl(parentGroup, "group", null),
            "Добавить группу"
          )
        );

        LightKin[] subgroups = parentGroup != null ? parentGroup.Subgroups : shop.RootGroups;
        if (subgroups.Length > 0)
        {
          tileControls.Add(
            ViewElementHlp.GetTechnicalTile(
              UrlHlp.EditUrl(parentGroup, "sorting_group", null),
              "Сортировать"
            )
          );
        }
      }

      if (state.EditMode && (parentGroup != null && parentGroup.Subgroups.Length == 0))
      {
        tileControls.Add(
          ViewElementHlp.GetTechnicalTile(
            UrlHlp.EditUrl(parentGroup, "fabric", null),
            "Добавить товар"
          )
        );
        if (parentGroup.Products.Length > 0)
        {
          tileControls.Add(
            ViewElementHlp.GetTechnicalTile(
              UrlHlp.EditUrl(parentGroup, "sorting_fabric", null),
              "Сортировать"
            )
          );
        }
      }

      if (tileControls.Count < 4)
      {
        while (tileControls.Count < 4)
        {
          tileControls.Add(ViewElementHlp.GetPlaceholderTile());
        }
      }

      return new HPanel(
        ViewElementHlp.GetViewTitle(heading).Hide(parentGroup == null),
        ViewPanelHlp.GetSearchFilterPanel(state, store, kind),
        new HPanel(
          new HPanel(
            tileControls.ToArray()
          ).Align(true).Media664(new HStyle().Align(null))
        )
      ); //.Width("100%");
    }

    public static IHtmlControl GetNewsView(bool isAdmin, int? newsId, out string title)
    {
      title = "";

      if (newsId == null)
        return null;

      LightObject news = DataBox.LoadObject(fabricConnection, NewsType.News, newsId.Value);
      if (news == null)
        return null;

      title = news.Get(NewsType.Title);

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        new HPanel(
          new HTextView(news.Get(NewsType.Text))
        )
      ).CssAttribute("line-height", "1.65em");
    }

    public static IHtmlControl GetSectionView(bool isAdmin, int? sectionId, out string title)
    {
      title = "";
      LightSection section = store.Sections.FindSection(sectionId);
      if (section == null)
        return new HPanel();

      title = section.Get(SectionType.Title);

      string designKind = section.Get(SectionType.DesignKind);
      if (designKind == "contact")
        return GetContactsView(isAdmin, section, out title);

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        new HPanel(
          new HTextView(section.Get(SectionType.Content)),
          DecorEdit.RedoButton(isAdmin, "Редактировать", UrlHlp.EditUrl("page", sectionId))
        )
      ).LineHeight("1.65em");
    }

    static readonly HBuilder h = null;

    static IHtmlControl GetContactsView(bool isAdmin, LightSection section, out string title)
    {
      title = section.Get(SectionType.Title);

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        std.RowPanel(std.DockFill()),
        new HPanel(
          new HTextView(section.Get(SectionType.Content)),
          new HElementControl(new HElement("iframe", h.src("/gis"),
            new HAttribute("width", "100%"), new HAttribute("height", "420px"), new HAttribute("frameborder", "0")), ""),
          new HTextView(section.Get(SectionType.UnderContent)),
          DecorEdit.RedoButton(isAdmin, "Редактировать", UrlHlp.EditUrl("page", section.Id))
        )
      ).LineHeight("1.65em");
    }

    public static IHtmlControl GetProductView(HttpContext httpContext, ShopState state,
      OrderCookie orderForView, int? groupId, int? productId,
      out string title, out string description)
    {
      title = "";
      description = "";

      if (productId == null)
        return new HPanel();

      Product product = shop.FindProduct(productId);
      if (product == null)
        return new HPanel();

      string productName = product.ProductName;
      title = FabricHlp.GetSeoTitle(product,
        (store.SEO.Get(SEOType.ProductTitlePattern) ?? "").Replace("<<product>>", productName)
      );

      description = FabricHlp.GetSeoDescription(product,
        (store.SEO.Get(SEOType.ProductDescriptionPattern) ?? "").Replace("<<product>>", productName)
      );

      string seoText = product.Get(SEOProp.Text);

      int productCount = orderForView.GetCount(product.ProductId);

      LightGroup group = shop.FindGroup(groupId);
      if (group == null)
        return new HPanel();

      if (shop.FindFabricKind(product.Get(FabricType.Kind))?.Get(MetaKindType.DesignKind) == "bed")
        return ViewBedHlp.GetBedView(httpContext, state, group, product);

      return new HPanel(
        ViewElementHlp.GetViewTitle(productName),
        new HXPanel(
          new HPanel(
            new HImage(UrlHlp.ImageUrl(product.ImageId, true)).WidthLimit("300px", "300px")
          ),
          new HPanel(
            new HLink(UrlHlp.ShopUrl("group", group.Id),
              new HLabel(group.Get(GroupType.Identifier))
                .Block().Upper().Color("#71A866")
            ),
            new HLabel(string.Format("{0} руб", product.Get(FabricType.Price)))
              .Block().Padding("1em 0 1em 0").CssAttribute("border-bottom", "3px solid #E5E5DC")
              .FontBold(true).FontSize("34px").Color("#71A866"),
            new HLabel(product.Annotation)
              .Block().Padding("24px 0").Color("#222").CssAttribute("border-bottom", "3px solid #E5E5DC"),
            new HPanel(
              Decor.EcoButton("Добавить в корзину",
                std.BeforeAwesome(@"\f07a", 4), null
              ).Padding(8, 35).BorderRadius(2)
                .Event(string.Format("product_{0}_add", product.ProductId), "", delegate (JsonData json)
                {
                  OrderCookie order = OrderHlp.GetOrCreateOrderCookie(httpContext);
                  order.Increment(product);
                }),
              new HLabel(productCount, new HBefore().Content("×").MarginRight(2).FontSize("75%"))
                .Hide(productCount == 0).Padding(8, 0).MarginLeft(8)
                .FontSize("18px").FontFamily("PT Sans").FontBold().Color("#888")
             ).Padding("24px 0")
          ).PaddingLeft(24)
        ).Media664(new HStyle(".{0} > div").MediaBlock()),
        ViewPanelHlp.GetVarietiesPanel(false, shop, product, product.VarietyId),
        new HPanel(
          new HTextView(product.Get(FabricType.Description)),
          ViewPanelHlp.GetFabricPropertiesTable(shop, product, state),
          new HTextView(seoText).Hide(StringHlp.IsEmpty(seoText))
        ),
        DecorEdit.AdminPanel(state.EditMode, state.SeoMode, group, "fabric", product.Id)
      ).LineHeight("1.65em");
    }

    public static IHtmlControl GetSearchView(HttpContext httpContext,
      OrderCookie orderForView, string searchText, out string title)
    {
      title = string.Format("Поиск: {0}", searchText);

      IEnumerable<Product> products = FabricHlp.SearchProducts(shop.AllProducts, searchText);

      List<IHtmlControl> tileControls = new List<IHtmlControl>();
      foreach (Product product in products)
      {
        IHtmlControl tile = ViewElementHlp.GetProductTile(httpContext, false, false, null, product, orderForView);
        tileControls.Add(tile);
      }

      if (tileControls.Count < 4)
      {
        while (tileControls.Count < 4)
        {
          tileControls.Add(ViewElementHlp.GetPlaceholderTile());
        }
      }

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        new HPanel(
          tileControls.ToArray()
        )
      ).Width("100%");
    }

    public static IHtmlControl GetCartView(HttpContext httpContext,
      OrderCookie orderForView, out string title)
    {
      int orderPrice = 0;
      if (orderForView.GetAllProductCount(shop, out orderPrice) == 0)
      {
        title = "Пустая корзина";
        return new HPanel(
          ViewElementHlp.GetViewTitle(title),
          new HXPanel(
            new HLink("/",
              Decor.EcoButton("Продолжить покупки").Padding(8, 35)
            ),
            std.DockFill()
          ).Width("100%")
        );
      }

      title = "Корзина";

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        ViewPanelHlp.GetCartTable(httpContext, orderForView, shop, (int)orderPrice),
        std.RowPanel(
          Decor.EcoButton("Очистить").Padding(8, 12).Title("Очистить корзину")
            .Event("cart_clear", "", delegate (JsonData json)
            {
              OrderCookie order = OrderHlp.GetOrCreateOrderCookie(httpContext);
              order.Clear();
            }),
          std.DockFill(),
          new HLink(UrlHlp.ShopUrl("order"),
            Decor.EcoButton("Сделать заказ").Padding(8, 35)
          )
        ).MarginTop(30)
      );
    }

    public static IHtmlControl GetPaymentView(HttpContext httpContext, ShopState state, string orderId,
      out string title)
    {
      title = "Состояние заказа в платежной системе";

      try
      {
        SiteSettings settings = SiteContext.Default.SiteSettings;
        int? orderNumber;
        int? orderStatus;
        int? amount;
        int? errorCode;
        string errorMessage;
        SberbankHlp.GetOrderStatus(settings.SberbankStatusUrl,
          settings.SberbankUserName, settings.SberbankPassword, orderId,
          out orderNumber, out orderStatus, out amount, out errorCode, out errorMessage
        );

        List<DisplayName> rows = new List<DisplayName>();
        rows.Add(new DisplayName("№ в платежной системе", orderId));
        rows.Add(new DisplayName("Сумма заказа, рублей", amount == null ? "" : (amount.Value / 100).ToString()));
        if (!StringHlp.IsEmpty(errorMessage))
          rows.Add(new DisplayName("Состояние оплаты", errorMessage));
        if (orderStatus != null)
          rows.Add(new DisplayName("Состояние заказа", SberbankHlp.OrderStatusToDisplay(orderStatus)));
        if (errorCode != null && errorCode != 0)
          rows.Add(new DisplayName("Описание ошибки", SberbankHlp.ErrorCodeToDisplay(errorCode)));

        return new HPanel(
          ViewElementHlp.GetViewTitle(title),
          new HGrid<DisplayName>(rows, delegate (DisplayName row)
            {
              return new HPanel(
                new HLabel(row.Name).Width(240).FontBold(),
                new HLabel(row.Display)
              ).PaddingTop(10).PaddingBottom(8);
            },
            new HRowStyle()
          )
        );
      }
      catch (Exception ex)
      {
        Logger.WriteException(ex);

        return ViewElementHlp.GetViewCompletion(title,
          string.Format("Ошибка при обращении к платежной системе: {0}", ex.Message)
        );
      }
    }

    public static IHtmlControl GetOrderView(HttpContext httpContext, ShopState state, 
      OrderCookie orderForView, out string title)
    {
      title = "Оформление заказа";

      if (state.Operation.Completed)
        return ViewElementHlp.GetViewCompletion(title, state.Operation.Message);

      int orderPrice = 0;
      int fabricCount = orderForView.GetAllProductCount(shop, out orderPrice);

      if (fabricCount == 0)
        return GetCartView(httpContext, orderForView, out title);

      IEnumerable<VirtualRowLink> rows = orderForView.AllProducts(shop);

      LightObject selectedPayment = store.Payments.Find(state.SelectedPaymentId);
      LightObject selectedDelivery = store.Deliveries.Find(state.SelectedDeliveryId);

      string selectedPaymentName = selectedPayment == null ? "Способ оплаты не выбран" :
        selectedPayment.Get(PaymentWayType.DisplayName);
      string selectedDeliveryName = selectedDelivery == null ? "Способ доставки не выбран" :
        selectedDelivery.Get(DeliveryWayType.DisplayName);
      int selectedDeliveryCost = selectedDelivery?.Get(DeliveryWayType.Cost) ?? 0;

      LightObject user = UserHlp.LoadUser("", httpContext.UserName());

      string gridBorder = "1px solid #DDD";

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        new HPanel(
          new HPanel(
            Decor.OrderLabel("Фамилия *"),
            Decor.OrderEdit("family", user?.Get(UserType.Family)),
            Decor.OrderLabel("Имя *"),
            Decor.OrderEdit("firstName", user?.Get(UserType.FirstName)),
            Decor.OrderLabel("Отчество"),
            Decor.OrderEdit("patronymic"),
            new HPanel(
              new HPanel(
                Decor.OrderLabel("Электронная почта *"),
                Decor.OrderEdit("email", user?.Get(UserType.Login))
              ).RelativeWidth(48),
              new HPanel().RelativeWidth(4),
              new HPanel(
                Decor.OrderLabel("Телефон"),
                Decor.OrderEdit("phone")
              ).RelativeWidth(48)
            ),
            Decor.OrderLabel("Адрес"),
            Decor.OrderEdit("address")
          ).RelativeWidth(48),
          new HPanel().RelativeWidth(4),
          new HPanel(
            Decor.OrderLabel("Комментарий"),
            Decor.OrderEditControl(new HTextArea("comment")).Height(165)
          ).RelativeWidth(48).VAlign(true)
        ).Media664(new HStyle(".{0} > div").MediaBlock()),
        new HPanel(
          new HPanel(
            new HGrid<LightObject>(store.Payments.All, delegate (LightObject payment)
            {
              return std.RowPanel(
                new HInputRadio("payment", payment.Id, false,
                  delegate
                  {
                    state.SelectedPaymentId = payment.Id;
                  }
                ).MarginLeft(5).MarginRight(8),
                std.DockFill(new HLabel(payment.Get(PaymentWayType.DisplayName))),
                new HLabel("*").Width(13),
                DecorEdit.RedoIconButton(state.EditMode, UrlHlp.EditUrl("payment", payment.Id))
              ).Padding(5);
            },
              new HRowStyle().Odd(new HTone().Background("#F9F9F9"))
            ),
            DecorEdit.RedoButton(state.EditMode, "Добавить способ оплаты", UrlHlp.EditUrl("payment", null))
          ).RelativeWidth(48).Padding(20).VAlign(true)
            .Background(Decor.orderBackground).BorderWithRadius(Decor.orderBorder, 2),
          new HPanel().Display("inline-block").Width("4%"),
          new HPanel(
            new HGrid<LightObject>(store.Deliveries.All, delegate (LightObject delivery)
            {
              int deliveryCost = delivery.Get(DeliveryWayType.Cost);
              return std.RowPanel(
                new HInputRadio("delivery", delivery.Id, false,
                  delegate (JsonData json)
                  {
                    state.SelectedDeliveryId = delivery.Id;
                  }
                ).MarginLeft(5).MarginRight(8),
                std.DockFill(new HLabel(delivery.Get(DeliveryWayType.DisplayName))),
                new HLabel(deliveryCost != 0 ? deliveryCost.ToString() : "*")
                  .Align(false).Width(34).Padding(0, 5),
                DecorEdit.RedoIconButton(state.EditMode, UrlHlp.EditUrl("delivery", delivery.Id))
              ).Padding(5);
            },
              new HRowStyle().Odd(new HTone().Background("#F9F9F9"))
            ),
            DecorEdit.RedoButton(state.EditMode, "Добавить способ доставки", UrlHlp.EditUrl("delivery", null))
          ).RelativeWidth(48).Padding(20)
            .Background(Decor.orderBackground).BorderWithRadius(Decor.orderBorder, 2)
        ).EditContainer("radioContent").MarginTop(10)
          .Media664(new HStyle(".{0} > div").MediaBlock()),
        new HPanel(
          new HLabel("Ваш заказ").Margin(20, 0)
            .FontFamily("PT Sans").FontSize("22px").Color("#6CAF22").FontBold(),
          new HGrid<VirtualRowLink>(null, rows,
            delegate (VirtualRowLink row)
            {
              int productId = row.Get(OrderProductType.ProductId);
              int price = row.Get(OrderProductType.Price);
              int count = row.Get(OrderProductType.Count);

              string productName = BedHlp.GetProductName(shop, productId, row);

              return std.RowPanel(
                std.DockFill(
                  new HLabel(productName).Padding(5)
                ),
                new HLabel(count.ToString())
                  .Width(60).BoxSizing().Padding(5).Align(null).BorderLeft(gridBorder),
                new HLabel(price)
                  .Width(60).BoxSizing().Padding(5).Align(false).BorderLeft(gridBorder),
                new HLabel(price * count)
                  .Width(60).BoxSizing().Padding(5).Align(false).FontBold().BorderLeft(gridBorder)
              ).Border(gridBorder).BorderBottom("0px");
            },
            new HRowStyle(),
            new HPanel(
              std.RowPanel(
                std.DockFill(new HLabel("Сумма")).Padding(5).Align(false),
                new HLabel(orderPrice).Width(180).BoxSizing().Padding(5)
                  .Align(false).BorderLeft(gridBorder)
              ).BorderBottom(gridBorder),
              std.RowPanel(
                std.DockFill(
                  new HLabel(selectedPaymentName)
                ).Padding(5).Align(false),
                new HLabel("*").Width(180).BoxSizing().Padding(5)
                  .Align(false).BorderLeft(gridBorder)
              ).BorderBottom(gridBorder),
              std.RowPanel(
                std.DockFill(
                  new HLabel(selectedDeliveryName)
                ).Padding(5).Align(false),
                new HLabel(selectedDeliveryCost != 0 ? selectedDeliveryCost.ToString() : "*")
                  .Width(180).BoxSizing().Padding(5)
                  .Align(false).BorderLeft(gridBorder)
              ).BorderBottom(gridBorder),
              std.RowPanel(
                std.DockFill(new HLabel("Итого, рублей").FontBold()).Padding(5).Align(false),
                new HLabel(orderPrice + selectedDeliveryCost).Width(180).BoxSizing().Padding(5)
                  .Align(false).FontBold().BorderLeft(gridBorder)
              )
            ).Border(gridBorder)
          )
        ),
        std.RowPanel(
          new HLink(UrlHlp.ShopUrl("cart"),
            Decor.EcoButton("Корзина").Padding(8, 12)
          ),
          std.DockFill(),
          Decor.EcoButton("Заказать").Padding(8, 35)
            .Event("orderFabrics", "orderData", delegate (JsonData json)
            {
              string family = json.GetText("family");
              string firstName = json.GetText("firstName");
              string patronymic = json.GetText("patronymic");
              string email = json.GetText("email");
              string phone = json.GetText("phone");
              string address = json.GetText("address");
              string comment = json.GetText("comment");

              if (StringHlp.IsEmpty(family))
              {
                state.Operation.Message = "Не задана фамилия";
                return;
              }

              if (StringHlp.IsEmpty(firstName))
              {
                state.Operation.Message = "Не задано имя";
                return;
              }

              if (StringHlp.IsEmpty(email))
              {
                state.Operation.Message = "Не задана электронная почта";
                return;
              }

              if (state.SelectedPaymentId == null)
              {
                state.Operation.Message = "Не выбран способ оплаты";
                return;
              }
              if (state.SelectedDeliveryId == null)
              {
                state.Operation.Message = "Не выбран способ доставки";
                return;
              }

              ObjectBox orderBox = new ObjectBox(orderConnection, "1=0");
              int createOrderId = orderBox.CreateObject(OrderType.Order, "", DateTime.UtcNow);

              LightObject order = new LightObject(orderBox, createOrderId);
              order.Set(OrderType.Family, family);
              order.Set(OrderType.FirstName, firstName);
              order.Set(OrderType.Patronymic, patronymic);
              order.Set(OrderType.EMail, email);
              order.Set(OrderType.Phone, phone);
              order.Set(OrderType.Address, address);
              order.Set(OrderType.Comment, comment);
              order.Set(OrderType.PaymentKind, state.SelectedPaymentId.Value);
              order.Set(OrderType.DeliveryKind, state.SelectedDeliveryId.Value);

              order.Set(OrderType.Products, orderForView.AsString());

              orderBox.Update();

              OrderHlp.GetOrCreateOrderCookie(httpContext).Clear();

              if (state.SelectedPaymentId != 24)
                state.Operation.Complete("Спасибо! Мы свяжемся с вами в ближайшее время", "");

              Logger.AddMessage("Заказ №{0} выполнен", order.Id);

              if (state.SelectedPaymentId == 24)
              {
                string orderId;
                string formUrl;
                string error;
                SiteSettings settings = SiteContext.Default.SiteSettings;
                SberbankHlp.RegisterOrder(settings.SberbankRegisterUrl,
                  settings.SberbankUserName, settings.SberbankPassword,
                  order.Id, (int)orderPrice * 100, 2400, 
									new Uri(new Uri(settings.SiteHost), "payment").ToString(), 
                  out orderId, out formUrl, out error
                );

                state.RedirectUrl = formUrl;
              }

              try
              {
                string answer = File.ReadAllText(httpContext.Server.MapPath("answer.txt"),
                  System.Text.Encoding.GetEncoding(1251));
                //Logger.AddMessage("Answer: {0}", answer);
                SiteSettings settings = SiteContext.Default.SiteSettings;
                SmtpClient smtpClient = AuthHlp.CreateSmtpClient(
                  settings.SmtpHost, settings.SmtpPort, settings.SmtpUserName, settings.SmtpPassword);
                AuthHlp.SendMail(smtpClient, settings.MailFrom,
                  email, string.Format("Тестовый заказ №{0}", order.Id),
                  answer
                );
              }
              catch (Exception ex)
              {
                Logger.WriteException(ex);
              }
            })
        ).MarginTop(30),
        std.OperationWarning(state.Operation)
      ).EditContainer("orderData");
    }

    public static IHtmlControl GetRegisterPanel(ShopState state,
      out string title)
    {
      title = "Зарегистрироваться";

      if (state.Operation.Completed)
        return ViewElementHlp.GetViewCompletion(title, state.Operation.Message);

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        ViewElementHlp.GetAuthHeader("register"),
        new HPanel(
          Decor.OrderLabel("Электронная почта"),
          Decor.AuthEdit("email"),
          Decor.OrderLabel("Имя"),
          Decor.AuthEdit("firstName"),
          Decor.OrderLabel("Фамилия"),
          Decor.AuthEdit("family"),
          Decor.OrderLabel("Пароль"),
          Decor.AuthEditControl(new HPasswordEdit("password")).Height(34),
          Decor.OrderLabel("Введите пароль повторно"),
          Decor.AuthEditControl(new HPasswordEdit("passwordRepeat")).Height(34),
          Decor.EcoButton("Зарегистрироваться").Padding(8, 35)
            .Event("user_register", "editContent",
              delegate (JsonData json)
              {
                string login = json.GetText("email");
                string firstName = json.GetText("firstName");
                string family = json.GetText("family");
                string password = json.GetText("password");
                string passwordRepeat = json.GetText("passwordRepeat");

                WebOperation operation = state.Operation;

                if (!operation.Validate(login, "Не задана электронная почта"))
                  return;
                if (!operation.Validate(!login.Contains("@"), "Некорректный адрес электронной почты"))
                  return;
                if (!operation.Validate(firstName, "Не задано имя"))
                  return;
                if (!operation.Validate(family, "Не задана фамилия"))
                  return;
                if (!operation.Validate(password, "Не задан пароль"))
                  return;
                if (!operation.Validate(password != passwordRepeat, "Повтор не совпадает с паролем"))
                  return;

                ObjectBox box = new ObjectBox(userConnection, "1=0");

                int? createUserId = box.CreateUniqueObject(UserType.User,
                  UserType.Login.CreateXmlIds("", login), DateTime.UtcNow);
                if (!operation.Validate(createUserId == null,
                  "Пользователь с такой электронной почтой уже существует"))
                {
                  return;
                }

                LightObject user = new LightObject(box, createUserId.Value);
                FabricHlp.SetCreateTime(user);
                user.Set(UserType.FirstName, firstName);
                user.Set(UserType.Family, family);
                user.Set(UserType.Password, password);

                box.Update();

                operation.Complete("Вы успешно зарегистрированы!", "");
              }
            ),
          std.OperationWarning(state.Operation)
        ).RelativeWidth(50).Padding(46).Align(true)
          .Background(Decor.authBackground)
      ).EditContainer("editContent").Align(null);
    }

    public static IHtmlControl GetLoginPanel(HttpContext httpContext, ShopState state,
      out string title)
    {
      title = "Войти";

      if (state.Operation.Completed)
        return ViewElementHlp.GetViewCompletion(title, state.Operation.Message);

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        ViewElementHlp.GetAuthHeader("login"),
        new HPanel(
          Decor.OrderLabel("Электронная почта"),
          Decor.AuthEdit("email"),
          Decor.OrderLabel("Пароль"),
          Decor.AuthEditControl(new HPasswordEdit("password")).Height(34),
          Decor.EcoButton("Войти").Padding(8, 35).Event("user_login", "editContent",
            delegate (JsonData json)
            {
              string login = json.GetText("email");
              string password = json.GetText("password");

              WebOperation operation = state.Operation;
              if (!operation.Validate(login, "Не задана электронная почта"))
                return;
              if (!operation.Validate(password, "Не задан пароль"))
                return;

              LightObject user = UserHlp.LoadUser("", login);
              if (!operation.Validate(user == null,
                "Пользователь с введенной электронной почтой не найден"))
                return;

              if (!operation.Validate(user.Get(UserType.Password) != password, "Неверный пароль"))
                return;

              httpContext.SetUserAndCookie(login);

              operation.Complete("Вы успешно вошли на сайт!", "");
            }),
          std.OperationWarning(state.Operation)
        ).RelativeWidth(50).Padding(46).Align(true)
          .Background(Decor.authBackground)
      ).EditContainer("editContent").Align(null);
    }

    public static IHtmlControl GetPasswordResetPanel(HttpContext httpContext, ShopState state,
      out string title)
    {
      title = "Забыли пароль?";

      if (state.Operation.Completed)
        return ViewElementHlp.GetViewCompletion(title, state.Operation.Message);

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        ViewElementHlp.GetAuthHeader("passwordreset"),
        new HPanel(
          Decor.OrderLabel("Электронная почта"),
          Decor.AuthEdit("email"),
          Decor.EcoButton("Отправить").Padding(8, 35)
        ).RelativeWidth(50).Padding(46).Align(true)
          .Background(Decor.authBackground)
      ).Align(null);
    }

    public static IHtmlControl GetNewsList(bool isAdmin, out string title)
    {
      title = "Новости";

      ObjectBox newsBox = new ObjectBox(fabricConnection,
        DataCondition.ForTypes(NewsType.News) + " order by act_from desc");

      return new HPanel(
        ViewElementHlp.GetViewTitle(title),
        new HPanel(
          DecorEdit.RedoButton(isAdmin, "Добавить новость", UrlHlp.EditUrl("news", null))
        ).MarginBottom("1.65em"),
        new HGrid<int>(newsBox.AllObjectIds,
          delegate (int newsId)
          {
            LightObject news = new LightObject(newsBox, newsId);
            string newsUrl = UrlHlp.ShopUrl("news", newsId);
            return std.RowPanel(
              new HLink(newsUrl,
                new HImage(UrlHlp.ImageUrl(newsId, false))
                .WidthFixed(236) //.Width(236)
              ),
              new HPanel(
                new HLabel(UrlHlp.DateToString(news.Get(ObjectType.ActFrom))).Block(),
                new HLink(newsUrl,
                  new HLabel(news.Get(NewsType.Title))
                    .FontFamily("PT Sans").FontSize("22px").FontBold().Color(Decor.PriceColor)
                ).Block().MarginTop(20).MarginBottom(20).TextDecoration("none"),
                new HTextView(news.Get(NewsType.Annotation)).Block(),
                DecorEdit.RedoButton(isAdmin, "Редактировать", UrlHlp.EditUrl("news", newsId))
              ).PaddingLeft(40).VAlign(true)
            ).MarginBottom("1.65em");
          },
          new HRowStyle()
        )
      );
    }
  }
}