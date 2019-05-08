using System;
using System.Threading.Tasks;
using Anotar.Serilog;
using Flurl.Http;
using SuperMemoAssistant.Services.HTML;

namespace SuperMemoAssistant.Plugins.Import.Extensions
{
  public static class HttpEx
  {
    public static IFlurlRequest CreateRequest(this string url)
    {
      return url.WithHeader(HttpConst.UserAgentField, HttpConst.UserAgentIE11);
    }

    public static async Task<string> SafeGetStringAsync(this string url)
    {
      try
      {
        return await url.CreateRequest().GetStringAsync();
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex, $"Exception while downloading '{url}'");
        return null;
      }
    }
  }
}
