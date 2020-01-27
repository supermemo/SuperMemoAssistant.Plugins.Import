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
// Created On:   2020/01/24 16:59
// Modified On:  2020/01/24 17:00
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using Flurl.Http;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Plugins.Import.Configs;
using SuperMemoAssistant.Plugins.Import.Extensions;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses.Plugin;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.Import.Tasks
{
  public class WebImporter
  {
    #region Constants & Statics

    public static WebImporter Instance { get; } = new WebImporter();

    #endregion




    #region Properties & Fields - Non-Public

    private WebsitesCfg Config => Svc<ImportPlugin>.Plugin.WebConfig;

    #endregion




    #region Constructors

    protected WebImporter() { }

    #endregion




    #region Methods

    public async Task<RespImport> Import(IEnumerable<string> urls)
    {
      var downloadResults = await Download(urls);
      var builders        = new List<ElementBuilder>();

      foreach (var (content, cfg, url) in downloadResults)
        builders.Add(
          new ElementBuilder(ElementType.Topic, new TextContent(true, content))
            .ConfigureWeb(cfg)
            .WithReference(
              r => r.ConfigureWeb(cfg, content, DateTime.Now.ToString(CultureInfo.InvariantCulture))
                    .WithLink(url))
            .DoNotDisplay()
        );

      var success = Svc.SM.Registry.Element.Add(
        out var results,
        ElemCreationFlags.CreateSubfolders,
        builders.ToArray()
      );

      return new RespImport(MessageType.ImportTabs, success, results);
    }

    private async Task<IEnumerable<(string content, WebsiteCfg cfg, string url)>> Download(IEnumerable<string> urls)
    {
      var dlTasks = urls.Select(Download);
      var results = await Task.WhenAll(dlTasks);

      return results.Where(c => c.content != null);
    }

    private async Task<(string content, WebsiteCfg cfg, string url)> Download(string url)
    {
      var cfg = Config.FindConfig(url);
      var matchingRule = cfg == null
        ? string.Empty
        : $", with matching rule {cfg.Name}";

      try
      {
        var httpReq = cfg?.CreateRequest(
            url,
            string.IsNullOrWhiteSpace(cfg.Cookie) ? null : new FlurlClient() /*.Configure(s => s.CookiesEnabled = false)*/)
          ?? url.CreateRequest();
        var content = await httpReq.GetStringAsync();

        if (content == null)
        {
          LogTo.Warning(
            $"Failed to download content for url '{url}'{matchingRule}.");
          return default;
        }

        content = await cfg.ProcessContent(content, url);

        return (content, cfg, url);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, $"Exception caught while importing url '{url}'{matchingRule}.");

        return default;
      }
    }

    #endregion
  }
}
