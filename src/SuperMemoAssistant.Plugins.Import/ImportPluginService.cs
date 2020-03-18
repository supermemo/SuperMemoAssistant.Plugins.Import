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
// Created On:   2020/01/24 11:16
// Modified On:  2020/01/24 18:01
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using Anotar.Serilog;
using PluginManager.Interop.Sys;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Requests;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses.Plugin;
using SuperMemoAssistant.Plugins.Import.Tasks;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Plugins.Import
{
  public class ImportPluginService : PerpetualMarshalByRefObject, IImportPluginService
  {
    #region Methods Impl

    /// <param name="extensionId"></param>
    /// <inheritdoc />
    [LogToErrorOnException]
    public RespConnect Connect(string extensionId)
    {
      LogTo.Debug($"Extension {extensionId} connected.");

      var assemblyVersion = GetType().GetAssemblyVersion();

      return new RespConnect(assemblyVersion);
    }

    /// <inheritdoc />
    [LogToErrorOnException]
    public RespConnect ConnectBrowser(string extensionId, string userAgent, string channel)
    {
      var resp = Connect($"{extensionId} ({userAgent})");

      if (ImportBrowserManager.Instance.OnBrowserConnected(extensionId, userAgent, channel) == false)
        return null;

      return resp;
    }

    /// <inheritdoc />
    [LogToErrorOnException]
    public void Disconnect(string extensionId)
    {
      LogTo.Debug($"Extension {extensionId} disconnected.");

      ImportBrowserManager.Instance.OnBrowserDisconnected(extensionId);
    }

    /// <inheritdoc />
    public void KeepAlive() { }

    /// <inheritdoc />
    public RemoteTask<RespImport> ImportUrls(ReqImportUrls req)
    {
      if (req?.Tabs == null)
        throw new ArgumentNullException(nameof(req));

      return WebImporter.Instance.Import(req.Tabs.Select(t => t.Url));
    }

    /// <inheritdoc />
    public RemoteTask<RespImport> ImportHtml(string html)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
