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
// Modified On:  2020/03/12 13:11
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Plugins.Import.Configs
{
  [Form(Mode = DefaultFields.None)]
  [Title("Feed Settings",
         IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
                "Cancel",
                IsCancel = true)]
  [DialogAction("save",
                "Save",
                IsDefault = true,
                Validates = true)]
  public class FeedCfg : CfgBase<FeedCfg>, INotifyPropertyChangedEx
  {
    #region Constructors

    public FeedCfg()
    {
      LastPubDate = DateTime.MinValue;
    }

    #endregion




    #region Properties & Fields - Public

    [Field]
    public bool Enabled { get; set; } = true;

    [Field(Name = "Name")]
    [Value(Must.NotBeEmpty)]
    public string Name { get; set; }
    [Field(Name = "Source URL (RSS / Atom)")]
    [Value(Must.NotBeEmpty)]
    public string SourceUrl { get; set; }

    [Field(Name = "Use feed guid ?")]
    public bool UseGuid { get; set; } = true;
    [Field(Name = "Use feed published date ?")]
    public bool UsePubDate { get; set; } = false;

    [JsonIgnore]
    [Field(Name = "Excluded/Included categories (default = exclude)")]
    [MultiLine]
    public string CategoryFiltersString
    {
      get => string.Join("\n", CategoryFilters);
      set => CategoryFilters = value.Replace("\r\n", "\n")
                                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(c => new CategoryFilter(c))
                                    .ToHashSet();
    }


    //
    // Config only

    public HashSet<CategoryFilter> CategoryFilters { get; set; } = new HashSet<CategoryFilter>();

    public HashSet<string> EntriesGuid { get; set; } = new HashSet<string>();

    public DateTime LastPubDate { get; set; }

    public DateTime LastRefreshDate { get; set; }


    //
    // Helpers

    [JsonIgnore]
    public DateTime PendingRefreshDate { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsChanged { get; set; }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
