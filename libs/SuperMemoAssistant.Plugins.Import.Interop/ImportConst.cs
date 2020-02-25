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
// Modified On:  2020/01/26 23:20
// Modified By:  Alexis

#endregion




using Extensions.System.IO;

namespace SuperMemoAssistant.Plugins.Import
{
  public static class ImportConst
  {
    #region Constants & Statics

    public const string ChannelName     = "Import_Su4jWSsxeyWW9UBS";
    public const int    DefaultPriority = 25;

    public const string RegistryFirefoxKey = @"Software\Mozilla\NativeMessagingHosts\supermemoassistant.plugins.import.browserextension";
    public const string RegistryChromeKey =
      @"Software\Google\Chrome\NativeMessagingHosts\supermemoassistant.plugins.import.browserextension";

    public static readonly FilePath ChromeManifestFilePath =
      new FilePath("SuperMemoAssistant.Plugins.Import.BrowserExtension.Manifest.Chrome.json");
    public static readonly FilePath FirefoxManifestFilePath =
      new FilePath("SuperMemoAssistant.Plugins.Import.BrowserExtension.Manifest.Firefox.json");

    #endregion
  }
}
