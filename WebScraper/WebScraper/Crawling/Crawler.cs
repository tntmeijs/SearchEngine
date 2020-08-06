using System;
using System.Collections.Generic;

using Ganss.XSS;

using Databases;

namespace Crawling
{
    /// <summary>
    /// Web crawler, keeps scraping the web for content
    /// </summary>
    internal class Crawler
    {
        private readonly HtmlSanitizer InputSanitizer;

        /// <summary>
        /// Database object passed to the constructor of the crawler, cached for
        /// future use
        /// </summary>
        private readonly Database ProgramDatabase;

        /// <summary>
        /// Most websites will link to their own pages internally
        /// To avoid requesting a new robots.txt file every single time, the
        /// robots.txt files are cached in memory
        /// </summary>
        private Dictionary<string, RobotsTxt> CachedRobotsTxtPerHost;

        /// <summary>
        /// Crawl the page at the specified URI for content
        /// </summary>
        /// <param name="uri">URI of the page to scrape</param>
        /// <returns>Information found while crawling the page</returns>
        private PageInfo CrawlPage(Uri uri)
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

                Console.WriteLine("Host \"" + uri.Host + "\" is unknown, adding to the robots.txt cache now.");
            }

            // Test whether a crawler is allowed to crawl this page
            if (robotsTxt.IsAllowed(uri))
            {
                Console.WriteLine("Parsing page: " + uri.AbsoluteUri);
                 
                // Crawl the page for information
                PageParser pageParser = new PageParser();
                pageInfo = pageParser.Parse(uri);

                // Remove any links that violate the robots.txt file of the current domain
                List<string> validSameDomainLinks = new List<string>();
                foreach (string link in pageInfo.SameDomainLinks)
                {
                    if (robotsTxt.IsAllowed(new Uri(link)))
                    {
                        validSameDomainLinks.Add(link);
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
        /// <param name="database">Database to save indexed pages into</param>
        public Crawler(Database database)
        {
            InputSanitizer = new HtmlSanitizer();
            ProgramDatabase = database;
            CachedRobotsTxtPerHost = new Dictionary<string, RobotsTxt>();
        }

        /// <summary>
        /// Start crawling the web
        /// </summary>
        /// <param name="tableName">Name of the database table to insert the data into</param>
        public void Start(string tableName)
        {
            //#TODO: Get rid of "tableName", this is not the place to pass databse
            //#      related information to a function

            //#DEBUG: just crawl the Reddit homepage for links
            PageInfo pageInfo = CrawlPage(new Uri("https://reddit.com"));
            ProgramDatabase.AddPage(pageInfo, tableName);
        }
    }
}
