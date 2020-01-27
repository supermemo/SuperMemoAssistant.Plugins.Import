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
// Modified On:  2020/01/25 10:38
// Modified By:  Alexis

#endregion




using System;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Serilog;
using Nito.AsyncEx;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses.Browser;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Plugins.Import
{
  public partial class BrowserHostService : IBrowserHostService
  {
    #region Constants & Statics

    private const long BrowserRespDelay = 2000;

    #endregion




    #region Properties & Fields - Non-Public

    private readonly AsyncManualResetEvent _browserRespLock;
    private          BrowserMessage        _result;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public RemoteTask<RespTabs> GetTabs()
    {
      //_browserRespLock.Reset();

      var msgType = MessageType.GetTabs;
      _host.Write(new MessageBase(msgType)).RunAsync();

      return WaitBrowserResp<RespTabs>(msgType);
    }

    #endregion




    #region Methods

    private async Task<T> WaitBrowserResp<T>(MessageType messageType)
      where T : class
    {
      try
      {
        var remainingTime = BrowserRespDelay;
        var startTime     = DateTime.Now;

        do
        {
          var cts = new CancellationTokenSource();
          cts.CancelAfter((int)remainingTime);

          await _browserRespLock.WaitAsync(cts.Token);

          if (_result != null && _result.Type == messageType)
          {
            if (_result.GetData<T>(out var data, out var jsonEx) == false)
              LogTo.Warning(jsonEx, $"Failed to deserialize browser response for MessageType {messageType}");

            return data;
          }

          remainingTime = startTime.Ticks + BrowserRespDelay - DateTime.Now.Ticks;
        } while (remainingTime > 0);

        return null;
      }
      catch (TaskCanceledException ex)
      {
        LogTo.Warning(ex, $"Delay expired while waiting for {Enum.GetName(typeof(MessageType), messageType)} response");
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception thrown while waiting for browser response");
      }
      finally
      {
        _browserRespLock.Reset();
      }
      
      return null;
    }

    private void SetBrowserResp(BrowserMessage msg)
    {
      _result = msg;
      _browserRespLock.Set();
    }

    #endregion
  }
}
