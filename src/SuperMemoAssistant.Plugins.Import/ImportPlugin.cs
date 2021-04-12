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




namespace SuperMemoAssistant.Plugins.Import
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;
  using System.Net;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Input;
  using Anotar.Serilog;
  using Configs;
  using global::Extensions.System.IO;
  using Interop.SMA.Notifications;
  using Interop.SuperMemo.Core;
  using Microsoft.Toolkit.Uwp.Notifications;
  using Microsoft.Win32;
  using Models;
  using Models.Feeds;
  using Services;
  using Services.IO.Keyboard;
  using Services.Sentry;
  using Services.ToastNotifications;
  using Services.UI.Configuration;
  using SuperMemoAssistant.Extensions;
  using Sys.IO.Devices;
  using Sys.Remoting;
  using Tasks;
  using UI;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global

  public class ImportPlugin : SentrySMAPluginBase<ImportPlugin>
  {
    #region Constants & Statics

    private static List<FeedData> _feedsData;

    #endregion




    #region Properties & Fields - Non-Public

    private ImportPluginService _importService;

    #endregion




    #region Constructors

    public ImportPlugin()
      : base("https://cb6b904487c743fda807d4b2b15663df@o218793.ingest.sentry.io/5506802")
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
    protected override void OnSMStarted(bool wasSMAlreadyStarted)
    {
      Svc.SMA.NotificationMgr.OnToastActivated += new ActionProxy<ToastActivationData>(OnToastActivated);

      if (wasSMAlreadyStarted)
        OnCollectionSelected(Svc.SM.Collection);

      DownloadAndImportFeedsAsync(true).RunAsync();

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

      base.OnSMStarted(wasSMAlreadyStarted);
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

    internal static bool ValidateToS()
    {
      var plugin = Svc<ImportPlugin>.Plugin;

      if (plugin.ImportConfig.HasAgreedToTOS)
        return true;

      var consent = TermsOfLicense.AskConsent();

      if (!consent)
        return false;

      plugin.ImportConfig.HasAgreedToTOS = true;
      plugin.SaveConfig();

      return true;
    }

    private static void CreateBrowserRegistryKeys()
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

    private static void ImportFromTheWeb(ImportType type)
    {
      if (!ValidateToS())
        return;

      Application.Current.Dispatcher.Invoke(
        () => new ImportWindow(type).ShowAndActivate()
      );
    }

    public static async Task DownloadAndImportFeedsAsync(bool inBackground)
    {
      var feedsData = await FeedsImporter.Instance.DownloadFeedsAsync().ConfigureAwait(false);

      if (feedsData.Count == 0 || feedsData.All(fd => fd.NewItems.Count == 0))
      {
        LogTo.Debug("No new entries downloaded.");
        return;
      }

      if (!ValidateToS())
        return;

      if (inBackground)
      {
        _feedsData = feedsData;

        $"You have {feedsData.Sum(fd => fd.NewItems.Count)} Feed Articles waiting to be imported."
          .ShowDesktopNotification(
            ToastButtonEx.Create("View List", ToastActivationType.Background, ("action", "importFeeds_view")),
            new ToastButtonSnooze());

        return;
      }

      Application.Current.Dispatcher.Invoke(
        () => new FeedsImportWindow(feedsData, false).ShowAndActivate()
      );
    }

    private void OnToastActivated(ToastActivationData activationData)
    {
      if (activationData.WasTriggeredByThisPlugin() == false)
        return;

      if (!ValidateToS())
        return;

      var action = activationData.Arguments.SafeGet("action");

      switch (action)
      {
        case "importFeeds_view":
          Application.Current.Dispatcher.Invoke(
            () => new FeedsImportWindow(_feedsData, true).ShowAndActivate()
          );
          break;
      }
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
