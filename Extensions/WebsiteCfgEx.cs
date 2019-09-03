#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2019/04/25 16:02
// Modified On:  2019/04/25 17:52
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using Flurl.Http;
using HtmlAgilityPack;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.Import.Configs;
using SuperMemoAssistant.Services.HTML;
using SuperMemoAssistant.Services.HTML.Extensions;

namespace SuperMemoAssistant.Plugins.Import.Extensions
{
  public static class WebsiteCfgEx
  {
    #region Methods

    public static ElementBuilder ConfigureWeb(
      this ElementBuilder builder,
      WebsiteCfg          cfg,
      IElement            fallbackRoot     = null,
      int                 fallbackPriority = ImportConst.DefaultPriority)
    {
      return builder.WithParent(cfg?.RootElement ?? fallbackRoot)
                    .WithPriority(cfg?.Priority ?? fallbackPriority);
    }

    public static References ConfigureWeb(
      this References r,
      WebsiteCfg      cfg,
      string          html,
      string          fallbackDate  = null,
      string          fallbackTitle = null)
    {
      return r.WithDate(cfg?.ParseDateString(html) ?? fallbackDate)
              .WithTitle(cfg?.ParseTitle(html) ?? fallbackTitle)
              .WithSource(cfg?.Name);
    }

    public static async Task<string> ProcessContent(this WebsiteCfg cfg, string html, string url)
    {
      if (cfg == null)
        return html;

      html = cfg.ApplyFilter(html);

      var baseUrl = new Uri(url);
      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(html);

      htmlDoc.EnsureAbsoluteLinks(baseUrl);

      if (cfg.LoadIframes)
        await htmlDoc.InlineIFrames(baseUrl, iframeUrl => iframeUrl.SafeGetStringAsync());

      return htmlDoc.ToHtml();
    }

    public static string ApplyFilter(this WebsiteCfg cfg, string html, string separator = "\r\n")
    {
      if (cfg?.Filters.None() ?? true)
        return html;

      var matches = cfg.Filters
                       .Select(f => f.Filter(html))
                       .Where(s => string.IsNullOrWhiteSpace(s) == false);

      return string.Join(separator, matches);
    }

    public static string ParseTitle(this WebsiteCfg cfg, string html)
    {
      return cfg?.TitleRegex?.Match(html).Groups.SafeGet(1);
    }

    public static string ParseDateString(this WebsiteCfg cfg, string html)
    {
      return cfg?.DateRegex?.Match(html).Groups.SafeGet(1);
    }

    public static IFlurlRequest CreateRequest(this WebsiteCfg cfg, string url, IFlurlClient client = null)
    {
      var req = url.CreateRequest(client, cfg.UserAgent);

      try
      {
        if (string.IsNullOrWhiteSpace(cfg.Cookie) == false)
          req.WithCookies(CookiesUtils.ParseCookies(cfg.Cookie, true));
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"Exception while parsing cookies: {cfg.Cookie}");
      }

      return req;
    }

    #endregion
  }
}
