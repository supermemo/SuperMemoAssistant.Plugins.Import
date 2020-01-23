using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Sys.Converters.Json;

namespace SuperMemoAssistant.Plugins.Import.Models
{
  internal class BrowserMessage
  {
    public BrowserMessageType Type { get; set; }

    [JsonConverter(typeof(JsonConverterObjectToString))]
    public string Data { get; set; }

    public T GetData<T>()
    {
      Data.Deserialize<T>();
    }
  }
}