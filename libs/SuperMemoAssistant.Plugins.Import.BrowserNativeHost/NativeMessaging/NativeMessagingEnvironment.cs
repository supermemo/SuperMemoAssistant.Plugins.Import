using System;
using System.IO;

namespace SuperMemoAssistant.Plugins.Import.NativeMessaging
{
  /// <summary>
  ///
  /// Lyre commit a744eefe559120bf06d3daee5bc20c36ebb01409 2019/08/29
  /// https://github.com/alexwiese/Lyre
  ///
  /// Provides methods for communicating over streams via the Chrome Native Messaging protocol.
  /// </summary>
  public static class NativeMessagingEnvironment
  {
    public static void SuppressConsoleOutput()
    {
      RedirectConsoleOutput(TextWriter.Null);
    }

    public static void RedirectConsoleOutputToErrorOutput()
    {
      RedirectConsoleOutput(new StreamWriter(Console.OpenStandardError()) {AutoFlush = true});
    }

    public static void RedirectConsoleOutput(TextWriter textWriter)
    {
      Console.SetOut(textWriter);
    }
  }
}