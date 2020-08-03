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
// Modified On:  2020/03/12 13:12
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.HTML.Models;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Services.UI.Configuration.ElementPicker;
using SuperMemoAssistant.Services.UI.Forms.Types;

namespace SuperMemoAssistant.Plugins.Import.Configs
{
  [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
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
  public class WebsiteCfg : CfgBase<WebsiteCfg>, IElementPickerCallback, INotifyPropertyChanged
  {
    #region Constructors

    public WebsiteCfg()
    {
      UrlPattern = new ObservableCollection<UrlPattern>();
    }

    #endregion




    #region Properties & Fields - Public

    [Field(Name = "Name")]
    public string Name { get; set; }

    [Field]
    [DirectContent]
    [JsonIgnore]
    public CrudList<UrlPattern> UrlPatternField { get; set; }

    [Field(Name = "Title regex (optional)")]
    public string TitleRegexPattern { get; set; }

    [Field(Name = "Date regex (optional)")]
    public string DateRegexPattern { get; set; }

    [Field(Name = "User agent (optional)")]
    public string UserAgent { get; set; }
    [Field(Name = "Cookie (optional)")]
    public string Cookie { get; set; }
    [Field(Name = "Link parameter (optional)")]
    public string LinkParameter { get; set; }

    [Field(Name = "Execute javascript")]
    public bool ExecuteJavascript { get; set; }

    [Field(Name = "Load iframes")]
    public bool LoadIframes { get; set; }

    [Field(Name = "Render svg (base64)")]
    public bool InlineSvg { get; set; }
    [Field(Name = "Inline images (base64)")]
    public bool InlineImages { get; set; }

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
    [Field(Name       = "Root Element",
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

    public ObservableCollection<UrlPattern> UrlPattern { get; set; }


    //
    // Helpers

    protected override IEnumerable<Type> InnerTypesToMap => new[] { typeof(UrlPattern), typeof(HtmlFilters) };

    [JsonIgnore]
    public ObservableCollection<HtmlFilter> Filters => FiltersRoot.Children;

    [JsonIgnore]
    public IElement RootElement => RootDictElementId <= 0 ? null : Svc.SM.Registry.Element[RootDictElementId];

    [JsonIgnore]
    public Regex TitleRegex { get; private set; }

    [JsonIgnore]
    public Regex DateRegex { get; private set; }

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

    public void OnUrlPatternChanged()
    {
      UrlPatternField = new CrudList<UrlPattern>("Configured urls:", UrlPattern)
      {
        Height              = new GridLength(120.0),
        SortingDirection    = ListSortDirection.Descending,
        SortingPropertyName = "Priority"
      };
    }

    private void OnTitleRegexPatternChanged()
    {
      try
      {
        TitleRegex = TitleRegexPattern == null
          ? null
          : new Regex(TitleRegexPattern, RegexOptions.Compiled);
      }
      catch
      {
        // Ignored
      }
    }

    private void OnDateRegexPatternChanged()
    {
      try
      {
        DateRegex = DateRegexPattern == null
          ? null
          : new Regex(DateRegexPattern, RegexOptions.Compiled);
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
