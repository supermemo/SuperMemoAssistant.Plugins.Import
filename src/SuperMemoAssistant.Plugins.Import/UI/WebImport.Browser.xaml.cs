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
// Modified On:  2020/01/26 21:32
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Forge.Forms;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.Import.Models.Browser;
using SuperMemoAssistant.Plugins.Import.Models.UI;
using SuperMemoAssistant.Plugins.Import.Tasks;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Plugins.Import.UI
{
  /// <summary>Interaction logic for WebImportUrls.xaml</summary>
  public partial class WebImportBrowser : UserControl, IImportControl, INotifyPropertyChanged
  {
    #region Constructors

    public WebImportBrowser()
    {
      InitializeComponent();

      CancelCommand = new RelayCommand(Close);
      ImportCommand = new AsyncRelayCommand(Import, () => IsAnyBrowserConnected);

      RefreshCommand.Execute(null);
    }

    #endregion




    #region Properties & Fields - Public

    public ICommand RefreshCommand => new AsyncRelayCommand(Refresh, () => IsAnyBrowserConnected);

    public bool IsAnyBrowserConnected => ImportBrowserManager.Instance.Count > 0;

    public ImportTabs Tabs { get; private set; }

    #endregion




    #region Properties Impl - Public

    public ICommand ImportCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion




    #region Methods

    private async Task Refresh()
    {
      Tabs = await ImportBrowserManager.Instance.GetTabs();
    }

    private async Task Import()
    {
      var urls = Tabs.Where(t => t.Selected)
                     .Select(t => t.Tab.Url);

      var (success, results) = await WebImporter.Instance.Import(urls);

      if (success == false)
      {
        var msg = results.GetErrorString();
        Show.Window().For(new Alert(msg, "Import: Error")).RunAsync();

        for (int i = results.Count - 1; i >= 0; i--)
          if (results[i].Success == false)
            Tabs.RemoveAt(i);
      }

      else
        Close();
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

    private void lvTabs_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ListView listView    = sender as ListView;
      GridView gridView    = listView.View as GridView;
      var      actualWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;

      for (var i = 0; i < gridView.Columns.Count - 1; i++)
        actualWidth -= gridView.Columns[i].ActualWidth;

      gridView.Columns[gridView.Columns.Count - 1].Width = actualWidth;
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
