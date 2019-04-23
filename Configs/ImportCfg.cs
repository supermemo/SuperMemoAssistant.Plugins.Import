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
// Created On:   2019/04/22 15:02
// Modified On:  2019/04/22 20:58
// Modified By:  Alexis

#endregion




using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.HTML.Models;
using SuperMemoAssistant.Services.UI.Configuration.ElementPicker;
using SuperMemoAssistant.Services.UI.Forms.Types;

namespace SuperMemoAssistant.Plugins.Import.Configs
{
  [Form(Mode = DefaultFields.None)]
  [Title("Website config",
    IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
    "Cancel",
    IsCancel = true)]
  [DialogAction("save",
    "Save",
    IsDefault = true,
    Validates = true)]
  public class ImportCfg : IElementPickerCallback, INotifyPropertyChanged
  {
    #region Properties & Fields - Public

    [Field(Name = "Name")]
    public string Name { get; set; }

    [Field]
    [DirectContent]
    public CrudList<UrlPattern> UrlPattern { get; set; } = new CrudList<UrlPattern>("Configured urls:");

    [Field(Name = "Title regex")]
    public string TitleRegexPattern { get; set; }

    [Field]
    public string Cookie { get; set; }

    [Field(Name = "Priority (%)")]
    [Value(Must.BeGreaterThanOrEqualTo,
      0,
      StrictValidation = true)]
    [Value(Must.BeLessThanOrEqualTo,
      100,
      StrictValidation = true)]
    public double Priority { get; set; } = ImportConst.DefaultPriority;

    [JsonIgnore]
    [Action(ElementPicker.ElementPickerAction,
      "Browse",
      Placement = Placement.Inline)]
    [Field(Name  = "Root Element",
      IsReadOnly = true)]
    public string ElementField => RootElement == null
      ? "N/A"
      : RootElement.ToString();

    [Field]
    [DirectContent]
    public HtmlFilters FiltersRoot { get; set; } = new HtmlFilters();


    //
    // Config only

    public int RootDictElementId { get; set; }


    //
    // Helpers

    [JsonIgnore]
    public ObservableCollection<HtmlFilter> Filters => FiltersRoot.Children;

    [JsonIgnore]
    public IElement RootElement => Svc.SMA.Registry.Element[RootDictElementId <= 0 ? 1 : RootDictElementId];

    [JsonIgnore]
    public Regex TitleRegex { get; private set; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return Name;
    }

    public void SetElement(IElement elem)
    {
      RootDictElementId = elem.Id;

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElementField)));
    }

    #endregion




    #region Methods

    // ReSharper disable once UnusedParameter.Local
    private void OnTitleRegexPatternChanged(object before, object after)
    {
      try
      {
        TitleRegex = after == null
          ? null
          : new Regex((string)after, RegexOptions.Compiled);
      }
      catch
      {
        // Ignored
      }
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
