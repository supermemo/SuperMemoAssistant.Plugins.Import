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
// Created On:   2019/04/22 14:49
// Modified On:  2019/04/29 12:56
// Modified By:  Alexis

#endregion




using System.Collections.ObjectModel;
using System.ComponentModel;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Forms.Types;

namespace SuperMemoAssistant.Plugins.Import.Configs
{
  [Form(Mode = DefaultFields.None)]
  public class WebsitesCfg : INotifyPropertyChanged
  {
    #region Constructors

    public WebsitesCfg()
    {
      Websites = new ObservableCollection<WebsiteCfg>();
    }

    #endregion




    #region Properties & Fields - Public

    [Field]
    [DirectContent]
    [JsonIgnore]
    public CrudList<WebsiteCfg> WebsitesField { get; set; }

    public ObservableCollection<WebsiteCfg> Websites { get; set; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return "Websites";
    }

    #endregion




    #region Methods

    public void OnWebsitesChanged()
    {
      WebsitesField = new CrudList<WebsiteCfg>("Configured websites:", Websites)
      {
        SortingDirection    = ListSortDirection.Ascending,
        SortingPropertyName = "Name"
      };
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
