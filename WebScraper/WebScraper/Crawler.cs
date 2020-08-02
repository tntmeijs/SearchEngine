using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using HtmlAgilityPack;

namespace WebScraper
{
    /// <summary>
    /// Web scraper, keeps scraping the web for content
    /// </summary>
    internal class Crawler
    {
        private struct RobotsTxtContent
        {
            public List<string> DisallowedPaths;
            public List<string> AllowedPaths;
        }

        private RobotsTxtContent ParseRobotsTxt(StreamReader streamReader)
        {
            RobotsTxtContent content = new RobotsTxtContent();
            content.DisallowedPaths = new List<string>();
            content.AllowedPaths = new List<string>();

            bool inUserAgentBlock = false;

            // Read until the end of the stream
            string line = null;
            while ((line = streamReader.ReadLine()) != null)
            {
                // Convert to lower case to make string-matching easier
                line = line.ToLower();

                // Ignore comments
                if (line.StartsWith("#"))
                {
                    continue;
                }

                // Look for the "any" user agent name
                // Our crawler is too insignificant so it is highly unlikely that
                // any robots.txt file mentions our user agent name
                if (line.StartsWith("user-agent"))
                {
                    if (line.Split(' ')[1] != "*")
                    {
                        continue;
                    }
                    else
                    {
                        inUserAgentBlock = true;
                    }
                }

                // User agent for this crawler has not been found yet
                if (!inUserAgentBlock)
                {
                    continue;
                }

                // Parse any disallowed URLs
                if (line.StartsWith("disallow"))
                {
                    content.DisallowedPaths.Add(line.Split(' ')[1]);
                    continue;
                }

                // Parse any allowed URLs
                if (line.StartsWith("allow"))
                {
                    content.AllowedPaths.Add(line.Split(' ')[1]);
                    continue;
                }
            }

            return content;
        }

        /// <summary>
        /// Scrape the page at the specified URL for content
        /// </summary>
        /// <param name="url">URL of the page to scrape</param>
        public void ScrapePage(string url)
        {
            Uri uri = new Uri(url);

            UriBuilder robotTxtUri = new UriBuilder();
            robotTxtUri.Scheme = uri.Scheme;
            robotTxtUri.Host = uri.Host;
            robotTxtUri.Path = "robots.txt";

            // Look for a "robots.txt" file
            HttpWebRequest request = WebRequest.Create(robotTxtUri.Uri) as HttpWebRequest;
            
            // Name of the web crawler, most likely not in use by any website
            request.UserAgent = "TM_CRAWLER";

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                RobotsTxtContent content = ParseRobotsTxt(reader);

                Console.WriteLine("Paths allowed to index:");
                foreach (string path in content.AllowedPaths)
                {
                    Console.WriteLine("  - " + path);
                }

                Console.WriteLine("Paths not allowed to index:");
                foreach (string path in content.DisallowedPaths)
                {
                    Console.WriteLine("  - " + path);
                }
            }

            response.Close();

            //HtmlWeb web = new HtmlWeb();
            //var html = web.Load(url);
        }
    }
}
