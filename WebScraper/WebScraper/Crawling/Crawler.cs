using System;
using System.Collections.Generic;

using Ganss.XSS;

using Databases;
using System.Threading;

namespace Crawling
{
    /// <summary>
    /// Web crawler, keeps scraping the web for content
    /// </summary>
    internal class Crawler
    {
        /// <summary>
        /// Random number generator
        /// </summary>
        private readonly Random RandomNumberGenerator;

        /// <summary>
        /// Ensures malicious code will not be inserted into the database
        /// </summary>
        private readonly HtmlSanitizer InputSanitizer;

        /// <summary>
        /// Most websites will link to their own pages internally
        /// To avoid requesting a new robots.txt file every single time, the
        /// robots.txt files are cached in memory
        /// </summary>
        private readonly Dictionary<string, RobotsTxt> CachedRobotsTxtPerHost;

        /// <summary>
        /// Crawl the page at the specified URI for content
        /// </summary>
        /// <param name="uri">URI of the page to scrape</param>
        /// <param name="requestDelayMs">Time in milliseconds between requests to avoid overloading websites</param>
        /// <returns>Information found while crawling the page</returns>
        private PageInfo CrawlPage(Uri uri, int requestDelayMs)
        {
            PageInfo pageInfo = new PageInfo();
            RobotsTxt robotsTxt = TryGetRobotsTxt(uri);

            // Test whether a crawler is allowed to crawl this page
            if (robotsTxt.IsAllowed(uri))
            {
                Console.WriteLine("Parsing page: " + uri.ToString());
                 
                // Crawl the page for information
                PageParser pageParser = new PageParser();
                pageInfo = pageParser.Parse(uri);

                // Do not overload the website
                Console.WriteLine("Sleeping thread for " + requestDelayMs + "ms to avoid overloading the website.");
                Thread.Sleep(requestDelayMs);

                // Remove any links that violate the robots.txt file of the current domain
                List<Uri> validSameDomainLinks = new List<Uri>();
                foreach (Uri link in pageInfo.Links)
                {
                    // External domain, no need to check robots.txt for it
                    if (link.Host != uri.Host)
                    {
                        continue;
                    }

                    // Same domain, check whether the crawler is allowed to access
                    // the page
                    if (robotsTxt.IsAllowed(link))
                    {
                        validSameDomainLinks.Add(link);
                    }
                }
            }
            else
            {
                Console.WriteLine("Skipping page: " + uri.ToString());
            }

            // Ensure no malicious input is sent to the database
            return SanitizePageInfo(pageInfo);
        }

        /// <summary>
        /// Helper function to retrieve robots.txt files
        /// </summary>
        /// <param name="uri">URI to retrieve the robots.txt file for</param>
        /// <returns>Structure containing robots.txt file information</returns>
        private RobotsTxt TryGetRobotsTxt(Uri uri)
        {
            // Attempt to retrieve a robots.txt file from the in-memory cache
            if (CachedRobotsTxtPerHost.ContainsKey(uri.Host))
            {
                Console.WriteLine("Host \"" + uri.Host + "\" has been retrieved from the robots.txt cache.");
                return CachedRobotsTxtPerHost[uri.Host];
            }
            else
            {
                // Request a new robots.txt file and cache it
                RobotsTxt robotsTxt = new RobotsTxt();
                robotsTxt.TryParse(uri);

                // Save the newly discovered robots.txt file in the in-memory cache
                CachedRobotsTxtPerHost[uri.Host] = robotsTxt;

                Console.WriteLine("Host \"" + uri.Host + "\" is unknown, adding to the robots.txt cache now.");
                return robotsTxt;
            }
        }

        /// <summary>
        /// Sanitize any text in the page information structure
        /// </summary>
        /// <param name="pageInfo">Page information structure to sanitize</param>
        /// <returns>Clean page information</returns>
        private PageInfo SanitizePageInfo(PageInfo pageInfo)
        {
            PageInfo cleanInfo = pageInfo;
            cleanInfo.Title = InputSanitizer.Sanitize(pageInfo.Title);
            cleanInfo.Description = InputSanitizer.Sanitize(pageInfo.Description);
            return cleanInfo;
        }

        /// <summary>
        /// Create a new page crawler instance
        /// </summary>
        public Crawler()
        {
            RandomNumberGenerator = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            InputSanitizer = new HtmlSanitizer();
            CachedRobotsTxtPerHost = new Dictionary<string, RobotsTxt>();
        }

        /// <summary>
        /// Start crawling the web
        /// </summary>
        /// <param name="minCrawlDelay">Minimum time between two requests to the same host</param>
        /// <param name="maxCrawlDelay">Maximum time between two requests to the same host</param>
        /// <param name="database">Database to save indexed pages into</param>
        public void Start(int minCrawlDelay, int maxCrawlDelay, Database database)
        {
            //#DEBUG: simply exit once we have crawled a thousand URLs
            int i = 1000;
            while (i-- > 0)
            {
                // Look for any discovered URLs and crawl them
                string[] urls = database.GetUncrawledUrls(100);

                foreach (string url in urls)
                {
                    // Random timeout in milliseconds to avoid overloading websites
                    int crawlDelayMs = RandomNumberGenerator.Next(minCrawlDelay * 1000, maxCrawlDelay * 1000);

                    // Crawl the page for information and links
                    PageInfo pageInfo = CrawlPage(new Uri(url), crawlDelayMs);

                    if (pageInfo.Uri != null && pageInfo.Links != null)
                    {
                        // Save the crawled page
                        database.AddOrUpdateCrawledPage(pageInfo);

                        // Convert from URI to string
                        List<string> links = new List<string>();
                        foreach (Uri link in pageInfo.Links)
                        {
                            RobotsTxt robotsTxt = TryGetRobotsTxt(link);

                            if (Uri.IsWellFormedUriString(link.ToString(), UriKind.Absolute) && robotsTxt.IsAllowed(link))
                            {
                                links.Add(link.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Incorrectly formed URI string or denied by robots.txt: " + link.ToString());
                            }
                        }

                        // Save the newly discovered URLs in the "pending" database
                        database.TryAddPendingUrls(links.ToArray());
                    }
                }
            }
        }
    }
}
