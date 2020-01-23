using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses;

namespace SuperMemoAssistant.Plugins.Import.Models
{
  public interface IBrowserPluginService
  {
    BrowserRespConnect Connect();
  }
}
