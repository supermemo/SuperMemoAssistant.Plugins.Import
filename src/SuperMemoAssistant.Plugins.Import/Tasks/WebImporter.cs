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

#endregion




namespace SuperMemoAssistant.Plugins.Import.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Anotar.Serilog;
  using Configs;
  using Extensions;
  using Flurl.Http;
  using Interop.SuperMemo.Content.Contents;
  using Interop.SuperMemo.Elements.Builders;
  using Interop.SuperMemo.Elements.Models;
  using Models.NativeMessaging;
  using Models.NativeMessaging.Responses.Plugin;
  using Services;
  using Sys.Net;
  using Sys.Remoting;

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

    public async Task<RespImport> ImportAsync(IEnumerable<string> urls)
    {
      var downloadResults = await DownloadAsync(urls).ConfigureAwait(false);
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

      var results = await Svc.SM.Registry.Element.AddAsync(
        ElemCreationFlags.CreateSubfolders | ElemCreationFlags.ReuseSubFolders,
        builders.ToArray()
      );

      return new RespImport(MessageType.ImportTabs, results.TrueForAll(r => r.Success), results);
    }

    private async Task<IEnumerable<(string content, WebsiteCfg cfg, string url)>> DownloadAsync(IEnumerable<string> urls)
    {
      var dlTasks = urls.Select(DownloadAsync);
      var results = await Task.WhenAll(dlTasks).ConfigureAwait(false);

      return results.Where(c => c.content != null);
    }

    private async Task<(string content, WebsiteCfg cfg, string url)> DownloadAsync(string url)
    {
      var cfg = Config.FindConfig(url);
      var matchingRule = cfg == null
        ? string.Empty
        : $", with matching rule {cfg.Name}";

      try
      {
        Uri    uri = new Uri(url);
        string content;

        switch (uri.Scheme)
        {
          case UriEx.UriSchemeHttp:
          case UriEx.UriSchemeHttps:
            content = await DownloadHttpAsync(url, cfg).ConfigureAwait(false);
            break;

          case UriEx.UriSchemeFile:
            content = await LoadFileAsync(url).ConfigureAwait(false);
            break;

          default:
            LogTo.Warning("Scheme {Scheme} is not supported. {Url} will be skipped.", uri.Scheme, url);
            return default;
        }

        if (content == null)
        {
          LogTo.Warning("Failed to download content for url '{Url}'{MatchingRule}.", url, matchingRule);
          return default;
        }

        content = await cfg.ProcessContent(content, url).ConfigureAwait(true);

        return (content, cfg, url);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught while importing url '{Url}'{MatchingRule}.", url, matchingRule);

        return default;
      }
    }

    private static Task<string> DownloadHttpAsync(string url, WebsiteCfg cfg)
    {
      IFlurlClient flurlClient = string.IsNullOrWhiteSpace(cfg?.Cookie) ? null : new FlurlClient();

      var httpReq = cfg?.CreateRequest(
          url,
          flurlClient /*.Configure(s => s.CookiesEnabled = false)*/)
        ?? url.CreateRequest();

      flurlClient?.Dispose();

      return httpReq.GetStringAsync();
    }

    private static async Task<string> LoadFileAsync(string url)
    {
      if (File.Exists(url) == false)
        return null;

      using (var sr = File.OpenText(url))
        return await sr.ReadToEndAsync().ConfigureAwait(false);
    }

    #endregion
  }
}
