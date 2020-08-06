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
            RobotsTxt robotsTxt;

            // Attempt to retrieve a robots.txt file from the in-memory cache
            if (CachedRobotsTxtPerHost.ContainsKey(uri.Host))
            {
                robotsTxt = CachedRobotsTxtPerHost[uri.Host];
                Console.WriteLine("Host \"" + uri.Host + "\" has been retrieved from the robots.txt cache.");
            }
            else
            {
                // Request a new robots.txt file and cache it
                robotsTxt = new RobotsTxt();
                robotsTxt.TryParse(uri);
                CachedRobotsTxtPerHost[uri.Host] = robotsTxt;

                // Do not overload the website
                Console.WriteLine("Sleeping thread for " + requestDelayMs + "ms to avoid overloading the website.");
                Thread.Sleep(requestDelayMs);

                Console.WriteLine("Host \"" + uri.Host + "\" is unknown, adding to the robots.txt cache now.");
            }

            // Test whether a crawler is allowed to crawl this page
            if (robotsTxt.IsAllowed(uri))
            {
                Console.WriteLine("Parsing page: " + uri.AbsoluteUri);
                 
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
                Console.WriteLine("Skipping page: " + uri.AbsoluteUri);
            }

            // Ensure no malicious input is sent to the database
            return SanitizePageInfo(pageInfo);
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
        /// <param name="tableName">Name of the database table to insert the data into</param>
        public void Start(int minCrawlDelay, int maxCrawlDelay, Database database, string tableName)
        {
            //#TODO: Get rid of "tableName", this is not the place to pass
            //#      database-related information to a function

            //#DEBUG: try to crawl 10.000 URLs
            int i = 10000;
            while (i-- > 0)
            {
                // Look for any discovered URLs and crawl them
                //#TODO: retrieve as many URLs as there are threads / tasks
                string[] urls = database.GetUncrawledUrls(1, tableName);

                foreach (string url in urls)
                {
                    // Random timeout in milliseconds to avoid overloading websites
                    int crawlDelayMs = RandomNumberGenerator.Next(minCrawlDelay, maxCrawlDelay) * 1000;

                    // Crawl the page for information and links
                    PageInfo pageInfo = CrawlPage(new Uri(url), crawlDelayMs);

                    if (pageInfo.Uri != null && pageInfo.Links != null)
                    {
                        // Save the crawled page
                        database.AddOrUpdateCrawledPage(pageInfo, tableName);

                        // Convert from URI to string
                        List<string> links = new List<string>();
                        foreach (Uri link in pageInfo.Links)
                        {
                            links.Add(link.ToString());
                        }

                        //#TODO: only add pages that have not been crawled yet
                        // Save the newly discovered URLs in the "pending" database
                        database.TryAddPendingUrls(links.ToArray(), tableName);
                    }
                }
            }
        }
    }
}
