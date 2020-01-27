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
// Modified On:  2020/01/26 18:24
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using SuperMemoAssistant.Plugins.Import.Models;
using SuperMemoAssistant.Plugins.Import.Models.UI;

namespace SuperMemoAssistant.Plugins.Import.UI
{
  /// <summary>Interaction logic for ImportWindow.xaml</summary>
  public partial class ImportWindow : MetroWindow
  {
    #region Properties & Fields - Non-Public

    private readonly ImportType     _initialType;
    private          IImportControl _importControl;

    #endregion




    #region Constructors

    public ImportWindow(ImportType initialType)
    {
      _initialType = initialType;
      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public TabItem ImportTabItem
    {
      set => _importControl = (IImportControl)value.FindChildren<UserControl>().FirstOrDefault();
    }

    #endregion




    #region Methods

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Enter:
          _importControl.ImportCommand.Execute(null);
          e.Handled = true;
          break;

        case Key.Escape:
          _importControl.CancelCommand.Execute(null);
          e.Handled = true;
          break;
      }
    }

    private void MetroWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
      switch (_initialType)
      {
        case ImportType.BrowserTabs:
          tcImport.SelectedIndex = 0;
          break;

        case ImportType.Url:
          tcImport.SelectedIndex = 1;
          break;

        default:
          throw new NotSupportedException($"Invalid ImportType {_initialType}");
      }
    }

    #endregion
  }
}
