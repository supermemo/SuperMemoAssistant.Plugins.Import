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
// Modified On:  2020/01/25 16:11
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Models.Browser;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Plugins.Import
{
  public class ImportBrowserManager : INotifyPropertyChanged
  {
    #region Constants & Statics

    public static ImportBrowserManager Instance { get; } = new ImportBrowserManager();

    #endregion




    #region Properties & Fields - Non-Public

    private ConcurrentDictionary<string, RemoteBrowser> BrowserServices { get; } =
      new ConcurrentDictionary<string, RemoteBrowser>();

    #endregion




    #region Constructors

    protected ImportBrowserManager() { }

    #endregion




    #region Properties & Fields - Public

    public int Count => BrowserServices.Count;

    #endregion




    #region Methods

    public async Task<ImportTabs> GetTabs()
    {
      try
      {
        if (BrowserServices.Count == 0)
          return null;

        var browserTabs = new ImportTabs();
        var remoteBrowsers = BrowserServices.Values.ToArray();

        var taskRes = await Task.WhenAll(remoteBrowsers.Select(rb => rb.HostSvc.GetTabs().GetTask()));

        for (int i = 0; i < remoteBrowsers.Length && taskRes[i]?.Tabs != null; i++)
          foreach (var tab in taskRes[i].Tabs)
            browserTabs.Add(new ImportTab(tab, remoteBrowsers[i]));

        return browserTabs;
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex, "Exception caught in GetTabs()");
        throw;
      }
    }

    public bool OnBrowserConnected(string extensionId, string userAgent, string channel)
    {
      if (BrowserServices.ContainsKey(channel))
        return false;

      try
      {
        var hostSvc = RemotingServicesEx.ConnectToIpcServer<IBrowserHostService>(channel);

        BrowserServices[extensionId] = new RemoteBrowser(extensionId, userAgent, hostSvc);

        return true;
      }
      catch (RemotingException)
      {
        return false;
      }
    }

    public void OnBrowserDisconnected(string extensionId)
    {
      BrowserServices.TryRemove(extensionId, out _);
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
