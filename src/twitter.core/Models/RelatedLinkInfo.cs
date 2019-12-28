﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace twitter.core.Models
{
    /// <summary>
    /// When links are included in a tweet and it's not a quote or retweet,
    /// look for og/twitter meta tags in the links. This info, if present,
    /// is displayed similar to quotes.
    /// </summary>
    public class RelatedLinkInfo
    {
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? SiteName { get; set; }

        public TwitterStatus ImageTwitterStatus => new TwitterStatus
        {
            Id = Guid.NewGuid().ToString(),
            ExtendedEntities = new Entities
            {
                Media = new[]
                {
                    new Media
                    {
                        Url = ImageUrl,
                        MediaUrl = ImageUrl,
                        DisplayUrl = ImageUrl,
                        ExpandedUrl = ImageUrl
                    }
                }
            }
        };

        public static async Task<RelatedLinkInfo?> GetRelatedLinkInfo(TwitterStatus status)
        {
            if (status.IsQuoted)
            {
                status.CheckedRelatedInfo = true;
                return status.RelatedLinkInfo;
            }

            var urls = status.Entities?.Urls;
            if (urls == null)
            {
                status.CheckedRelatedInfo = true;
                return status.RelatedLinkInfo;
            }
            foreach (var url in urls)
            {
                try
                {
                    var uri = url.ExpandedUrl ?? url.Url;
                    if (!UrlValid(uri)) continue;
                    var relatedLinkInfo = await ParseHeadersForLinkInfo(uri);
                    if (relatedLinkInfo == null) continue;

                    status.CheckedRelatedInfo = true;
                    if (!UrlValid(relatedLinkInfo.ImageUrl)) relatedLinkInfo.ImageUrl = null;
                    relatedLinkInfo.ImageUrl = status.ExtendedEntities?.Media?[0]?.MediaUrl ?? relatedLinkInfo.ImageUrl;
                    return status.RelatedLinkInfo ?? relatedLinkInfo;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.Message);
                }
            }

            status.CheckedRelatedInfo = true;
            return status.RelatedLinkInfo;
        }

        private static async Task<RelatedLinkInfo?> ParseHeadersForLinkInfo(string? url)
        {
            if (!UrlValid(url)) return null;

            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            var document = new HtmlDocument();
            document.Load(response.GetResponseStream());

            var metaTags = document.DocumentNode.SelectNodes("//meta");
            var metaInfo = new RelatedLinkInfo { Url = url };

            if (metaTags != null)
            {
                foreach (var tag in metaTags)
                {
                    var tagName = tag.Attributes["name"];
                    var tagContent = tag.Attributes["content"];
                    var tagProperty = tag.Attributes["property"];

                    if (tagName != null && tagContent != null)
                    {
                        switch (tagName.Value.ToLower())
                        {
                            case "title":
                                metaInfo.Title = WebUtility.HtmlDecode(tagContent.Value);
                                break;

                            case "description":
                                metaInfo.Description = WebUtility.HtmlDecode(tagContent.Value);
                                break;

                            case "twitter:title":
                                metaInfo.Title = string.IsNullOrEmpty(metaInfo.Title)
                                    ? WebUtility.HtmlDecode(tagContent.Value)
                                    : WebUtility.HtmlDecode(metaInfo.Title);
                                break;

                            case "twitter:description":
                                metaInfo.Description = string.IsNullOrEmpty(metaInfo.Description)
                                    ? WebUtility.HtmlDecode(tagContent.Value)
                                    : WebUtility.HtmlDecode(metaInfo.Description);
                                break;

                            case "twitter:image:src":
                                metaInfo.ImageUrl = string.IsNullOrEmpty(metaInfo.ImageUrl)
                                    ? tagContent.Value
                                    : metaInfo.ImageUrl;
                                break;

                            case "twitter:site":
                                metaInfo.SiteName = string.IsNullOrEmpty(metaInfo.SiteName)
                                    ? WebUtility.HtmlDecode(tagContent.Value)
                                    : WebUtility.HtmlDecode(metaInfo.SiteName);
                                break;
                        }
                    }
                    else if (tagProperty != null && tagContent != null)
                    {
                        switch (tagProperty.Value.ToLower())
                        {
                            case "og:title":
                                metaInfo.Title = string.IsNullOrEmpty(metaInfo.Title)
                                    ? WebUtility.HtmlDecode(tagContent.Value)
                                    : WebUtility.HtmlDecode(metaInfo.Title);
                                break;

                            case "og:description":
                                metaInfo.Description = string.IsNullOrEmpty(metaInfo.Description)
                                    ? WebUtility.HtmlDecode(tagContent.Value)
                                    : WebUtility.HtmlDecode(metaInfo.Description);
                                break;

                            case "og:image":
                                metaInfo.ImageUrl = string.IsNullOrEmpty(metaInfo.ImageUrl)
                                    ? tagContent.Value
                                    : metaInfo.ImageUrl;
                                break;

                            case "og:site_name":
                                metaInfo.SiteName = string.IsNullOrEmpty(metaInfo.SiteName)
                                    ? WebUtility.HtmlDecode(tagContent.Value)
                                    : WebUtility.HtmlDecode(metaInfo.SiteName);
                                break;
                        }
                    }
                }
            }

            return !string.IsNullOrEmpty(metaInfo.Title) && !string.IsNullOrEmpty(metaInfo.Description)
                ? metaInfo
                : null;
        }

        private static bool UrlValid(string? source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            try
            {
                var url = new Uri(source);
                var valid = url.IsWellFormedOriginalString();
                return valid;
            }
            catch
            {
                return false;
            }
        }
    }
}