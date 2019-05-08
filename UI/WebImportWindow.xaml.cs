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
using System.Globalization;
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
using SuperMemoAssistant.Plugins.Import.Extensions;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Windows.Input;
using Flurl.Http;

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


    private WebsitesCfg Config => Svc<ImportPlugin>.Plugin.WebConfig;



    #region Properties & Fields - Public

    public ICommand ImportCommand => new AsyncRelayCommand(Import);
    public ICommand CancelCommand => new RelayCommand(Close);

    #endregion




    #region Methods

    private async Task Import()
    {
      var downloadResults = await Download();
      var builders = new List<ElementBuilder>();

      foreach (var (content, cfg, url) in downloadResults)
        builders.Add(
          new ElementBuilder(ElementType.Topic, new TextContent(true, content))
            .ConfigureWeb(cfg)
            .WithReference(
              r => r.ConfigureWeb(cfg, content, DateTime.Now.ToString(CultureInfo.InvariantCulture))
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

    private async Task<IEnumerable<(string content, WebsiteCfg cfg, string url)>> Download()
    {
      var urls = tbUrls.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      
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
        var httpReq  = cfg?.CreateRequest(url) ?? url.CreateRequest();
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

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Enter:
          //BtnOk.SimulateClick();
          break;

        case Key.Escape:
          e.Handled = BtnCancel.SimulateClick();
          break;
      }
    }

    #endregion
  }
}
