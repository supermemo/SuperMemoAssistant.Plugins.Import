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
// Created On:   2019/04/10 18:06
// Modified On:  2019/04/22 14:22
// Modified By:  Alexis

#endregion


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Forge.Forms;
using SuperMemoAssistant.Plugins.Import.Configs;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Plugins.Import.Models.Feeds
{
  public class FeedList : ObservableCollection<FeedCfg>
  {
    #region Constructors

    /// <inheritdoc />
    public FeedList() { }

    /// <inheritdoc />
    public FeedList(List<FeedCfg> list) : base(list) { }

    /// <inheritdoc />
    public FeedList(IEnumerable<FeedCfg> collection) : base(collection) { }

    #endregion




    #region Properties & Fields - Public

    public ICommand NewCommand    => new AsyncRelayCommand(NewFeed);
    public ICommand DeleteCommand => new RelayCommand<FeedCfg>(DeleteFeed);
    public ICommand EditCommand   => new AsyncRelayCommand<FeedCfg>(EditFeed);
    public ICommand RunNowCommand => new AsyncRelayCommand(RunNow);

    #endregion




    #region Methods

    private async Task NewFeed()
    {
      var feed = new FeedCfg();
      var res  = await feed.ShowWindow();

      if (res.Action is "cancel")
        return;

      Add(feed);
    }

    private void DeleteFeed(FeedCfg feed)
    {
      if (Show.Window().For(new Confirmation("Are you sure ?")).Result.Model.Confirmed)
        Remove(feed);
    }

    private Task EditFeed(FeedCfg feed)
    {
      return feed.ShowWindow();
    }

    private Task RunNow()
    {
      return Svc<ImportPlugin>.Plugin.DownloadAndImportFeeds(false, false);
    }

    #endregion
  }
}
