using System;
using System.IO;
using System.Threading.Tasks;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging;
using SuperMemoAssistant.Plugins.Import.Models.NativeMessaging.Responses;

namespace SuperMemoAssistant.Plugins.Import.NativeMessaging
{
  public class NativeMessaging
  {
    public async Task Run()
    {
      var host = new NativeMessagingHost();

      try
      {
        while (true)
        {
          var response = await host.Read<BrowserMessageType>();
        }
      }
      catch (EndOfStreamException)
      {
        // Disconnected
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "An exception occured while pooling messages from browsers");
      }
    }

    private Task ProcessMessage(NativeMessagingHost host, BrowserMessageType type)
    {
      switch (type)
      {
        case BrowserMessageType.Connect:
          return Connect(host);

        default:
          LogTo.Error($"Received unknown BrowserMessageType {type}");
          break;
      }

      return Task.CompletedTask;
    }

    private async Task Connect(NativeMessagingHost host)
    {
      var assemblyVersion = typeof(ImportPlugin).GetAssemblyVersion();
      var resp = new BrowserRespConnect(assemblyVersion);

      await host.Write(resp);
    }
  }
}