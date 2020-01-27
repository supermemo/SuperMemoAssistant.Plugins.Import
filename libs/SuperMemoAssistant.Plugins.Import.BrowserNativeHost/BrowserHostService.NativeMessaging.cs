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
// Modified On:  2020/01/26 17:28
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Requests;
using SuperMemoAssistant.Plugins.Import.Models.Requests;
using SuperMemoAssistant.Plugins.Import.Models.Responses;

namespace SuperMemoAssistant.Plugins.Import
{
  public partial class BrowserHostService
  {
    #region Methods

    private async Task ProcessMessage(BrowserMessage msg)
    {
      try
      {
        using (await _pluginSvcLock.LockAsync())
        {
          if (PluginSvc == null)
          {
            await _host.Write(new RespError(MessageType.ImportTabs, "Plugin not connected"));
            return;
          }

          switch (msg.Type)
          {
            case MessageType.ImportTabs:
              PluginSvc.ImportUrls(msg.GetData<ReqImportUrls>());
              break;

            case MessageType.ImportHtml:
              break;

            case MessageType.GetTabs:
              SetBrowserResp(msg);
              break;

            default:
              LogTo.Error($"Received unknown MessageType {msg.Type}");
              break;
          }
        }
      }
      catch (ArgumentNullException ex)
      {
        LogTo.Error(ex, "Argument Null Exception caught in ProcessMessage");
      }
      catch (RemotingException) { }
    }

    private async Task<bool> WaitForBrowserConnect()
    {
      var response = await _host.Read<BrowserMessage>();

      if (response?.Type != MessageType.Connect)
        throw new InvalidOperationException($"Expected to receive MessageType Connect, but received {response?.Type} instead. Aborting");

      if (response.GetData<ReqConnect>(out var data, out var jsonEx) == false)
      {
        LogTo.Error(jsonEx, "Failed to deserialize Connect message from browser");
        return false;
      }

      _userAgent = data.UserAgent;

      LogTo.Debug($"Connected to browser \"{_userAgent}\"");

      return true;
    }

    private void OnBrowserDisconnected()
    {
      LogTo.Debug("Browser disconnected");

      if (PluginSvc == null)
        return;

      try
      {
        PluginSvc.Disconnect(_extensionId);
      }
      catch (RemotingException) { }
    }

    #endregion
  }
}
