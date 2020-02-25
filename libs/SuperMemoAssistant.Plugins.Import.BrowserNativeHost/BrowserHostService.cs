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
// Modified On:  2020/02/25 14:09
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using Anotar.Serilog;
using Lyre;
using Newtonsoft.Json;
using Nito.AsyncEx;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses.Plugin;

namespace SuperMemoAssistant.Plugins.Import
{
  public partial class BrowserHostService : MarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly AsyncLock           _pluginSvcLock;
    private readonly string              _extensionId;
    private readonly NativeMessagingHost _host;
    private          IpcService          _ipcService;
    private          string              _userAgent;

    #endregion




    #region Constructors

    public BrowserHostService(string extensionId)
    {
      _pluginSvcLock   = new AsyncLock();
      _browserRespLock = new AsyncManualResetEvent(false);
      _extensionId     = extensionId;
      _host            = new NativeMessagingHost();
    }

    #endregion




    #region Properties & Fields - Public

    public IImportPluginService PluginSvc { get; private set; }

    #endregion




    #region Methods Impl

    public override object InitializeLifetimeService()
    {
      return null;
    }

    #endregion




    #region Methods

    public async Task Run()
    {
      try
      {
        if (await WaitForBrowserConnect() == false)
          return;

        _ipcService = new IpcService(this);

        while (true)
          try
          {
            var response = await _host.Read<BrowserMessage>();

            await ProcessMessage(response);
          }
          catch (JsonSerializationException jsonEx)
          {
            LogTo.Error(jsonEx, "Invalid message received");
          }
      }
      catch (EndOfStreamException)
      {
        OnBrowserDisconnected();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "An exception occured in the Run() loop");
      }
    }

    public async Task<bool> OnPluginConnected(IImportPluginService pluginSvc, string channelName)
    {
      if (pluginSvc == null)
        return false;

      using (await _pluginSvcLock.LockAsync())
      {
        try
        {
          var resp = pluginSvc.ConnectBrowser(_extensionId, _userAgent, channelName);

          if (resp == null)
          {
            LogTo.Warning($"{_extensionId} failed to connect to Plugin");
            return false;
          }

          LogTo.Debug($"Connected to plugin, version {resp.Version}");

          await _host.Write(resp);
          PluginSvc = pluginSvc;

          return true;
        }
        catch (EndOfStreamException) { }

        return false;
      }
    }

    public async Task OnPluginDisconnected()
    {
      using (await _pluginSvcLock.LockAsync())
        if (PluginSvc != null)
        {
          PluginSvc = null;

          try
          {
            await _host.Write(new RespDisconnect());
          }
          catch (EndOfStreamException) { }
        }

      _ipcService.OnDisconnected().RunAsync();
    }

    #endregion
  }
}
