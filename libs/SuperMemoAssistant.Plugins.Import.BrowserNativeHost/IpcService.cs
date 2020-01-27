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
// Modified On:  2020/01/26 19:52
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models;

namespace SuperMemoAssistant.Plugins.Import
{
  internal class IpcService
  {
    #region Constants & Statics

    private const int RetryDelayMs = 2500;

    #endregion




    #region Properties & Fields - Non-Public

    private readonly BrowserHostService _hostService;

    private string _channelName;

    #endregion




    #region Constructors

    public IpcService(BrowserHostService hostService)
    {
      _hostService = hostService;

      StartIpcServer();
      OnDisconnected().RunAsync();
    }

    #endregion




    #region Properties & Fields - Public

    public bool Exit { get; set; } = false;

    #endregion




    #region Methods

    private void StartIpcServer()
    {
      _channelName = RemotingServicesEx.GenerateIpcServerChannelName();
      RemotingServicesEx.CreateIpcServer<IBrowserHostService, BrowserHostService>(_hostService, _channelName);
    }

    public async Task OnDisconnected()
    {
      try
      {
        if (await TryConnection())
          return;

        Task.Factory.StartNew(RetryConnection, TaskCreationOptions.LongRunning).RunAsync();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught in OnDisconnected");
        throw;
      }
    }

    private async Task<bool> TryConnection()
    {
      try
      {
        var pluginSvc = RemotingServicesEx.ConnectToIpcServer<IImportPluginService>(ImportConst.ChannelName);

        if (await _hostService.OnPluginConnected(pluginSvc, _channelName))
        {
          var localPluginSvc = pluginSvc;

          Task.Factory.StartNew(() => KeepAlive(localPluginSvc), TaskCreationOptions.LongRunning).RunAsync();
          return true;
        }
      }
      catch (RemotingException) { }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught in TryConnection");
        throw;
      }

      return false;
    }

    private async Task RetryConnection()
    {
      try
      {
        while (Exit == false && await TryConnection() == false)
          await Task.Delay(RetryDelayMs);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught in RetryConnection");
        throw;
      }
    }

    private async Task KeepAlive(IImportPluginService pluginSvc)
    {
      try
      {
        while (true)
        {
          pluginSvc.KeepAlive();

          await Task.Delay(RetryDelayMs);
        }
      }
      catch (RemotingException)
      {
        await _hostService.OnPluginDisconnected();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception caught in KeepAlive");
        throw;
      }
    }

    #endregion
  }
}
