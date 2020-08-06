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
        /// Links found to other pages
        /// </summary>
        public List<Uri> Links;
    }
}
