﻿#region License & Metadata

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

#endregion




namespace SuperMemoAssistant.Plugins.Import.UI
{
  using System.Windows;
  using System.Windows.Input;
  using Sys.Windows.Input;

  /// <summary>Interaction logic for TermsOfLicense.xaml</summary>
  public partial class TermsOfLicense : Window
  {
    #region Constructors

    protected TermsOfLicense()
    {
      OkCommand = new RelayCommand(Ok, OkCanExecute);

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public ICommand OkCommand { get; }

    public bool HasAgreedToTermsOfLicense { get; set; }
    public bool HasAgreedToBugWaiver      { get; set; }

    #endregion




    #region Methods

    private bool OkCanExecute()
    {
      return HasAgreedToTermsOfLicense && HasAgreedToBugWaiver;
    }

    private void Ok()
    {
      DialogResult = true;
    }

    public static bool AskConsent()
    {
      return Application.Current.Dispatcher.Invoke(() =>
      {
        var setup = new TermsOfLicense();

        setup.ShowDialog();

        return setup.DialogResult ?? false;
      });
    }

    #endregion
  }
}
