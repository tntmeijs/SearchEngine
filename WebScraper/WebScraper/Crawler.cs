using System;

using HtmlAgilityPack;

namespace WebScraper
{
    /// <summary>
    /// Web crawler, keeps scraping the web for content
    /// </summary>
    internal class Crawler
    {
        /// <summary>
        /// Scrape the page at the specified URI for content
        /// </summary>
        /// <param name="uri">URI of the page to scrape</param>
        public void ScrapePage(Uri uri)
        {
            RobotsTxt robotsTxt = new RobotsTxt();
            bool result = robotsTxt.TryParse(uri);

            if (result)
            {
                // Test whether a crawler is allowed to crawl this page
                if (robotsTxt.IsAllowed(uri))
                {
                    //#TODO: Parse page
                    //HtmlWeb web = new HtmlWeb();
                    //var html = web.Load(url);

                    Console.WriteLine("Parsing page: " + uri.AbsoluteUri);
                }
                else
                {
                    Console.WriteLine("Skipping page: " + uri.AbsoluteUri);
                }
            }
            else
            {
                Console.WriteLine("Unable to find a robots.txt file, or parsing failed.");
            }
        }
    }
}
