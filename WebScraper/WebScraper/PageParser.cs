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
                        UriBuilder uriBuilder = new UriBuilder
                        {
                            Scheme = uri.Scheme,
                            Host = uri.Host,
                            Path = hrefValue
                        };

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
