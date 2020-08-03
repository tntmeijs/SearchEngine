using System.Collections.Generic;

namespace WebScraper
{
    /// <summary>
    /// Page information retrieved while crawling a web page
    /// </summary>
    internal struct PageInfo
    {
        /// <summary>
        /// Verifies the usability of the structure
        /// Sets the "IsValid" flag in this structure
        /// </summary>
        /// <remarks>
        /// The most basic page information has at least one link to either an
        /// internal or an external web page, and it has a title. A description
        /// is optional but recommended.
        /// </remarks>
        public bool IsValid
        {
            get
            {
                bool titleOk = (Title.Length > 0);
                bool HasLinks = ((SameDomainLinks.Count > 0) || (ExternalDomainLinks.Count > 0));

                return (titleOk && HasLinks);
            }
        }

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
