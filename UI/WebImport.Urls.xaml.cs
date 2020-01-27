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
// Modified On:  2020/01/25 13:14
// Modified By:  Alexis

#endregion




using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Forge.Forms;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models.UI;
using SuperMemoAssistant.Plugins.Import.Tasks;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Plugins.Import.UI
{
  /// <summary>Interaction logic for WebImportUrls.xaml</summary>
  public partial class WebImportUrls : UserControl, IImportControl
  {
    #region Constructors

    public WebImportUrls()
    {
      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public ICommand ImportCommand => new AsyncRelayCommand(Import);
    public ICommand CancelCommand => new RelayCommand(Close);

    #endregion




    #region Methods

    private async Task Import()
    {
      var urls = tbUrls.SplitLines();

      var (success, results) = await WebImporter.Instance.Import(urls);

      if (success == false)
      {
        var msg = results.GetErrorString();
        Show.Window().For(new Alert(msg, "Import: Error")).RunAsync();
      }
    }

    private void Close()
    {
      Window.GetWindow(this).Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Enter when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
          ImportCommand.Execute(null);
          e.Handled = true;
          break;

        case Key.Escape:
          CancelCommand.Execute(null);
          e.Handled = true;
          break;
      }
    }

    #endregion
  }
}
