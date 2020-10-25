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
// Modified On:  2020/02/21 16:46
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Anotar.Serilog;
using Microsoft.Win32;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Configs;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Tasks;
using SuperMemoAssistant.Plugins.Import.UI;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Services.UI.Configuration;
using Extensions.System.IO;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Plugins.Import
{
  using Interop.SuperMemo.Core;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global

  public class ImportPlugin : SentrySMAPluginBase<ImportPlugin>
  {
    #region Properties & Fields - Non-Public

    private ImportPluginService _importService;

    #endregion




    #region Constructors

    public ImportPlugin()
      : base("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046")
    {
      ServicePointManager.DefaultConnectionLimit = 5;
    }

    #endregion




    #region Properties & Fields - Public

    public WebsitesCfg         WebConfig    => ImportConfig.Websites;
    public FeedsCfg            FeedsConfig  => ImportConfig.Feeds;
    public ImportCollectionCfg ImportConfig { get; private set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "Import";

    public override bool HasSettings => true;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void OnCollectionSelected(SMCollection col)
    {
      base.OnCollectionSelected(col);

      ImportConfig = Svc.CollectionConfiguration.Load<ImportCollectionCfg>() ?? new ImportCollectionCfg();

      _importService = new ImportPluginService();

      CreateBrowserRegistryKeys();

      PublishService<IImportPluginService, ImportPluginService>(_importService, ImportConst.ChannelName);
    }

    /// <inheritdoc />
    protected override void OnSMStarted()
    {
      base.OnSMStarted();

      Svc.SM.UI.ElementWdw.OnAvailable += new ActionProxy(ElementWindow_OnAvailable);

      Svc.HotKeyManager
         .RegisterGlobal(
           "ImportTabs",
           "Import content from a list of url",
           HotKeyScopes.Global,
           new HotKey(Key.B, KeyModifiers.CtrlAltShift),
           () => ImportFromTheWeb(ImportType.Url))
         .RegisterGlobal(
           "ImportBrowser",
           "Import tabs from open browsers",
           HotKeyScopes.Global,
           new HotKey(Key.A, KeyModifiers.CtrlAltShift),
           () => ImportFromTheWeb(ImportType.BrowserTabs));
    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
      var cfgWdw = ConfigurationWindow.ShowAndActivate(WebConfig, FeedsConfig);

      if (cfgWdw == null)
        return;

      cfgWdw.SaveMethod = SaveConfig;
    }
    
    /// <inheritdoc />
    protected override Application CreateApplication()
    {
      return new ImportApp();
    }

    #endregion




    #region Methods

    private void CreateBrowserRegistryKeys()
    {
      try
      {
        var         homePath = new DirectoryPath(AppDomain.CurrentDomain.BaseDirectory);
        RegistryKey hkcu     = Registry.CurrentUser;

        // Chrome
        var chromeKey = hkcu.CreateSubKey(ImportConst.RegistryChromeKey);
        chromeKey.SetValue("", homePath.CombineFile(ImportConst.ChromeManifestFilePath).FullPathWin);

        // Firefox
        var firefoxKey = hkcu.CreateSubKey(ImportConst.RegistryFirefoxKey);
        firefoxKey.SetValue("", homePath.CombineFile(ImportConst.FirefoxManifestFilePath).FullPathWin);

        hkcu.Close();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to create registry keys for browsers native messaging");
      }
    }

    private void ImportFromTheWeb(ImportType type)
    {
      Application.Current.Dispatcher.Invoke(
        () => new ImportWindow(type).ShowAndActivate()
      );
    }

    public async Task DownloadAndImportFeeds(bool downloadInBackground = true, bool lockProtection = true)
    {
      var feedsData = await FeedsImporter.Instance.DownloadFeeds();

      if (feedsData.Count == 0 || feedsData.All(fd => fd.NewItems.Count == 0))
      {
        LogTo.Debug("No new entries downloaded.");
        return;
      }

      Application.Current.Dispatcher.Invoke(
        () =>
        {
          LogTo.Debug("Creating FeedsImportWindow");
          new FeedsImportWindow(feedsData, lockProtection).ShowAndActivate();
        }
      );
    }

    private void ElementWindow_OnAvailable()
    {
      DownloadAndImportFeeds().RunAsync();
    }

    public void SaveConfig()
    {
      SaveConfig(null);
    }

    private void SaveConfig(INotifyPropertyChanged config)
    {
      Svc.CollectionConfiguration.SaveAsync<ImportCollectionCfg>(ImportConfig).RunAsync();
    }

    #endregion
  }
}
