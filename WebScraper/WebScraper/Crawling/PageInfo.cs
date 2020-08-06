using System;
using System.Collections.Generic;

namespace Crawling
{
    /// <summary>
    /// Page information retrieved while crawling a web page
    /// </summary>
    internal struct PageInfo
    {
        /// <summary>
        /// URI of this page
        /// </summary>
        public Uri Uri;

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
}
