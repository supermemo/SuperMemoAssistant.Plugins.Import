using System;
using System.Threading.Tasks;
using Anotar.Serilog;
using Flurl.Http;
using SuperMemoAssistant.Services.HTML;

namespace SuperMemoAssistant.Plugins.Import.Extensions
{
  public static class HttpEx
  {
    public static IFlurlRequest CreateRequest(this string url, IFlurlClient client = null, string userAgent = null)
    {
      userAgent = string.IsNullOrWhiteSpace(userAgent) ? HttpConst.UserAgentIE11 : userAgent;

      return client == null
        ? url.WithHeader(HttpConst.UserAgentField, userAgent)
        : client.Request(url).WithHeader(HttpConst.UserAgentField, userAgent);
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
