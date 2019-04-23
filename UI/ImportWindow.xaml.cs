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
// Created On:   2019/04/22 13:54
// Modified On:  2019/04/22 14:22
// Modified By:  Alexis

#endregion



using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Anotar.Serilog;
using Forge.Forms;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Plugins.Import.Configs;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.HTML;
using SuperMemoAssistant.Services.HTML.Extensions;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Plugins.Import.UI
{
  /// <summary>Interaction logic for ImportWindow.xaml</summary>
  public partial class ImportWindow : Window
  {
    #region Constructors

    public ImportWindow()
    {
      InitializeComponent();
    }

    #endregion


    private ImportCfgs Config => Svc<ImportPlugin>.Plugin.Config;

    private ImportCfg TryFindConfig(string url)
    {
      return Config.Configs.FirstOrDefault(c => c.UrlPattern.Any(p => p.Match(url)));
    }



    #region Properties & Fields - Public

    public ICommand ImportCommand => new AsyncRelayCommand(Import);

    

    #endregion




    #region Methods

    private async Task Import()
    {
      var downloadResults = await Download();
      var builders = new List<ElementBuilder>();

      foreach (var (content, cfg, url) in downloadResults)
        builders.Add(
          new ElementBuilder(ElementType.Topic, new TextContent(true, content))
            .WithParent(cfg?.RootElement)
            .WithPriority(cfg?.Priority ?? ImportConst.DefaultPriority)
            .WithReference(
              r => r.WithDate(DateTime.Now)
                    .WithTitle(cfg?.TitleRegex.Match(content).Groups.SafeGet(1).Value)
                    .WithSource(cfg?.Name)
                    .WithLink(url))
            .DoNotDisplay()
        );

      var res = Svc.SMA.Registry.Element.Add(
        out var addResults,
        ElemCreationFlags.CreateSubfolders,
        builders.ToArray()
      );

      if (res == false)
      {
        var msg = addResults.GetErrorString();
        Forge.Forms.Show.Window().For(new Alert(msg, "Import: Error")).RunAsync();
      }
    }

    private async Task<IEnumerable<(string content, ImportCfg cfg, string url)>> Download()
    {
      var urls = tbUrls.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      
      var throttler = new SemaphoreSlim(10);
      var downloadTasks = urls.Select(u => Download(u, throttler));

      return (await Task.WhenAll(downloadTasks)).Where(c => c.content != null);
    }

    private async Task<(string content, ImportCfg cfg, string url)> Download(string url, SemaphoreSlim throttler)
    {
      var cfg = TryFindConfig(url);
      var matchingRule = cfg == null
        ? string.Empty
        : $", with matching rule {cfg.Name}";

      try
      {
        await throttler.WaitAsync();

        var client = new HttpClient(new HttpClientHandler { UseCookies = false });

        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

        if (cfg != null && string.IsNullOrWhiteSpace(cfg.Cookie) == false)
          client.DefaultRequestHeaders.Add("Cookie", cfg.Cookie);
        
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        var httpResp = await client.SendAsync(httpReq);

        if (httpResp == null || !httpResp.IsSuccessStatusCode)
        {
          LogTo.Warning(
            $"Failed to download content for url '{url}'{matchingRule}. HTTP Status code : {httpResp?.StatusCode}");
          return default;
        }

        var content = await httpResp.Content.ReadAsStringAsync();

        if (cfg != null && cfg.Filters.Any())
          content = string.Join(
            "\r\n",
            cfg.Filters
               .Select(f => f.Filter(content))
               .Where(s => string.IsNullOrWhiteSpace(s) == false)
               .ToList());

        content = HtmlUtils.EnsureAbsoluteLinks(content, new Uri(url));

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
