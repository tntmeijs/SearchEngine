using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawling
{
    /// <summary>
    /// Allows for easy interpretation of robots.txt files
    /// </summary>
    internal class RobotsTxt
    {
        /// <summary>
        /// Allowed paths found in the robots.txt file
        /// </summary>
        public List<string> AllowedPaths { get; private set; }

        /// <summary>
        /// Disallowed paths found in the robots.txt file
        /// </summary>
        public List<string> DisallowedPaths { get; private set; }

        /// <summary>
        /// Crawl delay, optional, not all robots.txt files set this value
        /// </summary>
        public int CrawlDelay { get; private set; }

        /// <summary>
        /// Helper method to convert a wild card pattern into a valid Regex pattern
        /// </summary>
        /// <param name="pattern">Wild card pattern to convert into a Regex pattern</param>
        /// <returns>Valid Regex pattern</returns>
        public string WildCardToRegexPattern(string pattern)
        {
            // If the only thing in the path is a slash, it is meant to match any
            // character in that path
            // Same thing happens with empty patterns
            if (pattern.Length == 0 || (pattern.Length == 1 && pattern[0] == '/'))
            {
                return "\\/.*";
            }

            // The replacing logic is all about replacing the wild card characters
            // with their Regex equivalents, most of the time the wild card
            // characters have a different meaning in the context of Regex
            // 
            // The line below perform the following replacements:
            //      /   -->     \/
            //      !   -->     \!
            //      .   -->     \.
            //      +   -->     \+
            //      $   -->     \$
            //      ^   -->     \^
            //      ?   -->     \?
            //      *   -->     .*
            return pattern
                .Replace("/", "\\/")
                .Replace("!", "\\!")
                .Replace(".", "\\.")
                .Replace("+", "\\+")
                .Replace("$", "\\$")
                .Replace("^", "\\^")
                .Replace("?", "\\?")
                .Replace("*", ".*");
        }

        /// <summary>
        /// Attempt to retrieve a robots.txt file and parse the file when found
        /// </summary>
        /// <param name="uri">URI to try to find a robots.txt file for</param>
        /// <returns>True when a robots.txt file was found, false otherwise</returns>
        public bool TryParse(Uri uri)
        {
            AllowedPaths = new List<string>();
            DisallowedPaths = new List<string>();
            CrawlDelay = -1;

            // The code below contains some logic similar to this:
            //      lineBuffer.Split(':')[1]
            // This code is risky, but if the robots.txt file is valid, it should
            // always succeed and never throw an index out of bounds exception
            try
            {
                // Construct a uri that points to the expected location of a robots.txt file
                UriBuilder baseUri = new UriBuilder
                {
                    Scheme = uri.Scheme,
                    Host = uri.Host,
                    Path = "robots.txt"
                };

                // Attempt to retrieve a robots.txt file
                HttpWebRequest request = WebRequest.Create(baseUri.Uri) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                // Response is OK, start parsing the robots.txt file
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Look for the "User-Agent: *" line
                    // This crawler should use the rules in the "any" section of robots.txt
                    string lineBuffer = null;
                    bool inAnyUserAgentBlock = false;
                    while ((lineBuffer = reader.ReadLine()) != null)
                    {
                        // Simplify string parsing by turning all characters into
                        // lowercase characters and remove all whitespace
                        lineBuffer = lineBuffer.ToLower();
                        lineBuffer = lineBuffer.Replace(" ", "");
                        
                        // Ignore comments
                        if (lineBuffer.StartsWith("#"))
                        {
                            continue;
                        }

                        // Ignore any user agent identifiers except for "*"
                        if (lineBuffer.StartsWith("user-agent"))
                        {
                            if (lineBuffer.Split(':')[1] == "*")
                            {
                                inAnyUserAgentBlock = true;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        // User agent identifier has not been found yet
                        if (!inAnyUserAgentBlock)
                        {
                            continue;
                        }

                        // Parse allowed paths
                        if (lineBuffer.StartsWith("allow"))
                        {
                            AllowedPaths.Add(lineBuffer.Split(':')[1]);
                            continue;
                        }

                        // Parse disallowed paths
                        if (lineBuffer.StartsWith("disallow") && lineBuffer != "disallow:")
                        {
                            DisallowedPaths.Add(lineBuffer.Split(':')[1]);
                            continue;
                        }

                        // Parse crawl delays
                        if (lineBuffer.StartsWith("crawl-delay"))
                        {
                            CrawlDelay = int.Parse(lineBuffer.Split(':')[1]);
                            continue;
                        }
                    }

                    // Success
                    response.Close();
                    return true;
                }

                // Failure
                response.Close();
                return false;
            }
            catch (Exception)
            {
                // Fatal error occurred while parsing robots.txt
                // Some websites use a honey pot in robots.txt to automatically
                // blacklist malicious crawlers. To avoid accidentally parsing one
                // of those URLs, we return true here and set the "DisallowedPaths"
                // to the root path. This should prevent the crawler from submitting
                // any more requests to this domain.
                AllowedPaths = new List<string>();
                DisallowedPaths = new List<string>() { "/" };
                return true;
            }
        }

        /// <summary>
        /// Test whether a web crawler is allowed to crawl the page
        /// </summary>
        /// <param name="uri">URI to test</param>
        /// <returns>True when allowed to crawl, false when not</returns>
        public bool IsAllowed(Uri uri)
        {
            // Test all disallowed paths first
            foreach (string path in DisallowedPaths)
            {
                // Convert robots.txt patterns into Regex patterns
                string pattern = WildCardToRegexPattern(path);

                // Disallow access if the pattern matches
                if (Regex.Match(uri.PathAndQuery, pattern).Success)
                {
                    return false;
                }
            }

            // Test all allowed paths next
            foreach (string path in AllowedPaths)
            {
                // Convert robots.txt patterns into Regex patterns
                string pattern = WildCardToRegexPattern(path);

                // Allow access if the pattern matches
                if (Regex.Match(uri.PathAndQuery, pattern).Success)
                {
                    return true;
                }
            }

            return true;
        }
    }
}
