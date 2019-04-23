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
// Created On:   2019/04/22 13:15
// Modified On:  2019/04/22 21:01
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Configs;
using SuperMemoAssistant.Plugins.Import.UI;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.Import
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  public class ImportPlugin : SentrySMAPluginBase<ImportPlugin>
  {
    #region Constructors

    public ImportPlugin() { }

    #endregion




    #region Properties & Fields - Public

    public ImportCfgs Config { get; private set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "Import";

    public override bool HasSettings => true;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {
      Config = Svc.Configuration.Load<ImportCfgs>().Result ?? new ImportCfgs();

      Svc.HotKeyManager
         .RegisterGlobal(
           "Import",
           "Import content from the web",
           HotKeyScope.Global,
           new HotKey(Key.A, KeyModifiers.CtrlAltShift),
           ImportFromTheWeb);
    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
      Application.Current.Dispatcher.Invoke(
        () => new ConfigurationWindow(Config).ShowAndActivate()
      );
    }

    protected override Application CreateApplication()
    {
      return new ImportApp();
    }

    #endregion




    #region Methods

    private void ImportFromTheWeb()
    {
      Application.Current.Dispatcher.Invoke(
        () => new ImportWindow().ShowAndActivate()
      );
    }

    #endregion
  }
}
