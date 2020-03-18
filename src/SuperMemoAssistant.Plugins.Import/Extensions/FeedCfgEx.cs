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
// Created On:   2019/04/13 21:07
// Modified On:  2019/04/14 02:54
// Modified By:  Alexis

#endregion




using System;
using CodeHollow.FeedReader;
using SuperMemoAssistant.Plugins.Import.Configs;

namespace SuperMemoAssistant.Plugins.Import.Extensions
{
  public static class FeedCfgEx
  {
    #region Methods

    public static string InterpolateUrl(this FeedCfg feedCfg)
    {
      return feedCfg.SourceUrl.Interpolate(
        ("now", DateTime.Now),
        ("lastPubDate", feedCfg.LastPubDate),
        ("lastRefreshDate", feedCfg.LastRefreshDate)
      );
    }

    public static bool ShouldExcludeCategory(this FeedCfg feedCfg, FeedItem feedItem)
    {
      foreach (var catFilter in feedCfg.CategoryFilters)
        switch (catFilter.Mode)
        {
          case Models.FilterMode.Exclude:
            if (feedItem.Categories?.Contains(catFilter.Category) ?? false)
              return true;

            break;

          case Models.FilterMode.Include:
            if (feedItem.Categories?.Contains(catFilter.Category) ?? false)
              return false;

            break;

          default:
            throw new NotImplementedException($"No such FilterMode {catFilter.Mode}");
        }

      return false;
    }

    #endregion
  }
}
