using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Commune.Basis;
using Commune.Html;
using System.Drawing;
using NitroBolt.Wui;
using Commune.Data;
using Shop.Engine;
using System.Web.Http;
using System.Net;
using System.Net.Http;

namespace Shop.Prototype
{
  public class SiteController : ApiController
  {
    [HttpGet, HttpPost]
    [Route("")]
    public HttpResponseMessage Main()
    {
      return HWebApiSynchronizeHandler.Process<object>(this.Request, MainView.HViewCreator("/catalog"));
    }

    [HttpGet, HttpPost]
    [Route("editor")]
    public HttpResponseMessage Editor()
    {
      return HWebApiSynchronizeHandler.Process<object>(this.Request, ContentEdit.HView);
    }

    [HttpGet, HttpPost]
    [Route("seo")]
    public HttpResponseMessage Seo()
    {
      return HWebApiSynchronizeHandler.Process<object>(this.Request, SeoEdit.HView);
    }

    [HttpGet, HttpPost]
    [Route("{mode}")]
    public HttpResponseMessage Route(string mode)
    {
      string directory = string.Format("/{0}", mode);
      return HWebApiSynchronizeHandler.Process<object>(this.Request, MainView.HViewCreator(directory));
    }

    [HttpGet, HttpPost]
    [Route("{kind}/{translitId}")]
    public HttpResponseMessage Route(string kind, string translitId)
    {
      string directory = string.Format("/{0}/{1}", kind, translitId);
      return HWebApiSynchronizeHandler.Process<object>(this.Request, MainView.HViewCreator(directory));
    }

    [HttpGet, HttpPost]
    [Route("catalog/{groupTranslitId}/{productTranslitId}")]
    public HttpResponseMessage ShopRoute(string groupTranslitId, string productTranslitId)
    {
      string directory = string.Format("/catalog/{0}/{1}", groupTranslitId, productTranslitId);
      return HWebApiSynchronizeHandler.Process<object>(this.Request, MainView.HViewCreator(directory));
    }

    [HttpGet, HttpPost]
    [Route("filesupload")]
    public HttpResponseMessage FilesUpload()
    {
      return HttpLoader.FilesUploader();
    }

    [HttpGet, HttpPost]
    [Route("tileupload")]
    public HttpResponseMessage TileUpload()
    {
      return HttpLoader.TileUploader(ViewElementHlp.thumbSize);
    }

    [HttpGet]
    [Route("gis")]
    public HttpResponseMessage Gis()
    {
      return HttpLoader.Gis();
    }
  }
}