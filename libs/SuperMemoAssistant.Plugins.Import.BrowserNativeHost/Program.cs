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
// Created On:   2020/01/24 10:10
// Modified On:  2020/01/24 13:40
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO.Diagnostics;
using SuperMemoAssistant.Services.Sentry;

// ReSharper disable LocalizableElement
// ReSharper disable UnusedParameter.Local

namespace SuperMemoAssistant.Plugins.Import
{
  internal class Program
  {
    #region Constants & Statics

    private static IDisposable _sentry;
    private static Logger _logger;

    #endregion




    #region Methods

    /// <summary>
    ///   This process will be instantiated by the browser.
    /// </summary>
    /// <param name="args"></param>
    private static async Task Main(string[] args)
    {
      if (args.Length < 1)
      {
        PrintUsage();
        return;
      }

      string ext;

      if (args[0].StartsWith("chrome-extension://", StringComparison.InvariantCultureIgnoreCase))
        ext = args[0];

      else if (args.Length >= 2 && args[1].Length == 38
        && args[1].StartsWith("{", StringComparison.InvariantCultureIgnoreCase)
        && args[1].EndsWith("}", StringComparison.InvariantCultureIgnoreCase))
        ext = args[1];

      else
      {
        PrintUsage();
        return;
      }

      SetupLogger(ext);

      var nativeMessaging = new BrowserHostService(ext);

      await nativeMessaging.Run();

      _sentry.Dispose();
      _logger.Shutdown();
    }

    private static void SetupLogger(string ext)
    {
      //
      // Logs

      ext = ext.Replace("chrome-extension://", "chrome-").Replace("/", "")
               .Replace("{", "firefox-").Replace("}", "");

      _logger = LoggerFactory.Create(
        typeof(Program).GetAssemblyName() + $"_{ext}",
        new ConfigurationService(SMAFileSystem.SharedConfigDir),
        l => l.LogToSentry());

      //
      // Sentry

      var hostType = typeof(Program);
      var releaseName = $"Import.BrowserNativeHost@{hostType.GetAssemblyVersion()}";

      _sentry = SentryEx.Initialize("https://a63c3dad9552434598dae869d2026696@sentry.io/1362046", releaseName);
    }

    private static void PrintUsage()
    {
      var processFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
      var exeFileName     = Path.GetFileName(processFilePath);

      Console.WriteLine($"Chrome: {exeFileName} <extension-id> [--parent-window=xx]\nFirefox: {exeFileName} <path-to-manifest> <extension-id>");
      
      Environment.Exit(1);
    }

    #endregion
  }
}
