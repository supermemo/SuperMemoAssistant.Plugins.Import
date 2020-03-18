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
// Created On:   2019/04/25 14:07
// Modified On:  2019/04/25 17:42
// Modified By:  Alexis

#endregion


using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using SuperMemoAssistant.Plugins.Import.Configs;

namespace SuperMemoAssistant.Plugins.Import.Models.Feeds
{
  public class FeedItemExt : FeedItem
  {
    #region Constructors

    /// <inheritdoc />
    public FeedItemExt() { }

    /// <inheritdoc />
    public FeedItemExt(FeedItem feedItem, WebsiteCfg webCfg)
    {
      WebCfg               = webCfg;
      Author               = feedItem.Author;
      Categories           = feedItem.Categories;
      Content              = feedItem.Content;
      Description          = feedItem.Description;
      Id                   = feedItem.Id;
      Link                 = feedItem.Link;
      PublishingDate       = feedItem.PublishingDate;
      PublishingDateString = feedItem.PublishingDateString;
      SpecificItem         = feedItem.SpecificItem;
      Title                = feedItem.Title;
    }

    /// <inheritdoc />
    public FeedItemExt(BaseFeedItem feedItem) : base(feedItem) { }

    #endregion




    #region Properties & Fields - Public

    public WebsiteCfg WebCfg { get; }

    public bool IsSelected { get; set; } = true;

    #endregion
  }
}
