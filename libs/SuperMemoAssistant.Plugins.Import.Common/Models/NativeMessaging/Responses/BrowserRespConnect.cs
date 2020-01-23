using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses
{
  public class BrowserRespConnect
  {
    public BrowserRespConnect(string versionName)
    {
      VersionName = versionName;
    }

    public string VersionName { get; set; }
  }
}