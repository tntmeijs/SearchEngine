using System;
using System.Collections.Generic;

using HtmlAgilityPack;

namespace WebScraper
{
    /// <summary>
    /// Parse a page to find important information and index any links
    /// </summary>
    internal class PageParser
    {
        /// <summary>
        /// Information on a page that is useful to the crawler
        /// </summary>
        public struct PageInfo
        {
            /// <summary>
            /// Title of the web page
            /// </summary>
            public string Title;

            /// <summary>
            /// Description meta data
            /// </summary>
            public string Description;

            /// <summary>
            /// Links found to pages on the same domain
            /// </summary>
            public List<string> SameDomainLinks;

            /// <summary>
            /// Links found to pages on external domains
            /// </summary>
            public List<string> ExternalDomainLinks;
        }

        public PageInfo Parse(Uri uri)
        {
            PageInfo pageInfo = new PageInfo
            {
                Title = string.Empty,
                Description = string.Empty,
                SameDomainLinks = new List<string>(),
                ExternalDomainLinks = new List<string>()
            };

            HtmlWeb web = new HtmlWeb();
            HtmlDocument html = web.Load(uri);

            HtmlNode title = html.DocumentNode.SelectSingleNode("//head/title");
            pageInfo.Title = (title == null) ? string.Empty : title.InnerText;

            // Retrieve all link nodes
            var nodes = html.DocumentNode.Descendants("a");
            foreach (HtmlNode node in nodes)
            {
                string hrefValue = node.GetAttributeValue("href", string.Empty);
                if (hrefValue.Length > 0)
                {
                    if (hrefValue[0] == '/')
                    {
                        // Links to a page but without a host name, assume this is a same domain link
                        UriBuilder uriBuilder = new UriBuilder();
                        uriBuilder.Scheme = uri.Scheme;
                        uriBuilder.Host = uri.Host;
                        uriBuilder.Path = hrefValue;

                        pageInfo.SameDomainLinks.Add(uriBuilder.ToString());
                    }
                    else
                    {
                        // Href did not start with a slash, try to turn it into an URI
                        try
                        {
                            Uri hrefAsUri = new Uri(hrefValue);

                            if (hrefAsUri.Host == uri.Host)
                            {
                                // Same domain
                                pageInfo.SameDomainLinks.Add(hrefAsUri.ToString());
                            }
                            else
                            {
                                // Different domain
                                pageInfo.ExternalDomainLinks.Add(hrefAsUri.ToString());
                            }
                        }
                        catch (Exception)
                        {
                            // Failed to turn link into a valid URI, ignore and continue
                        }
                    }
                }
            }

            return pageInfo;
        }
    }
}
