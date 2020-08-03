using System;
using System.Collections.Generic;

namespace WebScraper
{
    /// <summary>
    /// Web crawler, keeps scraping the web for content
    /// </summary>
    internal class Crawler
    {
        /// <summary>
        /// Crawl the page at the specified URI for content
        /// </summary>
        /// <param name="uri">URI of the page to scrape</param>
        public void CrawlPage(Uri uri)
        {
            RobotsTxt robotsTxt = new RobotsTxt();
            bool result = robotsTxt.TryParse(uri);

            if (result)
            {
                // Test whether a crawler is allowed to crawl this page
                if (robotsTxt.IsAllowed(uri))
                {
                    Console.WriteLine("Parsing page: " + uri.AbsoluteUri);
                 
                    // Crawl the page for information
                    PageParser pageParser = new PageParser();
                    PageParser.PageInfo pageInfo = pageParser.Parse(uri);

                    // Remove any links that violate the robots.txt file of the current domain
                    List<string> validSameDomainLinks = new List<string>();
                    foreach (string link in pageInfo.SameDomainLinks)
                    {
                        if (robotsTxt.IsAllowed(new Uri(link)))
                        {
                            validSameDomainLinks.Add(link);
                        }
                        else
                        {
                            Console.WriteLine("Violates robots.txt: " + link);
                        }
                    }

                    Console.WriteLine("\nTITLE");
                    Console.WriteLine("\t" + pageInfo.Title);

                    Console.WriteLine("\nSAME DOMAIN");
                    foreach (string link in validSameDomainLinks)
                    {
                        Console.WriteLine("\t" + link);
                    }

                    Console.WriteLine("\nEXTERNAL DOMAIN");
                    foreach (string link in pageInfo.ExternalDomainLinks)
                    {
                        Console.WriteLine("\t" + link);
                    }
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
