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




namespace SuperMemoAssistant.Plugins.Import.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using Anotar.Serilog;
  using CodeHollow.FeedReader;
  using Configs;
  using Extensions;
  using Flurl.Http;
  using Forge.Forms;
  using Interop.SuperMemo.Content.Contents;
  using Interop.SuperMemo.Elements.Builders;
  using Interop.SuperMemo.Elements.Models;
  using Models.Feeds;
  using Services;
  using SuperMemoAssistant.Extensions;

  public class FeedsImporter
  {
    #region Constants & Statics

    public static FeedsImporter Instance { get; } = new FeedsImporter();

    #endregion




    #region Properties & Fields - Non-Public

    private WebsitesCfg WebsitesConfig => Svc<ImportPlugin>.Plugin.WebConfig;
    private FeedsCfg    FeedsConfig    => Svc<ImportPlugin>.Plugin.FeedsConfig;

    #endregion




    #region Constructors

    protected FeedsImporter() { }

    #endregion




    #region Methods

    public async Task<List<FeedData>> DownloadFeedsAsync()
    {
      LogTo.Debug("Downloading feeds");
      var res = await Task.WhenAll(FeedsConfig.Feeds
                                              .Where(fc => fc.Enabled)
                                              .Select(DownloadFeedAsync)).ConfigureAwait(false);

      var feedsData = res.Where(fd => fd != null).ToList();

      LogTo.Debug("Downloaded {V} items", feedsData.Sum(fd => fd.NewItems.Count));

      return feedsData;
    }

    private async Task<FeedData> DownloadFeedAsync(FeedCfg feedCfg)
    {
      try
      {
        var lastRefresh = DateTime.Now;
        var feed        = await FeedReader.ReadAsync(feedCfg.InterpolateUrl()).ConfigureAwait(false);

        if (feed?.Items == null || feed.Items.Count <= 0)
          return null;

        feedCfg.PendingRefreshDate = lastRefresh;
        var feedData = await DownloadFeedContentsAsync(new FeedData(feedCfg, feed)).ConfigureAwait(false);

        if (feedData == null)
          return null;

        feedData.NewItems = feedData.NewItems
                                    .OrderByDescending(fi => fi.PublishingDate ?? DateTime.MinValue)
                                    .ToList();

        return feedData;
      }
      catch (Exception ex)
      {
        LogTo.Warning(ex, "Exception while reading feed {Name}", feedCfg.Name);
      }

      return null;
    }

    private async Task<FeedData> DownloadFeedContentsAsync(FeedData feedData)
    {
      var feedCfg = feedData.FeedCfg;

      var feedItemTasks = feedData.Feed.Items.Select(
        feedItem => DownloadFeedContentAsync(feedCfg,
                                             feedItem)
      );
      var feedItems = await Task.WhenAll(feedItemTasks).ConfigureAwait(false);

      feedData.NewItems.AddRange(feedItems.Where(fi => fi != null));

      return feedData.NewItems.Any() == false ? null : feedData;
    }

    private async Task<FeedItemExt> DownloadFeedContentAsync(FeedCfg  feedCfg,
                                                             FeedItem feedItem)
    {
      WebsiteCfg webCfg = null;

      try
      {
        if (feedItem.Link != null)
        {
          webCfg        = WebsitesConfig.FindConfig(feedItem.Link);
          feedItem.Link = feedItem.MakeLink(webCfg);
        }

        //
        // Check & update publishing dates

        if (feedCfg.UsePubDate)
        {
          if (feedItem.PublishingDate == null)
          {
            LogTo.Warning("Date missing, or unknown format for feed {Name}, item title '{Title}', raw date '{PublishingDateString}'",
                          feedCfg.Name, feedItem.Title, feedItem.PublishingDateString);
            return null;
          }

          if (feedItem.PublishingDate <= feedCfg.LastPubDate)
            return null;
        }

        //
        // Check guid

        if (feedCfg.UseGuid && feedCfg.EntriesGuid.Contains(feedItem.Id))
          return null;

        //
        // Check categories

        if (feedCfg.ShouldExcludeCategory(feedItem))
          return null;

        //
        // Download content or use inline content

        if (feedItem.Link != null)
        {
          var httpReq = webCfg?.CreateRequest(
              feedItem.Link,
              string.IsNullOrWhiteSpace(webCfg.Cookie) ? null : new FlurlClient() /*.Configure(s => s.CookiesEnabled = false)*/)
            ?? feedItem.Link.CreateRequest();

          var httpResp = await httpReq.GetStringAsync().ConfigureAwait(false);

          if (httpResp != null)
          {
            feedItem.Content = httpResp;
          }
          else
          {
            feedItem.Content = null;
            LogTo.Warning("Failed to download content for feed {Name}, item title '{Title}', link '{Link}'.", feedCfg.Name,
                          feedItem.Title, feedItem.Link);
          }
        }

        else
        {
          feedItem.Content ??= feedItem.Description;
        }

        if (string.IsNullOrWhiteSpace(feedItem.Content))
          return null;

        //
        // Process content if necessary & push back

        feedItem.Content = await webCfg.ProcessContent(feedItem.Content, feedItem.Link).ConfigureAwait(false);

        // Add feed item
        return new FeedItemExt(feedItem, webCfg);
      }
      catch (UriFormatException ex)
      {
        LogTo.Warning(ex, "Invalid content URI in feed {Name}, item title '{Title}', link '{Link}'.", feedCfg.Name,
                      feedItem.Title, feedItem.Link);
      }
      catch (FlurlHttpException ex)
      {
        LogTo.Warning(ex, "Failed to download content for feed {Name}, item title '{Title}', link '{Link}'.", feedCfg.Name,
                      feedItem.Title, feedItem.Link);
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while downloading content for feed {Name}, item title '{Title}', link '{Link}'", feedCfg.Name,
                    feedItem.Title, feedItem.Link);
      }

      return null;
    }

#pragma warning disable 1998
    public static async Task ImportFeedsAsync(
#pragma warning restore 1998
      ICollection<FeedData> feedsData)
    {
      try
      {
        var builders = new List<ElementBuilder>();

        foreach (var feedData in feedsData)
        foreach (var feedItem in feedData.NewItems.Where(fi => fi.IsSelected))
        {
          var webCfg = feedItem.WebCfg;
          var html   = feedItem.Content;

          builders.Add(
            new ElementBuilder(ElementType.Topic,
                               new TextContent(true, html))
              .ConfigureWeb(webCfg)
              .WithReference(
                // ReSharper disable once PossibleInvalidOperationException
                r => r.WithDate(feedItem.PublishingDate?.ToString(CultureInfo.InvariantCulture) ?? webCfg.ParseDateString(html))
                      .WithTitle(feedItem.Title                                                 ?? webCfg.ParseTitle(html))
                      .WithAuthor(feedItem.Author)
                      .WithComment(StringEx.Join(", ", feedItem.Categories))
                      .WithSource($"Feed: {feedData.FeedCfg.Name} (<a>{feedData.FeedCfg.SourceUrl}</a>)")
                      .WithLink(feedItem.Link)
              )
              .DoNotDisplay()
          );
        }

        var res = Svc.SM.Registry.Element.Add(
          out var results,
          ElemCreationFlags.CreateSubfolders,
          builders.ToArray()
        );

        if (res == false)
        {
          var msg = results.GetErrorString();
          Show.Window().For(new Alert(msg, "Feeds: Error")).RunAsync();

          return;
        }

        // Update feeds state

        foreach (var feedData in feedsData)
        {
          var lastPubDate = feedData.FeedCfg.LastPubDate;
          var feedItems   = feedData.NewItems.Where(fi => fi.IsSelected).ToList();

          foreach (var feedItem in feedItems)
          {
            // published date time
            if (feedItem.PublishingDate.HasValue)
              feedData.FeedCfg.LastPubDate = feedItem.PublishingDate > lastPubDate
                ? feedItem.PublishingDate.Value
                : lastPubDate;

            // Guid
            feedData.FeedCfg.EntriesGuid.Add(feedItem.Id);
          }

          if (feedItems.Any())
            feedData.FeedCfg.LastRefreshDate = feedData.FeedCfg.PendingRefreshDate;
        }

        Svc<ImportPlugin>.Plugin.SaveConfig();
      }
      catch (Exception ex)
      {
        // TODO: report error through callback & display
        LogTo.Error(ex, "Exception while importing feed item in SuperMemo");
      }
    }

    #endregion
  }
}
