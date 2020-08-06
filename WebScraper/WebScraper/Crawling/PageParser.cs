using System;
using System.Collections.Generic;

using HtmlAgilityPack;

namespace Crawling
{
    /// <summary>
    /// Parse a page to find important information and index any links
    /// </summary>
    internal class PageParser
    {
        /// <summary>
        /// Parse a web page
        /// </summary>
        /// <param name="uri">Web page to parse</param>
        /// <returns>Parsed web page information</returns>
        public PageInfo Parse(Uri uri)
        {
            PageInfo pageInfo = new PageInfo
            {
                Uri = uri,
                Title = string.Empty,
                Description = string.Empty,
                Links = new List<Uri>()
            };

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument html = web.Load(uri);

                HtmlNode title = html.DocumentNode.SelectSingleNode("//title");
                pageInfo.Title = (title == null) ? string.Empty : title.InnerText;

                try
                {
                    HtmlNode description = html.DocumentNode.SelectSingleNode("//meta[@name='description']");
                    pageInfo.Description = (description == null) ? string.Empty : description.Attributes["content"].Value;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while attempting to read site description:\t" + e.Message);
                }

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

                            pageInfo.Links.Add(uriBuilder.Uri);
                        }
                        else
                        {
                            // Href did not start with a slash, try to turn it into an URI
                            try
                            {
                                Uri hrefAsUri = new Uri(hrefValue);
                                pageInfo.Links.Add(hrefAsUri);
                            }
                            catch (Exception)
                            {
                                // Failed to turn link into a valid URI, ignore and continue
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while parsing website:\t" + e.Message);
            }

            return pageInfo;
        }
    }
}
